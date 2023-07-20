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

    public void _SendBool(bool value)
    {
       m.client.Publish(m, value);
    }
    public void _SendInt(int value)
    {
       m.client.Publish(m, value);
    }
    public void _SendLong(long value)
    {
       m.client.Publish(m, value);
    }
    public void _SendFloat(float value)
    {
       m.client.Publish(m, value);
    }
    public void _SendDouble(double value)
    {
       m.client.Publish(m, value);
    }
    public void _SendString(string value)
    {
       m.client.Publish(m, value);
    }
    public void _SendByteArray(byte[] value)
    {
       m.client.Publish(m, value);
    }

    //surcharge
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
