using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MQTT_JJ;

public class MQTT_JJ_Switch_StringBool : MonoBehaviour
{
    public MQTT_JJ mqtt;

    //public Topic topic = new Topic() { dataType = DataType._bool };

    public bool val;

    public void Start()
    {
        if (!mqtt.gameObject.activeSelf) enabled = false;
        mqtt.ConnectionSucceeded += Mqtt_ConnectionSucceeded;
    }

    void Mqtt_ConnectionSucceeded()
    {
        //mqtt._SubscribeTopic(topic.topic, OnNewMessage);
        mqtt.ConnectionSucceeded -= Mqtt_ConnectionSucceeded;
    }
    public void OnNewMessage(MQTT_JJ_Message message)
    {
        //var payload = message.GetData();
        //if (payload != null)
        //    val = (bool)payload;
    }

    public void _Switch()
    {
        //mqtt.Publish(topic, !val);
    }
}
