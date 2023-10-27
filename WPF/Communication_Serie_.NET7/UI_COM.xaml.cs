using System;
using System.Collections.Generic;
using System.IO.Ports;
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

namespace Communication_Serie_.NET7
{
    public partial class UI_COM : UserControl
    {
        public Communication_Serie _CS;
        public SerialDataReceivedEventHandler _DataReceivedHandler;

        public UI_COM()
        {
            InitializeComponent();
            Communication_Serie.PortCom_Fill(cbx_port);
            cbx_port.SelectedIndex = cbx_port.Items.Count - 1;
            Communication_Serie.Bauds_Fill(cbx_bauds);
            cbx_bauds.SelectedValue = "9600";
        }

        public void _Link(Communication_Serie cs, SerialDataReceivedEventHandler dataReceivedHandler)
        {
            _CS = cs;
            _DataReceivedHandler = dataReceivedHandler;
        }

        private void btn_connection_deconnection_Click(object sender, RoutedEventArgs e)
        {
            if (btn_connection_deconnection.Content.ToString() == "Connexion")
            {
                if (_CS == null || _CS.Port != cbx_port.Text || _CS.Baud!=cbx_bauds.Text) 
                    _CS = new Communication_Serie(cbx_port.Text, cbx_bauds.Text, _DataReceivedHandler);
                _CS.PortCom_ON();
                btn_connection_deconnection.Content = "Déconnexion";
            }
            else
            {
                _CS.PortCom_OFF();
                btn_connection_deconnection.Content = "Connexion";
            }
        }

        private void _PortRefresh(object sender, MouseButtonEventArgs e)
        {
            Communication_Serie.PortCom_Fill(cbx_port);
        }
        public void _PortRefresh()
        {
            Communication_Serie.PortCom_Fill(cbx_port);
        }

        public void _SetPort(int port)
        {
            _SetPort(port.ToString());
        }

        public void _SetPort(string port)
        {
            cbx_bauds.Text = port;
        }
    }
}
