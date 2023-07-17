using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MQTT_JJ_Message_String_Float : MQTT_JJ_Message
{
    public new Type type = Type.String_Float;

    public new object GetData()
    {
        if (val != null)
            return val;

        val = float.TryParse(StringData(), out float value) ? value : null;
        return val;
    }
}
