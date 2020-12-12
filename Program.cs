using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace syncxmlcli
{
    public class Message
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Msg { get; set; }
        public string Stamp { get; set; }

        public override string ToString()
        {
            return $"From: {From}\nTo: {To}\n{Msg}\nStamp: {Stamp}";
        }
    }

    public class SynchronousSocketClient
    {
        public static int TAM = 1024;
        private static string _ip = "127.0.0.1";
        private static int _port = 11000;

        public static void StartClient()
        {
            ReadServerIpPort();
            IPAddress ipAddress = System.Net.IPAddress.Parse(_ip);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, _port);

            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(remoteEP);

            Console.WriteLine("Socket connected to {0}\n", socket.RemoteEndPoint.ToString());

            //Message request = new Message { From = "11", To = "22", Msg = "Hola!", Stamp = "Aitor E." };
            Message request = ObtenerMensaje();

            Send(socket, request);

            Message response = Receive(socket);

            Console.WriteLine($"\nRespuesta:\n{response}\n");

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        public static void ReadServerIpPort()
        {
            string s;
            System.Console.WriteLine("Datos del servidor: ");
            string defIp = GetLocalIpAddress().ToString();
            System.Console.Write("Dir. IP [{0}]: ", defIp);
            s = Console.ReadLine();
            if ((s.Length > 0) && (s.Replace(".", "").Length == s.Length - 3))
            {
                _ip = s;
            }
            else
            {
                _ip = defIp;
            }
            System.Console.Write("PUERTO [{0}]: ", _port);
            s = Console.ReadLine();
            if (Int32.TryParse(s, out int i))
            {
                _port = i;
            }
        }

        private static IPAddress GetLocalIpAddress()
        {
            List<IPAddress> ipAddressList = new List<IPAddress>();
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            int t = ipHostInfo.AddressList.Length;
            string ip;
            for (int i = 0; i < t; i++)
            {
                ip = ipHostInfo.AddressList[i].ToString();
                if (ip.Contains(".") && !ip.Equals("127.0.0.1")) ipAddressList.Add(ipHostInfo.AddressList[i]);
            }
            if (ipAddressList.Count > 0)
            {
                return ipAddressList[0];//devuelve la primera posible
            }
            return null;
        }

        private static Message ObtenerMensaje()
        {
            System.Console.WriteLine("Mensaje: ");
            System.Console.Write("From:  ");
            string f = Console.ReadLine();
            System.Console.Write("To:    ");
            string t = Console.ReadLine();
            System.Console.Write("Msg:   ");
            string m = Console.ReadLine();
            System.Console.Write("Stamp: ");
            string s = Console.ReadLine();

            return new Message { From = f, To = t, Msg = m, Stamp = s };
        }

        private static void Send(Socket socket, Message message)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Message));
            Stream stream = new MemoryStream();
            serializer.Serialize(stream, message);
            byte[] byteData = ((MemoryStream)stream).ToArray();
            // string xml = Encoding.ASCII.GetString(byteData, 0, byteData.Length);
            // Console.WriteLine(xml);
            int bytesSent = socket.Send(byteData);
        }

        private static Message Receive(Socket socket)
        {
            byte[] bytes = new byte[TAM];
            int bytesRec = socket.Receive(bytes);
            string xml = Encoding.ASCII.GetString(bytes, 0, bytesRec);
            // Console.WriteLine(xml);
            byte[] byteArray = Encoding.ASCII.GetBytes(xml);
            MemoryStream stream = new MemoryStream(byteArray);
            Message response = (Message)new XmlSerializer(typeof(Message)).Deserialize(stream);
            return response;
        }

        public static int Main(String[] args)
        {
            StartClient();
            return 0;
        }
    }
}