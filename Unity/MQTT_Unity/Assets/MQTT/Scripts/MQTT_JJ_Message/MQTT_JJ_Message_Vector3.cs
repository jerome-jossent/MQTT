using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

public class MQTT_JJ_Message_Vector3 : MQTT_JJ_Message
{
    public new Type type = Type.Vector3;

    byte[] x_b = new byte[4];
    byte[] y_b = new byte[4];
    byte[] z_b = new byte[4];
    float x, y, z;

    public new object GetData()
    {
        if (val != null)
            return val;

        try
        {
            //float : 4 octets
            System.Array.Copy(data, 0, x_b, 0, 4);
            System.Array.Copy(data, 4, y_b, 0, 4);
            System.Array.Copy(data, 8, z_b, 0, 4);

            x = System.BitConverter.ToSingle(x_b, 0);
            y = System.BitConverter.ToSingle(y_b, 0);
            z = System.BitConverter.ToSingle(z_b, 0);

            val = new Vector3(x, y, z);
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
        }
        return val;
    }
}
