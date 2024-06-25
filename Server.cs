using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DiceRoller
{
    public class Server
    {
        // Singleton Instance
        private static Server instance;

        // Server Components
        private TcpListener tcpListener;
        private ConcurrentDictionary<TcpClient, Task> clients = new ConcurrentDictionary<TcpClient, Task>();
        private ConcurrentDictionary<TcpClient, string> userNames = new ConcurrentDictionary<TcpClient, string>();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        // Events
        public event EventHandler<string> MessageReceived;
        public event EventHandler<string> LogMessage;
        public event EventHandler<string> UserConnected;
        public event EventHandler<string> UserDisconnected;

        // Properties
        public bool IsServerRunning { get; private set; }

        // Singleton Instance
        private Server() { }

        public static Server Instance => instance ?? (instance = new Server());

        // Start Server
        public void StartServer(string ip, int port)
        {
            IPAddress localAddr = IPAddress.Parse(ip);
            tcpListener = new TcpListener(localAddr, port);
            tcpListener.Start();
            IsServerRunning = true;
            Log("[Server] Server started on " + ip + ":" + port);

            Task.Run(() => AcceptClients(), cancellationTokenSource.Token);
            Task.Run(() => SendKeepAliveMessages(), cancellationTokenSource.Token);
        }

        // Stop Server
        public void StopServer()
        {
            IsServerRunning = false;
            tcpListener.Stop();
            foreach (var client in clients.Keys)
            {
                client.Close();
            }
            clients.Clear();
            Log("[Server] Server stopped.");
        }

        // Get Client Count
        public int GetClientCount() => clients.Count;

        // Broadcast Message to Clients
        public void BroadcastMessage(string message, TcpClient sender)
        {
            Log($"[Server] Trying to broadcast message to {clients.Count} clients.");
            foreach (var client in clients.Keys)
            {
                if (client.Connected && client != sender)
                {
                    try
                    {
                        NetworkStream stream = client.GetStream();
                        if (stream.CanWrite)
                        {
                            StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                            writer.WriteLine(message);
                            Log($"[Server] Message sent to client {client.GetHashCode()}.");
                        }
                    }
                    catch (ObjectDisposedException ex)
                    {
                        HandleClientDisconnection(client, ex);
                    }
                    catch (Exception ex)
                    {
                        HandleClientDisconnection(client, ex);
                    }
                }
                else
                {
                    Log($"[Server] Skipping client {client.GetHashCode()} due to it being not connected or being the sender.");
                }
            }
        }

        // Accept Clients
        private async Task AcceptClients()
        {
            while (IsServerRunning)
            {
                try
                {
                    TcpClient client = await tcpListener.AcceptTcpClientAsync();
                    Log("[Server] Client connected.");
                    if (clients.TryAdd(client, Task.Run(() => HandleClient(client))))
                    {
                        Log("[Server] Client added successfully.");
                    }
                    else
                    {
                        Log("[Server] Client was not added (possibly already added).");
                    }
                }
                catch (Exception ex)
                {
                    Log("[Server] Error accepting client: " + ex.Message);
                }
            }
        }

        // Handle Client
        private async Task HandleClient(TcpClient client)
        {
            StreamReader reader = new StreamReader(client.GetStream(), Encoding.UTF8);
            StreamWriter writer = new StreamWriter(client.GetStream(), Encoding.UTF8) { AutoFlush = true };

            // Recevoir le nom d'utilisateur lors de la connexion
            string username = await reader.ReadLineAsync();
            if (userNames.TryAdd(client, username))
            {
                UserConnected?.Invoke(this, username);
                BroadcastMessage("CONNECTED_USERS:" + string.Join(",", userNames.Values), null);
            }

            try
            {
                string message;
                while ((message = await reader.ReadLineAsync()) != null)
                {
                    Log($"[Server] Received from {client.GetHashCode()}: {message}");
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        MessageReceived?.Invoke(this, message);
                        BroadcastMessage(message, client);
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"[Server] Client {client.GetHashCode()} disconnected: {ex.Message}");
            }
            finally
            {
                if (userNames.TryRemove(client, out string removedUsername))
                {
                    UserDisconnected?.Invoke(this, removedUsername);
                    BroadcastMessage("USER_DISCONNECTED:" + removedUsername, null);
                    BroadcastMessage("CONNECTED_USERS:" + string.Join(",", userNames.Values), null);
                }
                clients.TryRemove(client, out _);
                Log($"[Server] Client {client.GetHashCode()} removed. Total clients: {clients.Count}");
            }
        }

        // Send Keep-Alive Messages
        private async Task SendKeepAliveMessages()
        {
            while (IsServerRunning)
            {
                try
                {
                    foreach (var client in clients.Keys)
                    {
                        if (client.Connected)
                        {
                            try
                            {
                                NetworkStream stream = client.GetStream();
                                if (stream.CanWrite)
                                {
                                    StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                                    writer.WriteLine("KEEP_ALIVE");
                                    Log($"[Server] Sent KEEP_ALIVE to client {client.GetHashCode()}");
                                }
                            }
                            catch (Exception ex)
                            {
                                HandleClientDisconnection(client, ex);
                            }
                        }
                    }
                    await Task.Delay(60000); // Send keep-alive every 60 seconds
                }
                catch (Exception ex)
                {
                    Log($"[Server] Error in keep-alive task: {ex.Message}");
                }
            }
        }

        // Handle Client Disconnection
        private void HandleClientDisconnection(TcpClient client, Exception ex)
        {
            if (ex != null)
            {
                Log($"[Server] Error handling client {client.GetHashCode()}: {ex.Message}");
            }
            if (userNames.TryRemove(client, out string removedUsername))
            {
                UserDisconnected?.Invoke(this, removedUsername);
                BroadcastMessage("USER_DISCONNECTED:" + removedUsername, null);
            }
            clients.TryRemove(client, out _);
            Log($"[Server] Client {client.GetHashCode()} removed. Total clients: {clients.Count}");
            client.Close();
        }

        // Log Messages
        private void Log(string message)
        {
            if (!message.Contains("KEEP_ALIVE"))
            {
                Console.WriteLine(message);
                LogMessage?.Invoke(this, message);
            }
        }
    }
}
