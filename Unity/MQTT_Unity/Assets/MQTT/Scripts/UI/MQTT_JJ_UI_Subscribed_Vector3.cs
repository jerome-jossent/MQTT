using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MQTT_JJ_Message_Vector3))]
public class MQTT_JJ_UI_Subscribed_Vector3 : MonoBehaviour
{
    public TMPro.TMP_Text txt;
    MQTT_JJ_Message_Vector3 m;

    private void Start()
    {
        if (txt == null) txt = GetComponent<TMPro.TMP_Text>();
        m = GetComponent<MQTT_JJ_Message_Vector3>();
        m.onNewVector3.AddListener(OnNewValue);
    }

    void OnNewValue(Vector3? value)
    {
        txt.text = m.value.ToString();
    }
}