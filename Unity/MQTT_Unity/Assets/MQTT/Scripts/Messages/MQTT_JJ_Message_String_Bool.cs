using UnityEngine;
using UnityEngine.Events;
using static MQTT_JJ_Message;

[ExecuteInEditMode]
[RequireComponent(typeof(MQTT_JJ_Message))]
public class MQTT_JJ_Message_String_Bool : MonoBehaviour
{
    MQTT_JJ_Message m;
    public bool? value;
    public UnityEvent<bool?> onNewBool;

    void Start()
    {
        m = GetComponent<MQTT_JJ_Message>();
        m._datatype = DataType.String_Bool;

        UnityEvent<byte[]> a = new UnityEvent<byte[]>();
        a.AddListener(OnNewBool);
        m.client._SubscribeTopic(m.topic, a, m.qos);
    }

    void OnNewBool(byte[] data)
    {
        m._NewData(data);
        string result = m._StringData();
        value = null;
        if (result == "1") value = true;
        if (result == "0") value = false;
        if (result.ToLower() == "true") value = true;
        if (result.ToLower() == "false") value = false;
        onNewBool?.Invoke(value);
    }
}