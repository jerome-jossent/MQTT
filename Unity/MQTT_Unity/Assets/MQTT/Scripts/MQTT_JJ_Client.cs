using System.Collections.Generic;
using UnityEngine;
using M2MqttUnity;
using uPLibrary.Networking.M2Mqtt.Messages;
using System;
using UnityEngine.Events;
using System.Linq;
using System.Text;
using static MQTT_JJ_Message;
using System.Collections;

public class MQTT_JJ_Client : M2MqttUnityClient
{
    static public MQTT_JJ_Client _instance;
    



    public class Topic_Payload
    {
        public string topic;
        public byte[] payload;
    }
    public class UnityEventNewData_QOS
    {
        public UnityEvent<byte[]> newData;
        public QOS_Level qos;
    }
    public enum StatusType { none, disconnected, connecting, connected, connection_fail, connection_lost }

    #region PARAMETERS
    public bool autoReconnect_ifConnectionLost;
    bool reconnection_asked;

    public StatusType _status
    {
        get { return status; }
        set
        {
            status = value;
            _statusChanged?.Invoke(value);
            statusType = _status.ToString();
        }
    }
    StatusType status;
    [SerializeField, ReadOnly] string statusType;
    public UnityEvent<StatusType> _statusChanged;

    public bool isConnected { get { return _status == StatusType.connected; } }

    List<Topic_Payload> eventMessages = new List<Topic_Payload>();
    object eventMessagesLock = new object();

    public Dictionary<string, List<UnityEventNewData_QOS>> topics_subscribed_unityevent = new Dictionary<string, List<UnityEventNewData_QOS>>();

    [SerializeField, ReadOnly] string[] topics_readonly;
    #endregion

    protected override void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("Warning : more than 1 MQTT client");

        base.Awake();
        
    }

    protected override void Start()
    {
        _status = StatusType.none;
        _LoadBrokerConfigurationFromSavedParameters();
        base.Start();

        StartIncomingMessageManager();
    }

    void OnDestroy()
    {
        Disconnect();
    }

    public override void Connect()
    {
        _status = StatusType.connecting;
        base.Connect();
    }

    public void _ResetConnection()
    {
        Disconnect();
        _LoadBrokerConfigurationFromSavedParameters();
        Connect();
    }

    public void _LoadBrokerConfigurationFromSavedParameters()
    {
        if (brokerAddress == "")
            brokerAddress = MQTT_JJ_static_Parameters._GetString(MQTT_JJ_static_Parameters.PlayerPrefNames.MQTT_IP);

        if (brokerPort == 0)
            brokerPort = int.Parse(MQTT_JJ_static_Parameters._GetString(MQTT_JJ_static_Parameters.PlayerPrefNames.MQTT_Port, "0"));

        if (_ClientID_JJ == "")
            _ClientID_JJ = MQTT_JJ_static_Parameters._GetString(MQTT_JJ_static_Parameters.PlayerPrefNames.MQTT_ID);

        Debug.Log(brokerAddress);
    
    }

    #region MQTT Event
    protected override void OnConnecting()
    {
        _status = StatusType.connecting;
        base.OnConnecting();
    }

    protected override void OnConnected()
    {
        _status = StatusType.connected;
        //base.OnConnected(); // SubscribeTopics, mais pas gérer de la même manière, donc ne sert pas  !!
        Debug.LogFormat("Connected to {0}:{1}...\n", brokerAddress, brokerPort.ToString()); // pour être sûr

        if (autoReconnect_ifConnectionLost && reconnection_asked)
        {
            reconnection_asked = false;
            UnsubscribeTopics();
        }

        SubscribeTopics();
    }

    protected override void OnConnectionFailed(string errorMessage)
    {
        _status = StatusType.connection_fail;
        base.OnConnectionFailed(errorMessage);
    }

    protected override void OnDisconnected()
    {
        _status = StatusType.disconnected;
        base.OnDisconnected();
    }

    protected override void OnConnectionLost()
    {
        _status = StatusType.connection_lost;
        base.OnConnectionLost();

        if (autoReconnect_ifConnectionLost)
        {
            reconnection_asked = true;
            base.Connect();
        }
    }
    #endregion

    #region SUBSCRIBE/UNSUBSCRIBE
    protected override void SubscribeTopics()
    {
        foreach (var item in topics_subscribed_unityevent)
        {
            string topic = item.Key;
            foreach (UnityEventNewData_QOS item2 in item.Value)
                _SubscribeTopic(topic, item2.newData, item2.qos);
        }
    }

    protected override void UnsubscribeTopics()
    {
        foreach (string topic in topics_subscribed_unityevent.Keys)
            client.Unsubscribe(new string[] { topic });
    }

    //s'abonne maintenant ou s'abonnera à la connexion où cette méthode est exécutée automatiquement
    public void _SubscribeTopic(string topic, UnityEvent<byte[]> onNewMessage, QOS_Level qos)
    {
        if (isConnected)
        {
            if (client == null)
                Debug.Log("MQTT Client not ready !");
            else
                //Subscribed
                client.Subscribe(new string[] { topic }, new byte[] { QOS_Converter(qos) });
        }
        else
        {
            //Subscribe for later
            if (!topics_subscribed_unityevent.ContainsKey(topic))
                topics_subscribed_unityevent.Add(topic, new List<UnityEventNewData_QOS>());

            UnityEventNewData_QOS uend = new UnityEventNewData_QOS() { newData = onNewMessage, qos = qos };

            if (!topics_subscribed_unityevent[topic].Contains(uend))
                topics_subscribed_unityevent[topic].Add(uend);
        }

        Update_topics_readonly();
    }

    public void _UnsubscribeTopic(string topic)
    {
        client.Unsubscribe(new string[] { topic });
        if (topics_subscribed_unityevent.ContainsKey(topic))
            topics_subscribed_unityevent.Remove(topic);

        Update_topics_readonly();
    }

    void Update_topics_readonly()
    {
        topics_readonly = topics_subscribed_unityevent.Keys.ToArray();
    }
    #endregion

    #region Incoming Message Manager
    protected override void DecodeMessage(string topic, byte[] message)
    {
        //empile incoming messages
        lock (eventMessagesLock)
        {
            eventMessages.Add(new Topic_Payload() { topic = topic, payload = message });
        }
    }

    void StartIncomingMessageManager()
    {
        StartCoroutine(SubscribedMessageHandler());
    }

    IEnumerator SubscribedMessageHandler()
    {
        //depile & dispatch incoming messages
        while (isActiveAndEnabled)
        {
            if (eventMessages.Count > 0)
            {
                lock (eventMessagesLock)
                {
                    foreach (Topic_Payload msg in eventMessages)
                        DispatchMessage(msg);
                    eventMessages.Clear();
                }
            }
            yield return new WaitForSeconds(0.001f);
        }
    }

    void DispatchMessage(Topic_Payload message)
    {
        if (topics_subscribed_unityevent.ContainsKey(message.topic))
            foreach (UnityEventNewData_QOS item in topics_subscribed_unityevent[message.topic])
                item.newData.Invoke(message.payload);
    }
    #endregion

    #region PUBLISH
    public void Publish(MQTT_JJ_Message m, bool val)
    {
        string message_txt = val ? "true" : "false";
        Publish(m, message_txt);
    }
    public void Publish(MQTT_JJ_Message m, int val)
    {
        string message_txt = val.ToString();
        Publish(m, message_txt);
    }
    public void Publish(MQTT_JJ_Message m, long val)
    {
        string message_txt = val.ToString();
        Publish(m, message_txt);
    }
    public void Publish(MQTT_JJ_Message m, float val)
    {
        string message_txt = val.ToString();
        Publish(m, message_txt);
    }
    public void Publish(MQTT_JJ_Message m, double val)
    {
        string message_txt = val.ToString();
        Publish(m, message_txt);
    }
    public void Publish(MQTT_JJ_Message m, string message_txt)
    {
        byte[] message = m.encoding.GetBytes(message_txt);
        Publish(m, message);
    }
    public void Publish(MQTT_JJ_Message m, byte[] message)
    {
        if (client == null || !client.IsConnected) return;
        client.Publish(m.topic, message, QOS_Converter(m.qos), m.retain);
    }
    #endregion
}