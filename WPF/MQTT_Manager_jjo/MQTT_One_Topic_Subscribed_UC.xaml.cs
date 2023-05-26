using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using CompactExifLib;
using static MQTT_Manager_jjo.MQTT_Enums;

namespace MQTT_Manager_jjo
{
    public partial class MQTT_One_Topic_Subscribed_UC : UserControl
    {
        public MQTT_One_Topic_Subscribed objet;

        public MQTT_One_Topic_Subscribed_UC()
        {
            InitializeComponent();
            cbx_datatype.ItemsSource = Enum.GetValues(typeof(DataType)).Cast<DataType>();
        }

        public MQTT_One_Topic_Subscribed_UC _Link(MQTT_One_Topic_Subscribed objet)
        {
            this.objet = objet;
            objet._uc = this;
            return this;
        }

        public void DisplayJsonFromMetaData(byte[] data)
        {
            MemoryStream imageStream = new MemoryStream(data);
            ExifData exif = new ExifData(imageStream);
            exif.GetTagValue(ExifTag.ImageDescription, out string metadatas_json, StrCoding.Utf8);
            Dispatcher.Invoke(() =>
            {
                ObjectExplorer.FillTreeView(tvw_json, metadatas_json, "JSON in ImageDescription");
                ObjectExplorer.ExpandAll(tvw_json);
            });
        }

        public void DisplayMetaData(byte[] data)
        {
            MemoryStream imageStream = new MemoryStream(data);
            ExifData exif = new ExifData(imageStream);
            exif.GetTagValue(ExifTag.ImageDescription, out string metadatas, StrCoding.Utf8);
            AddToMessageList(metadatas);
        }

        public void AddToMessageList(string txt)
        {
            Dispatcher.Invoke(() =>
            {
                tbk_value.Text = txt;
            });
        }

        public void DisplayImage(byte[] data)
        {
            Dispatcher.Invoke(() =>
            {
                img.Source = ToImage(data);
            });
        }

        public BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        void btn_subscribe_Click(object sender, RoutedEventArgs e)
        {
            objet.dataType = (DataType)Enum.Parse(typeof(DataType), cbx_datatype.Text);
            string topic = tbx_topic.Text;

            var mqtt_uc = objet.mqtt_uc;

            if (mqtt_uc.topics_subscribed == null)
                mqtt_uc.topics_subscribed = new Dictionary<string, Action<byte[]?>>();

            //unsubscribe
            if (mqtt_uc.topics_subscribed.ContainsKey(topic) && mqtt_uc.mqttClient.IsConnected)
                mqtt_uc.MQTTClient_Unubscribes(topic);

            //update dictionnary
            if (mqtt_uc.topics_subscribed.ContainsKey(topic))
                mqtt_uc.topics_subscribed[topic] = objet.ManageIncomingData;
            else
                mqtt_uc.topics_subscribed.Add(topic, objet.ManageIncomingData);

            //force connection & subscribe
            if (!mqtt_uc.isConnected)
                mqtt_uc.MQTTClient_Connect();
            else
                mqtt_uc.MQTTClient_Subscribes();            
        }
    }
}
