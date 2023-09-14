using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MQTT_JJ;

public class MQTT_JJ_SwitchBoolean : MonoBehaviour
{
    public MQTT_JJ mqtt;

    public Topic topic = new Topic() { dataType = DataType._bool };

    bool val;

    public void Start()
    {
        if (!mqtt.gameObject.activeSelf) enabled = false;
        mqtt.ConnectionSucceeded += Mqtt_ConnectionSucceeded;
    }

    void Mqtt_ConnectionSucceeded()
    {
        mqtt._SubscribeTopic(topic.topic, OnNewMessage);
        mqtt.ConnectionSucceeded -= Mqtt_ConnectionSucceeded;
    }
    public void OnNewMessage(MQTTMessage_JJ message)
    {
        var payload = message._GetDataAsBool();
        if (payload != null)
            val = (bool)payload;
    }

    public void _Switch()
    {
        mqtt.Publish(topic, !val);
    }
}
