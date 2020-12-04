using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Configuration;

namespace client
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            tbIp.Text = ConfigurationManager.AppSettings["server_ip"];
            tbUsername.Text = ConfigurationManager.AppSettings["Username"];
        }

        private void OnBtnSaveClick(object sender, EventArgs e)
        {
            if (tbUsername.Text != "" && tbUsername.Text.Length <= 20)
            {
                var addressParts = tbIp.Text.Split('.');
                byte part;
                var partsCount = (from x in addressParts where (byte.TryParse(x, out part) == true) select x).Count();

                if (partsCount == 4)
                {
                    Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    currentConfig.AppSettings.Settings["server_ip"].Value = tbIp.Text;
                    currentConfig.AppSettings.Settings["Username"].Value = tbUsername.Text;
                    currentConfig.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");
                    Close();
                }
                else
                {
                    MessageBox.Show("Введен некорректный ip адрес сервера. Пожалуйста, повторите попытку");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Имя пользователя не может быть пустым и превышать длину в 20 символов");
                return;
            }
        }
    }
}

