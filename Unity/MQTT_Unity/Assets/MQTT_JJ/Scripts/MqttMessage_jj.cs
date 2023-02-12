using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MqttMessage_jj
{
    public enum Topic { newPlayer, tempo, buzz, autre }

    public Topic topic;
    public string message;
    public DateTime dateTime;

    public MqttMessage_jj(Topic topic, string message)
    {
        this.topic = topic;
        this.message = message;
        dateTime = DateTime.Now;
    }
    public MqttMessage_jj(string topic, string message)
    {
        this.topic = GetTopic(topic);
        this.message = message;
        dateTime = DateTime.Now;
    }

    Topic GetTopic(string topic)
    {
        switch (topic)
        {
            case "buzzer/tempo": return Topic.tempo;
            case "buzzer/BUZZ": return Topic.buzz;
            case "buzzer/NewPlayer": return Topic.newPlayer;
            default: return Topic.autre;
        }
    }

    public override string ToString()
    {
        return dateTime.ToString("HH:mm:ss.fff") + " [" + topic + "] " + message;
    }
}