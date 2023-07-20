using UnityEngine;
using UnityEngine.Events;
using static MQTT_JJ_Message;

[ExecuteInEditMode]
[RequireComponent(typeof(MQTT_JJ_Message))]
public class MQTT_JJ_Message_Color_RGBA : MonoBehaviour
{
    [SerializeField] MQTT_JJ_Message m;
    public Color? value;
    public UnityEvent<Color?> onNewColor;
    byte[] rgba = new byte[4];
    byte r, g, b, a;

    void Start()
    {
        if (m == null)
        {
            MQTT_JJ_Message[] ms = GetComponents<MQTT_JJ_Message>();
            if (ms.Length > 1)
                Debug.LogWarning("Attention plusieurs composants Message présent");
            else
                m = ms[0];
        }
        m._datatype = DataType.Color_RGBA;

        UnityEvent<byte[]> a = new UnityEvent<byte[]>();
        a.AddListener(OnNewColor);
        m.client._SubscribeTopic(m.topic, a, m.qos);
    }

    void OnNewColor(byte[] data)
    {
        m._NewData(data);
        try
        {
            r = data[0];
            g = data[1];
            b = data[2];
            a = data[3];

            float R, G, B, A;
            R = (float)r / 255;
            G = (float)g / 255;
            B = (float)b / 255;
            A = (float)a / 255;

            value = new Color(R, G, B, A);
        }
        catch (System.Exception ex)
        {
            Debug.Log("Error while reading data to make Vector 3D : " + ex);
            value = null;
        }
        onNewColor?.Invoke(value);
    }
}