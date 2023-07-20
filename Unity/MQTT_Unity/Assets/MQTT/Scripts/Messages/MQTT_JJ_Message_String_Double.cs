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
        double val;

        string txt = m._StringData();
        txt = txt.Replace(",", ".");
        bool test = double.TryParse(txt, out val);
        if (!test)
        {
            txt = txt.Replace(".", ",");
            test = double.TryParse(txt, out val);
        }
        value = test ? val : null;
        onNewDouble?.Invoke(value);
    }
}