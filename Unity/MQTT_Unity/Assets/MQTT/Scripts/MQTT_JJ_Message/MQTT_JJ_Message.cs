using System;
using UnityEngine;

public class MQTT_JJ_Message
{
    public string topic;
    public byte[] data;
    public Type type;
    public object val
    {
        set { _val = value; }
        get
        {
            if (_val == null)
                _val = GetData();
            return _val;
        }
    }
    object _val;
    string _string;


    public enum Type
    {
        //Bool, 
        String_Bool,
        String,
        String_Int32,
        String_Long,
        String_Float,
        String_Double,

        Image_Texture2D,
        Vector3
    }

    public enum Encode { ASCII, BigEndianUnicode, Unicode, UTF7, UTF8, UTF32 }
    public Encode encode
    {
        get { return _encode; }
        set
        {
            _encode = value;
            switch (encode)
            {
                case Encode.ASCII: encoding = System.Text.Encoding.ASCII; break;
                case Encode.BigEndianUnicode: encoding = System.Text.Encoding.BigEndianUnicode; break;
                case Encode.Unicode: encoding = System.Text.Encoding.Unicode; break;
                case Encode.UTF7: encoding = System.Text.Encoding.UTF7; break;
                case Encode.UTF8: encoding = System.Text.Encoding.UTF8; break;
                case Encode.UTF32: encoding = System.Text.Encoding.UTF32; break;
            }
        }
    }
    Encode _encode = Encode.UTF8;

    System.Text.Encoding encoding = System.Text.Encoding.UTF8;

    internal void DebugLogErrorWithThis()
    {
        Debug.Log("Erreur sur le topic \"" + topic + "\" avec : \"" + StringData() + "\"");
    }

    public override string ToString()
    {
        return topic + ":" + StringData();
    }

    internal string StringData()
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

    public object GetData() { return StringData(); }

}