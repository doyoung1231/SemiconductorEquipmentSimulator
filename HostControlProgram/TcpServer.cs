using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HostControlProgram
{
    public class TcpServer
    {
        private List<TcpListener> _listeners = new List<TcpListener>();
        private bool _running = false;

        public event Action<string> OnDataReceived;
        public event Action<string> OnClientConnected;
        public event Action OnClientDisconnected;

        public void Start(int[] ports)
        {
            _running = true;
            foreach (int port in ports)
            {
                var listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                _listeners.Add(listener);

                var t = new Thread(() => ListenLoop(listener));
                t.IsBackground = true;
                t.Start();

                Console.WriteLine($"[Server] Listening on port {port}");
            }
        }

        public void Stop()
        {
            _running = false;
            foreach (var l in _listeners) l.Stop();
        }

        private void ListenLoop(TcpListener listener)
        {
            while (_running)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    string ip = ((IPEndPoint)client.Client.RemoteEndPoint)
                                .Address.ToString();
                    OnClientConnected?.Invoke(ip);

                    var t = new Thread(() => HandleClient(client));
                    t.IsBackground = true;
                    t.Start();
                }
                catch { break; }
            }
        }

        private void HandleClient(TcpClient client)
        {
            var stream = client.GetStream();
            var buffer = new byte[4096];
            string leftover = "";
            try
            {
                while (true)
                {
                    int n = stream.Read(buffer, 0, buffer.Length);
                    if (n == 0) break;

                    string received = leftover +
                        Encoding.UTF8.GetString(buffer, 0, n);
                    string[] msgs = received.Split('\n');

                    for (int i = 0; i < msgs.Length - 1; i++)
                    {
                        string msg = msgs[i].Trim();
                        if (!string.IsNullOrEmpty(msg))
                            OnDataReceived?.Invoke(msg);
                    }
                    leftover = msgs[msgs.Length - 1];
                }
            }
            catch { }
            finally
            {
                client.Close();
                OnClientDisconnected?.Invoke();
            }
        }
    }
}