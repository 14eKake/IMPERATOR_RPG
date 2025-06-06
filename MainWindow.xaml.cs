using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace DiceRoller
{
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private Server server;
        public string Username { get; set; }
        private string serverIp;
        private int serverPort;
        private bool isReconnecting = false;
        private ObservableCollection<string> connectedUsers = new ObservableCollection<string>();

        public Server Server => server;

        public MainWindow()
        {
            InitializeComponent();
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            ConnectedUsersList.ItemsSource = connectedUsers;
            server = Server.Instance;
            server.MessageReceived -= Server_MessageReceived;
            server.MessageReceived += Server_MessageReceived;
            server.UserConnected -= Server_UserConnected;
            server.UserConnected += Server_UserConnected;
            server.UserDisconnected -= Server_UserDisconnected;
            server.UserDisconnected += Server_UserDisconnected;
        }

        private void Server_UserConnected(object sender, string username)
        {
            Dispatcher.Invoke(() => connectedUsers.Add(username));
        }

        private void Server_UserDisconnected(object sender, string username)
        {
            Dispatcher.Invoke(() => connectedUsers.Remove(username));
        }

        public void StartServer(string ip, int port)
        {
            server.MessageReceived -= Server_MessageReceived;
            server.MessageReceived += Server_MessageReceived;
            server.LogMessage += Server_LogMessage;
            server.StartServer(ip, port);
            ChatBox.AppendText($"Serveur démarré sur {ip}:{port}\n");
        }

        public void ConnectToServer(string ip, int port, string username)
        {
            Username = username;
            serverIp = ip;
            serverPort = port;
            client = new TcpClient();
            try
            {
                client.Connect(ip, port);
                ChatBox.AppendText($"Connecté au serveur {ip}:{port}\n");
                UsernameDisplay.Text = $"Pseudo: {Username}";

                StreamWriter writer = new StreamWriter(client.GetStream(), Encoding.UTF8) { AutoFlush = true };
                writer.WriteLine(username);

                Task.Run(() => ReceiveMessages(client));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Échec de la connexion : {ex.Message}");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (client != null && client.Connected)
            {
                SendMessage(client, $"USER_DISCONNECTED:{Username}");
                client.Close();
            }
        }

        private void Server_MessageReceived(object sender, string message)
        {
            Dispatcher.Invoke(() =>
            {
                Console.WriteLine("Message received by server: " + message);
                ProcessReceivedMessage(message);
            });
        }

        private void Server_LogMessage(object sender, string logMessage)
        {
            Dispatcher.Invoke(() => AppendToChat(logMessage + "\n"));
        }

        private async Task ReceiveMessages(TcpClient client)
        {
            NetworkStream stream = null;
            StreamReader reader = null;

            try
            {
                stream = client.GetStream();
                reader = new StreamReader(stream, Encoding.UTF8);

                string message;
                while ((message = await reader.ReadLineAsync()) != null)
                {
                    if (message.Contains("KEEP_ALIVE")) continue;

                    Dispatcher.Invoke(() => ProcessReceivedMessage(message));
                }
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine("NetworkStream closed: " + ex.Message);
            }
            catch (IOException ex)
            {
                if (!isReconnecting)
                {
                    isReconnecting = true;
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Le serveur a mis trop de temps à répondre, déconnexion.");
                        ReconnectToServer();
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => MessageBox.Show("Erreur lors de la réception des données : " + ex.Message));
            }
            finally
            {
                reader?.Dispose();
                stream?.Dispose();
            }
        }

        private void ReconnectToServer()
        {
            try
            {
                client.Close();
                ConnectToServer(serverIp, serverPort, Username);
                isReconnecting = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Échec de la reconnexion : " + ex.Message);
                Task.Delay(10000).ContinueWith(_ => ReconnectToServer());
            }
        }

        private void ProcessReceivedMessage(string message)
        {
            if (message.StartsWith("CONNECTED_USERS:"))
            {
                UpdateConnectedUsersList(message.Substring("CONNECTED_USERS:".Length));
            }
            else if (message.StartsWith("USER_DISCONNECTED:"))
            {
                string username = message.Substring("USER_DISCONNECTED:".Length);
                connectedUsers.Remove(username);
            }
            else if (message.StartsWith("DICE_RESULT:"))
            {
                // Traite les résultats de dés avec du texte enrichi
                string content = message.Substring("DICE_RESULT:".Length).Trim();
                string[] parts = content.Split(new[] { "|SuccessCount:" }, StringSplitOptions.None);
                string coloredMessage = parts[0].Trim();
                int successCount = int.Parse(parts[1].Trim());

                DisplayColoredMessage(coloredMessage, successCount);
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    ChatBox.AppendText(message + "\n");
                    ChatBox.ScrollToEnd();
                });
            }
        }


        private void UpdateConnectedUsersList(string userList)
        {
            string[] users = userList.Split(',');
            connectedUsers.Clear();
            foreach (string user in users)
            {
                connectedUsers.Add(user);
            }
        }


        private void AppendToChat(string message)
        {
            ChatBox.AppendText(message);
            ChatBox.ScrollToEnd();
        }

        private void CharacterSheet_Click(object sender, RoutedEventArgs e)
        {
            CharacterSheet sheet = new CharacterSheet(this);
            sheet.Show();
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(InputBox.Text) && client != null && client.Connected)
            {
                SendMessageToServerOrChat($"{Username}: {InputBox.Text}");
                InputBox.Clear();
            }
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(InputBox.Text))
            {
                SendMessageToServerOrChat($"{Username}: {InputBox.Text}");
                InputBox.Clear();
                e.Handled = true;
            }
        }

        public void SendMessageToServerOrChat(string message)
        {
            if (server != null && server.IsServerRunning)
            {
                server.BroadcastMessage(message, null);
            }
            else if (client != null && client.Connected)
            {
                SendMessage(client, message);
            }
        }

        private void SendMessage(TcpClient client, string message)
        {
            if (client == null || !client.Connected)
            {
                MessageBox.Show("Client non connecté ou null.");
                return;
            }

            StreamWriter writer = new StreamWriter(client.GetStream(), Encoding.UTF8) { AutoFlush = true };
            writer.WriteLine(message);
        }

        private void DisplayColoredMessage(string message, int successCount)
        {
            var paragraph = new Paragraph();
            var matches = Regex.Matches(message, @"<run color=""(green|red)"">(\d+)</run>");

            int lastPos = 0;

            foreach (Match match in matches)
            {
                if (match.Index > lastPos)
                {
                    paragraph.Inlines.Add(new Run(message.Substring(lastPos, match.Index - lastPos)));
                }

                string color = match.Groups[1].Value;
                string text = match.Groups[2].Value;

                var run = new Run(text)
                {
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color))
                };
                paragraph.Inlines.Add(run);

                lastPos = match.Index + match.Length;
            }

            if (lastPos < message.Length)
            {
                paragraph.Inlines.Add(new Run(message.Substring(lastPos)));
            }

            paragraph.Inlines.Add(new LineBreak());
            paragraph.Inlines.Add(new Run($"SuccessCount: {successCount}")
            {
                FontWeight = FontWeights.Bold
            });

            ChatBox.Document.Blocks.Add(paragraph);
            ChatBox.Document.Blocks.Add(new Paragraph(new Run("\n")));
            ChatBox.ScrollToEnd();
        }

    }
}