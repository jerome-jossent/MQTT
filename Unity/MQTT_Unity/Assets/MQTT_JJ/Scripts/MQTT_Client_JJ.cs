using M2MqttUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using static MqttMessage_jj;

public class MQTT_Client_JJ : M2MqttUnityClient
{
    public IP_Port_UI IP_Port_UI;

    public MQTT_Client_JJ _instance;
    public MqttClient _client;

    public UnityEvent onConnected;
    public UnityEvent onConnecting;
    public UnityEvent onConnectionFailed;
    public UnityEvent onConnectionLost;
    public UnityEvent onDisconnected;
    public string errorMessage;

    Dictionary<string, List<MQTT_JJ>> topic_subscribers = new Dictionary<string, List<MQTT_JJ>>();


    public override void Connect()
    {
        _instance.brokerAddress = IP_Port_UI._ip;
        _instance.brokerPort= IP_Port_UI._port;

        Debug.Log(_instance.brokerAddress);
        Debug.Log(_instance.brokerPort);

        base.Connect();
    }

    public override void Disconnect()
    {
        base.Disconnect();
    }

    protected override void Awake()
    {
        if (_instance == null)
            _instance = this;
        base.Awake();
        _client = client;
    }

    protected override void OnConnecting()
    {
        base.OnConnecting();
        onConnecting.Invoke();
    }

    protected override void OnConnected()
    {
        base.OnConnected();
        onConnected.Invoke();
    }

    protected override void OnConnectionFailed(string errorMessage)
    {
        base.OnConnectionFailed(errorMessage);
        this.errorMessage = errorMessage;
        onConnectionFailed.Invoke();
    }

    protected override void OnDisconnected()
    {
        base.OnDisconnected();
        onDisconnected.Invoke();
    }

    protected override void OnConnectionLost()
    {
        base.OnConnectionLost();
        onConnectionLost.Invoke();
    }

    //protected override void SubscribeTopics()
    //{
    //    //client.Subscribe(new string[] { "M2MQTT_Unity/test" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
    //}

    //protected override void UnsubscribeTopics()
    //{
    //    //client.Unsubscribe(new string[] { "M2MQTT_Unity/test" });
    //}


    public void _Subscribe(MQTT_JJ mQTT_Subscribe_ByteArray_Image,
                           string topic_to_subscribe)
    {
        client.Subscribe(new string[] { topic_to_subscribe },
                                 new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE }
                                 );
        if (!topic_subscribers.ContainsKey(topic_to_subscribe))
            topic_subscribers.Add(topic_to_subscribe, new List<MQTT_JJ>());

        topic_subscribers[topic_to_subscribe].Add(mQTT_Subscribe_ByteArray_Image);

        Debug.Log("subscribe on " + topic_to_subscribe);
    }

    protected override void DecodeMessage(string topic, byte[] message)
    {
        foreach (MQTT_JJ item in topic_subscribers[topic])
        {
            item._DecodeMessage(message);
        }
    }

}
