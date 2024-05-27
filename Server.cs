using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private static Server instance;
        private TcpListener tcpListener;
        private ConcurrentDictionary<TcpClient, Task> clients = new ConcurrentDictionary<TcpClient, Task>();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public event EventHandler<string> MessageReceived;
        public event EventHandler<string> LogMessage; // Nouvel événement pour les logs

        public bool IsServerRunning { get; private set; }

        private Server() { }

        public static Server Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Server();
                }
                return instance;
            }
        }

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

        private async Task AcceptClients()
        {
            while (IsServerRunning)
            {
                try
                {
                    TcpClient client = await tcpListener.AcceptTcpClientAsync();
                    Console.WriteLine("Client connected.");
                    if (clients.TryAdd(client, Task.Run(() => HandleClient(client))))
                    {
                        Console.WriteLine("Client added successfully.");
                        Log("[Server] Client added successfully.");

                    }
                    else
                    {
                        Console.WriteLine("Client was not added (possibly already added).");
                        Log("[Server] Client was not added (possibly already added).");

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting client: {ex.Message}");
                    Log("[Server] Error accepting client: " + ex.Message);

                }
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            StreamReader reader = new StreamReader(client.GetStream(), Encoding.UTF8);
            try
            {
                string message;
                while ((message = await reader.ReadLineAsync()) != null)
                {
                    Log($"[Server] Received from {client.GetHashCode()}: {message}");
                    Console.WriteLine($"Received from {client.GetHashCode()}: {message}");
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        MessageReceived?.Invoke(this, message); // Trigger the MessageReceived event
                        BroadcastMessage(message, client);
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"[Server] Client {client.GetHashCode()} disconnected: {ex.Message}");

                Console.WriteLine($"Client {client.GetHashCode()} disconnected: {ex.Message}");
            }
            finally
            {
                clients.TryRemove(client, out _);
                Console.WriteLine($"Client {client.GetHashCode()} removed. Total clients: {clients.Count}");
                Log($"[Server] Client {client.GetHashCode()} removed. Total clients: {clients.Count}");

            }
        }
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
                                Log($"[Server] Error sending keep-alive message to client {client.GetHashCode()}: {ex.Message}");
                                Console.WriteLine($"Error sending keep-alive message to client {client.GetHashCode()}: {ex.Message}");
                                clients.TryRemove(client, out _);
                                Console.WriteLine($"Client {client.GetHashCode()} removed due to error. Total clients: {clients.Count}");
                                Log($"[Server] Client {client.GetHashCode()} removed due to error. Total clients: {clients.Count}");

                            }
                        }
                    }
                    await Task.Delay(60000); // Envoyer un message de keep-alive toutes les 60 secondes
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in keep-alive task: {ex.Message}");
                    Log($"[Server] Error in keep-alive task: {ex.Message}");

                }
            }
        }

        public void StopServer()
        {
            IsServerRunning = false;
            tcpListener.Stop();
            foreach (var client in clients.Keys)
            {
                client.Close();
            }
            clients.Clear();
            Console.WriteLine("Server stopped.");
            Log("[Server] Server stopped.");

        }

        public void BroadcastMessage(string message, TcpClient sender)
        {
            Log($"[Server] Trying to broadcast message to {clients.Count} clients.");
            Console.WriteLine($"Trying to broadcast message to {clients.Count} clients.");
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
                            writer.WriteLine(message);
                            Log($"[Server] Message sent to client {client.GetHashCode()}.");
                            Console.WriteLine($"Message sent to client {client.GetHashCode()}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"[Server] Error sending message to client {client.GetHashCode()}: {ex.Message}");
                        Console.WriteLine($"Error sending message to client {client.GetHashCode()}: {ex.Message}");
                        clients.TryRemove(client, out _);
                        Console.WriteLine($"Client {client.GetHashCode()} removed due to error. Total clients: {clients.Count}");
                        Log($"[Server] Client {client.GetHashCode()} removed due to error. Total clients: {clients.Count}");

                    }
                }
                else
                {
                    Console.WriteLine($"Skipping client {client.GetHashCode()} due to it being not connected.");
                    Log($"[Server] Skipping client {client.GetHashCode()} due to it being not connected.");
                }
            }
        }


        public int GetClientCount()
        {
            return clients.Count;
        }

        private void Log(string message)
        {
            if (message.Contains("KEEP_ALIVE"))
            {
                return;
            }
            Console.WriteLine(message);
            LogMessage?.Invoke(this, message); // Trigger the LogMessage event
        }
    }
}
