using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MQTT_Server
{
    public partial class login_password : UserControl
    {
        public string _login { get; set; }
        public string _password { get; set; }
        public bool _on {  get; set; }

        MainWindow mainWindow;

        public login_password(MainWindow mainWindow, string login, string password, bool on = true)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            _login = login;
            _password = password;
            _on = on;

            _tbk_login.Text = login;
            _tbk_password.Text = password;
        }

        private void ON(object sender, MouseButtonEventArgs e)
        {
            _btn_off.Visibility = Visibility.Visible;
            _on = true;
        }

        private void OFF(object sender, MouseButtonEventArgs e)
        {
            _btn_off.Visibility = Visibility.Collapsed;
            _on = false;
        }

        private void Delete(object sender, MouseButtonEventArgs e)
        {
            mainWindow.Delete(this);
        }
    }
}
