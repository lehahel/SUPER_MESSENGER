using System;
using System.Windows.Forms;
using System.Configuration;
using System.Drawing;
using ChatClient;

namespace client
{
    public partial class MainForm : Form
    {
        private Sendrec _sendrec;
        private bool _isConnected;

        private delegate void TextToBox(string txt);
        private delegate void ShowError(string txt);

        public MainForm()
        {
            InitializeComponent();

            _sendrec = new Sendrec();
            _sendrec.OnMessage += SendrecOnMessage;
            _sendrec.OnError += SendrecOnError;
        }

        private void SendrecOnError(string errorText)
        {
            if (InvokeRequired)
            {
                var str = new ShowError(SendrecOnError);
                Invoke(str, errorText);
            }
            else
            {
                MessageBox.Show(errorText);
            }
        }

        private void SendrecOnMessage(string message)
        {
            AppendText(message);
            if(!message.Contains(":"))
            {
                tsmSetting.Enabled = true;
                btnSend.Enabled = false;
                btnConnect.Text = "Подключиться";                

                _isConnected = false;
                _sendrec.CloseForm();
            }

        }

        public string GetMessage()
        {
            return tbMessage.Text;
        }

        private void OnClientLoad(object sender, EventArgs e)
        {
            btnSend.Enabled = false;
        }

        private void OnBtnSendClick(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(tbMessage.Text) && tbMessage.Text.Length < 65000)
            {
                MessageBox.Show("Сообщение не может быть пустым и быть больше 65000 символов");
            }
            else
            {
                _sendrec.SendMessageFromSocket(GetMessage());
                tbMessage.Text = "";
            }
        }

        private void OnClientFormClosing(object sender, FormClosingEventArgs e)
        {
            _sendrec?.CloseForm();
        }

        private string CheckSettings()
        {
            if (String.IsNullOrEmpty(ConfigurationManager.AppSettings["server_ip"]))
            {
                return("Не указан ip адрес сервера");
            }

            if (String.IsNullOrEmpty(ConfigurationManager.AppSettings["username"]))
            {
                return ("Не указано имя пользователя");
            }

            return null;
        }

        private void OnBtnConnectClick(object sender, EventArgs e)
        {
            var error = CheckSettings();
            if (error == null)
            {
                if (_isConnected)
                {
                    tsmSetting.Enabled = true;
                    btnSend.Enabled = false;
                    btnConnect.Text = "Подключиться";

                    _isConnected = false;
                    _sendrec.CloseForm();
                }
                else
                {
                    tsmSetting.Enabled = false;
                    _sendrec.Connect();

                    if (_sendrec.IsConnected())
                    {
                        btnSend.Enabled = true;
                        btnConnect.Text = "Отключиться";

                        _isConnected = true;
                    }
                    else
                    {
                        MessageBox.Show("Не могу подключиться к серверу");
                        btnConnect.Text = "Подключиться";
                        tsmSetting.Enabled = true;

                        _isConnected = false;

                        return;
                    }
                }
            }
            else
            {
                MessageBox.Show(error);
            }
        }

	    public void AppendText(string text)
	    {
            if (InvokeRequired)
            {
                var str = new TextToBox(AppendText);
                Invoke(str, text);
            }
            else
            {
                if (text.StartsWith("*"))
                {
                    rtbLog.SelectionColor = Color.Blue;
                }

                rtbLog.AppendText(text + "\r\n");
                rtbLog.ScrollToCaret();
            }
		}



        private void OnTsmExitClick(object sender, EventArgs e)
        {
            _sendrec.CloseForm();
            Close();
        }
        
        private void OnTsmSettingClick(object sender, EventArgs e)
        {
            var settings = new Settings();
            settings.ShowDialog();
        }

        private void rtbLog_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
