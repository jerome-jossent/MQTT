using System;
using System.Collections.Generic;
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

using MQTTnet;
using OpenCvSharp;
using CompactExifLib;
using System.IO;
using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MQTT_Video_Publisher
{
    public partial class MainWindow : System.Windows.Window, INotifyPropertyChanged
    {
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public int nbframe
        {
            get
            {
                if (_videoCapture == null) return 0;
                return _videoCapture.PosFrames;
            }
            set
            {
                if (_videoCapture.PosFrames != value)
                {
                    _videoCapture.PosFrames = value;
                    OnPropertyChanged();
                }
            }
        }


        MQTTnet.Client.IMqttClient mqttClient;
        public string topic_FrameSended = "video/frame/data";
        Dictionary<string, Action<byte[]?>> topics_subscribed;

        CancellationTokenSource cts;
        bool nextframe;
        bool is_video_playing;
        bool is_video_pausing;
        //bool slider_video_mouse_changing_by_code;
        //int nbframe_wanted = -1;

        string _IP;
        int _port;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Fill_cbx_ips();
            INIT_topics();
        }

        void INIT_topics()
        {
            topics_subscribed = new Dictionary<string, Action<byte[]?>>();
            topics_subscribed["video/frame/next"] = NextFrame;
        }
        void NextFrame(byte[]? data)
        {
            nextframe = true;
        }

        #region IHM
        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MQTTClient_Stop();
            PlayingVideoStop();
        }

        void Connect_Click(object sender, MouseButtonEventArgs e)
        {
            _IP = _cbx_ips.Text;
            _port = int.Parse(_tbx_port.Text);
            if (MQTTClient_Init(_IP, _port))
            {
                Save_ips();
                btn_disconnect.Visibility = Visibility.Visible;
            }
        }

        void Disconnect_Click(object sender, MouseButtonEventArgs e)
        {
            MQTTClient_Stop();
            btn_disconnect.Visibility = Visibility.Hidden;
        }

        void btn_pickfile(object sender, MouseButtonEventArgs e)
        {
            SelectVideoFile();
        }

        void Publish_video_Click(object sender, RoutedEventArgs e)
        {
            Video_PlayAndPublish();
        }

        void btn_video_play_Click(object sender, MouseButtonEventArgs e)
        {
            is_video_pausing = false;
            if (!is_video_playing)
                Video_PlayAndPublish();
            btn_video_play.Visibility = Visibility.Hidden;
        }

        void btn_video_pause_Click(object sender, MouseButtonEventArgs e)
        {
            is_video_pausing = true;
            btn_video_play.Visibility = Visibility.Visible;
        }

        void btn_video_stop_Click(object sender, MouseButtonEventArgs e)
        {
            PlayingVideoStop();
        }


        private void slider_video_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _videoCapture.PosFrames = (int)slider_video.Value;
        }
        //void slider_video_valuechanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    if (slider_video_mouse_changing_by_code) return;
        //    is_video_pausing = true;
        //    nbframe_wanted = (int)slider_video.Value;
        //    is_video_pausing = false;
        //}
        #endregion

        #region IP & IHM
        void Fill_cbx_ips()
        {
            List<string> ips = Get_Saved_ips();
            _cbx_ips.Items.Clear();
            foreach (string ip in ips)
                _cbx_ips.Items.Add(ip);
            if (_cbx_ips.Items.Count > 0)
                _cbx_ips.SelectedIndex = 0;
        }

        void SelectedIP_Delete_Click(object sender, MouseButtonEventArgs e)
        {
            string val = _cbx_ips.Text;

            List<string> ips = Get_Saved_ips();
            if (ips.Contains(val))
                ips.Remove(val);
            Save_ips(ips);
            Fill_cbx_ips();
            if (_cbx_ips.Items.Count > 0)
                _cbx_ips.SelectedIndex = 0;
        }

        List<string> Get_Saved_ips()
        {
            string IP_all = Properties.Settings.Default["IPs"].ToString();
            string[] ips = IP_all.ToString().Split(";", options: StringSplitOptions.RemoveEmptyEntries);
            return ips.ToList();
        }

        void Save_ips()
        {
            List<string> ips = new List<string>();
            foreach (var item in _cbx_ips.Items)
                ips.Add(item.ToString());

            if (!ips.Contains(_cbx_ips.Text))
                ips.Add(_cbx_ips.Text);
            Save_ips(ips);
        }

        void Save_ips(List<string> ips)
        {
            string s = string.Join(";", ips);
            Properties.Settings.Default["IPs"] = s;
            Properties.Settings.Default.Save();
        }
        #endregion

        #region MQTT
        //https://github.com/dotnet/MQTTnet/blob/master/Samples/Client/Client_Subscribe_Samples.cs

        bool MQTTClient_Init(string IP, int port)
        {
            var mqttFactory = new MqttFactory();
            mqttClient = mqttFactory.CreateMqttClient();

            if (IP == "")
            {
                SetStatusConnection(Colors.Blue, "Enter a valid IP");
                return false;
            }

            var mqttClientOptions = new MQTTnet.Client.MqttClientOptionsBuilder()
                .WithTcpServer(IP, port)
                .Build();

            mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;

            mqttClient.ConnectingAsync += MqttClient_ConnectingAsync;
            mqttClient.ConnectedAsync += MqttClient_ConnectedAsync;
            mqttClient.DisconnectedAsync += MqttClient_DisconnectedAsync;

            try
            {
                mqttClient.ConnectAsync(mqttClientOptions, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                SetStatusConnection(Colors.Red, ex.Message);
                return false;
            }
            return true;
        }

        void MQTTClient_Disconnect()
        {
            if (mqttClient == null) return;

            try
            {
                mqttClient.DisconnectAsync(new MQTTnet.Client.MqttClientDisconnectOptionsBuilder().WithReason(MQTTnet.Client.MqttClientDisconnectOptionsReason.NormalDisconnection).Build()).GetAwaiter().GetResult();
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
            MQTTClient_Subscribes();
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        Task MqttClient_DisconnectedAsync(MQTTnet.Client.MqttClientDisconnectedEventArgs arg)
        {
            SetStatusConnection(Colors.Red);
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        void SetStatusConnection(Color c, string message = "")
        {
            Dispatcher.BeginInvoke(() =>
            {
                _ell_connection_status.ToolTip = message;
                _ell_connection_status.Fill = new SolidColorBrush(c);
            }
            );
        }

        void MQTTClient_Subscribes()
        {
            var mqtt_Subscriber = new MQTTnet.Client.MqttClientSubscribeOptions();
            foreach (var item in topics_subscribed)
            {
                var filter = new MQTTnet.Packets.MqttTopicFilter() { Topic = item.Key };
                mqtt_Subscriber.TopicFilters.Add(filter);
            }

            mqttClient.SubscribeAsync(mqtt_Subscriber, System.Threading.CancellationToken.None);
        }

        Task MqttClient_ApplicationMessageReceivedAsync(MQTTnet.Client.MqttApplicationMessageReceivedEventArgs arg)
        {
            string topic = arg.ApplicationMessage.Topic;
            if (topics_subscribed.ContainsKey(topic))
            {
                byte[]? data = arg.ApplicationMessage.PayloadSegment.Array;
                topics_subscribed[topic].DynamicInvoke(data);
            }
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        void MQTT_Publish(string topic, byte[] payload, bool retain = false)
        {
            if (mqttClient == null || !mqttClient.IsConnected)
                return;

            MqttApplicationMessage applicationMessage = new MqttApplicationMessageBuilder()
                                                            .WithTopic(topic)
                                                            .WithPayload(payload)
                                                            .WithRetainFlag(retain)
                                                            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                                                            .Build();

            mqttClient.PublishAsync(applicationMessage, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
        }

        void MQTTClient_Stop()
        {
            System.Threading.Thread.Sleep(50);
            MQTTClient_Disconnect();
        }
        #endregion

        void Video_PlayAndPublish()
        {
            Publish_videofile(tbx_videofile.Text);
        }

        void SelectVideoFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            System.IO.FileInfo fi = new FileInfo(tbx_videofile.Text);
            if (fi.Exists)
                openFileDialog.InitialDirectory = fi.DirectoryName;

            if (openFileDialog.ShowDialog() == true)
                tbx_videofile.Text = openFileDialog.FileName;
        }

        #region VIDEO FILE


        void Publish_videofile(string file)
        {
            cts = new CancellationTokenSource();
            Task task = new Task(() => { PlayVideo_Thread(file); }, cts.Token);
            task.Start();
        }

        void PlayingVideoStop()
        {
            if (cts != null && !cts.IsCancellationRequested)
            {
                if (is_video_pausing)
                    is_video_pausing = false;
                is_video_playing = false;
                cts.Cancel();
                cts.Dispose();
                UpdateFrames("- / -");
            }
        }
        VideoCapture _videoCapture;
        void PlayVideo_Thread(string file)
        {
            is_video_playing = true;


            //webcam
            //_videoCapture = new VideoCapture(0);
            //_videoCapture.Open(0);
            //_videoCapture.FrameWidth = 1920;
            //_videoCapture.FrameHeight = 1080;
            //_videoCapture.Fps = 30;

            //file
            _videoCapture = new VideoCapture(file);

            Mat frame = new Mat();

            DateTime t0 = DateTime.Now;
            nbframe = 0;

            double fps = _videoCapture.Fps;
            double periode_entre_frame_ms = 1000 / fps;

            Dispatcher.BeginInvoke(() =>
            {
                slider_video.Minimum = 0;
                slider_video.Maximum = _videoCapture.FrameCount;
            });

            //mesure du temps entre frame pour effectuer une lecture "temps réel" de la vidéo
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            while (is_video_playing && !cts.IsCancellationRequested)
            {
                while (is_video_pausing)
                    Thread.Sleep(10);

                watch.Restart();

                //_videoCapture.PosFrames = nbframe;
                //if (nbframe_wanted != -1)
                //{
                //    nbframe = nbframe_wanted;
                //    nbframe_wanted = -1;
                //}

                //capture de l'image
                _videoCapture.Read(frame);
                //nbframe++;

                //fin ou erreur ?
                if (frame.Empty())
                {
                    //if (slider_video_mouse_changing_by_code)
                    {
                        break;
                    }
                    //continue;
                }
                //Dispatcher.BeginInvoke(() =>
                //{
                //    slider_video_mouse_changing_by_code = true;
                //    slider_video.Value = nbframe;
                //    slider_video_mouse_changing_by_code = false;
                //});

                //conversion en JPEG puis en byte array
                Cv2.ImEncode(".jpg", frame, out byte[] data);

                //DateTime t = DateTime.Now;
                //ajout des metadatas
                string metadatas = "{" +
                    "frame=" + nbframe +
                    //", time_s=" + (t - t0).TotalSeconds.ToString("0.000").Replace(",", ".") +
                    ", time_s=" + (nbframe / _videoCapture.Fps).ToString("0.000").Replace(",", ".") +
                    "}";
                data = AddMetadatas(data, metadatas);

                //publie l'image
                MQTT_Publish(topic_FrameSended, data);

                //Mise à jour de l'ihm : affichage du nbr de frame
                UpdateFrames(nbframe + " / " + _videoCapture.FrameCount);

                //attente "temps réel"
                watch.Stop();
                double ms_to_wait = periode_entre_frame_ms - watch.ElapsedMilliseconds;
                if (ms_to_wait > 0)
                    Thread.Sleep((int)ms_to_wait);
            }
            PlayingVideoStop();
        }

        void UpdateFrames(string message)
        {
            Dispatcher.BeginInvoke(() =>
            {
                video_frames.Text = message;
            });
        }

        byte[] AddMetadatas(byte[] data, string metadatas)
        {
            MemoryStream imageStream = new MemoryStream(data);
            ExifData exif = new ExifData(imageStream);

            exif.SetTagValue(ExifTag.ImageDescription, metadatas, StrCoding.Utf8);

            MemoryStream newImageStream = new MemoryStream();
            exif.SaveJpeg(imageStream, newImageStream);
            return newImageStream.ToArray();
        }



        #endregion

        //#region métadata image JPG
        //void PrintIfdData(StringBuilder sb, ExifIfd Ifd, ExifData d)
        //{
        //    const int MaxContentLength = 35; // Maximum character count for the content length
        //    const int MaxRawDataOutputCount = 40; // Maximum number of bytes for the raw data output
        //    ExifTagType TagType;
        //    ExifTag TagSpec;
        //    int ValueCount, TagDataIndex, TagDataByteCount;
        //    byte[] TagData;
        //    ExifTagId TagId;

        //    sb.Append("--- IFD ");
        //    sb.Append(Ifd.ToString());
        //    sb.Append(" ---\n");
        //    bool HeaderPrinted = false;
        //    d.InitTagEnumeration(Ifd);
        //    while (d.EnumerateNextTag(out TagSpec))
        //    {
        //        if (!HeaderPrinted)
        //        {
        //            sb.Append("Name                       ID      Type        Value   Byte   ");
        //            AlignedAppend(sb, "Content", MaxContentLength + 2);
        //            sb.Append("Raw data\n");
        //            sb.Append("                                               count   count\n");
        //            HeaderPrinted = true;
        //        }

        //        d.GetTagRawData(TagSpec, out TagType, out ValueCount, out TagData, out TagDataIndex);
        //        AlignedAppend(sb, GetExifTagName(TagSpec), 27);

        //        TagId = ExifData.ExtractTagId(TagSpec);
        //        sb.Append("0x");
        //        sb.Append(((ushort)TagId).ToString("X4"));
        //        sb.Append("  ");

        //        AlignedAppend(sb, TagType.ToString(), 9);
        //        sb.Append("  ");
        //        AlignedAppend(sb, ValueCount.ToString(), 6, true);
        //        sb.Append("  ");

        //        TagDataByteCount = ExifData.GetTagByteCount(TagType, ValueCount);
        //        AlignedAppend(sb, TagDataByteCount.ToString("D"), 6, true);
        //        sb.Append("  ");

        //        AppendInterpretedContent(sb, d, TagSpec, TagType, MaxContentLength);
        //        sb.Append("  ");

        //        int k = TagDataByteCount;
        //        if (k > MaxRawDataOutputCount) k = MaxRawDataOutputCount;
        //        for (int i = 0; i < k; i++)
        //        {
        //            sb.Append(TagData[TagDataIndex + i].ToString("X2"));
        //            sb.Append(" ");
        //        }
        //        if (k < TagDataByteCount) sb.Append("…");
        //        sb.Append("\n");
        //    }
        //    sb.Append("\n");
        //}

        //private string GetExifTagName(ExifTag TagSpec)
        //{
        //    string s = TagSpec.ToString();
        //    if ((s.Length > 0) && (s[0] >= '0') && (s[0] <= '9'))
        //    {
        //        s = "???"; // If the name starts with a digit there is no name defined in the enum type "ExifTag"
        //    }
        //    return (s);
        //}

        //private void AppendInterpretedContent(StringBuilder sb, ExifData d, ExifTag TagSpec, ExifTagType TagType, int Length)
        //{
        //    string s = "";

        //    try
        //    {
        //        if (TagType == ExifTagType.Ascii)
        //        {
        //            if (!d.GetTagValue(TagSpec, out s, StrCoding.Utf8)) s = "???";
        //        }
        //        else if ((TagType == ExifTagType.Byte) && ((TagSpec == ExifTag.XpTitle) || (TagSpec == ExifTag.XpComment) || (TagSpec == ExifTag.XpAuthor) ||
        //                 (TagSpec == ExifTag.XpKeywords) || (TagSpec == ExifTag.XpSubject)))
        //        {
        //            if (!d.GetTagValue(TagSpec, out s, StrCoding.Utf16Le_Byte)) s = "???";
        //        }
        //        else if ((TagType == ExifTagType.Undefined) && (TagSpec == ExifTag.UserComment))
        //        {
        //            if (!d.GetTagValue(TagSpec, out s, StrCoding.IdCode_Utf16)) s = "???";
        //        }
        //        else if ((TagType == ExifTagType.Undefined) && ((TagSpec == ExifTag.ExifVersion) || (TagSpec == ExifTag.FlashPixVersion) ||
        //                 (TagSpec == ExifTag.InteroperabilityVersion)))
        //        {
        //            if (!d.GetTagValue(TagSpec, out s, StrCoding.UsAscii_Undef)) s = "???";
        //        }
        //        else if ((TagType == ExifTagType.Undefined) && ((TagSpec == ExifTag.SceneType) || (TagSpec == ExifTag.FileSource)))
        //        {
        //            d.GetTagRawData(TagSpec, out _, out _, out byte[] RawData);
        //            if (RawData.Length > 0) s += RawData[0].ToString();
        //        }
        //        else if ((TagType == ExifTagType.Byte) || (TagType == ExifTagType.UShort) || (TagType == ExifTagType.ULong))
        //        {
        //            d.GetTagValueCount(TagSpec, out int k);
        //            for (int i = 0; i < k; i++)
        //            {
        //                d.GetTagValue(TagSpec, out uint v, i);
        //                if (i > 0) s += ", ";
        //                s += v.ToString();
        //            }
        //        }
        //        else if (TagType == ExifTagType.SLong)
        //        {
        //            d.GetTagValueCount(TagSpec, out int k);
        //            for (int i = 0; i < k; i++)
        //            {
        //                d.GetTagValue(TagSpec, out int v, i);
        //                if (i > 0) s += ", ";
        //                s += v.ToString();
        //            }
        //        }
        //        else if ((TagType == ExifTagType.SRational) || (TagType == ExifTagType.URational))
        //        {
        //            d.GetTagValueCount(TagSpec, out int k);
        //            for (int i = 0; i < k; i++)
        //            {
        //                d.GetTagValue(TagSpec, out ExifRational v, i);
        //                if (i > 0) s += ", ";
        //                s += v.ToString();
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        s = "!Error!";
        //    }
        //    s = s.Replace('\r', ' ');
        //    s = s.Replace('\n', ' ');
        //    AlignedAppend(sb, s, Length);
        //}

        //private void AlignedAppend(StringBuilder sb, string s, int CharCount, bool RightAlign = false)
        //{
        //    if (s.Length <= CharCount)
        //    {
        //        int SpaceCount = CharCount - s.Length;
        //        if (RightAlign) sb.Append(' ', SpaceCount);
        //        sb.Append(s);
        //        if (!RightAlign) sb.Append(' ', SpaceCount);
        //    }
        //    else
        //    {
        //        sb.Append(s.Substring(0, CharCount - 1));
        //        sb.Append('…');
        //    }
        //}
        //#endregion

    }
}
