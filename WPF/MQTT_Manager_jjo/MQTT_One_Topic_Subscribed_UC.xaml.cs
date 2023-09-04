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
        public MQTT_One_Topic_Subscribed _objet;

        public MQTT_One_Topic_Subscribed_UC()
        {
            InitializeComponent();
            cbx_datatype.ItemsSource = Enum.GetValues(typeof(DataType)).Cast<DataType>();
        }

        public MQTT_One_Topic_Subscribed_UC _Link(MQTT_One_Topic_Subscribed objet)
        {
            this._objet = objet;
            objet._Link(this);
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
            if (cbx_datatype.Text == "") return;

            _objet.dataType = (DataType)Enum.Parse(typeof(DataType), cbx_datatype.Text);
            _objet._topic = tbx_topic.Text;

            var mqtt_uc = _objet.mqtt_uc;

            if (mqtt_uc.topics_subscribed == null)
                mqtt_uc.topics_subscribed = new Dictionary<string, Action<byte[]?>>();

            //unsubscribe
            if (mqtt_uc.topics_subscribed.ContainsKey(_objet._topic) && mqtt_uc.mqttClient.IsConnected)
                mqtt_uc.MQTTClient_Unubscribes(_objet._topic);

            //add or update dictionnary
            if (mqtt_uc.topics_subscribed.ContainsKey(_objet._topic))
                mqtt_uc.topics_subscribed[_objet._topic] = _objet.ManageIncomingData;
            else
                mqtt_uc.topics_subscribed.Add(_objet._topic, _objet.ManageIncomingData);

            //force connection & subscribe
            if (!mqtt_uc.isConnected)
                mqtt_uc.MQTTClient_Connect();
            else
                mqtt_uc.MQTTClient_Subscribes();

            tbx_topic.IsEnabled = false;
            btn_subscribe.IsEnabled = false;
        }

        void cbx_datatype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string typ = e.AddedItems[0].ToString();
            //enable bouton
            btn_subscribe.IsEnabled = (typ != "");
        }
    }
}
