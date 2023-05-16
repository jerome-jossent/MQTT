using MQTTnet;
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

namespace MQTT_Manager_jjo
{

    public partial class MQTT_Manager_UC : UserControl
    {
        public MQTTnet.Client.IMqttClient mqttClient;

        public event EventHandler connected;
        public event EventHandler diconnected;

        public Dictionary<string, Action<byte[]?>> topics_subscribed;

        string _IP;
        int _port;

        public bool isConnected { get => mqttClient != null && mqttClient.IsConnected; }

        public MQTT_Manager_UC()
        {
            InitializeComponent();
        }

        #region IHM

        void SetStatusConnection(Color c, string message = "")
        {
            Dispatcher.BeginInvoke(() =>
            {
                _ell_connection_status.ToolTip = message;
                _ell_connection_status.Fill = new SolidColorBrush(c);
            }
            );
        }

        void MQTT_Manager_UC_Loaded(object sender, RoutedEventArgs e)
        {
            Fill_cbx_ips();
        }

        void MQTT_Manager_UC_Unloaded(object sender, RoutedEventArgs e)
        {
            MQTTClient_Stop();
        }

        void Connect_Click(object sender, MouseButtonEventArgs e)
        {
            MQTTClient_Connect();
        }
        public void MQTTClient_Connect()
        {
            _IP = _cbx_ips.Text;
            _port = int.Parse(_tbx_port.Text);
            if (MQTTClient_Init(_IP, _port))
            {
                Save_ips();
                btn_disconnect.Visibility = Visibility.Visible;
            }
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

        #region MQTT
        //https://github.com/dotnet/MQTTnet/blob/master/Samples/Client/Client_Subscribe_Samples.cs

        bool MQTTClient_Init(string IP, int port)
        {
            var mqttFactory = new MqttFactory();
            mqttClient = mqttFactory.CreateMqttClient();

            if (IP == "")
            {
                SetStatusConnection(Colors.Blue, "Enter a valid IP");
                return false;
            }

            var mqttClientOptions = new MQTTnet.Client.MqttClientOptionsBuilder()
                .WithTcpServer(IP, port)
                .Build();

            mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;

            mqttClient.ConnectingAsync += MqttClient_ConnectingAsync;
            mqttClient.ConnectedAsync += MqttClient_ConnectedAsync;
            mqttClient.DisconnectedAsync += MqttClient_DisconnectedAsync;

            try
            {
                mqttClient.ConnectAsync(mqttClientOptions, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                SetStatusConnection(Colors.Red, ex.Message);
                return false;
            }
            return true;
        }

        void MQTTClient_Disconnect()
        {
            if (mqttClient == null) return;

            try
            {
                mqttClient.DisconnectAsync(new MQTTnet.Client.MqttClientDisconnectOptionsBuilder().WithReason(MQTTnet.Client.MqttClientDisconnectOptionsReason.NormalDisconnection).Build()).GetAwaiter().GetResult();
                mqttClient.ConnectingAsync -= MqttClient_ConnectingAsync;
                mqttClient.ConnectedAsync -= MqttClient_ConnectedAsync;
                mqttClient.DisconnectedAsync -= MqttClient_DisconnectedAsync;

                mqttClient.Dispose();
            }
            catch (Exception ex)
            {

            }
        }

        Task MqttClient_ConnectingAsync(MQTTnet.Client.MqttClientConnectingEventArgs arg)
        {
            SetStatusConnection(Colors.Orange);
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        Task MqttClient_ConnectedAsync(MQTTnet.Client.MqttClientConnectedEventArgs arg)
        {
            SetStatusConnection(Colors.Green);
            MQTTClient_Subscribes();
            connected?.Invoke(this, arg);
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        Task MqttClient_DisconnectedAsync(MQTTnet.Client.MqttClientDisconnectedEventArgs arg)
        {
            SetStatusConnection(Colors.Red);
            diconnected?.Invoke(this, arg);
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        /// <summary>
        /// Will subscribes to all topics in "topics_subscribed" dictionnary
        /// </summary>
        public void MQTTClient_Subscribes()
        {
            if (topics_subscribed == null)
                return;

            var mqtt_Subscriber = new MQTTnet.Client.MqttClientSubscribeOptions();
            foreach (var item in topics_subscribed)
            {
                var filter = new MQTTnet.Packets.MqttTopicFilter() { Topic = item.Key };
                mqtt_Subscriber.TopicFilters.Add(filter);
            }

            mqttClient.SubscribeAsync(mqtt_Subscriber, System.Threading.CancellationToken.None);
        }

        public bool MQTTClient_Subscribes(string topic, Action<byte[]?> action)
        {
            if (topics_subscribed.ContainsKey(topic))
                return false;

            topics_subscribed.Add(topic, action);

            var mqtt_Subscriber = new MQTTnet.Client.MqttClientSubscribeOptions();
            var filter = new MQTTnet.Packets.MqttTopicFilter() { Topic = topic };
            mqtt_Subscriber.TopicFilters.Add(filter);

            mqttClient.SubscribeAsync(mqtt_Subscriber, System.Threading.CancellationToken.None);
            return true;
        }

        public void MQTTClient_Unubscribes(string topic)
        {
            if (topics_subscribed != null && topics_subscribed.ContainsKey(topic))
                topics_subscribed.Remove(topic);

            var mqtt_Unsubscriber = new MQTTnet.Client.MqttClientUnsubscribeOptions();
            mqtt_Unsubscriber.TopicFilters.Add(topic);

            mqttClient.UnsubscribeAsync(mqtt_Unsubscriber, System.Threading.CancellationToken.None);
        }

        Task MqttClient_ApplicationMessageReceivedAsync(MQTTnet.Client.MqttApplicationMessageReceivedEventArgs arg)
        {
            string topic = arg.ApplicationMessage.Topic;
            if (topics_subscribed.ContainsKey(topic))
            {
                byte[]? data = arg.ApplicationMessage.PayloadSegment.Array;
                topics_subscribed[topic].DynamicInvoke(data);
            }
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        public void MQTT_Publish(string topic, byte[] payload, bool retain = false)
        {
            if (mqttClient == null || !mqttClient.IsConnected)
                return;

            MqttApplicationMessage applicationMessage = new MqttApplicationMessageBuilder()
                                                            .WithTopic(topic)
                                                            .WithPayload(payload)
                                                            .WithRetainFlag(retain)
                                                            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                                                            .Build();

            mqttClient.PublishAsync(applicationMessage, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
        }

        public void MQTTClient_Stop()
        {
            System.Threading.Thread.Sleep(50);
            MQTTClient_Disconnect();
        }
        #endregion

    }
}