using System;
using UnityEngine;
using UnityEngine.Events;
using static MQTT_JJ_Message;

[ExecuteInEditMode]
[RequireComponent(typeof(MQTT_JJ_Message))]
public class MQTT_JJ_Message_String_Int32 : MonoBehaviour
{
    MQTT_JJ_Message m;
    public int? value;
    public UnityEvent<Int32?> onNewInt32;

    void Start()
    {
        m = GetComponent<MQTT_JJ_Message>();
        m._datatype = DataType.String_Int32;

        UnityEvent<byte[]> a = new UnityEvent<byte[]>();
        a.AddListener(OnNewInt32);
        m.client._SubscribeTopic(m.topic, a, m.qos);
    }

    void OnNewInt32(byte[] data)
    {
        m._NewData(data);
        value = int.TryParse(m._StringData(), out int val) ? val : null;
        onNewInt32?.Invoke(value);
    }
}