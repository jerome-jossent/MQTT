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

    public bool autoReconnect_ifConnectionLost; //to test
    bool reconnection_asked;

    public enum StatusType { none, disconnected, connecting, connected, connection_fail, connection_lost }
    public StatusType _status
    {
        get { return status; }
        set
        {
            status = value;
            StatusChanged?.Invoke(value);
        }
    }
    StatusType status;
    public UnityEvent<StatusType> StatusChanged;

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
        public UnityEvent<MQTT_JJ_Message> onNewMessage;
    }

    //public class MQTTMessage_JJ
    //{
    //    public string topic;
    //    public byte[] data;

    //    internal bool? _GetDataAsBool()
    //    {
    //        if (data == null || data.Length == 0)
    //            return null;
    //        try
    //        {
    //            string result = System.Text.Encoding.UTF8.GetString(data);

    //            if (result == "1") return true;
    //            if (result == "0") return false;
    //            if (result.ToLower() == "true") return true;
    //            if (result.ToLower() == "false") return false;
    //            return null;
    //        }
    //        catch (Exception ex)
    //        {
    //            Debug.LogException(ex);
    //            return null;
    //        }
    //    }
    //    internal string _GetDataAsString()
    //    {
    //        if (data == null || data.Length == 0)
    //            return null;
    //        try
    //        {
    //            return System.Text.Encoding.UTF8.GetString(data);
    //        }
    //        catch (Exception ex)
    //        {
    //            Debug.LogException(ex);
    //            return null;
    //        }
    //    }
    //    internal int? _GetDataAsInt32() { return int.TryParse(_GetDataAsString(), out int value) ? value : null; }
    //    internal long? _GetDataAsLong() { return long.TryParse(_GetDataAsString(), out long value) ? value : null; }
    //    internal float? _GetDataAsFloat() { return float.TryParse(_GetDataAsString(), out float value) ? value : null; }
    //    internal double? _GetDataAsDouble() { return double.TryParse(_GetDataAsString(), out double value) ? value : null; }

    //    internal void DebugLogErrorWithThis()
    //    {
    //        Debug.Log("Erreur sur le topic \"" + topic + "\" avec : " + _GetDataAsString());
    //    }

    //    public override string ToString()
    //    {
    //        return topic + ":" + _GetDataAsString();
    //    }
    //}

    List<MQTT_JJ_Message> eventMessages = new List<MQTT_JJ_Message>();

    public Dictionary<string, List<Action<MQTT_JJ_Message>>> topics_subscribed = new Dictionary<string, List<Action<MQTT_JJ_Message>>>();
    public Dictionary<string, List<UnityEvent<MQTT_JJ_Message>>> topics_subscribed_unityevent = new Dictionary<string, List<UnityEvent<MQTT_JJ_Message>>>();
    public string[] topics_readonly;

    protected override void Start()
    {
        _status = StatusType.none;
        if (brokerAddress == "")
            brokerAddress = MQTT_JJ_static_Parameters._GetString(MQTT_JJ_static_Parameters.PlayerPrefNames.MQTT_IP);

        if (brokerPort == 0)
            brokerPort = int.Parse(MQTT_JJ_static_Parameters._GetString(MQTT_JJ_static_Parameters.PlayerPrefNames.MQTT_Port, "0"));

        if (_ClientID_JJ == "")
            _ClientID_JJ = MQTT_JJ_static_Parameters._GetString(MQTT_JJ_static_Parameters.PlayerPrefNames.MQTT_ID);

        base.Start();
    }

    /// <summary>
    /// WARNING QOS NON SAUVEGARDE !! en cas de re-subscribe...
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="mqtt_newMessage"></param>
    /// <param name="qosLevel"></param>
    public void _SubscribeTopic(string topic, Action<MQTT_JJ_Message> mqtt_newMessage, QOS_Level qosLevel = QOS_Level.AT_MOST_ONCE)
    {
        client.Subscribe(new string[] { topic }, new byte[] { QOS_Converter(qosLevel) });

        if (!topics_subscribed.ContainsKey(topic))
            topics_subscribed.Add(topic, new List<Action<MQTT_JJ_Message>>());

        if (!topics_subscribed[topic].Contains(mqtt_newMessage))
            topics_subscribed[topic].Add(mqtt_newMessage);

        Update_topics_readonly();
    }

    public void _SubscribeTopic(string topic, UnityEvent<MQTT_JJ_Message> onNewMessage, QOS_Level qosLevel = QOS_Level.AT_MOST_ONCE)
    {
        client.Subscribe(new string[] { topic }, new byte[] { QOS_Converter(qosLevel) });

        if (!topics_subscribed_unityevent.ContainsKey(topic))
            topics_subscribed_unityevent.Add(topic, new List<UnityEvent<MQTT_JJ_Message>>());

        if (!topics_subscribed_unityevent[topic].Contains(onNewMessage))
            topics_subscribed_unityevent[topic].Add(onNewMessage);

        Update_topics_readonly();
    }

    public void _UnsubscribeTopic(string topic)
    {
        client.Unsubscribe(new string[] { topic });
        if (topics_subscribed_unityevent.ContainsKey(topic))
            topics_subscribed_unityevent.Remove(topic);

        if (topics_subscribed.ContainsKey(topic))
            topics_subscribed.Remove(topic);

        Update_topics_readonly();
    }

    void Update_topics_readonly()
    {
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
        if (client == null || !client.IsConnected) return;
        client.Publish(topic.topic, message, QOS_Converter(topic.qos), topic.retain);
    }
    #endregion

    public override void Connect()
    {
        _status = StatusType.connecting;
        base.Connect();
    }

    public void _ResetConnection()
    {
        Disconnect();

        brokerAddress = MQTT_JJ_static_Parameters._GetString(MQTT_JJ_static_Parameters.PlayerPrefNames.MQTT_IP);
        brokerPort = int.Parse(MQTT_JJ_static_Parameters._GetString(MQTT_JJ_static_Parameters.PlayerPrefNames.MQTT_Port, "0"));
        _ClientID_JJ = MQTT_JJ_static_Parameters._GetString(MQTT_JJ_static_Parameters.PlayerPrefNames.MQTT_ID);

        Connect();
    }

    protected override void OnConnecting()
    {
        _status = StatusType.connecting;
        //DebugToScrollView._instance?._Print($"Connecting to broker on {brokerAddress}:{brokerPort}...", true);
    }

    protected override void OnConnected()
    {
        _status = StatusType.connected;
        //DebugToScrollView._instance?._Print($"Connected to broker on {brokerAddress}:{brokerPort}", false);
        base.OnConnected(); // SubscribeTopics !!

        if (autoReconnect_ifConnectionLost && reconnection_asked)
        {
            reconnection_asked = false;

            //unsubscribe all
            UnsubscribeTopics();

            //subscribe all
            foreach (var item in topics_subscribed)
            {
                string topic = item.Key;
                foreach (Action<MQTT_JJ_Message> item2 in item.Value)
                    _SubscribeTopic(topic, item2);
            }

            foreach (var item in topics_subscribed_unityevent)
            {
                string topic = item.Key;
                foreach (UnityEvent<MQTT_JJ_Message> item2 in item.Value)
                    _SubscribeTopic(topic, item2);
            }
        }
    }

    protected override void UnsubscribeTopics()
    {
        foreach (string topic in topics_subscribed.Keys)
            client.Unsubscribe(new string[] { topic });
        //topics_subscribed.Clear();

        foreach (string topic in topics_subscribed_unityevent.Keys)
            client.Unsubscribe(new string[] { topic });
        //topics_subscribed_unityevent.Clear();
    }

    protected override void OnConnectionFailed(string errorMessage)
    {
        _status = StatusType.connection_fail;
        //Debug.Log("OnConnectionFailed " + errorMessage);
        //DebugToScrollView._instance?._Print("Connection failed : " + errorMessage, true);
        base.OnConnectionFailed(errorMessage);
    }

    protected override void OnDisconnected()
    {
        _status = StatusType.disconnected;
        //Debug.Log("OnDisconnected");
        //DebugToScrollView._instance?._Print("Disconnected", true);
    }

    protected override void OnConnectionLost()
    {
        _status = StatusType.connection_lost;
        //Debug.Log("OnConnectionLost");
        //DebugToScrollView._instance?._Print("Connection lost", true);
        base.OnConnectionLost();

        if (autoReconnect_ifConnectionLost)
        {
            reconnection_asked = true;
            base.Connect();
        }
    }

    protected override void DecodeMessage(string topic, byte[] message)
    {
        eventMessages.Add(new MQTT_JJ_Message() { topic = topic, data = message });
    }

    protected override void Update()
    {
        base.Update();
        if (eventMessages.Count > 0)
        {
            foreach (MQTT_JJ_Message msg in eventMessages)
                ProcessMessage(msg);
            eventMessages.Clear();
        }
    }

    void ProcessMessage(MQTT_JJ_Message message)
    {
        if (topics_subscribed.ContainsKey(message.topic))
            foreach (Action<MQTT_JJ_Message> item in topics_subscribed[message.topic])
                item.Invoke(message);

        if (topics_subscribed_unityevent.ContainsKey(message.topic))
            foreach (UnityEvent<MQTT_JJ_Message> item in topics_subscribed_unityevent[message.topic])
                item.Invoke(message);
    }

    void OnDestroy()
    {
        Disconnect();
    }
}