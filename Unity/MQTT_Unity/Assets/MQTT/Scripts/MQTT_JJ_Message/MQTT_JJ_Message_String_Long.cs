using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MQTT_JJ_Message_String_Long : MQTT_JJ_Message
{
    public new Type type = Type.String_Long;

    public new object GetData()
    {
        if (val != null)
            return val;

        val = long.TryParse(StringData(), out long value) ? value : null;
        return val;
    }
}
