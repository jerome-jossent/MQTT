using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MqttMessage_jj;

public class MQTT_Subscribe_Vector3_ByteArray : MonoBehaviour
{
    public Vector3 _valeur;
    [SerializeField] TMPro.TMP_Text text;

    byte[] x_b = new byte[4];
    byte[] y_b = new byte[4];
    byte[] z_b = new byte[4];
    float x, y, z;

    //spécifique à "vector 3"
    internal  void _DecodeMessage(byte[] message)
    {
        try
        {
            //float : 4 octets
            System.Array.Copy(message, 0, x_b, 0, 4);
            System.Array.Copy(message, 4, y_b, 0, 4);
            System.Array.Copy(message, 8, z_b, 0, 4);

            x = System.BitConverter.ToSingle(x_b, 0);
            y = System.BitConverter.ToSingle(y_b, 0);
            z = System.BitConverter.ToSingle(z_b, 0);

            _valeur = new Vector3(x, y, z);

            if (text != null)
                text.text = _valeur.ToString();
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
        }
    }
}
