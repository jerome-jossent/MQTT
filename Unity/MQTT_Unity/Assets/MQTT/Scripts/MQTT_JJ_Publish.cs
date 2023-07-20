using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MQTT_JJ_Message))]
public class MQTT_JJ_Publish : MonoBehaviour
{
    MQTT_JJ_Message m;

    void Start()
    {
        m = GetComponent<MQTT_JJ_Message>();
    }

    public void _Send(bool value)
    {
        m.client.Publish(m, value);
    }
    public void _Send(int value)
    {
        m.client.Publish(m, value);
    }
    public void _Send(long value)
    {
        m.client.Publish(m, value);
    }
    public void _Send(float value)
    {
        m.client.Publish(m, value);
    }
    public void _Send(double value)
    {
        m.client.Publish(m, value);
    }
    public void _Send(string value)
    {
        m.client.Publish(m, value);
    }
    public void _Send(byte[] value)
    {
        m.client.Publish(m, value);
    }
}
