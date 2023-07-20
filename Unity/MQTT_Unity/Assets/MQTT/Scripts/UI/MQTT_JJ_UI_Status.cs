using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class MQTT_JJ_UI_Status : MonoBehaviour
{
    UnityEngine.UI.Image led;

    [SerializeField, RequiredField(RequiredField.FieldColor.Green)]
    MQTT_JJ_Client client;

    public Color orange = new Color(1, 0.4f, 0, 1);
    public Color brown = new Color(0.6f, 0.3f, 0, 1);

    private void Awake()
    {
        led = GetComponent<UnityEngine.UI.Image>();
    }

    private void Start()
    {
        //StartCoroutine(WaitToExecute(10));

        client._statusChanged.AddListener(NewStatus);
    }

    //IEnumerator WaitToExecute(int tentative)
    //{
    //    while (client == null && tentative > 0)
    //    {
    //        yield return new WaitForSeconds(0.1f);
    //        tentative--;
    //    }
    //    if (!(tentative > 0))
    //        Debug.Log("MQTT_JJ_UI_ConnectionDeconnection Failed to init");
    //}

    void NewStatus(MQTT_JJ_Client.StatusType status)
    {
        switch (status)
        {
            case MQTT_JJ_Client.StatusType.none: led.color = Color.black; break;
            case MQTT_JJ_Client.StatusType.disconnected: led.color = Color.red; break;
            case MQTT_JJ_Client.StatusType.connecting: led.color = Color.yellow; break;
            case MQTT_JJ_Client.StatusType.connected: led.color = Color.green; break;
            case MQTT_JJ_Client.StatusType.connection_fail: led.color = orange; break;
            case MQTT_JJ_Client.StatusType.connection_lost: led.color = brown; break;
            default: led.color = Color.magenta; break;
        }
    }
}
