using System;
using UnityEngine;

public class MQTT_JJ_Message_String_Bool : MQTT_JJ_Message
{
    public new Type type = Type.String_Bool;

    public new object GetData()
    {
        if (val != null)
            return val;

        if (data == null || data.Length == 0)
            return null;

        string result = "";
        try
        {
            result = StringData();
            if (result == "1") val = true;
            if (result == "0") val = false;
            if (result.ToLower() == "true") val = true;
            if (result.ToLower() == "false") val = false;
            return val;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString() + " val = " + result);
            return null;
        }
    }
}