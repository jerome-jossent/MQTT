using UnityEngine;

[RequireComponent(typeof(MQTT_JJ_Message_String))]
public class MQTT_JJ_UI_Subscribed_String : MonoBehaviour
{
    public TMPro.TMP_Text txt;
    MQTT_JJ_Message_String m;

    private void Start()
    {
        if (txt == null) txt = GetComponent<TMPro.TMP_Text>();
        m = GetComponent<MQTT_JJ_Message_String>();
        m.onNewString.AddListener(OnNewString);
    }

    void OnNewString(string value)
    {
        txt.text = m.value;
    }
}
