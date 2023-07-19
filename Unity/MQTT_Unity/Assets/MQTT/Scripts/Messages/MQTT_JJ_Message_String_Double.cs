using UnityEngine;
using UnityEngine.Events;
using static MQTT_JJ_Message;

[ExecuteInEditMode]
[RequireComponent(typeof(MQTT_JJ_Message))]
public class MQTT_JJ_Message_String_Double : MonoBehaviour
{
    MQTT_JJ_Message m;
    public double? value;
    public UnityEvent<double?> onNewDouble;

    void Start()
    {
        m = GetComponent<MQTT_JJ_Message>();
        m._datatype = DataType.String_Double;

        UnityEvent<byte[]> a = new UnityEvent<byte[]>();
        a.AddListener(OnNewDouble);
        m.client._SubscribeTopic(m.topic, a, m.qos);
    }

    void OnNewDouble(byte[] data)
    {
        m._NewData(data);
        value = double.TryParse(m._StringData(), out double val) ? val : null;
        onNewDouble?.Invoke(value);
    }
}