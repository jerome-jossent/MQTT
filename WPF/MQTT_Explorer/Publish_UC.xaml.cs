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

namespace MQTT_Explorer
{
    public partial class Publish_UC : UserControl
    {
        public class PublishArguments: EventArgs
        {
            public string topic;
            public string payload_string;
            public bool retain;
        }

        public event EventHandler<PublishArguments> _Publish_Event;



        public Publish_UC()
        {
            InitializeComponent();
        }

        private void Publish_Click(object sender, MouseButtonEventArgs e)
        {
            _Publish_Event?.Invoke(this, new PublishArguments() { 
                topic = _topic.Text, 
                payload_string = _payload.Text,
            retain = _retain.IsChecked == true
            });
        }
    }
}
