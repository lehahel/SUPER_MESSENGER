using System.Net;
using System.Net.Sockets;

namespace WindowsFormsApplication1
{
    class UserOnline
    {
        public string Username { get; set; }
        public Socket ClientSockets { get; set; }
        public bool Nick { get; set; }
    }
}
