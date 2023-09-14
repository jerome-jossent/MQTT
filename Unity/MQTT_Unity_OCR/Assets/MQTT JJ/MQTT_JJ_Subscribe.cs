using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MQTT_JJ;
using UnityEngine.Events;

[System.Serializable]
public class MQTT_JJ_Subscribe : MonoBehaviour
{
    public MQTT_JJ mqtt;

    public string topic;

    public enum DataType { _bytearray, _string, _bool, _int, _long, _float, _double, }
    public DataType dataType;

    public object payload;
    public UnityEvent<byte[]> onNewByteArray;
    public UnityEvent<string> onNewString;
    public UnityEvent<bool> onNewBool;
    public UnityEvent<int> onNewInt;
    public UnityEvent<long> onNewLong;
    public UnityEvent<float> onNewFloat;
    public UnityEvent<double> onNewDouble;

    public void Start()
    {
        if (!mqtt.gameObject.activeSelf) enabled = false;
        mqtt.ConnectionSucceeded += Mqtt_ConnectionSucceeded;
    }

    void Mqtt_ConnectionSucceeded()
    {
        mqtt._SubscribeTopic(topic, OnNewMessage);
        mqtt.ConnectionSucceeded -= Mqtt_ConnectionSucceeded;
    }

    public void OnNewMessage(MQTTMessage_JJ message)
    {

        switch (dataType)
        {
            case DataType._bool:
                payload = message._GetDataAsBool();
                if (payload != null) { onNewBool?.Invoke((bool)payload); return; }
                break;

            case DataType._int:
                payload = message._GetDataAsInt32();
                if (payload != null) { onNewInt?.Invoke((int)payload); return; }
                break;
            case DataType._long:
                payload = message._GetDataAsLong();
                if (payload != null) { onNewLong?.Invoke((long)payload); return; }
                break;
            case DataType._float:
                payload = message._GetDataAsFloat();
                if (payload != null) { onNewFloat?.Invoke((float)payload); return; }
                break;
            case DataType._double:
                payload = message._GetDataAsDouble();
                if (payload != null) { onNewDouble?.Invoke((double)payload); return; }
                break;
            case DataType._string:
                payload = message._GetDataAsString();
                if (payload != null) { onNewString?.Invoke((string)payload); return; }
                break;
            case DataType._bytearray:
                payload = message.data;
                if (payload != null) { onNewByteArray?.Invoke((byte[])payload); return; }
                break;
            default:
                break;
        }

        message.DebugLogErrorWithThis();
    }
}
