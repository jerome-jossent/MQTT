using System;
using System.Collections.Generic;
using System.ComponentModel;
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

using MQTTnet;

namespace MQTT_PortiK_Init_for_PLC
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region BINDED PROPERTY
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string _IP
        {
            get { return IP; }
            set
            {
                if (IP == value) return;
                IP = value;
                OnPropertyChanged("_IP");
            }
        }
        string IP = "192.168.31.6";// Properties.Resources.ResourceManager.GetString("_IP");

        public string _Port
        {
            get { return Port; }
            set
            {
                if (Port == value) return;
                Port = value;
                OnPropertyChanged("_Port");
            }
        }
        string Port = "1883";// Properties.Resources.ResourceManager.GetString("_Port");

        public string _TempsBetweenPub
        {
            get { return TempsBetweenPub; }
            set
            {
                if (TempsBetweenPub == value) return;
                TempsBetweenPub = value;
                OnPropertyChanged("_TempsBetweenPub");
            }
        }
        string TempsBetweenPub = "50";// Properties.Resources.ResourceManager.GetString("_TempsBetweenPub");




        #endregion

        MQTTnet.Client.IMqttClient mqttClient;
        Dictionary<string, string> data;

        #region Window Management
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        void Init_Click(object sender, MouseButtonEventArgs e)
        {
            MQTT_ConnectAndPublishTopics();
        }

        void Close_Click(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        void Window_Closing(object sender, CancelEventArgs e)
        {
            MQTTClient_Stop();
        }

        void Display(string message)
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                      new Action(() =>
                      {
                          DebugMessage_UC debugMessage_UC = new DebugMessage_UC(message);
                          _lst.Items.Add(debugMessage_UC);

                          while (_lst.Items.Count > 50)
                              _lst.Items.RemoveAt(0);

                          _lst.ScrollIntoView(_lst.Items[_lst.Items.Count - 1]);
                      }));
        }

        void ListClear_Click(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                      new Action(() => { _lst.Items.Clear(); }));
        }
        #endregion

        #region MQTT
        //https://github.com/dotnet/MQTTnet/blob/master/Samples/Client/Client_Subscribe_Samples.cs

        void MQTT_ConnectAndPublishTopics()
        {
            Topics_Init();
            if (mqttClient == null)
                MQTTClient_Init();
            MQTT_PublishTopics();
        }

        void Topics_Init()
        {
            data = new Dictionary<string, string>();
            data.Add("plc/light/camera", "true");
            data.Add("plc/light/green", "true");
            data.Add("plc/light/orange", "true");
            data.Add("plc/light/red", "true");
            data.Add("plc/frequence/acq", "500");
            data.Add("plc/flips", "123456");
            data.Add("plc/reset/encoder", "000000000123456789");
            //data.Add("plc/top/encoder/camera", "28");
            //data.Add("plc/current/speed", "90");
        }

        void MQTT_PublishTopics()
        {
            int TempsBetweenPub;
            int.TryParse(_TempsBetweenPub, out TempsBetweenPub);

            foreach (var topic in data)
            {
                MQTT_Publish(topic.Key, topic.Value);
                Display("pub on \"" + topic.Key + "\" : \"" + topic.Value + "\"");
                System.Threading.Thread.Sleep(TempsBetweenPub);
            }
        }

        void MQTTClient_Init()
        {
            var mqttFactory = new MqttFactory();
            mqttClient = mqttFactory.CreateMqttClient();

            int port = -1;
            int.TryParse(_Port, out port);

            var mqttClientOptions = new MQTTnet.Client.MqttClientOptionsBuilder()
                .WithTcpServer(_IP, port)
                .Build();

            mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;

            mqttClient.ConnectingAsync += MqttClient_ConnectingAsync;
            mqttClient.ConnectedAsync += MqttClient_ConnectedAsync;
            mqttClient.DisconnectedAsync += MqttClient_DisconnectedAsync;
            mqttClient.ConnectAsync(mqttClientOptions, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
        }

        Task MqttClient_ConnectingAsync(MQTTnet.Client.MqttClientConnectingEventArgs arg)
        {
            Display("Connecting");
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        Task MqttClient_ConnectedAsync(MQTTnet.Client.MqttClientConnectedEventArgs arg)
        {
            Display("Connected");
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        Task MqttClient_DisconnectedAsync(MQTTnet.Client.MqttClientDisconnectedEventArgs arg)
        {
            Display("Disconnecting");
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        Task MqttClient_ApplicationMessageReceivedAsync(MQTTnet.Client.MqttApplicationMessageReceivedEventArgs arg)
        {
            Display("Nouveau message !");
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        void MQTTClient_Stop()
        {
            mqttClient?.DisconnectAsync(new MQTTnet.Client.MqttClientDisconnectOptions()).GetAwaiter().GetResult();//.DisconnectAsync().GetAwaiter();
        }

        public void MQTT_Publish(string topic, string payload)
        {
            MqttApplicationMessage applicationMessage = new MqttApplicationMessageBuilder()
                                                            .WithTopic(topic)
                                                            .WithPayload(payload)
                                                            .Build();

            mqttClient.PublishAsync(applicationMessage, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
        }


        //public static async Task Subscribe_Multiple_Topics()
        //{
        //    /*
        //     * This sample subscribes to several topics in a single request.
        //     */

        //    var mqttFactory = new MqttFactory();

        //    using (var mqttClient = mqttFactory.CreateMqttClient())
        //    {
        //        var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("broker.hivemq.com").Build();

        //        await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        //        // Create the subscribe options including several topics with different options.
        //        // It is also possible to all of these topics using a dedicated call of _SubscribeAsync_ per topic.
        //        var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
        //            .WithTopicFilter(
        //                f =>
        //                {
        //                    f.WithTopic("mqttnet/samples/topic/1");
        //                })
        //            .WithTopicFilter(
        //                f =>
        //                {
        //                    f.WithTopic("mqttnet/samples/topic/2").WithNoLocal();
        //                })
        //            .WithTopicFilter(
        //                f =>
        //                {
        //                    f.WithTopic("mqttnet/samples/topic/3").WithRetainHandling(MqttRetainHandling.SendAtSubscribe);
        //                })
        //            .Build();

        //        var response = await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

        //        Console.WriteLine("MQTT client subscribed to topics.");

        //        // The response contains additional data sent by the server after subscribing.
        //        response.DumpToConsole();
        //    }
        //}

        #endregion
    }
}
