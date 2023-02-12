using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MQTT_JJ : MonoBehaviour
{
    public MQTT_Client_JJ clientjj;
    public string topic_to_subscribe;

    protected void Start()
    {
        clientjj.onConnected.AddListener(OnConnected);
        clientjj.onDisconnected.AddListener(OnDisconnected);
    }

    protected void Update()
    {       
    }

    void OnConnected()
    {
        SubscribeTopic();
    }

    void OnDisconnected()
    {
        // ?
    }

    internal void SubscribeTopic()
    {
        clientjj._Subscribe(this, topic_to_subscribe);
    }

    internal abstract void _DecodeMessage(byte[] message);

}
