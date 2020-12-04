using System;
using System.Windows.Forms;
using System.Threading;
using System.Configuration;

namespace WindowsFormsApplication1
{
    public partial class Server : Form
    {
        private Serv _servSock = new Serv();

        public Server()
        {
           InitializeComponent();
           lbip.Text = Serv.ip;
        }

        public void Btnstart_Click(object sender, EventArgs e)
        {
            if (Btnstart.Text == "Запустить")
            {
                Btnstart.Text = "Остановить";
                tbmaxclient.Enabled = false;

                System.Configuration.Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                currentConfig.AppSettings.Settings["Max_client"].Value = tbmaxclient.Text;
                currentConfig.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");

                new Thread(_servSock.Start).Start();
            }
            else
            {
                tbmaxclient.Enabled = true;
                Btnstart.Text = "Запустить";

                _servSock.Closeform();
            }
        }

        private void OnServLoad(object sender, EventArgs e)
        {
            tbmaxclient.Text = ConfigurationManager.AppSettings["Max_client"]; 
        }

        private void ServFormClosing(object sender, FormClosingEventArgs e)
        {
            _servSock.Closeform();
        }
    }
}
