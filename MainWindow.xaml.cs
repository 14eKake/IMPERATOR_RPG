using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiceRoller
{
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private Server server;
        public string Username { get; set; }
        private string serverIp; // Ajouter cette variable
        private int serverPort;  // Ajouter cette variable
        private bool isReconnecting = false;

        public Server Server
        {
            get { return server; }
        }

        public MainWindow()
        {
            InitializeComponent();
            server = Server.Instance; // Utiliser l'instance singleton du serveur
            server.MessageReceived -= Server_MessageReceived; // Désabonner pour éviter les abonnements multiples
            server.MessageReceived += Server_MessageReceived;

        }

        // Démarrer le serveur
        public void StartServer(string ip, int port)
        {
            server.MessageReceived -= Server_MessageReceived;  // Assurez-vous de désabonner avant de réabonner
            server.MessageReceived += Server_MessageReceived;
            server.LogMessage += Server_LogMessage; // S'abonner aux logs du serveur
            server.StartServer(ip, port);
            ChatBox.AppendText($"Serveur démarré sur {ip}:{port}\n");
        }

        // Connecter au serveur
        public void ConnectToServer(string ip, int port, string username)
        {
            Username = username;
            serverIp = ip; // Stocker l'IP du serveur
            serverPort = port; // Stocker le port du serveur
            client = new TcpClient();
            try
            {
                client.Connect(ip, port);
                ChatBox.AppendText($"Connecté au serveur {ip}:{port}\n");
                UsernameDisplay.Text = $"Pseudo: {Username}";
                Task.Run(() => ReceiveMessages(client));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Échec de la connexion : {ex.Message}");
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
            Dispatcher.Invoke(() =>
            {
                AppendToChat(logMessage + "\n");
            });
        }

        private async Task ReceiveMessages(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            try
            {
                string message;
                while ((message = await reader.ReadLineAsync()) != null)
                {
                    if (message.Contains("KEEP_ALIVE"))
                    {
                        continue; // Ignorer les messages KEEP_ALIVE
                    }

                    Dispatcher.Invoke(() =>
                    {
                        ProcessReceivedMessage(message);
                    });
                }
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
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Erreur lors de la réception des données : " + ex.Message);
                });
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
                Task.Delay(10000).ContinueWith(_ => ReconnectToServer()); // Retenter la reconnexion après 10 secondes
            }
        }



        public void ProcessReceivedMessage(string message)
        {
            try
            {
                // Supprimer tous les caractères avant "DICE_RESULT:" s'il est présent
                int index = message.IndexOf("DICE_RESULT:");
                if (index >= 0)
                {
                    message = message.Substring(index);
                }

                if (message.StartsWith("DICE_RESULT:"))
                {
                    // Retirer le préfixe "DICE_RESULT:"
                    string content = message.Substring("DICE_RESULT:".Length).Trim();
                    string[] parts = content.Split(new[] { "|SuccessCount:" }, StringSplitOptions.None);
                    string coloredMessage = parts[0].Trim();
                    int successCount = int.Parse(parts[1].Trim());

                    // Afficher le message coloré
                    DisplayColoredMessage(coloredMessage, successCount);
                }
                else
                {
                    AppendToChat(message + "\n");
                }
            }
            catch (JsonException ex)
            {
                AppendToChat("Erreur lors de la réception du message : " + ex.Message + "\n");
            }
            catch (FormatException ex)
            {
                AppendToChat("Erreur lors de l'analyse du SuccessCount : " + ex.Message + "\n");
            }
        }


        private void DisplayColoredMessage(string message, int successCount)
        {
            var paragraph = new Paragraph();
            var matches = Regex.Matches(message, @"<run color=""(green|red)"">(\d+)</run>");

            int lastPos = 0;

            foreach (Match match in matches)
            {
                // Ajouter le texte non coloré avant la balise <run>
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

            // Ajouter le texte restant après la dernière balise <run>
            if (lastPos < message.Length)
            {
                paragraph.Inlines.Add(new Run(message.Substring(lastPos)));
            }

            // Ajouter le SuccessCount à la fin du message
            paragraph.Inlines.Add(new LineBreak());
            paragraph.Inlines.Add(new Run($"SuccessCount: {successCount}")
            {
                FontWeight = FontWeights.Bold
            });

            ChatBox.Document.Blocks.Add(paragraph);

            // Ajouter un retour à la ligne et enlever le gras
            var newParagraph = new Paragraph(new Run("\n"));
            ChatBox.Document.Blocks.Add(newParagraph);

            ChatBox.ScrollToEnd();
        }


        private bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || (strInput.StartsWith("[") && strInput.EndsWith("]")))
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void UpdateChatBox(FormattedMessage message)
        {
            var paragraph = new Paragraph();
            int lastPos = 0;
            foreach (var format in message.Formats)
            {
                if (format.Index > lastPos)
                {
                    paragraph.Inlines.Add(new Run(message.Text.Substring(lastPos, format.Index - lastPos)));
                }

                var color = (Color)ColorConverter.ConvertFromString(format.Color);
                var run = new Run(message.Text.Substring(format.Index, format.Length))
                {
                    Foreground = new SolidColorBrush(color)
                };
                paragraph.Inlines.Add(run);
                lastPos = format.Index + format.Length;
            }

            if (lastPos < message.Text.Length)
            {
                paragraph.Inlines.Add(new Run(message.Text.Substring(lastPos)));
            }

            ChatBox.Document.Blocks.Add(paragraph);
            ChatBox.ScrollToEnd();
        }

        private void AppendToChat(string message)
        {
            ChatBox.AppendText(message);
            ChatBox.ScrollToEnd();
        }

        private void CharacterSheet_Click(object sender, RoutedEventArgs e)
        {
            CharacterSheet sheet = new CharacterSheet(this); // Passez 'this' pour garder une référence à MainWindow
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
                e.Handled = true;  // Empêche le bip système par défaut pour la touche Entrée
            }
        }

        public void SendMessageToServerOrChat(string message)
        {
            if (server != null && server.IsServerRunning)
            {
                Console.WriteLine("Server is not null and running. Broadcasting message.");
                server.BroadcastMessage(message, null);  // Envoi au serveur pour diffusion aux clients
            }
            else if (client != null && client.Connected)
            {
                SendMessage(client, message);  // Envoi direct si le client n'est pas le serveur
            }
            else
            {
                Console.WriteLine("Neither server nor client is available to send the message.");
            }
            // Ne pas appeler AppendToChat ici pour éviter d'afficher le message avant qu'il ne soit diffusé par le serveur
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

    }

    public class FormattedMessage
    {
        public string Text { get; set; }
        public List<FormatSpecification> Formats { get; set; } = new List<FormatSpecification>();
        public int SuccessCount { get; set; }
    }

    public class FormatSpecification
    {
        public int Index { get; set; }
        public int Length { get; set; }
        public string Color { get; set; }
    }
}
