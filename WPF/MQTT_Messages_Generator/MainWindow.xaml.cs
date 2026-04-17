using System;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MQTTnet;

namespace MQTT_Messages_Generator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        protected void OnPropertyChanged(string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public event PropertyChangedEventHandler PropertyChanged;


        MQTTnet.Client.IMqttClient mqtt_client;

        CancellationTokenSource _cts;
        string topic1 = "MonTopic0";
        string topic2 = "MonTopic1";
        string topic3 = "MonTopic2";

        public bool _run
        {
            get => run; set
            {
                run = value; OnPropertyChanged();
                _btn_start_stop_Update();
            }
        }
        bool run;

        bool run_once;

        public int _occurences { get => occurences; set { occurences = value; OnPropertyChanged(); } }
        int occurences;

        int occurences_saved = 2;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MQTTExplorer_Start();
            Publish_Date_Start();
            _occurences = occurences_saved;
            _btn_start_stop_Update();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            _cts.Cancel();
            Thread.Sleep(100);
        }

        #region MQTT Management
        void MQTTExplorer_Start()
        {
            var mqttFactory = new MqttFactory();
            mqtt_client = mqttFactory.CreateMqttClient();

            var mqttClientOptions = new MQTTnet.Client.MqttClientOptionsBuilder()
                .WithClientId("MQTT_Messages_Generator")
                .WithTcpServer("127.0.0.1", 5461)
                .WithCredentials("loginsecret01", "mdpsecret01")
                .Build();

            //mqtt_explorer.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;

            mqtt_client.ConnectingAsync += MqttClient_ConnectingAsync;
            mqtt_client.ConnectedAsync += MqttClient_ConnectedAsync;
            mqtt_client.DisconnectedAsync += MqttClient_DisconnectedAsync;

            mqtt_client.ConnectAsync(mqttClientOptions, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
        }

        void MQTTExplorer_Stop()
        {
            if (mqtt_client == null) return;

            var mqttClientOptions = new MQTTnet.Client.MqttClientDisconnectOptionsBuilder()
                .Build();

            mqtt_client.DisconnectAsync(mqttClientOptions, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
        }

        Task MqttClient_ConnectingAsync(MQTTnet.Client.MqttClientConnectingEventArgs arg)
        {
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        Task MqttClient_ConnectedAsync(MQTTnet.Client.MqttClientConnectedEventArgs arg)
        {
            Dispatcher.BeginInvoke(() => { Title = "Connected"; });
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        Task MqttClient_DisconnectedAsync(MQTTnet.Client.MqttClientDisconnectedEventArgs arg)
        {
            return MQTTnet.Internal.CompletedTask.Instance;
        }



        void MQTT_Publish(string topic, string text, bool retain = false)
        {
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(text);
            MQTT_Publish(topic, utf8Bytes, retain);
        }



        void MQTT_Publish(string topic, byte[] payload, bool retain = false)
        {
            if (mqtt_client == null || !mqtt_client.IsConnected)
                return;

            MqttApplicationMessage applicationMessage = new MqttApplicationMessageBuilder()
                                                            .WithTopic(topic)
                                                            .WithPayload(payload)
                                                            .WithRetainFlag(retain)
                                                            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                                                            .Build();

            mqtt_client.PublishAsync(applicationMessage, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
        }

        #endregion

        void Publish_Date_Start()
        {
            _cts = new CancellationTokenSource();
            Task.Run(() => Thread_PublishDate(_cts.Token));
        }

        void Thread_PublishDate(CancellationToken cancellationToken)
        {
            while (true)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_run)
                    {
                        if (_occurences > 0)
                        {
                            _occurences--;
                            Publish();
                        }
                        else
                        {
                            _occurences = occurences_saved;
                            _run = false;
                        }
                    }

                    if (run_once)
                    {
                        run_once = false;
                        Publish();
                    }

                    Thread.Sleep(100);
                }
            }
        }

        void Publish()
        {
            DATA data = new DATA();
            DATA_light data_light = new DATA_light();
            string msg;
            //msg = data_light.ToJSON();
            msg = data.ToJSON();
            MQTT_Publish(topic1, msg);
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!_run)
                occurences_saved = _occurences;
            _run = !_run;
        }

        private void _btn_start_stop_Update()
        {
            Dispatcher.BeginInvoke(new Action(() => { _btn_start_stop.Content = _run ? "Pause" : "Start"; }));
        }

        private void Button_2_Click(object sender, RoutedEventArgs e)
        {
            DATA data = new DATA();
            string msg = "";

            msg = data.ToJSON();
            MQTT_Publish(topic1, msg);

            data.datas_list[0].val_bool = !data.datas_list[0].val_bool;
            msg = data.ToJSON();
            MQTT_Publish(topic1, msg);
            Thread.Sleep(100);
            MQTT_Publish(topic2, msg);
            Thread.Sleep(100);
            MQTT_Publish(topic3, msg);
        }
    }
}