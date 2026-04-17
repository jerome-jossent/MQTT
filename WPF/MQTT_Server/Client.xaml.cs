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
    public partial class Client : UserControl
    {
        public string ClientId { get; }
        DateTime connection_Date;

        public Client(string clientId, string username)
        {
            InitializeComponent();

            ClientId = clientId;
            connection_Date = DateTime.Now;
            string d = connection_Date.ToString("dd/MM/yyyy HH:mm:ss.fff");

            _tbk_date.Text = "[" + d + "]";
            _tbk_ID.Text = ClientId;
            _tbk_username.Text = username;
        }

    }
}
