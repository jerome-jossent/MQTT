using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CompactExifLib;
using MQTT_Publisher_OpenCV.Properties;
using MQTTnet;
using OpenCvSharp;

namespace MQTT_Publisher
{
    public partial class MainWindow : System.Windows.Window
    {
        MQTTnet.Client.IMqttClient mqttClient;

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
            Fill_cbx_ips();
            INIT_topics();
        }

        void Connect_Click(object sender, MouseButtonEventArgs e)
        {
            if (MQTTClient_Init())
                btn_disconnect.Visibility = Visibility.Visible;
        }

        void Disconnect_Click(object sender, MouseButtonEventArgs e)
        {
            MQTTClient_Stop();
            btn_disconnect.Visibility = Visibility.Hidden;
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WebCamStop();
            MQTTClient_Stop();
            folder_running = false;
            flux_folder_Thread?.Abort();
        }

        void Publish_texte_Click(object sender, RoutedEventArgs e) { Publish_texte(); }
        void Publish_bool_Click(object sender, RoutedEventArgs e) { Publish_bool(); }
        void Publish_entier_Click(object sender, RoutedEventArgs e) { Publish_entier(); }
        void Publish_virgule_Click(object sender, RoutedEventArgs e) { Publish_virgule(); }
        void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { Publish_virgule_slider(e.NewValue); }

        void Publish_image_Click(object sender, RoutedEventArgs e) { Publish_image(); }
        void Publish_imageBIS_Click(object sender, RoutedEventArgs e) { Publish_imageBIS(); }

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

        void Publish_texte() { MQTT_Publish(topics[typetopic.texte], tbx_text.Text, ckx_text.IsChecked == true); }
        void Publish_bool() { MQTT_Publish(topics[typetopic.booleen], ckb_booleen.IsChecked == true ? "true" : "false", ckx_booleen.IsChecked == true); }
        void Publish_entier() { MQTT_Publish(topics[typetopic.entier], tbx_entier.Text, ckx_entier.IsChecked == true); }
        void Publish_virgule() { MQTT_Publish(topics[typetopic.virgule], tbx_virgule.Text, ckx_virgule.IsChecked == true); }
        void Publish_virgule_slider(double newValue)
        {
            if (!this.IsLoaded) return;
            tbx_virgule.Text = newValue.ToString("0.00");
            Publish_virgule();
            //MQTT_Publish(topics[typetopic.virgule], tbx_virgule.Text, ckx_virgule.IsChecked == true);
        }

        void Publish_image()
        {
            byte[] data;
            Mat mat = new Mat(tbx_image.Text);
            Cv2.ImEncode(".jpg", mat, out data);
            MQTT_Publish(topics[typetopic.image], data, ckx_image.IsChecked == true);
        }

        void Publish_imageBIS()
        {
            byte[] data;
            Mat mat = new Mat(tbx_imageBIS.Text);
            Cv2.ImEncode(".jpg", mat, out data);
            MQTT_Publish(topics[typetopic.image], data, ckx_imageBIS.IsChecked == true);
        }

        void Publish_geoimage()
        {
            byte[] data;
            Mat mat = new Mat(tbx_geoimage.Text);
            if (mat.Empty())
            {
                tbx_geoimage.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            tbx_geoimage.Foreground = new SolidColorBrush(Colors.Black);
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
        // Define the cancellation token.
        CancellationTokenSource cts;

        void Publish_fluximage_webcam()
        {
            cts = new CancellationTokenSource();

            Task task = new Task(() => { Webcam_Thread(); }, cts.Token);
            task.Start();
        }

        void WebCamStop()
        {
            if (cts != null)
            {
                webcam_running = false;
                cts.Cancel();
                cts.Dispose();
            }
        }

        void Webcam_Thread()
        {
            webcam_running = true;

            VideoCapture _videoCapture = new VideoCapture(0);
            _videoCapture.Open(0);

            _videoCapture.FrameWidth = 1920;
            _videoCapture.FrameHeight = 1080;
            _videoCapture.Fps = 30;
            Mat frame = new Mat();

            DateTime t = DateTime.Now;
            TimeSpan dt = TimeSpan.FromSeconds(1);
            t += dt;
            int nbframe = 0;

            while (webcam_running)
            {
                //capture de l'image
                _videoCapture.Read(frame);

                //conversion en JPEG puis en byte array
                Cv2.ImEncode(".jpg", frame, out byte[] data);

                //publie l'image
                MQTT_Publish(topics[typetopic.fluximages_webcam], data, ckx_webcam.IsChecked == true);

                //mesure fps
                nbframe++;
                if (DateTime.Now > t)
                {
                    string titre = nbframe + " fps";
                    Dispatcher.BeginInvoke(() => Title = titre);
                    nbframe = 0;
                    t += dt;
                }
            }
        }
        #endregion

        void Publish_fluximage_folder()
        {
            flux_folder_Thread = new System.Threading.Thread(Folder_Thread);
            flux_folder_Thread.Start();
        }

        void Folder_Thread(object? obj)
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
            //    MQTT_Publish(topics[typetopic.fluximages_folder], data, ckx_fluximage_folder.IsChecked == true);
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

        //Projet/Propriétés/Paramètres/ liens / nom+type
        //Projet/Régénérer
        //==>            Settings.Default["IPs"] = s;
        //==>            Settings.Default.Save();
        void Fill_cbx_ips()
        {
            string Z = Settings.Default["IPs"].ToString();
            string[] z = Z.ToString().Split(";", options: StringSplitOptions.RemoveEmptyEntries);
            //z = new string[] { "127.0.0.1" };
            foreach (string item in z)
                _cbx_ips.Items.Add(item);
        }
        void Save_ips()
        {
            List<string> ips = new List<string>();
            foreach (var item in _cbx_ips.Items)
                ips.Add(item.ToString());

            if (!ips.Contains(_cbx_ips.Text))
                ips.Add(_cbx_ips.Text);

            string s = string.Join(";", ips);
            Settings.Default["IPs"] = s;
            Settings.Default.Save();
        }

        #region MQTT
        //https://github.com/dotnet/MQTTnet/blob/master/Samples/Client/Client_Subscribe_Samples.cs

        bool MQTTClient_Init()
        {
            var mqttFactory = new MqttFactory();
            mqttClient = mqttFactory.CreateMqttClient();
            string IP = _cbx_ips.Text;

            if (IP == "")
            {
                SetStatusConnection(Colors.Blue);
                return false;
            }

            Save_ips();
            int port = int.Parse(_tbx_port.Text);

            var mqttClientOptions = new MQTTnet.Client.MqttClientOptionsBuilder()
                .WithTcpServer(IP, port)
                .Build();

            // mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;

            mqttClient.ConnectingAsync += MqttClient_ConnectingAsync;
            mqttClient.ConnectedAsync += MqttClient_ConnectedAsync;
            mqttClient.DisconnectedAsync += MqttClient_DisconnectedAsync;
            mqttClient.ConnectAsync(mqttClientOptions, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
            return true;
        }

        void MQTTClient_Disconnect()
        {
            if (mqttClient == null) return;

            try
            {
                mqttClient.DisconnectAsync(new MQTTnet.Client.MqttClientDisconnectOptionsBuilder().WithReason(MQTTnet.Client.MqttClientDisconnectReason.NormalDisconnection).Build()).GetAwaiter().GetResult();
                mqttClient.ConnectingAsync -= MqttClient_ConnectingAsync;
                mqttClient.ConnectedAsync -= MqttClient_ConnectedAsync;
                mqttClient.DisconnectedAsync -= MqttClient_DisconnectedAsync;

                mqttClient.Dispose();
            }
            catch (Exception ex)
            {

            }
        }

        Task MqttClient_ConnectingAsync(MQTTnet.Client.MqttClientConnectingEventArgs arg)
        {
            SetStatusConnection(Colors.Orange);
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        Task MqttClient_ConnectedAsync(MQTTnet.Client.MqttClientConnectedEventArgs arg)
        {
            SetStatusConnection(Colors.Green);
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        Task MqttClient_DisconnectedAsync(MQTTnet.Client.MqttClientDisconnectedEventArgs arg)
        {
            SetStatusConnection(Colors.Red);
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        void SetStatusConnection(Color c)
        {
            Dispatcher.BeginInvoke(() => _ell_connection_status.Fill = new SolidColorBrush(c));
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








        void MQTT_Publish(string topic, byte[] payload, bool retain = false)
        {
            if (mqttClient == null || !mqttClient.IsConnected)
                if (!MQTTClient_Init()) return;

            MqttApplicationMessage applicationMessage = new MqttApplicationMessageBuilder()
                                                            .WithTopic(topic)
                                                            .WithPayload(payload)
                                                            .WithRetainFlag(retain)
                                                            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                                                            .Build();

            mqttClient.PublishAsync(applicationMessage, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
        }
        void MQTT_Publish(string topic, Stream payload, bool retain = false)
        {
            if (mqttClient == null || !mqttClient.IsConnected)
                if (!MQTTClient_Init()) return;

            MqttApplicationMessage applicationMessage = new MqttApplicationMessageBuilder()
                                                            .WithTopic(topic)
                                                            .WithPayload(payload)
                                                            .WithRetainFlag(retain)
                                                            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                                                            .Build();

            mqttClient.PublishAsync(applicationMessage, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
        }
        void MQTT_Publish(string topic, string payload, bool retain = false)
        {
            if (mqttClient == null || !mqttClient.IsConnected)
                if (!MQTTClient_Init()) return;

            MqttApplicationMessage applicationMessage = new MqttApplicationMessageBuilder()
                                                            .WithTopic(topic)
                                                            .WithPayload(payload)
                                                            .WithRetainFlag(retain)
                                                            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                                                            .Build();

            mqttClient.PublishAsync(applicationMessage, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}