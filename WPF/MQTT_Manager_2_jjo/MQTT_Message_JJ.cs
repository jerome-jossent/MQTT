using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Manager_2_jjo
{
    public class MQTT_Message_JJ
    {
        public string topic;
        public byte[] payload;
        public bool retain;

        public MQTT_Message_JJ(string topic, byte[] payload, bool retain = false)
        {
            this.topic = topic;
            this.payload = payload;
            this.retain = retain;
        }

        /// <summary>
        /// payload encoded in UTF8
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="payload">Encoded in UTF8</param>
        /// <param name="retain"></param>
        public MQTT_Message_JJ(string topic, string payload, bool retain = false) : this(topic, Encoding.UTF8.GetBytes(payload), retain) { }

        public void Subscribe(MQTT_Client_JJ mqtt_client, Action<byte[]?> action)
        {
            mqtt_client._Subscribe(topic, action);
        }

        public void Unsubscribe(MQTT_Client_JJ mqtt_client)
        {
            mqtt_client._Unubscribe(topic);
        }

        public static string ToString(byte[] payload)
        {
            return Encoding.UTF8.GetString(payload);
        }
    }
}
