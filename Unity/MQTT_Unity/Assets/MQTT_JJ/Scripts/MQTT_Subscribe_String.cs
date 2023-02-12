using UnityEngine;

public class MQTT_Subscribe_String : MQTT_JJ
{
    public string _valeur;
    [SerializeField] TMPro.TMP_Text text;

    //spécifique à "string"
    internal override void _DecodeMessage(byte[] message)
    {
        try
        {
            _valeur = System.Text.Encoding.UTF8.GetString(message);
            if (text != null)
                text.text = _valeur;
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
        }
    }
}