using UnityEngine;
using UnityEngine.Events;
using static MQTT_JJ_Message;

[ExecuteInEditMode]
[RequireComponent(typeof(MQTT_JJ_Message))]
public class MQTT_JJ_Message_String_Long : MonoBehaviour
{
    MQTT_JJ_Message m;
    public long? value;
    public UnityEvent<long?> onNewLong;

    void Start()
    {
        m = GetComponent<MQTT_JJ_Message>();
        m._datatype = DataType.String_Long;

        UnityEvent<byte[]> a = new UnityEvent<byte[]>();
        a.AddListener(OnNewLong);
        m.client._SubscribeTopic(m.topic, a, m.qos);
    }

    void OnNewLong(byte[] data)
    {
        m._NewData(data);
        value = long.TryParse(m._StringData(), out long val) ? val : null;
        onNewLong?.Invoke(value);
    }
}