using System.Collections.Generic;
using UnityEngine;
using M2MqttUnity;
using uPLibrary.Networking.M2Mqtt.Messages;
using System;
using UnityEngine.Events;
using System.Linq;
using System.Text;

public class MQTT_JJ : M2MqttUnityClient
{

    #region QOS : Quality Of Service
    public enum QOS_Level { AT_MOST_ONCE = 0, AT_LEAST_ONCE = 1, EXACTLY_ONCE = 2 }

    byte QOS_Converter(QOS_Level level)
    {
        switch (level)
        {
            case QOS_Level.AT_LEAST_ONCE: return MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE;
            case QOS_Level.EXACTLY_ONCE: return MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE;
            case QOS_Level.AT_MOST_ONCE:
            default:
                return MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE;
        }
    }
    #endregion
    public enum DataType { _bool, _int, _long, _float, _double, _string }

    [System.Serializable]
    public class Topic
    {
        public string topic;
        public bool retain = false;
        public QOS_Level qos = QOS_Level.AT_MOST_ONCE;
        public DataType dataType;
    }

    [System.Serializable]
    public class Topic_Sub : Topic
    {
        public UnityEvent<MQTTMessage_JJ> onNewMessage;
    }

    public class MQTTMessage_JJ
    {
        public string topic;
        public byte[] data;

        internal bool? _GetDataAsBool()
        {
            if (data == null || data.Length == 0)
                return null;
            try
            {
                string result = System.Text.Encoding.UTF8.GetString(data);

                if (result == "1") return true;
                if (result == "0") return false;
                if (result.ToLower() == "true") return true;
                if (result.ToLower() == "false") return false;
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }
        internal string _GetDataAsString()
        {
            if (data == null || data.Length == 0)
                return null;
            try
            {
                return System.Text.Encoding.UTF8.GetString(data);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }
        internal int? _GetDataAsInt32() { return int.TryParse(_GetDataAsString(), out int value) ? value : null; }
        internal long? _GetDataAsLong() { return long.TryParse(_GetDataAsString(), out long value) ? value : null; }
        internal float? _GetDataAsFloat() { return float.TryParse(_GetDataAsString(), out float value) ? value : null; }
        internal double? _GetDataAsDouble() { return double.TryParse(_GetDataAsString(), out double value) ? value : null; }

        internal void DebugLogErrorWithThis()
        {
            Debug.Log("Erreur sur le topic \"" + topic + "\" avec : " + _GetDataAsString());
        }
    }

    List<MQTTMessage_JJ> eventMessages = new List<MQTTMessage_JJ>();

    public Dictionary<string, List<Action<MQTTMessage_JJ>>> topics_subscribed = new Dictionary<string, List<Action<MQTTMessage_JJ>>>();
    public Dictionary<string, List<UnityEvent<MQTTMessage_JJ>>> topics_subscribed_unityevent = new Dictionary<string, List<UnityEvent<MQTTMessage_JJ>>>();
    public string[] topics_readonly;

    protected override void Start()
    {
        //brokerAddress = Parameters._GetString(Parameters.PlayerPrefNames.MQTT_IP);
        //brokerPort = int.Parse(Parameters._GetString(Parameters.PlayerPrefNames.MQTT_Port));
        //_ClientID_JJ = Parameters._GetString(Parameters.PlayerPrefNames.MQTT_ID);
        base.Start();
    }

    public void _SubscribeTopic(string topic, Action<MQTTMessage_JJ> mqtt_newMessage, QOS_Level qosLevel = QOS_Level.AT_MOST_ONCE)
    {
        client.Subscribe(new string[] { topic }, new byte[] { QOS_Converter(qosLevel) });

        if (!topics_subscribed.ContainsKey(topic))
            topics_subscribed.Add(topic, new List<Action<MQTTMessage_JJ>>());
        topics_subscribed[topic].Add(mqtt_newMessage);

        topics_readonly = topics_subscribed.Keys.Concat(topics_subscribed_unityevent.Keys).ToArray();
    }

    public void _SubscribeTopic(string topic, UnityEvent<MQTTMessage_JJ> onNewMessage, QOS_Level qosLevel = QOS_Level.AT_MOST_ONCE)
    {
        client.Subscribe(new string[] { topic }, new byte[] { QOS_Converter(qosLevel) });

        if (!topics_subscribed_unityevent.ContainsKey(topic))
            topics_subscribed_unityevent.Add(topic, new List<UnityEvent<MQTTMessage_JJ>>());
        topics_subscribed_unityevent[topic].Add(onNewMessage);

        topics_readonly = topics_subscribed.Keys.Concat(topics_subscribed_unityevent.Keys).ToArray();
    }

    #region PUBLISH
    public void Publish(Topic topic, bool val)
    {
        string message_txt = val ? "true" : "false";
        Publish(topic, message_txt);
    }

    public void Publish(Topic topic, int val)
    {
        string message_txt = val.ToString();
        Publish(topic, message_txt);
    }
    public void Publish(Topic topic, long val)
    {
        string message_txt = val.ToString();
        Publish(topic, message_txt);
    }
    public void Publish(Topic topic, float val)
    {
        string message_txt = val.ToString();
        Publish(topic, message_txt);
    }
    public void Publish(Topic topic, double val)
    {
        string message_txt = val.ToString();
        Publish(topic, message_txt);
    }

    public void Publish(Topic topic, string message_txt)
    {
        byte[] message = Encoding.UTF8.GetBytes(message_txt);
        Publish(topic, message);
    }

    public void Publish(Topic topic, byte[] message)
    {
        if (!client.IsConnected) return;
        client.Publish(topic.topic, message, QOS_Converter(topic.qos), topic.retain);
    }
    #endregion

    protected override void OnConnecting()
    {
    }

    protected override void OnConnected()
    {
        base.OnConnected(); // SubscribeTopics !!
    }


    protected override void UnsubscribeTopics()
    {
        foreach (string topic in topics_subscribed.Keys)
            client.Unsubscribe(new string[] { topic });
    }

    protected override void OnConnectionFailed(string errorMessage)
    {
        //Debug.Log("OnConnectionFailed " + errorMessage);
    }

    protected override void OnDisconnected()
    {
        //Debug.Log("OnDisconnected");
    }

    protected override void OnConnectionLost()
    {
        //Debug.Log("OnConnectionLost");
    }

    protected override void DecodeMessage(string topic, byte[] message)
    {
        eventMessages.Add(new MQTTMessage_JJ() { topic = topic, data = message });
    }

    protected override void Update()
    {
        base.Update();
        if (eventMessages.Count > 0)
        {
            foreach (MQTTMessage_JJ msg in eventMessages)
                ProcessMessage(msg);
            eventMessages.Clear();
        }
    }

    void ProcessMessage(MQTTMessage_JJ message)
    {
        if (topics_subscribed.ContainsKey(message.topic))
            foreach (Action<MQTTMessage_JJ> item in topics_subscribed[message.topic])
                item.Invoke(message);

        if (topics_subscribed_unityevent.ContainsKey(message.topic))
            foreach (UnityEvent<MQTTMessage_JJ> item in topics_subscribed_unityevent[message.topic])
                item.Invoke(message);
    }

    void OnDestroy()
    {
        Disconnect();
    }

}