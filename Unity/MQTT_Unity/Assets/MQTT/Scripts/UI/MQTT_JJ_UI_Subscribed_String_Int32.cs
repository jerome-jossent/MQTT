using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MQTT_JJ_Message_String_Int32))]
public class MQTT_JJ_UI_Subscribed_String_Int32 : MonoBehaviour
{
    public TMPro.TMP_Text txt;
    MQTT_JJ_Message_String_Int32 m;

    private void Start()
    {
        if (txt == null) txt = GetComponent<TMPro.TMP_Text>();
        m = GetComponent<MQTT_JJ_Message_String_Int32>();
        m.onNewInt32.AddListener(OnNewValue);
    }

    void OnNewValue(Int32? value)
    {
        txt.text = m.value.ToString();
    }
}