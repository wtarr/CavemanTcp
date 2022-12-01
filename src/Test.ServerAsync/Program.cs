﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CavemanTcp;

namespace Test.ServerAsync
{
    class Program
    {
        static bool _RunForever = true;
        static CavemanTcpServer _Server = null;

        static void Main(string[] args)
        {
            InitializeServer();
            _Server.Start();

            Console.WriteLine("Listening on tcp://127.0.0.1:8000");

            while (_RunForever)
            {
                Console.Write("Command [? for help]: ");
                string userInput = Console.ReadLine();
                if (String.IsNullOrEmpty(userInput)) continue;

                if (userInput.Equals("?"))
                {
                    Console.WriteLine("-- Available Commands --");
                    Console.WriteLine("");
                    Console.WriteLine("   cls                          Clear the screen");
                    Console.WriteLine("   q                            Quit the program");
                    Console.WriteLine("   start                        Start listening for connections (listening: " + (_Server != null ? _Server.IsListening.ToString() : "false") + ")");
                    Console.WriteLine("   stop                         Stop listening for connections  (listening: " + (_Server != null ? _Server.IsListening.ToString() : "false") + ")");
                    Console.WriteLine("   list                         List connected clients");
                    Console.WriteLine("   send [guid] [data]           Send data to a specific client");
                    Console.WriteLine("   sendt [ms] [guid] [data]     Send data to a specific client with specified timeout");
                    Console.WriteLine("   read [guid] [count]          Read [count] bytes from a specific client");
                    Console.WriteLine("   readt [ms] [guid] [count]    Read [count] bytes from a specific client with specified timeout");
                    Console.WriteLine("   kick [guid]                  Disconnect a specific client from the server");
                    Console.WriteLine("   dispose                      Dispose of the server");
                    Console.WriteLine("   stats                        Retrieve statistics");
                    Console.WriteLine("");
                    continue;
                }

                if (userInput.Equals("c") || userInput.Equals("cls"))
                {
                    Console.Clear();
                    continue;
                }

                if (userInput.Equals("q"))
                {
                    _RunForever = false;
                    break;
                }

                if (userInput.Equals("start"))
                { 
                    _Server.Start();
                }

                if (userInput.Equals("stop"))
                {
                    _Server.Stop();
                }

                if (userInput.Equals("list"))
                {
                    List<ClientMetadata> clients = _Server.GetClients().ToList();
                    if (clients != null)
                    {
                        Console.WriteLine("Clients: " + clients.Count);
                        foreach (ClientMetadata curr in clients)
                        {
                            Console.WriteLine("  " + curr.ToString());
                        }
                    }
                    else
                    {
                        Console.WriteLine("(null)");
                    }
                }

                if (userInput.StartsWith("send "))
                {
                    string[] parts = userInput.Split(new char[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);
                    Guid guid = Guid.Parse(parts[1]);
                    string data = parts[2];

                    WriteResult wr = _Server.SendAsync(guid, data).Result;
                    if (wr.Status == WriteResultStatus.Success)
                        Console.WriteLine("Success");
                    else
                        Console.WriteLine("Non-success status: " + wr.Status);
                }

                if (userInput.StartsWith("sendt "))
                {
                    string[] parts = userInput.Split(new char[] { ' ' }, 4, StringSplitOptions.RemoveEmptyEntries);
                    int timeoutMs = Convert.ToInt32(parts[1]);
                    Guid guid = Guid.Parse(parts[2]);
                    string data = parts[3];

                    WriteResult wr = _Server.SendWithTimeoutAsync(timeoutMs, guid , data).Result;
                    if (wr.Status == WriteResultStatus.Success)
                        Console.WriteLine("Success");
                    else
                        Console.WriteLine("Non-success status: " + wr.Status);
                }

                if (userInput.StartsWith("read "))
                {
                    string[] parts = userInput.Split(new char[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);
                    Guid guid = Guid.Parse(parts[1]);
                    int count = Convert.ToInt32(parts[2]);

                    ReadResult rr = _Server.ReadAsync(guid, count).Result;
                    if (rr.Status == ReadResultStatus.Success)
                        Console.WriteLine("Retrieved " + rr.BytesRead + " bytes: " + Encoding.UTF8.GetString(rr.Data));
                    else
                        Console.WriteLine("Non-success status: " + rr.Status.ToString());
                }

                if (userInput.StartsWith("readt "))
                {
                    string[] parts = userInput.Split(new char[] { ' ' }, 4, StringSplitOptions.RemoveEmptyEntries);
                    int timeoutMs = Convert.ToInt32(parts[1]);
                    Guid guid = Guid.Parse(parts[2]);
                    int count = Convert.ToInt32(parts[3]);

                    ReadResult rr = _Server.ReadWithTimeoutAsync(timeoutMs, guid, count).Result;
                    if (rr.Status == ReadResultStatus.Success)
                        Console.WriteLine("Retrieved " + rr.BytesRead + " bytes: " + Encoding.UTF8.GetString(rr.Data));
                    else
                        Console.WriteLine("Non-success status: " + rr.Status.ToString());
                }

                if (userInput.StartsWith("kick "))
                {
                    string[] parts = userInput.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    Guid guid = Guid.Parse(parts[1]);

                    _Server.DisconnectClient(guid);
                }

                if (userInput.Equals("dispose"))
                {
                    _Server.Dispose();
                }

                if (userInput.Equals("stats"))
                {
                    Console.WriteLine(_Server.Statistics);
                }
            }
        }

        static void InitializeServer()
        {
            _Server = new CavemanTcpServer("127.0.0.1", 8000, false, null, null);
            _Server.Logger = Logger;

            _Server.Events.ClientConnected += (s, e) =>
            {
                Console.WriteLine("Client " + e.Client.ToString() + " connected to server");
            };

            _Server.Events.ClientDisconnected += (s, e) =>
            {
                Console.WriteLine("Client " + e.Client.ToString() + " disconnected from server");
            };
        }

        static void Logger(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
