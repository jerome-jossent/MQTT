using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MQTT_JJ;

public class MQTT_JJ_Publish : MonoBehaviour
{
    public MQTT_JJ mqtt;

    public Topic topic;

    public TMPro.TMP_InputField inputField;


    public void _SendString(string value)
    {
        mqtt.Publish(topic, value);
    }

    public void _SendBool(bool value)
    {
        mqtt.Publish(topic, value);
    }

    public void _SendInt_FromInputField()
    {
        if (inputField == null) return;

        if (int.TryParse(inputField.text, out int val))
            mqtt.Publish(topic, val);
    }
}
