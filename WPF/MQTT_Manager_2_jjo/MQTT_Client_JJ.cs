using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using MQTTnet;
using MQTTnet.Client;

namespace MQTT_Manager_2_jjo
{
    public class MQTT_Client_JJ : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        IMqttClient mqttClient;

        public event EventHandler<MQTT_Status_JJ> _onStatusChange;
        public event EventHandler _onConnecting;
        public event EventHandler _onConnected;
        public event EventHandler _onDisconnected;
        public event Func<MqttApplicationMessageReceivedEventArgs, Task> _onIncomingMessageEvent;

        Dictionary<string, Action<byte[]?>> topics_subscribed = new Dictionary<string, Action<byte[]?>>();

        string _IP;
        int _port;
        bool _manageIncomingMessageManually;
        string _login;
        string _mdp;
        string _clientID;
        MQTT_Message_JJ _lastwill;

        bool isConnecting;
        public enum MQTT_Status_JJ { nul, disconnected, connecting, connected }
        public MQTT_Status_JJ _MQTT_Status_JJ
        {
            get
            {
                if (mqttClient == null) return MQTT_Status_JJ.nul;
                if (mqttClient.IsConnected) return MQTT_Status_JJ.connected;
                if (isConnecting) return MQTT_Status_JJ.connecting;
                return MQTT_Status_JJ.disconnected;
            }
        }

        public void _onMQTTStatusChanged() 
        { 
            OnPropertyChanged("_MQTT_Status_JJ"); 
            _onStatusChange.Invoke(this, _MQTT_Status_JJ);
        }

        #region CONNECT

        /// <summary>
        /// 
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="port"></param>
        /// <param name="manageIncomingMessageManually">False : use Subscribe Methods, True : subscribe _incomingMessageEvent before connect</param>
        public bool _Connect(string IP, int port, bool manageIncomingMessageManually = false)
        {
            _IP = IP;
            _port = port;
            _manageIncomingMessageManually = manageIncomingMessageManually;
            return _Connect();
        }

        public bool _Connect(string IP, int port, string clientID, bool manageIncomingMessageManually = false)
        {
            _IP = IP;
            _port = port;
            _manageIncomingMessageManually = manageIncomingMessageManually;
            _clientID = clientID;
            return _Connect();
        }

        public bool _Connect(string IP, int port, MQTT_Message_JJ lastwill, bool manageIncomingMessageManually = false)
        {
            _IP = IP;
            _port = port;
            _manageIncomingMessageManually = manageIncomingMessageManually;
            _lastwill = lastwill;
            return _Connect();
        }
        public bool _Connect(string IP, int port, string clientID, MQTT_Message_JJ lastwill, bool manageIncomingMessageManually = false)
        {
            _IP = IP;
            _port = port;
            _manageIncomingMessageManually = manageIncomingMessageManually;
            _clientID = clientID;
            _lastwill = lastwill;
            return _Connect();
        }

        public bool _Connect(string IP, int port, string login, string mdp, string clientID, bool manageIncomingMessageManually = false)
        {
            _IP = IP;
            _port = port;
            _manageIncomingMessageManually = manageIncomingMessageManually;
            _clientID = clientID;
            _login = login;
            _mdp = mdp;
            return _Connect();
        }

        public bool _Connect(string IP, int port, string login, string mdp, string clientID, MQTT_Message_JJ lastwill, bool manageIncomingMessageManually = false)
        {
            _IP = IP;
            _port = port;
            _manageIncomingMessageManually = manageIncomingMessageManually;
            _clientID = clientID;
            _lastwill = lastwill;
            _login = login;
            _mdp = mdp;
            return _Connect();
        }

        bool _Connect()
        {
            if (_IP == "")
                return false;

            _Disconnect();

            mqttClient = new MqttFactory().CreateMqttClient();

            var optionsBuilder = new MqttClientOptionsBuilder()
                .WithTcpServer(_IP, _port);

            if (_clientID != "")
                optionsBuilder = optionsBuilder.WithClientId(_clientID);

            if (_login != "")
                optionsBuilder = optionsBuilder.WithCredentials(_login, _mdp);

            if (_lastwill != null)
                optionsBuilder = optionsBuilder
                    .WithWillTopic(_lastwill.topic)
                    .WithWillPayload(_lastwill.payload)
                    .WithWillRetain(_lastwill.retain);

            var mqttClientOptions = optionsBuilder.Build();

            if (_manageIncomingMessageManually)
                mqttClient.ApplicationMessageReceivedAsync += _onIncomingMessageEvent;
            else
                mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;

            mqttClient.ConnectingAsync += MqttClient_ConnectingAsync;
            mqttClient.ConnectedAsync += MqttClient_ConnectedAsync;
            mqttClient.DisconnectedAsync += MqttClient_DisconnectedAsync;

            bool success;
            try
            {
                mqttClient.ConnectAsync(mqttClientOptions, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
                success = true;
            }
            catch (Exception ex)
            {
                success = false;
            }
            return success;
        }
        #endregion

        public void _Disconnect()
        {
            mqttClient?.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection).Build());
            _onMQTTStatusChanged();
        }

        Task MqttClient_ConnectingAsync(MqttClientConnectingEventArgs arg)
        {
            isConnecting = true;
            _onMQTTStatusChanged();
            _onConnecting?.Invoke(this, arg);
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            isConnecting = false;
            _onMQTTStatusChanged();
            _onConnected?.Invoke(this, arg);
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        Task MqttClient_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            mqttClient.ConnectingAsync -= MqttClient_ConnectingAsync;
            mqttClient.ConnectedAsync -= MqttClient_ConnectedAsync;
            mqttClient.DisconnectedAsync -= MqttClient_DisconnectedAsync;

            mqttClient.Dispose();
            _onMQTTStatusChanged();
            _onDisconnected?.Invoke(this, arg);

            return MQTTnet.Internal.CompletedTask.Instance;
        }

        Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            if (topics_subscribed.ContainsKey(arg.ApplicationMessage.Topic))
                topics_subscribed[arg.ApplicationMessage.Topic].DynamicInvoke(arg.ApplicationMessage.PayloadSegment.Array);
            else
            {
                //message perdu !
                arg = arg;
            }

            return MQTTnet.Internal.CompletedTask.Instance;
        }


        public void _Subscribe(string topic, Action<byte[]?> action)
        {
            if (_MQTT_Status_JJ != MQTT_Status_JJ.connected)
                throw new Exception("Client is " + _MQTT_Status_JJ.ToString());

            var mqtt_Subscriber = new MqttClientSubscribeOptions();

            var filter = new MQTTnet.Packets.MqttTopicFilter() { Topic = topic };
            mqtt_Subscriber.TopicFilters.Add(filter);

            if (topics_subscribed.ContainsKey(topic))
                topics_subscribed[topic] = action;
            else
                topics_subscribed.Add(topic, action);

            mqttClient.SubscribeAsync(mqtt_Subscriber, System.Threading.CancellationToken.None);
        }

        public void _Publish(MQTT_Message_JJ message)
        {
            _Publish(message.topic, message.payload, message.retain);
        }

        public void _Publish(string topic, string text, bool retain = false)
        {
            byte[] payload = Encoding.UTF8.GetBytes(text);
            _Publish(topic, payload, retain);
        }

        public void _Publish(string topic, byte[] payload, bool retain = false)
        {
            if (_MQTT_Status_JJ != MQTT_Status_JJ.connected)
                return;

            MqttApplicationMessage applicationMessage = new MqttApplicationMessageBuilder()
                                                            .WithTopic(topic)
                                                            .WithPayload(payload)
                                                            .WithRetainFlag(retain)
                                                            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                                                            .Build();

            mqttClient.PublishAsync(applicationMessage, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
        }


        public void _Unubscribe(string topic)
        {
            if (string.IsNullOrEmpty(topic))
                return;

            if (topics_subscribed.ContainsKey(topic))
                topics_subscribed.Remove(topic);

            var mqtt_Unsubscriber = new MqttClientUnsubscribeOptions();
            mqtt_Unsubscriber.TopicFilters.Add(topic);

            mqttClient.UnsubscribeAsync(mqtt_Unsubscriber, System.Threading.CancellationToken.None);
        }

        public void _Unubscribe(string[] topics)
        {
            var mqtt_Unsubscriber = new MqttClientUnsubscribeOptions();
            foreach (var topic in topics)
            {
                if (string.IsNullOrEmpty(topic))
                    continue;

                if (topics_subscribed.ContainsKey(topic))
                    topics_subscribed.Remove(topic);

                mqtt_Unsubscriber.TopicFilters.Add(topic);

            }
            mqttClient.UnsubscribeAsync(mqtt_Unsubscriber, System.Threading.CancellationToken.None);
        }

        public void _UnubscribeAll()
        {
            _Unubscribe(topics_subscribed.Keys.ToArray());
            topics_subscribed.Clear();
        }

    }
}
