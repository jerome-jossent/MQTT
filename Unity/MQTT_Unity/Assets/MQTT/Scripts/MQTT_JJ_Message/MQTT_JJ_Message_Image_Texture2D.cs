using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

public class MQTT_JJ_Message_Image_Texture2D : MQTT_JJ_Message
{
    public new Type type = Type.Image_Texture2D;

    public new object GetData()
    {
        if (val != null)
            return val;
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(data);
        val = tex;
        return val;
    }
}
