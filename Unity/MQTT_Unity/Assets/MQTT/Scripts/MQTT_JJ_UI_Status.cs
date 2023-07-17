using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MQTT_JJ_UI_Status : MonoBehaviour
{
    UnityEngine.UI.Image led;

    public Color orange = new Color(1, 0.4f, 0, 1);
    public Color brown = new Color(0.6f, 0.3f, 0, 1);

    private void Awake()
    {
        led = GetComponent<UnityEngine.UI.Image>();
    }

    public void NewStatus(MQTT_JJ.StatusType status)
    {
        switch (status)
        {
            case MQTT_JJ.StatusType.none: led.color = Color.black; break;
            case MQTT_JJ.StatusType.disconnected: led.color = Color.red; break;
            case MQTT_JJ.StatusType.connecting: led.color = Color.yellow; break;
            case MQTT_JJ.StatusType.connected: led.color = Color.green; break;
            case MQTT_JJ.StatusType.connection_fail: led.color = orange; break;
            case MQTT_JJ.StatusType.connection_lost: led.color = brown; break;
            default: led.color = Color.magenta; break;
        }
    }
}
