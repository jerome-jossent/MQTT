using UnityEngine;
using UnityEngine.Events;
using static MQTT_JJ_Message;

[ExecuteInEditMode]
[RequireComponent(typeof(MQTT_JJ_Message))]
public class MQTT_JJ_Message_Vector3 : MonoBehaviour
{
    MQTT_JJ_Message m;
    public Vector3? value;
    public UnityEvent<Vector3?> onNewVector3;
    byte[] x_b = new byte[4];
    byte[] y_b = new byte[4];
    byte[] z_b = new byte[4];
    float x, y, z;

    void Start()
    {
        m = GetComponent<MQTT_JJ_Message>();
        m._datatype = DataType.Vector3;

        UnityEvent<byte[]> a = new UnityEvent<byte[]>();
        a.AddListener(OnNewVector3);
        m.client._SubscribeTopic(m.topic, a, m.qos);
    }

    void OnNewVector3(byte[] data)
    {
        m._NewData(data);
        try
        {
            //float : 4 octets
            System.Array.Copy(data, 0, x_b, 0, 4);
            System.Array.Copy(data, 4, y_b, 0, 4);
            System.Array.Copy(data, 8, z_b, 0, 4);

            x = System.BitConverter.ToSingle(x_b, 0);
            y = System.BitConverter.ToSingle(y_b, 0);
            z = System.BitConverter.ToSingle(z_b, 0);

            value = new Vector3(x, y, z);
        }
        catch (System.Exception ex)
        {
            Debug.Log("Error while reading data to make Vector 3D : " + ex);
            value = null;
        }
        onNewVector3?.Invoke(value);
    }
}