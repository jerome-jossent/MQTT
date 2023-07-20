using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        enum typetopic { all, texte, booleen, entier, virgule, image, fluximages_webcam, fluximages_folder, geoimage, vector3 };
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


        private void Publish_vector3_Click(object sender, RoutedEventArgs e) { Publish_vector3(); }
        #endregion

        void INIT_topics()
        {
            topics = new Dictionary<typetopic, string>();
            topics.Add(typetopic.texte, "test\\string");
            topics.Add(typetopic.booleen, "test\\bool");
            topics.Add(typetopic.entier, "test\\int");
            topics.Add(typetopic.virgule, "test\\float");
            topics.Add(typetopic.image, "test\\image");
            topics.Add(typetopic.fluximages_webcam, "fluximages_webcam");
            topics.Add(typetopic.fluximages_folder, "fluximages_folder");
            topics.Add(typetopic.geoimage, "geoimage");
            topics.Add(typetopic.vector3, "test\\vector3");
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
        }

        void Publish_image()
        {
            byte[] data;
            tbx_image.Text = tbx_image.Text.Replace("\"", "");
            Mat mat = new Mat(tbx_image.Text);
            if (mat.Empty())
            {
                tbx_image.Background = Brushes.Red;
            }
            else
            {
                tbx_image.Background = Brushes.White;
                Cv2.ImEncode(".jpg", mat, out data);
                MQTT_Publish(topics[typetopic.image], data, ckx_image.IsChecked == true);
            }
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

        void Publish_vector3(float x, float y, float z)
        {
            byte[] data = CombomBinaryArray(BitConverter.GetBytes(x), BitConverter.GetBytes(y), BitConverter.GetBytes(z));
            MQTT_Publish(topics[typetopic.vector3], data, ckx_vector3.IsChecked == true);
        }
        void Publish_vector3()
        {
            float x = float.Parse(tbx_vector3_x.Text.Replace(".", ","));
            float y = float.Parse(tbx_vector3_y.Text.Replace(".", ","));
            float z = float.Parse(tbx_vector3_z.Text.Replace(".", ","));
            Publish_vector3(x, y, z);
        }

        void Publish_vector4(string topic, float x, float y, float z, float w, bool retain = false)
        {
            byte[] data = CombomBinaryArray(BitConverter.GetBytes(x), BitConverter.GetBytes(y), BitConverter.GetBytes(z), BitConverter.GetBytes(w));
            MQTT_Publish(topic, data, retain);
        }

        private byte[] CombomBinaryArray(byte[] srcArray1, byte[] srcArray2)
        {
            //Create a new array based on the total number of two array elements to be merged
            byte[] newArray = new byte[srcArray1.Length + srcArray2.Length];

            //Copy the first array to the newly created array
            Array.Copy(srcArray1, 0, newArray, 0, srcArray1.Length);

            //Copy the second array to the newly created array
            Array.Copy(srcArray2, 0, newArray, srcArray1.Length, srcArray2.Length);

            return newArray;
        }

        private byte[] CombomBinaryArray(byte[] srcArray1, byte[] srcArray2, byte[] srcArray3)
        {
            //Create a new array based on the total number of two array elements to be merged
            byte[] newArray = new byte[srcArray1.Length + srcArray2.Length + srcArray3.Length];

            //Copy the first array to the newly created array
            Array.Copy(srcArray1, 0, newArray, 0, srcArray1.Length);
            //Copy the second array to the newly created array
            Array.Copy(srcArray2, 0, newArray, srcArray1.Length, srcArray2.Length);
            //Copy the third array to the newly created array
            Array.Copy(srcArray3, 0, newArray, srcArray1.Length + srcArray2.Length, srcArray3.Length);

            return newArray;
        }

        private byte[] CombomBinaryArray(byte[] srcArray1, byte[] srcArray2, byte[] srcArray3, byte[] srcArray4)
        {
            //Create a new array based on the total number of two array elements to be merged
            byte[] newArray = new byte[srcArray1.Length + srcArray2.Length + srcArray3.Length + srcArray4.Length];

            //Copy the first array to the newly created array
            Array.Copy(srcArray1, 0, newArray, 0, srcArray1.Length);
            //Copy the second array to the newly created array
            Array.Copy(srcArray2, 0, newArray, srcArray1.Length, srcArray2.Length);
            //Copy the third array to the newly created array
            Array.Copy(srcArray3, 0, newArray, srcArray1.Length + srcArray2.Length, srcArray3.Length);

            Array.Copy(srcArray4, 0, newArray, srcArray1.Length + srcArray2.Length + srcArray3.Length, srcArray4.Length);

            return newArray;
        }

        private void xy_move(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var xy_point = e.GetPosition(xy);

                float x = (float)(xy_point.X / xy.Width) * 2 - 1;
                float y = 1 - (float)(xy_point.Y / xy.Height) * 2;

                tbx_vector3_x.Text = x.ToString();
                tbx_vector3_y.Text = y.ToString();

                Publish_vector3(x, y, float.Parse(tbx_vector3_z.Text));
            }
        }

        private void xz_move(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var xz_point = e.GetPosition(xz);

                float x = (float)(xz_point.X / xy.Width) * 2 - 1;
                float z = 1 - (float)(xz_point.Y / xy.Height) * 2;

                tbx_vector3_x.Text = x.ToString();
                tbx_vector3_z.Text = z.ToString();

                Publish_vector3(x, float.Parse(tbx_vector3_y.Text), z);
            }
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
                cts?.Cancel();
                cts?.Dispose();
            }
        }

        void Webcam_Thread()
        {
            webcam_running = true;

            int indexdevice = 0;

            VideoCapture _videoCapture = new VideoCapture(indexdevice);
            _videoCapture.Open(indexdevice);

            //_videoCapture.FrameWidth = 1920;
            //_videoCapture.FrameHeight = 1080;
            //_videoCapture.Fps = 30;
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
                MQTT_Publish(topics[typetopic.fluximages_webcam], data);//, ckx_webcam.IsChecked == true);

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
            if (_cbx_ips.Items.Count > 0)
                _cbx_ips.SelectedIndex = 0;
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

        //private void Slider_crop_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    if (crop_left == null || crop_right == null || crop_top == null || crop_bottom == null) return;
        //    if (tbk_crop_l == null || tbk_crop_r == null || tbk_crop_t == null || tbk_crop_b == null) return;

        //    float l = (float)crop_left.Value;
        //    float t = (float)crop_top.Value;
        //    float b = (float)crop_bottom.Value;
        //    float r = (float)crop_right.Value;

        //    Publish_vector4("crop", l, t, r, b);

        //    tbk_crop_l.Text = l.ToString();
        //    tbk_crop_t.Text = t.ToString();
        //    tbk_crop_b.Text = b.ToString();
        //    tbk_crop_r.Text = r.ToString();

        //    //Thickness margin = new Thickness(crop_left.Value, crop_top.Value, crop_right.Value, crop_bottom.Value);
        //    //string margin_json = Newtonsoft.Json.JsonConvert.SerializeObject(margin);
        //    //MQTT_Publish("crop", margin_json);
        //}

        private void Slider_crop_value_changed(object sender, RoutedEventArgs e)
        {
            if (crop_left_right == null || crop_bottom_top==null) return;
            if (tbk_crop_lr == null || tbk_crop_bt == null ) return;

            float l = (float)crop_left_right.LowerValue;
            float r = (float)crop_left_right.HigherValue;
            float b = (float)crop_bottom_top.HigherValue;
            float t = (float)crop_bottom_top.LowerValue;

            Publish_vector4("crop", l, t, r, b);

            tbk_crop_lr.Text = l.ToString("0.###") + ";" + r.ToString("0.###");
            tbk_crop_bt.Text = b.ToString("0.###") + ";" + t.ToString("0.###");
        }
    }
}