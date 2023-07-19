using UnityEngine;
using UnityEngine.Events;
using static MQTT_JJ_Message;

[ExecuteInEditMode]
[RequireComponent(typeof(MQTT_JJ_Message))]
public class MQTT_JJ_Message_String : MonoBehaviour
{
    MQTT_JJ_Message m;
    public string value;
    public UnityEvent<string> onNewString;

    void Start()
    {
        m = GetComponent<MQTT_JJ_Message>();
        m._datatype = DataType.String;

        UnityEvent<byte[]> a = new UnityEvent<byte[]>();
        a.AddListener(OnNewString);
        m.client._SubscribeTopic(m.topic, a, m.qos);
    }

    void OnNewString(byte[] data)
    {
        m._NewData(data);
        value = m._StringData();
        onNewString?.Invoke(value);
    }
}