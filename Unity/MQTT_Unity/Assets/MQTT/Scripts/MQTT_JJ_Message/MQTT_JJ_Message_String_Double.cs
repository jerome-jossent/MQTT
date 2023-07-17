using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MQTT_JJ_Message_String_Double : MQTT_JJ_Message
{
    public new Type type = Type.String_Double;

    public new object GetData()
    {
        if (val != null)
            return val;

        val = double.TryParse(StringData(), out double value) ? value : null;
        return val;
    }
}
