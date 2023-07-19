using UnityEngine;
//using static MQTT_JJ;
using UnityEngine.Events;

[System.Serializable]
public class MQTT_JJ_Subscribe : MonoBehaviour
{
    public MQTT_JJ mqtt;

    public MQTT_JJ_Message msg;


    public string topic;
    [SerializeField] bool displayMessageInConsole = false;

    public UnityEvent<MQTT_JJ_Message> onNewMessage;

    public object payload;

    [SerializeField] bool alreadySubscribed = false;

    private void Update()
    {
        if (alreadySubscribed) return;

        if (mqtt != null)
        {
            _Subscribe();
            alreadySubscribed = true;
        }
    }

    void Mqtt_ConnectionSucceeded()
    {
        _Subscribe();
        mqtt.ConnectionSucceeded -= Mqtt_ConnectionSucceeded;
    }

    public void _Subscribe()
    {
/////////////////////////////////////////// mqtt._SubscribeTopic(topic, OnNewMessage);
    }

    public void _Unsubscribe()
    {
        mqtt?._UnsubscribeTopic(topic);
    }

    public void OnDestroy()
    {
        _Unsubscribe();
    }

    public void OnNewMessage(MQTT_JJ_Message message)
    {
        if (displayMessageInConsole)
            Debug.Log(message.ToString());

        try
        {
            onNewMessage?.Invoke(message);
        }
        catch (System.Exception)
        {
            message._DebugLogErrorWithThis();
        }
    }
}
