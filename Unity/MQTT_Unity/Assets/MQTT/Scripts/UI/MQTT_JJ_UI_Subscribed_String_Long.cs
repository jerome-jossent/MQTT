using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MQTT_JJ_Message_String_Long))]
public class MQTT_JJ_UI_Subscribed_String_Long : MonoBehaviour
{
    public TMPro.TMP_Text txt;
    MQTT_JJ_Message_String_Long m;

    private void Start()
    {
        if (txt == null) txt = GetComponent<TMPro.TMP_Text>();
        m = GetComponent<MQTT_JJ_Message_String_Long>();
        m.onNewLong.AddListener(OnNewValue);
    }

    void OnNewValue(long? value)
    {
        txt.text = m.value.ToString();
    }
}