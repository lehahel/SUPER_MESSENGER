using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Configuration;

namespace WindowsFormsApplication1
{
    public class Serv
    {
        private List<UserOnline> _onlineUsers = new List<UserOnline>();
        private static String _host = Dns.GetHostName();
        private static IPAddress _ipAddr = Dns.GetHostByName(_host).AddressList[0];
        public static string ip = _ipAddr.ToString();
        private IPEndPoint _ipEndpoint = new IPEndPoint(_ipAddr, 20000);
        private Socket _servSocket;

        public void Start(object obj) // ставлю  на прослушкур
        {
            _servSocket = new Socket(_ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _servSocket.Bind(_ipEndpoint);
            _servSocket.Listen(10);

            while (true) // Добавляю клиентов в лист
            {
                try
                {
                    var socket = _servSocket.Accept();
                   
                    if (_onlineUsers.Count < Int32.Parse(ConfigurationManager.AppSettings["Max_client"]))
                    {
                        var usOnline = new UserOnline();
                        usOnline.ClientSockets = socket;
                        _onlineUsers.Add(usOnline);
                        new Thread(ReceiveMessage).Start(usOnline);
                    }
                    else
                    {
                        socket.Send(Encoding.UTF8.GetBytes("Достигнуто максимальное количество клиентов для данного сервера. Попробуйте подключиться позже."));
                        Thread.Sleep(100);
                        socket.Close();
                    }
                }
                catch (SocketException)
                {
                    break;
                }
            }
        }

	    private void ReceiveMessage(object obj) // Получаю сообщения
	    {
		    var usOnline = (UserOnline) obj;
			var buffer = new byte[1024];

		    while (true)
		    {
                try
                {
                    IPEndPoint remoteIpEndPoint = usOnline.ClientSockets.RemoteEndPoint as IPEndPoint;
                    var mapUser = new User();
                    var byterec = usOnline.ClientSockets.Receive(buffer);
                    var data = Encoding.UTF8.GetString(buffer, 0, byterec);

                    if (usOnline.Nick == true)
                    {
                        SendToAll(usOnline.Username + " : " + data);
                        mapUser.Username = usOnline.Username;
                        mapUser.User_ip = remoteIpEndPoint.Address.ToString();
                        mapUser.Message = data;
                        mapUser.Date = DateTime.Now;

                        var dataRep = new DataRepository();
                        dataRep.Create(mapUser);
                    }
                    else // если только вошел
                    {
                        usOnline.Nick = true;
                        usOnline.Username = data;
                        mapUser.Username = data;

                        var userlist = new DataRepository().GetUsers();
                        if (userlist != null)
                        {
                            userlist.Reverse();

                            int dubluser = 0;
                            for (int i = _onlineUsers.Count - 1; i >= 0; i--)
                            {
                                if (_onlineUsers[i].Username == data)
                                    dubluser++;
                            }

                            if (dubluser < 2)
                            {
                                var messages = from s in userlist select $"{PrepareMessage(s.Username)} : {PrepareMessage(s.Message)}";
                                var messageLog = string.Join("\n", messages.ToArray());
                                usOnline.ClientSockets.Send(Encoding.UTF8.GetBytes(messageLog));

                                for (int i = _onlineUsers.Count - 1; i >= 0; i--)
                                {
                                    _onlineUsers[i].ClientSockets.Send(Encoding.UTF8.GetBytes("*В чат вошел : " + data));
                                }
                            }
                            else
                            {
                                usOnline.ClientSockets.Send(Encoding.UTF8.GetBytes("Пользователь с таким именем уже присутствует в чате"));
                                return;
                            }
                        }
                    }
                }
			    catch (Exception)
			    {
                    _onlineUsers.Remove(usOnline);
				    break;
				}
			}
		}

        private string PrepareMessage(string message)// для норм вывода истории
        {
            if (string.IsNullOrEmpty(message))
            {
                return "";
            }
            return message.Trim('\r', '\n', '\t', ' ');
        }

		private void SendToAll(string message) // отправляю клиентам из листа
		{
            if (string.IsNullOrEmpty(message) || _onlineUsers == null)
			{
				return;
			}

			var buffer = Encoding.UTF8.GetBytes(message);

			for (int i = _onlineUsers.Count - 1; i >= 0; i--)
			{
				try
				{
                    _onlineUsers[i].ClientSockets.Send(buffer);
				}
				catch (Exception)
				{
                    _onlineUsers.RemoveAt(i);
                }
			}
		}


        public void Closeform() // завершаю работу
        {
            SendToAll("*Сервер завершил свою работу");
            _onlineUsers.ForEach(x => x.ClientSockets.Close());
            _onlineUsers.Clear();

            _servSocket?.Close();
        }
    }
}
