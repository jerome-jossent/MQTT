using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Portik_Data_Explorer
{
    internal class SendDataSetByMQTT
    {
        static MQTTnet.Client.IMqttClient mqttClient;

        public struct MQTT_Parameters
        {
            public string IP;
            public int port;
        }

        internal static void Send(string mqtt_topic, byte[] payload, MQTT_Parameters mqtt_Parameters)
        {
            if (mqttClient == null) INIT_Client(mqtt_Parameters);

            MqttApplicationMessage applicationMessage = new MqttApplicationMessageBuilder()
                                                            .WithTopic(mqtt_topic)
                                                            .WithPayload(payload)
                                                            //.WithRetainFlag(retain)
                                                            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                                                            .Build();

            mqttClient.PublishAsync(applicationMessage, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
        }

        private static void INIT_Client(MQTT_Parameters mqtt_Parameters)
        {
            var mqttFactory = new MqttFactory();
            mqttClient = mqttFactory.CreateMqttClient();
            var mqttClientOptions = new MQTTnet.Client.MqttClientOptionsBuilder()
                .WithTcpServer(mqtt_Parameters.IP, mqtt_Parameters.port)
                .Build();
            mqttClient.ConnectAsync(mqttClientOptions, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
        }

    }
}
