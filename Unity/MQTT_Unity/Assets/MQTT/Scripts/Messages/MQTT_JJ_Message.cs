using System;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;

public class MQTT_JJ_Message : MonoBehaviour
{
    public enum QOS_Level { AT_MOST_ONCE = 0, AT_LEAST_ONCE = 1, EXACTLY_ONCE = 2 }
    public enum Encode { None, ASCII, BigEndianUnicode, Unicode, UTF7, UTF8, UTF32 }
    public enum DataType
    {
        ByteArray,
        String,

        Bool,           // TODO
        Int32,          // TODO
        Long,           // TODO
        Float,          // TODO
        Double,         // TODO

        String_Bool,
        String_Int32,
        String_Long,
        String_Float,
        String_Double,

        Vector3,
        Image_Texture2D,
        Color_RGBA,
    }

    #region PARAMETERS
    [SerializeField, RequiredField(RequiredField.FieldColor.Green)]
    public MQTT_JJ_Client client;

    public string topic;

    public DataType _datatype;

    internal byte[] data;

    public QOS_Level qos = QOS_Level.AT_MOST_ONCE;

    string _string;

    public Encode _textCodec = Encode.UTF8;
    public System.Text.Encoding encoding
    {
        get
        {
            switch (_textCodec)
            {
                case Encode.ASCII: return System.Text.Encoding.ASCII;
                case Encode.BigEndianUnicode: return System.Text.Encoding.BigEndianUnicode;
                case Encode.Unicode: return System.Text.Encoding.Unicode;
                case Encode.UTF7: return System.Text.Encoding.UTF7;
                case Encode.UTF8: return System.Text.Encoding.UTF8;
                case Encode.UTF32: return System.Text.Encoding.UTF32;
                case Encode.None:
                default:
                    Debug.Log("no encoding mode choosed");
                    return null;
            }
        }
    }

    [Tooltip("Published message need to be store on broker (probably for new clients)")]
    public bool retain;
    #endregion

    public void _DebugLogErrorWithThis()
    {
        Debug.Log("Erreur sur le topic \"" + topic + "\" avec : \"" + _StringData() + "\"");
    }

    public override string ToString()
    {
        return topic + ":" + _StringData();
    }

    public string _StringData()
    {
        if (_string != null)
            return _string;

        if (data == null || data.Length == 0)
            return null;

        try
        {
            _string = encoding.GetString(data);
            return _string;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return null;
        }
    }

    public void _Reset()
    {
        _string = null;
    }

    internal void _NewData(byte[] data)
    {
        this.data = data;
        _string = null;
    }
    public static byte QOS_Converter(QOS_Level level)
    {
        switch (level)
        {
            case QOS_Level.AT_LEAST_ONCE: return MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE;
            case QOS_Level.EXACTLY_ONCE: return MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE;
            case QOS_Level.AT_MOST_ONCE:
            default:
                return MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE;
        }
    }
}