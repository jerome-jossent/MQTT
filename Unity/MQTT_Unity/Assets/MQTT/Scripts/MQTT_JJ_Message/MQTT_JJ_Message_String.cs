using System;
using UnityEngine;

public class MQTT_JJ_Message_String : MQTT_JJ_Message
{
    public new Type type = Type.String;

    public new object GetData()
    {
        val = StringData();
        return val;
    }
}
