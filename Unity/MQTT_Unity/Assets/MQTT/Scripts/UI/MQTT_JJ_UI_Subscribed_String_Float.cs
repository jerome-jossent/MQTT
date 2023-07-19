using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MQTT_JJ_Message_String_Float))]
public class MQTT_JJ_UI_Subscribed_String_Float : MonoBehaviour
{
    public TMPro.TMP_Text txt;
    MQTT_JJ_Message_String_Float m;

    private void Start()
    {
        if (txt == null) txt = GetComponent<TMPro.TMP_Text>();
        m = GetComponent<MQTT_JJ_Message_String_Float>();
        m.onNewFloat.AddListener(OnNewValue);
    }

    void OnNewValue(float? value)
    {
        txt.text = m.value.ToString();
    }
}