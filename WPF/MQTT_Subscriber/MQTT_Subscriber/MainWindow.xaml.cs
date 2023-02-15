using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MQTTnet;
using static System.Net.Mime.MediaTypeNames;

using System.Drawing;
using Image = System.Drawing.Image;
using CompactExifLib;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Numerics;

namespace MQTT_Subscriber
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        MQTTnet.Client.IMqttClient mqttClient;
        string IP = "35.210.193.228";   //"127.0.0.1";
        int port = 1883;

        enum typetopic { texte, booleen, entier, virgule, image, fluximages_webcam, fluximages_folder, geoimage, vector3 };
        Dictionary<typetopic, string> topics;


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ObservableCollection<FrameworkElement> _messages_recus
        {
            get { return messages_recus; }
            set
            {
                messages_recus = value;
                OnPropertyChanged("_messages_recus");
            }
        }
        ObservableCollection<FrameworkElement> messages_recus;

        public BitmapImage _bmp
        {
            get { return bmp; }
            set
            {
                bmp = value;
                OnPropertyChanged("_bmp");
            }
        }
        BitmapImage bmp;

        public BitmapImage _bmp_webcam
        {
            get { return bmp_webcam; }
            set
            {
                bmp_webcam = value;
                OnPropertyChanged("_bmp_webcam");
            }
        }
        BitmapImage bmp_webcam;

        public BitmapImage _bmp_folder
        {
            get { return bmp_folder; }
            set
            {
                bmp_folder = value;
                OnPropertyChanged("_bmp_folder");
            }
        }
        BitmapImage bmp_folder;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            INITS();
        }

        void INITS()
        {
            INIT_topics();
            MQTTClient_Init();
            MQTTClient_Subscribes();
            messages_recus = new ObservableCollection<FrameworkElement>();
        }

        void INIT_topics()
        {
            topics = new Dictionary<typetopic, string>();
            topics.Add(typetopic.texte, "texte");
            topics.Add(typetopic.booleen, "booleen");
            topics.Add(typetopic.entier, "entier");
            topics.Add(typetopic.virgule, "virgule");
            topics.Add(typetopic.image, "image");
            topics.Add(typetopic.fluximages_webcam, "fluximages_webcam");
            topics.Add(typetopic.fluximages_folder, "fluximages_folder");
            topics.Add(typetopic.geoimage, "geoimage");
            topics.Add(typetopic.vector3, "vector3");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MQTTClient_Stop();
        }

        #region MQTT
        //https://github.com/dotnet/MQTTnet/blob/master/Samples/Client/Client_Subscribe_Samples.cs

        void MQTTClient_Init()
        {
            var mqttFactory = new MqttFactory();
            mqttClient = mqttFactory.CreateMqttClient();

            var mqttClientOptions = new MQTTnet.Client.MqttClientOptionsBuilder()
                .WithTcpServer(IP, port)
                .Build();

            mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;

            mqttClient.ConnectingAsync += MqttClient_ConnectingAsync;
            mqttClient.ConnectedAsync += MqttClient_ConnectedAsync;
            mqttClient.DisconnectedAsync += MqttClient_DisconnectedAsync;
            mqttClient.ConnectAsync(mqttClientOptions, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
        }

        void MQTTClient_Subscribes()
        {
            var mqtt_Subscriber = new MQTTnet.Client.MqttClientSubscribeOptions();
            foreach (var item in topics)
            {
                var filter = new MQTTnet.Packets.MqttTopicFilter() { Topic = item.Value };
                mqtt_Subscriber.TopicFilters.Add(filter);
            }

            mqttClient.SubscribeAsync(mqtt_Subscriber, System.Threading.CancellationToken.None);
        }

        Task MqttClient_ConnectingAsync(MQTTnet.Client.MqttClientConnectingEventArgs arg)
        {
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        Task MqttClient_ConnectedAsync(MQTTnet.Client.MqttClientConnectedEventArgs arg)
        {
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        Task MqttClient_DisconnectedAsync(MQTTnet.Client.MqttClientDisconnectedEventArgs arg)
        {
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        Task MqttClient_ApplicationMessageReceivedAsync(MQTTnet.Client.MqttApplicationMessageReceivedEventArgs arg)
        {
            ManageMessage(arg);
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        void MQTTClient_Stop()
        {
            mqttClient?.DisconnectAsync(new MQTTnet.Client.MqttClientDisconnectOptions()).GetAwaiter().GetResult();//.DisconnectAsync().GetAwaiter();
        }
        #endregion

        void AddToMessageList(string text)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                TextBlock tb = new TextBlock() { Text = text };
                _messages_recus.Insert(0, tb);

                while (_messages_recus.Count > 200)
                    _messages_recus.RemoveAt(200);
            }, null);
        }


        void ManageMessage(MQTTnet.Client.MqttApplicationMessageReceivedEventArgs arg)
        {
            string topic = arg.ApplicationMessage.Topic;
            string txt;
            typetopic mykindOfTopic = topics.FirstOrDefault(x => x.Value == topic).Key;
            switch (mykindOfTopic)
            {
                case typetopic.texte:
                    txt = Encoding.Default.GetString(arg.ApplicationMessage.Payload);
                    AddToMessageList(txt);
                    //this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    //{
                    //    string txt = Encoding.Default.GetString(arg.ApplicationMessage.Payload);
                    //    TextBlock tb = new TextBlock() { Text = txt };
                    //    _messages_recus.Add(tb);
                    //}, null);
                    break;

                case typetopic.booleen:
                    txt = Encoding.Default.GetString(arg.ApplicationMessage.Payload);
                    AddToMessageList(txt);
                    break;

                case typetopic.entier:
                    txt = Encoding.Default.GetString(arg.ApplicationMessage.Payload);
                    AddToMessageList(txt);
                    break;

                case typetopic.virgule:
                    txt = Encoding.Default.GetString(arg.ApplicationMessage.Payload);
                    AddToMessageList(txt);
                    break;

                case typetopic.image:
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        //réception d'un byte array d'une BitmapImage
                        _bmp = ToImage(arg.ApplicationMessage.Payload);

                        //using (var ms = new MemoryStream(arg.ApplicationMessage.Payload))
                        //{




                        //    ExifData d = new ExifData(ms);

                        //    StringBuilder sb = new StringBuilder(200000);
                        //    PrintIfdData(sb, ExifIfd.PrimaryData, d);
                        //    PrintIfdData(sb, ExifIfd.PrivateData, d);
                        //    PrintIfdData(sb, ExifIfd.GpsInfoData, d);
                        //    PrintIfdData(sb, ExifIfd.Interoperability, d);
                        //    PrintIfdData(sb, ExifIfd.ThumbnailData, d);

                        //    string lbll = sb.ToString();
                        //    lbll = lbll;
                        //}


                    }, null);
                    break;

                case typetopic.fluximages_webcam:
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        _bmp_webcam = ToImage(arg.ApplicationMessage.Payload);
                    }, null);
                    break;

                case typetopic.fluximages_folder:
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        _bmp_folder = ToImage(arg.ApplicationMessage.Payload);
                    }, null);
                    break;

                case typetopic.geoimage:
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        //réception d'un byte array d'une BitmapImage
                        GeoImage geoImage = GeoImage.FromJson(arg.ApplicationMessage.Payload);
                        _bmp = ToImage(geoImage.ImageData);
                        string txt = geoImage.time + "\n" +
                                     geoImage.dateTime.ToString("yyyy/MM/dd HH:mm:ss.fff") + "\n" +
                                     geoImage.counterLatch + "\n" +
                                     geoImage.counterwindow_begin + "\n" +
                                     geoImage.counterwindow_end + "\n" +
                                     "SIZE = " + geoImage.ImageData.Length;
                        ;
                        //TextBlock tb = new TextBlock() { Text = txt };
                        //_messages_recus.Add(tb);

                        AddToMessageList(txt);

                    }, null);
                    break;

                case typetopic.vector3:
                    float[] vector3 = GetVector3FromByteArray(arg.ApplicationMessage.Payload);
                    txt = "(" + vector3[0] + " ; " + vector3[1] + " ; " + vector3[2] + ")";
                    AddToMessageList(txt);
                    break;

                default:
                    Microsoft.VisualBasic.Interaction.MsgBox("TODO\n" + mykindOfTopic.ToString());

                    break;
            }
        }

        float[] GetVector3FromByteArray(byte[] payload)
        {
            float[] _valeur = new float[3];
            byte[] x_b = new byte[4];
            byte[] y_b = new byte[4];
            byte[] z_b = new byte[4];
            try
            {
                //float : 4 octets
                Array.Copy(payload, 0, x_b, 0, 4);
                Array.Copy(payload, 4, y_b, 0, 4);
                Array.Copy(payload, 8, z_b, 0, 4);

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











        private void PrintIfdData(StringBuilder sb, ExifIfd Ifd, ExifData d)
        {
            const int MaxContentLength = 35; // Maximum character count for the content length
            const int MaxRawDataOutputCount = 40; // Maximum number of bytes for the raw data output
            ExifTagType TagType;
            ExifTag TagSpec;
            int ValueCount, TagDataIndex, TagDataByteCount;
            byte[] TagData;
            ExifTagId TagId;

            sb.Append("--- IFD ");
            sb.Append(Ifd.ToString());
            sb.Append(" ---\n");
            bool HeaderPrinted = false;
            d.InitTagEnumeration(Ifd);
            while (d.EnumerateNextTag(out TagSpec))
            {
                if (!HeaderPrinted)
                {
                    sb.Append("Name                       ID      Type        Value   Byte   ");
                    AlignedAppend(sb, "Content", MaxContentLength + 2);
                    sb.Append("Raw data\n");
                    sb.Append("                                               count   count\n");
                    HeaderPrinted = true;
                }

                d.GetTagRawData(TagSpec, out TagType, out ValueCount, out TagData, out TagDataIndex);
                AlignedAppend(sb, GetExifTagName(TagSpec), 27);

                TagId = ExifData.ExtractTagId(TagSpec);
                sb.Append("0x");
                sb.Append(((ushort)TagId).ToString("X4"));
                sb.Append("  ");

                AlignedAppend(sb, TagType.ToString(), 9);
                sb.Append("  ");
                AlignedAppend(sb, ValueCount.ToString(), 6, true);
                sb.Append("  ");

                TagDataByteCount = ExifData.GetTagByteCount(TagType, ValueCount);
                AlignedAppend(sb, TagDataByteCount.ToString("D"), 6, true);
                sb.Append("  ");

                AppendInterpretedContent(sb, d, TagSpec, TagType, MaxContentLength);
                sb.Append("  ");

                int k = TagDataByteCount;
                if (k > MaxRawDataOutputCount) k = MaxRawDataOutputCount;
                for (int i = 0; i < k; i++)
                {
                    sb.Append(TagData[TagDataIndex + i].ToString("X2"));
                    sb.Append(" ");
                }
                if (k < TagDataByteCount) sb.Append("…");
                sb.Append("\n");
            }
            sb.Append("\n");
        }


        private string GetExifTagName(ExifTag TagSpec)
        {
            string s = TagSpec.ToString();
            if ((s.Length > 0) && (s[0] >= '0') && (s[0] <= '9'))
            {
                s = "???"; // If the name starts with a digit there is no name defined in the enum type "ExifTag"
            }
            return (s);
        }


        private void AppendInterpretedContent(StringBuilder sb, ExifData d, ExifTag TagSpec, ExifTagType TagType, int Length)
        {
            string s = "";

            try
            {
                if (TagType == ExifTagType.Ascii)
                {
                    if (!d.GetTagValue(TagSpec, out s, StrCoding.Utf8)) s = "???";
                }
                else if ((TagType == ExifTagType.Byte) && ((TagSpec == ExifTag.XpTitle) || (TagSpec == ExifTag.XpComment) || (TagSpec == ExifTag.XpAuthor) ||
                         (TagSpec == ExifTag.XpKeywords) || (TagSpec == ExifTag.XpSubject)))
                {
                    if (!d.GetTagValue(TagSpec, out s, StrCoding.Utf16Le_Byte)) s = "???";
                }
                else if ((TagType == ExifTagType.Undefined) && (TagSpec == ExifTag.UserComment))
                {
                    if (!d.GetTagValue(TagSpec, out s, StrCoding.IdCode_Utf16)) s = "???";
                }
                else if ((TagType == ExifTagType.Undefined) && ((TagSpec == ExifTag.ExifVersion) || (TagSpec == ExifTag.FlashPixVersion) ||
                         (TagSpec == ExifTag.InteroperabilityVersion)))
                {
                    if (!d.GetTagValue(TagSpec, out s, StrCoding.UsAscii_Undef)) s = "???";
                }
                else if ((TagType == ExifTagType.Undefined) && ((TagSpec == ExifTag.SceneType) || (TagSpec == ExifTag.FileSource)))
                {
                    d.GetTagRawData(TagSpec, out _, out _, out byte[] RawData);
                    if (RawData.Length > 0) s += RawData[0].ToString();
                }
                else if ((TagType == ExifTagType.Byte) || (TagType == ExifTagType.UShort) || (TagType == ExifTagType.ULong))
                {
                    d.GetTagValueCount(TagSpec, out int k);
                    for (int i = 0; i < k; i++)
                    {
                        d.GetTagValue(TagSpec, out uint v, i);
                        if (i > 0) s += ", ";
                        s += v.ToString();
                    }
                }
                else if (TagType == ExifTagType.SLong)
                {
                    d.GetTagValueCount(TagSpec, out int k);
                    for (int i = 0; i < k; i++)
                    {
                        d.GetTagValue(TagSpec, out int v, i);
                        if (i > 0) s += ", ";
                        s += v.ToString();
                    }
                }
                else if ((TagType == ExifTagType.SRational) || (TagType == ExifTagType.URational))
                {
                    d.GetTagValueCount(TagSpec, out int k);
                    for (int i = 0; i < k; i++)
                    {
                        d.GetTagValue(TagSpec, out ExifRational v, i);
                        if (i > 0) s += ", ";
                        s += v.ToString();
                    }
                }
            }
            catch
            {
                s = "!Error!";
            }
            s = s.Replace('\r', ' ');
            s = s.Replace('\n', ' ');
            AlignedAppend(sb, s, Length);
        }


        private void AlignedAppend(StringBuilder sb, string s, int CharCount, bool RightAlign = false)
        {
            if (s.Length <= CharCount)
            {
                int SpaceCount = CharCount - s.Length;
                if (RightAlign) sb.Append(' ', SpaceCount);
                sb.Append(s);
                if (!RightAlign) sb.Append(' ', SpaceCount);
            }
            else
            {
                sb.Append(s.Substring(0, CharCount - 1));
                sb.Append('…');
            }
        }
    }
}
