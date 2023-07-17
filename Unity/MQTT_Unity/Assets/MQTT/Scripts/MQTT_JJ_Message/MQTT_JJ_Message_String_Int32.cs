using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MQTT_JJ_Message_String_Int32 : MQTT_JJ_Message
{
    public new Type type = Type.String_Int32;

    public new object GetData()
    {
        if (val != null)
            return val;

        val = int.TryParse(StringData(), out int value) ? value : null;
        return val;
    }
}