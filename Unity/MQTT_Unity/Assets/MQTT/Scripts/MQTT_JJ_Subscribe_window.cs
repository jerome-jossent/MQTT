using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MQTT_JJ_Subscribe_window : MonoBehaviour
{
    [SerializeField] MQTT_JJ mqtt;

    [SerializeField] TMP_InputField if_topic;
    [SerializeField] GameObject items;
    [SerializeField] GameObject prefab_subscribe_string;

    public void _Create()
    {
        string topic = if_topic.text;
        Debug.Log("a");
        GameObject go = Instantiate(prefab_subscribe_string);
        go.name= "Sub \"" + topic + "\"";
        go.transform.SetParent(items.transform);

        //Debug.Log("b1");
        MQTT_JJ_Subscribe_GameObject sc = go.GetComponent<MQTT_JJ_Subscribe_GameObject>();
        sc._Link(mqtt, topic, MQTT_JJ.DataType._string);
        //Debug.Log("c");
    }

}
