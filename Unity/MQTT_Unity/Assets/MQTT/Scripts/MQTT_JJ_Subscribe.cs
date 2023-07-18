using UnityEngine;
using static MQTT_JJ;
using UnityEngine.Events;

[System.Serializable]
public class MQTT_JJ_Subscribe : MonoBehaviour
{
    public MQTT_JJ mqtt;

    public string topic;
    [SerializeField] bool displayMessageInConsole = false;
    public DataType dataType;
    public MQTT_JJ_Message.Type type;

    public UnityEvent<MQTT_JJ_Message> onNewMessage;

    public object payload;
    //public UnityEvent<bool> onNewBool;
    //public UnityEvent<int> onNewInt;
    //public UnityEvent<long> onNewLong;
    //public UnityEvent<float> onNewFloat;
    //public UnityEvent<double> onNewDouble;
    //public UnityEvent<string> onNewString;


    public void Start()
    {
        //if (!mqtt.gameObject.activeSelf) enabled = false;
        //mqtt.ConnectionSucceeded += Mqtt_ConnectionSucceeded;
    }

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
        mqtt._SubscribeTopic(topic, OnNewMessage);
    }

    public void OnDestroy()
    {
        mqtt?._UnsubscribeTopic(topic);
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
            message.DebugLogErrorWithThis();
        }

        //switch (dataType)
        //{
        //    case DataType._bool:
        //        payload = message._GetDataAsBool();
        //        if (payload != null) { onNewBool?.Invoke((bool)payload); return; }
        //        break;

        //    case DataType._int:
        //        payload = message._GetDataAsInt32();
        //        if (payload != null) { onNewInt?.Invoke((int)payload); return; }
        //        break;

        //    case DataType._long:
        //        payload = message._GetDataAsLong();
        //        if (payload != null) { onNewLong?.Invoke((long)payload); return; }
        //        break;

        //    case DataType._float:
        //        payload = message._GetDataAsFloat();
        //        if (payload != null) { onNewFloat?.Invoke((float)payload); return; }
        //        break;

        //    case DataType._double:
        //        payload = message._GetDataAsDouble();
        //        if (payload != null) { onNewDouble?.Invoke((double)payload); return; }
        //        break;

        //    case DataType._string:
        //        payload = message._GetDataAsString();
        //        if (payload != null) { onNewString?.Invoke((string)payload); return; }
        //        break;

        //    default:
        //        break;
        //}

        //message.DebugLogErrorWithThis();
    }
}
