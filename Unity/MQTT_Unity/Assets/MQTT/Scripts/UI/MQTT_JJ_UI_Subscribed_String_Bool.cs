using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MQTT_JJ_Message_String_Bool))]
public class MQTT_JJ_UI_Subscribed_String_Bool : MonoBehaviour
{
    public TMPro.TMP_Text txt;
    MQTT_JJ_Message_String_Bool m;

    private void Start()
    {
        if (txt == null) txt = GetComponent<TMPro.TMP_Text>();
        m = GetComponent<MQTT_JJ_Message_String_Bool>();
        m.onNewBool.AddListener(OnNewValue);
    }

    void OnNewValue(bool? value)
    {
        txt.text = m.value.ToString();
    }
}