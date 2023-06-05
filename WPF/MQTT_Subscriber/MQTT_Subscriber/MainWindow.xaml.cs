using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using Image = System.Drawing.Image;
using CompactExifLib;
using System.Runtime.CompilerServices;
using MQTT_Manager_jjo;

namespace MQTT_Subscriber
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public BitmapImage _bmp
        {
            get { return bmp; }
            set
            {
                bmp = value;
                OnPropertyChanged();
            }
        }
        BitmapImage bmp;

        enum DataType { _boolean, _integer, _long, _float, _double, _string, _image, _image_with_metadatas, _image_with_json_in_metadata, _vector3, _color }
        DataType dataType;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            INITS();
        }

        void INITS()
        {
            mqtt_uc.topics_subscribed = new Dictionary<string, Action<byte[]?>>();
//            cbx_datatype.ItemsSource = Enum.GetValues(typeof(DataType)).Cast<DataType>();
//            messages_recus = new ObservableCollection<FrameworkElement>();
        }
        private void SubscribeAdd(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MQTT_One_Topic_Subscribed mots = new(mqtt_uc);
            Dispatcher.Invoke(() =>
            {
                MQTT_One_Topic_Subscribed_UC mots_uc = new();
                mots_uc._Link(mots);
                lvw_sub.Items.Add(mots_uc);
            });
        }

        //void btn_subscribe_Click(object sender, RoutedEventArgs e)
        //{
        //    dataType = (DataType)Enum.Parse(typeof(DataType), cbx_datatype.Text);
        //    string topic = tbx_topic.Text;

        //    //unsubscribe
        //    if (mqtt_uc.topics_subscribed.ContainsKey(topic) && mqtt_uc.mqttClient.IsConnected)
        //        mqtt_uc.MQTTClient_Unubscribes(topic);

        //    //update dictionnary
        //    if (mqtt_uc.topics_subscribed.ContainsKey(topic))
        //        mqtt_uc.topics_subscribed[topic] = ManageIncomingData;
        //    else
        //        mqtt_uc.topics_subscribed.Add(topic, ManageIncomingData);

        //    //force connection & subscribe
        //    if (!mqtt_uc.isConnected)
        //        mqtt_uc.MQTTClient_Connect();
        //    else
        //    {
        //        //subscribe
        //        mqtt_uc.MQTTClient_Subscribes();
        //    }
        //}


        //void ManageIncomingData(byte[]? data)
        //{
        //    if (data == null) return;

        //    DateTime dateTime = DateTime.Now; // à utiliser !?!?

        //    string txt;
        //    switch (dataType)
        //    {
        //        case DataType._boolean:
        //            txt = Encoding.Default.GetString(data);
        //            bool val_bool = bool.Parse(txt);
        //            txt = val_bool.ToString();
        //            AddToMessageList(txt);
        //            break;

        //        case DataType._integer:
        //            txt = Encoding.Default.GetString(data);
        //            int val_int = int.Parse(txt);
        //            txt = val_int.ToString();
        //            AddToMessageList(txt);
        //            break;

        //        case DataType._long:
        //            txt = Encoding.Default.GetString(data);
        //            long val_long = long.Parse(txt);
        //            txt = val_long.ToString();
        //            AddToMessageList(txt);
        //            break;

        //        case DataType._float:
        //            txt = Encoding.Default.GetString(data);
        //            float val_float = float.Parse(txt);
        //            txt = val_float.ToString();
        //            AddToMessageList(txt);
        //            break;

        //        case DataType._double:
        //            txt = Encoding.Default.GetString(data);
        //            double val_double = double.Parse(txt);
        //            txt = val_double.ToString();
        //            AddToMessageList(txt);
        //            break;

        //        case DataType._string:
        //            txt = Encoding.Default.GetString(data);
        //            AddToMessageList(txt);
        //            break;

        //        case DataType._image:
        //            DisplayImage(data);
        //            break;

        //        case DataType._image_with_metadatas:
        //            DisplayImage(data);
        //            DisplayMetaData(data);
        //            break;

        //        case DataType._image_with_json_in_metadata:
        //            DisplayImage(data);
        //            DisplayJsonFromMetaData(data);
        //            break;

        //        case DataType._vector3:
        //            float[] vec3 = GetVector3FromByteArray(data);
        //            txt = vec3[0].ToString() + " ; " +
        //                  vec3[1].ToString() + " ; " +
        //                  vec3[2].ToString();
        //            AddToMessageList(txt);
        //            break;

        //        case DataType._color:

        //            break;

        //        default:
        //            break;
        //    }
        //}

        //void DisplayJsonFromMetaData(byte[] data)
        //{
        //    MemoryStream imageStream = new MemoryStream(data);
        //    ExifData exif = new ExifData(imageStream);
        //    exif.GetTagValue(ExifTag.ImageDescription, out string metadatas_json, StrCoding.Utf8);
        //    Dispatcher.Invoke(() =>
        //    {
        //        ObjectExplorer.FillTreeView(tvw_json, metadatas_json, "JSON in ImageDescription");
        //        ObjectExplorer.ExpandAll(tvw_json);
        //    });
        //}

        //void DisplayMetaData(byte[] data)
        //{
        //    MemoryStream imageStream = new MemoryStream(data);
        //    ExifData exif = new ExifData(imageStream);
        //    exif.GetTagValue(ExifTag.ImageDescription, out string metadatas, StrCoding.Utf8);
        //    AddToMessageList(metadatas);
        //}

        //void AddToMessageList(string txt)
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        TextBlock tb = new TextBlock() { Text = txt };
        //        _messages_recus.Insert(0, tb);
        //    });
        //}

        //void DisplayImage(byte[] data)
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        _bmp = ToImage(data);
        //    });
        //}

        float[] GetVector3FromByteArray(byte[] data)
        {
            float[] _valeur = new float[3];
            //float : 4 octets
            byte[] x_b = new byte[4];
            byte[] y_b = new byte[4];
            byte[] z_b = new byte[4];
            try
            {
                Array.Copy(data, 0, x_b, 0, 4);
                Array.Copy(data, 4, y_b, 0, 4);
                Array.Copy(data, 8, z_b, 0, 4);

                _valeur[0] = BitConverter.ToSingle(x_b, 0);
                _valeur[1] = BitConverter.ToSingle(y_b, 0);
                _valeur[2] = BitConverter.ToSingle(z_b, 0);

            }
            catch (Exception ex)
            {

            }
            return _valeur;
        }

        public static Image ImageFromByteArray(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            using (Image image = Image.FromStream(ms, true, true))
            {
                return (Image)image.Clone();
            }
        }

        public BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

    }
}