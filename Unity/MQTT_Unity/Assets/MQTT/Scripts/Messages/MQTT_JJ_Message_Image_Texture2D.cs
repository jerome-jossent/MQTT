using UnityEngine;
using UnityEngine.Events;
using static MQTT_JJ_Message;

[ExecuteInEditMode]
[RequireComponent(typeof(MQTT_JJ_Message))]
public class MQTT_JJ_Message_Image_Texture2D : MonoBehaviour
{
    MQTT_JJ_Message m;
    public Texture2D? value;
    public UnityEvent<Texture2D?> onNewImageTexture2D;

    void Start()
    {
        m = GetComponent<MQTT_JJ_Message>();
        m._datatype = DataType.Image_Texture2D;

        UnityEvent<byte[]> a = new UnityEvent<byte[]>();
        a.AddListener(OnNewImageTexture2D);
        m.client._SubscribeTopic(m.topic, a, m.qos);
    }

    void OnNewImageTexture2D(byte[] data)
    {
        m._NewData(data);
        Texture2D tex = new Texture2D(2, 2);
        try
        {
            tex.LoadImage(data);
            value = tex;
            onNewImageTexture2D?.Invoke(value);
        }
        catch (System.Exception ex)
        {
            Debug.Log("Error while reading data to make 2D Texture : " + ex);
            value = null;
        }
    }
}