using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MQTT_JJ;

public class MQTT_JJ_Publish_SetCounter_18digits : MonoBehaviour
{
    public MQTT_JJ mqtt;

    //public Topic topic;
    public TMPro.TMP_InputField inputfield;

    public void _Send1_Digits()
    {
        string txt = inputfield.text;
        while (txt.Length < 18)
            txt = "0" + txt;

        txt = txt.Trim();
        inputfield.text = txt;
        //mqtt.Publish(topic, txt);
    }
}
