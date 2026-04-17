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

namespace MQTT_Manager_2_jjo
{
    public partial class MQTT_Client_JJ_UC : UserControl
    {
        MQTT_Client_JJ client;
        string _IP;
        int _port;

        Dictionary<MQTT_Client_JJ.MQTT_Status_JJ, Color> statusColor = new Dictionary<MQTT_Client_JJ.MQTT_Status_JJ, Color>() {
            { MQTT_Client_JJ.MQTT_Status_JJ.nul, Colors.DarkGray },
            { MQTT_Client_JJ.MQTT_Status_JJ.disconnected, Colors.Red},
            { MQTT_Client_JJ.MQTT_Status_JJ.connecting, Colors.Orange },
            { MQTT_Client_JJ.MQTT_Status_JJ.connected, Colors.LimeGreen },
        };

        public MQTT_Client_JJ_UC()
        {
            InitializeComponent();
            Loaded += MQTT_Client_JJ_UC_Loaded;
            Unloaded += MQTT_Client_JJ_UC_Unloaded;
        }

        public void _Link(MQTT_Client_JJ client)
        {
            client._onStatusChange += Client__onStatusChange;
            this.client = client;
            client._onMQTTStatusChanged();
        }

        void Client__onStatusChange(object? sender, MQTT_Client_JJ.MQTT_Status_JJ e)
        {
            SetStatusConnection(statusColor[e], e.ToString());
        }

        #region IHM

        void MQTT_Client_JJ_UC_Loaded(object sender, RoutedEventArgs e)
        {
            Fill_cbx_ips();
        }

        void MQTT_Client_JJ_UC_Unloaded(object sender, RoutedEventArgs e)
        {
            MQTTClient_Stop();
        }


        public void _SetIP(string IP) { _cbx_ips.Text = IP; }
        public void _SetPort(int port) { _tbx_port.Text = port.ToString(); }
        public void _SetLogin(string login) { _tbx_login.Text = login; }
        public void _SetPassword(string password) { _tbx_password.Text = password; }
        public void _SetClientID(string clientID) { _tbx_ID.Text = clientID; }




        void Connect_Click(object sender, MouseButtonEventArgs e)
        {
            MQTTClient_Connect();
        }

        void SetStatusConnection(Color c, string message = "")
        {
            Dispatcher.BeginInvoke(
                () =>
                    {
                        _ell_connection_status.ToolTip = message;
                        _ell_connection_status.Fill = new SolidColorBrush(c);
                    }
            );
        }

        void MQTTClient_Connect()
        {
            _IP = _cbx_ips.Text;
            _port = int.Parse(_tbx_port.Text);
            if (client._Connect(_IP, _port,
                login: _tbx_login.Text,
                mdp: _tbx_password.Text,
                clientID: _tbx_ID.Text,
                new MQTT_Message_JJ(_tbx_lastwill_topic.Text,
                                    _tbx_lastwill_message.Text,
                                    _ckb_lastwill_retain.IsChecked == true)
                ))
            {
                Save_ips();
                btn_disconnect.Visibility = Visibility.Visible;
            }
        }

        void MQTTClient_Stop()
        {
            client?._Disconnect();
        }

        void Disconnect_Click(object sender, MouseButtonEventArgs e)
        {
            MQTTClient_Stop();
            btn_disconnect.Visibility = Visibility.Hidden;
        }
        #endregion

        #region IP & IHM
        void Fill_cbx_ips()
        {
            List<string> ips = Get_Saved_ips();
            _cbx_ips.Items.Clear();
            foreach (string ip in ips)
                _cbx_ips.Items.Add(ip);
            if (_cbx_ips.Items.Count > 0)
                _cbx_ips.SelectedIndex = 0;
        }

        void SelectedIP_Delete_Click(object sender, MouseButtonEventArgs e)
        {
            string val = _cbx_ips.Text;

            List<string> ips = Get_Saved_ips();
            if (ips.Contains(val))
                ips.Remove(val);
            Save_ips(ips);
            Fill_cbx_ips();
            if (_cbx_ips.Items.Count > 0)
                _cbx_ips.SelectedIndex = 0;
        }

        List<string> Get_Saved_ips()
        {
            string IP_all = Properties.Settings.Default["IPs"].ToString();
            string[] ips = IP_all.ToString().Split(";", options: StringSplitOptions.RemoveEmptyEntries);
            return ips.ToList();
        }

        void Save_ips()
        {
            List<string> ips = new List<string>();
            foreach (var item in _cbx_ips.Items)
                ips.Add(item.ToString());

            if (!ips.Contains(_cbx_ips.Text))
                ips.Add(_cbx_ips.Text);
            Save_ips(ips);
        }

        void Save_ips(List<string> ips)
        {
            string s = string.Join(";", ips);
            Properties.Settings.Default["IPs"] = s;
            Properties.Settings.Default.Save();
        }
        #endregion
    }
}