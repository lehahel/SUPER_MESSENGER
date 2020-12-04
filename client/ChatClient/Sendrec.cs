using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Threading;

namespace ChatClient
{
    public class Sendrec
    {
        private IPEndPoint _ipEndPoint;
        private Socket _sendes;

        public delegate void MessageRcv(string message);
        public delegate void Error(string message);
        public event MessageRcv OnMessage;
        public event Error OnError;

        public void Connect()
        {
            IPAddress ipAddr = IPAddress.Parse(ConfigurationManager.AppSettings["server_ip"]);
            _ipEndPoint = new IPEndPoint(ipAddr, 20000);
            _sendes = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                _sendes.Connect(_ipEndPoint);
                new Thread(RecServ).Start();
            }
            catch (Exception)
            {
                return;
            }
        }

        public bool IsConnected()
        {
            return _sendes != null && _sendes.Connected;
        }

        private void RecServ()
        {
            var message = ConfigurationManager.AppSettings["Username"];
            var sendBuffer = Encoding.UTF8.GetBytes(message);
            try
            {
                _sendes.Send(sendBuffer);
            }
            catch (Exception)
            {
                return;
            }

            while (true)
            {
                try
                {
                    var recBuffer = new byte[1024];
                    var byterec = _sendes.Receive(recBuffer);
                    var data = Encoding.UTF8.GetString(recBuffer, 0, byterec);

                    if (data != null)
                    {
                        OnMessage?.Invoke(data);
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        public void SendMessageFromSocket(string message)
        {
            try
            {
                var msg = Encoding.UTF8.GetBytes(message);
                _sendes.Send(msg);
            }
            catch (Exception)
            {
                OnError?.Invoke("При отправке сообщения произошла ошибка");
            }
        }

        public void CloseForm()
        {
            _sendes?.Close();
        }
    }
}
