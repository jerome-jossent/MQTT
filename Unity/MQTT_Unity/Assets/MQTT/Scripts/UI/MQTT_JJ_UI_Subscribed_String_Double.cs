using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MQTT_JJ_Message_String_Double))]
public class MQTT_JJ_UI_Subscribed_String_Double : MonoBehaviour
{
    public TMPro.TMP_Text txt;
    MQTT_JJ_Message_String_Double m;

    private void Start()
    {
        if (txt == null) txt = GetComponent<TMPro.TMP_Text>();
        m = GetComponent<MQTT_JJ_Message_String_Double>();
        m.onNewDouble.AddListener(OnNewValue);
    }

    void OnNewValue(double? value)
    {
        txt.text = m.value.ToString();
    }
}