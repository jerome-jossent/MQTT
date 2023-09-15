using MQTT_Manager_jjo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace MQTT_StringToFile
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        MQTT_One_Topic_Subscribed topic;

        //veolia/itv/advise/inference_lite
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            topic = new MQTT_One_Topic_Subscribed(mqtt_client);

            topic_subscribed_UC._Link(topic);

            mqtt_client.mqttClient.ConnectedAsync += MqttClient_ConnectedAsync;
        }

        private Task MqttClient_ConnectedAsync(MQTTnet.Client.MqttClientConnectedEventArgs arg)
        {
            //subscribes
            mqtt_client.MQTTClient_Subscribes();
            return null;
        }

        public string filepath
        {
            get => _filepath;
            set
            {
                if (_filepath == value) return;
                _filepath = value;
                OnPropertyChanged();
            }
        }
        string _filepath;

        private void _btn_selectfile_Click(object sender, MouseButtonEventArgs e)
        {
            var fileDialog = new System.Windows.Forms.SaveFileDialog();
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filepath = fileDialog.FileName;
            }
        }
    }
}