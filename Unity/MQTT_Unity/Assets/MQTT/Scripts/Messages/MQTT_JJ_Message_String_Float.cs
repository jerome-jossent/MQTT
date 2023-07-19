using UnityEngine;
using UnityEngine.Events;
using static MQTT_JJ_Message;

[ExecuteInEditMode]
[RequireComponent(typeof(MQTT_JJ_Message))]
public class MQTT_JJ_Message_String_Float : MonoBehaviour
{
    MQTT_JJ_Message m;
    public float? value;
    public UnityEvent<float?> onNewFloat;

    void Start()
    {
        m = GetComponent<MQTT_JJ_Message>();
        m._datatype = DataType.String_Float;

        UnityEvent<byte[]> a = new UnityEvent<byte[]>();
        a.AddListener(OnNewFloat);
        m.client._SubscribeTopic(m.topic, a, m.qos);
    }

    void OnNewFloat(byte[] data)
    {
        m._NewData(data);
        value = float.TryParse(m._StringData(), out float val) ? val : null;
        onNewFloat?.Invoke(value);
    }
}