using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CompactExifLib;
using MQTTnet;
using OpenCvSharp;

namespace MQTT_Publisher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        MQTTnet.Client.IMqttClient mqttClient;
        string IP = "127.0.0.1";
        int port = 1883;

        enum typetopic { texte, booleen, entier, virgule, image, fluximages_webcam, fluximages_folder, geoimage };
        Dictionary<typetopic, string> topics;

        bool webcam_running;
        bool folder_running;
        System.Threading.Thread flux_webcam_Thread;
        System.Threading.Thread flux_folder_Thread;

        long counterfake = 123456;

        #region Window MANAGEMENT
        public MainWindow()
        {
            InitializeComponent();
            INIT_topics();
        }

        private void Connect_Click(object sender, MouseButtonEventArgs e)
        {
            MQTTClient_Init();
            btn_disconnect.Visibility = Visibility.Visible;
        }

        private void Disconnect_Click(object sender, MouseButtonEventArgs e)
        {
            MQTTClient_Stop();
            btn_disconnect.Visibility = Visibility.Hidden;
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MQTTClient_Stop();
            folder_running = false;
            flux_folder_Thread?.Abort();
        }

        void Publish_texte_Click(object sender, RoutedEventArgs e) { Publish_texte(); }
        void Publish_bool_Click(object sender, RoutedEventArgs e) { Publish_bool(); }
        void Publish_entier_Click(object sender, RoutedEventArgs e) { Publish_entier(); }
        void Publish_virgule_Click(object sender, RoutedEventArgs e) { Publish_virgule(); }
        void Publish_image_Click(object sender, RoutedEventArgs e) { Publish_image(); }
        void Publish_fluximage_webcam_Click(object sender, RoutedEventArgs e) { Publish_fluximage_webcam(); }
        void Publish_fluximage_folder_Click(object sender, RoutedEventArgs e) { Publish_fluximage_folder(); }
        void Publish_all_Click(object sender, RoutedEventArgs e) { Publish_all(); }

        private void Publish_geoimage_Click(object sender, RoutedEventArgs e) { Publish_geoimage(); }
        #endregion

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
        }

        void Publish_texte() { MQTT_Publish(topics[typetopic.texte], tbx_text.Text); }
        void Publish_bool() { MQTT_Publish(topics[typetopic.booleen], ckb_booleen.IsChecked == true ? "true" : "false"); }
        void Publish_entier() { MQTT_Publish(topics[typetopic.entier], tbx_entier.Text); }
        void Publish_virgule() { MQTT_Publish(topics[typetopic.virgule], tbx_virgule.Text); }
        void Publish_image()
        {


            //////METADATA WRITE & READ

            //ExifData d0 = new ExifData(tbx_image.Text);

            //d0.SetTagValue(ExifTag.Artist, "JJO Test : 0", StrCoding.Utf8); //"Auteurs"
            //d0.SetTagValue(ExifTag.DateTimeOriginal, new DateTime(1980, 10, 29)); //"Prise de vue"
            //d0.SetTagValue(ExifTag.ImageDescription, "JJO Test : 3", StrCoding.Utf8); //"Objet"
            //d0.SetTagValue(ExifTag.Model, "JJO Test : 6", StrCoding.Utf8); //"Modèle d'appareil photo"
            //d0.SetTagValue(ExifTag.UserComment, "JJO Test : 8", StrCoding.Utf8); //"Commentaires"
            //d0.Save();


            //StringBuilder sb0 = new StringBuilder(200000);
            //PrintIfdData(sb0, ExifIfd.PrimaryData, d0);
            //PrintIfdData(sb0, ExifIfd.PrivateData, d0);
            //PrintIfdData(sb0, ExifIfd.GpsInfoData, d0);
            //PrintIfdData(sb0, ExifIfd.Interoperability, d0);
            //PrintIfdData(sb0, ExifIfd.ThumbnailData, d0);

            //string lbllé = sb0.ToString();

            byte[] data;
            Mat mat = new Mat(tbx_image.Text);
            Cv2.ImEncode(".jpg", mat, out data);




            ////////////byte[] imageBytes = m.ToArray();
            /////////            string base64String = Convert.ToBase64String(data);


            //var uri = new Uri(tbx_image.Text);
            //BitmapImage bitmapImage = new BitmapImage(uri);


            ////MemoryStream memoryStream = new MemoryStream(data);
            ////ImageType SourceImageType = ExifData.CheckStreamTypeAndCompatibility(memoryStream);
            ////ExifData d = new ExifData(memoryStream);
            ////d.SetTagValue(ExifTag.Artist, "JJO Test : 0", StrCoding.Utf8); //"Auteurs"
            ////d.SetTagValue(ExifTag.DateTimeOriginal, new DateTime(1980, 10, 29)); //"Prise de vue"
            ////d.SetTagValue(ExifTag.ImageDescription, "JJO Test : 3", StrCoding.Utf8); //"Objet"
            ////d.SetTagValue(ExifTag.Model, "JJO Test : 6", StrCoding.Utf8); //"Modèle d'appareil photo"
            ////d.SetTagValue(ExifTag.UserComment, "JJO Test : 8", StrCoding.Utf8); //"Commentaires"
            ////ImageType SourceImageType2 = ExifData.CheckStreamTypeAndCompatibility(memoryStream);
            ////MemoryStream memoryStream2 = new MemoryStream();
            ////d.Save(memoryStream, memoryStream2);
            ////data = memoryStream2.ToArray();


            //StringBuilder sb = new StringBuilder(200000);
            //PrintIfdData(sb, ExifIfd.PrimaryData, d);
            //PrintIfdData(sb, ExifIfd.PrivateData, d);
            //PrintIfdData(sb, ExifIfd.GpsInfoData, d);
            //PrintIfdData(sb, ExifIfd.Interoperability, d);
            //PrintIfdData(sb, ExifIfd.ThumbnailData, d);

            //string lbll = sb.ToString();



            //MemoryStream memoryStream = null;
            //BinaryReader binaryReader = new BinaryReader(fileStream);
            //memoryStream = new MemoryStream(binaryReader.ReadBytes((int)fileStream.Length));
            //binaryReader.Close();


            ////////////byte[] imageBytes = m.ToArray();
            ////////////string base64String = Convert.ToBase64String(imageBytes);





            //JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            //encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    encoder.Save(ms);
            //    data = ms.ToArray();
            //}

            //Mat mat = Mat.FromImageData(data);
            //Cv2.ImShow("e", mat);
            //Cv2.WaitKey(1);

            MQTT_Publish(topics[typetopic.image], data);
        }

        void Publish_geoimage()
        {
            byte[] data;
            Mat mat = new Mat(tbx_geoimage.Text);
            Cv2.ImEncode(".jpg", mat, out data);

            counterfake += 1000;

            GeoImage geoImage = new GeoImage()
            {
                ImageData = data,
                dateTime = DateTime.Now,
                counterLatch = counterfake,
                time = System.Diagnostics.Stopwatch.GetTimestamp()
            };
            byte[] jsondata = geoImage.GetJsonUTF8();

            MQTT_Publish(topics[typetopic.geoimage], jsondata);
        }

        #region WEBCAM
        void Publish_fluximage_webcam()
        {
            flux_webcam_Thread = new System.Threading.Thread(Webcam_Thread);
            flux_webcam_Thread.Start();
        }
        private void WebCamStop()
        {
            webcam_running = false;
            flux_webcam_Thread?.Abort();
        }

        private void Webcam_Thread(object? obj)
        {
            webcam_running = true;

            VideoCapture _videoCapture = new VideoCapture(0);
            _videoCapture.Open(0);

            _videoCapture.FrameWidth = 640;
            _videoCapture.FrameHeight = 480;
            _videoCapture.Fps = 30;
            Mat frame = new Mat();
            while (webcam_running)
            {
                _videoCapture.Read(frame);

                //supersize pour avoir pareil que Porik
                //Cv2.Resize(frame, frame, new OpenCvSharp.Size(2364, 2056));

                //conversion en JPEG
                Cv2.ImEncode(".jpg", frame, out byte[] data);

                //var stream = new MemoryStream(data);

                //ecriture dans metadatas
                #region bob
                //ExifData d = new ExifData(stream);
                //d.InitTagEnumeration(ExifIfd.PrimaryData);
                //ExifTagType TagType;
                //ExifTag TagSpec;
                //int ValueCount, TagDataIndex, TagDataByteCount;
                //ExifTagId TagId;
                //byte[] TagData;

                //while (d.EnumerateNextTag(out TagSpec))
                //{
                //    d.GetTagRawData(TagSpec, out TagType, out ValueCount, out TagData, out TagDataIndex);
                //    string tagname = GetExifTagName(TagSpec);

                //    TagId = ExifData.ExtractTagId(TagSpec);
                //    string stringtagid = ((ushort)TagId).ToString("X4");
                //    string tagType = TagType.ToString();
                //    TagDataByteCount = ExifData.GetTagByteCount(TagType, ValueCount);
                //   string stringTagDataByteCount =TagDataByteCount.ToString("D");

                //    AppendInterpretedContent(sb, d, TagSpec, TagType, MaxContentLength);
                //    sb.Append("  ");

                //    int k = TagDataByteCount;
                //    if (k > MaxRawDataOutputCount) k = MaxRawDataOutputCount;
                //    for (int i = 0; i < k; i++)
                //    {
                //        sb.Append(TagData[TagDataIndex + i].ToString("X2"));
                //        sb.Append(" ");
                //    }
                //    if (k < TagDataByteCount) sb.Append("…");
                //    sb.Append("\n");
                //}
                #endregion
                //ExifData d = new ExifData(stream);

                //StringBuilder sb = new StringBuilder(200000);

                //PrintIfdData(sb, ExifIfd.PrimaryData, d);
                //PrintIfdData(sb, ExifIfd.PrivateData, d);
                //PrintIfdData(sb, ExifIfd.GpsInfoData, d);
                //PrintIfdData(sb, ExifIfd.Interoperability, d);
                //PrintIfdData(sb, ExifIfd.ThumbnailData, d);

                //string lbll = sb.ToString();

                //                byte[] data = frame.ToBytes();
                MQTT_Publish(topics[typetopic.fluximages_webcam], data);
                System.Threading.Thread.Sleep(50);
            }
        }
        #endregion

        void Publish_fluximage_folder()
        {
            flux_folder_Thread = new System.Threading.Thread(Folder_Thread);
            flux_folder_Thread.Start();
        }

        private void Folder_Thread(object? obj)
        {
            folder_running = true;

            //VideoCapture _videoCapture = new VideoCapture(0);
            //_videoCapture.Open(0);

            //_videoCapture.FrameWidth = 640;
            //_videoCapture.FrameHeight = 480;
            //_videoCapture.Fps = 30;
            //Mat frame = new Mat();
            //while (folder_running)
            //{
            //    _videoCapture.Read(frame);
            //    byte[] data = frame.ToBytes();
            //    MQTT_Publish(topics[typetopic.fluximages_folder], data);
            //}

        }

        void Publish_all()
        {
            Publish_texte();
            Publish_bool();
            Publish_entier();
            Publish_virgule();
            Publish_image();
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

            // mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;

            mqttClient.ConnectingAsync += MqttClient_ConnectingAsync;
            mqttClient.ConnectedAsync += MqttClient_ConnectedAsync;
            mqttClient.DisconnectedAsync += MqttClient_DisconnectedAsync;
            mqttClient.ConnectAsync(mqttClientOptions, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
        }

        void MQTTClient_Disconnect()
        {
            mqttClient.ConnectingAsync -= MqttClient_ConnectingAsync;
            mqttClient.ConnectedAsync -= MqttClient_ConnectedAsync;
            mqttClient.DisconnectedAsync -= MqttClient_DisconnectedAsync;
            mqttClient.DisconnectAsync(new MQTTnet.Client.MqttClientDisconnectOptionsBuilder().WithReason(MQTTnet.Client.MqttClientDisconnectReason.NormalDisconnection).Build());
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
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        void MQTTClient_Stop()
        {
            WebCamStop();
            System.Threading.Thread.Sleep(50);
            MQTTClient_Disconnect();
        }
        #endregion










        #region métadata image JPG



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
        #endregion












        void MQTT_Publish(string topic, byte[] payload)
        {
            MqttApplicationMessage applicationMessage = new MqttApplicationMessageBuilder()
                                                            .WithTopic(topic)
                                                            .WithPayload(payload)
                                                            .Build();

            mqttClient.PublishAsync(applicationMessage, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
        }
        void MQTT_Publish(string topic, Stream payload)
        {
            MqttApplicationMessage applicationMessage = new MqttApplicationMessageBuilder()
                                                            .WithTopic(topic)
                                                            .WithPayload(payload)
                                                            .Build();

            mqttClient.PublishAsync(applicationMessage, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
        }
        void MQTT_Publish(string topic, string payload)
        {
            MqttApplicationMessage applicationMessage = new MqttApplicationMessageBuilder()
                                                            .WithTopic(topic)
                                                            .WithPayload(payload)
                                                            .Build();

            mqttClient.PublishAsync(applicationMessage, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}