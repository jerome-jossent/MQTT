using AForge.Video.DirectShow;
using CompactExifLib;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using MQTTnet;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace MQTT_Video_Publisher
{
    public partial class MainWindow : System.Windows.Window, INotifyPropertyChanged
    {
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public string topic_FrameSended
        {
            get { return _topic_FrameSended; }
            set
            {
                if (_topic_FrameSended == value) return;
                _topic_FrameSended = value;
                Properties.Settings.Default["topic_FrameSended"] = _topic_FrameSended;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }
        string _topic_FrameSended = (string)Properties.Settings.Default["topic_FrameSended"];

        public string topic_Wait
        {
            get { return _topic_Wait; }
            set
            {
                if (_topic_Wait == value) return;
                _topic_Wait = value;
                Properties.Settings.Default["topic_wait"] = _topic_Wait;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }
        string _topic_Wait = (string)Properties.Settings.Default["topic_wait"];

        public int nbframe
        {
            get
            {
                if (_videoCapture == null) return 0;
                return _videoCapture.PosFrames;
            }
            set
            {
                if (_videoCapture.PosFrames == value) return;
                _videoCapture.PosFrames = value;
                OnPropertyChanged();
            }
        }

        public string videofile
        {
            get => _videofile;
            set
            {
                if (_videofile == value) return;
                _videofile = value;
                Properties.Settings.Default["videofile"] = _videofile;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }
        string _videofile = (string)Properties.Settings.Default["videofile"];

        public bool? videofile_loop
        {
            get => _videofile_loop;
            set
            {
                if (value == _videofile_loop) return;
                _videofile_loop = value;
                Properties.Settings.Default["videofile_loop"] = (bool)_videofile_loop;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }


        bool? _videofile_loop = (bool)Properties.Settings.Default["videofile_loop"];


        MQTTnet.Client.IMqttClient mqttClient;
        Dictionary<string, Action<byte[]?>> topics_subscribed;

        VideoCapture _videoCapture;
        CancellationTokenSource cts;

        public bool? nextframeToView
        {
            get => _nextframeToView; set
            {
                if (_nextframeToView == value)
                    return;
                _nextframeToView = value;
                OnPropertyChanged();
            }
        }
        bool? _nextframeToView = false;

        public bool? nextframeToSend
        {
            get => _nextframeToSend; set
            {
                if (_nextframeToSend == value)
                    return;
                _nextframeToSend = value;
                OnPropertyChanged();
            }
        }
        bool? _nextframeToSend = false;

        bool is_video_playing;
        bool is_video_pausing;
        bool userIsDraggingSlider;
        //bool slider_video_mouse_changing_by_code;
        //int nbframe_wanted = -1;

        string _IP;
        int _port;

        bool sendNextFrame;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Fill_cbx_ips();
            INIT_capturedevices();
            INIT_videoplayer();
        }

        Dictionary<string, FilterInfo> devices;
        Dictionary<string, VideoCapabilities> devices_resolution;
        Dictionary<string, int> devices_index;
        VideoCapabilities res;

        private void INIT_capturedevices()
        {
            cbx_capture_devices.Items.Clear();
            devices = new Dictionary<string, FilterInfo>();
            devices_index = new Dictionary<string, int>();

            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            // Parcourir la liste des périphériques vidéo
            for (int i = 0; i < videoDevices.Count; i++)
            {
                FilterInfo device = videoDevices[i];
                cbx_capture_devices.Items.Add(device.Name);
                devices.Add(device.Name, device);
                devices_index.Add(device.Name, i + 700);
            }
        }
        string capture_device_name;
        int capture_device_index;
        private void cbx_capture_devices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            capture_device_name = cbx_capture_devices.SelectedValue.ToString();
            capture_device_index = devices_index[capture_device_name];
            FilterInfo device = devices[capture_device_name];

            // Créer une instance de la classe VideoCaptureDevice pour le périphérique vidéo actuel
            VideoCaptureDevice videoSource = new VideoCaptureDevice(device.MonikerString);

            // Récupérer les résolutions supportées par le périphérique
            VideoCapabilities[] videoCapabilities = videoSource.VideoCapabilities;
            devices_resolution = new Dictionary<string, VideoCapabilities>();
            foreach (VideoCapabilities capability in videoCapabilities)
            {
                string txt = capability.FrameSize.Width + "x" + capability.FrameSize.Height + " [" + capability.MaximumFrameRate + "fps]";
                cbx_capture_device_resolution.Items.Add(txt);
                devices_resolution.Add(txt, capability);
            }
        }

        private void cbx_capture_device_resolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string resname = cbx_capture_device_resolution.SelectedValue as string;
            res = devices_resolution[resname];
        }

        private void btn_video_capture_play_Click(object sender, MouseButtonEventArgs e)
        {
            is_video_pausing = false;
            if (!is_video_playing)
                CaptureVideo_PlayAndPublish();
            btn_video_capture_play.Visibility = Visibility.Hidden;
        }
        private void btn_video_capture_pause_Click(object sender, MouseButtonEventArgs e)
        {
            is_video_pausing = true;
            btn_video_capture_play.Visibility = Visibility.Visible;
        }

        void INIT_videoplayer()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += videoplayer_Tick;
            timer.Start();
        }
        void NextFrame(byte[]? data)
        {
            string r = Encoding.UTF8.GetString(data).ToLower();
            switch (r)
            {
                case "":
                case "true":
                case "1":
                        sendNextFrame = true;
                    break;
            }
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

        void videoplayer_Tick(object? sender, EventArgs e)
        {
            if ((_videoCapture != null) && !_videoCapture.IsDisposed && (_videoCapture.FrameCount > 0) && (!userIsDraggingSlider))
            {
                Dispatcher.BeginInvoke(() =>
                {
                    //slider_video.Minimum = 0;
                    //slider_video.Maximum = _videoCapture.FrameCount;// mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
                    slider_video.Value = _videoCapture.PosFrames;// mePlayer.Position.TotalSeconds;
                });
            }
        }

        //private void slider_video_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    _videoCapture.PosFrames = (int)slider_video.Value;
        //}
        //void slider_video_valuechanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    if (slider_video_mouse_changing_by_code) return;
        //    is_video_pausing = true;
        //    nbframe_wanted = (int)slider_video.Value;
        //    is_video_pausing = false;
        //}
        private void sliProgress_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            nbframe = (int)slider_video.Value;
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //lblProgressStatus.Text = TimeSpan.FromSeconds(slider_video.Value).ToString(@"hh\:mm\:ss");
        }
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
                //.WithReceiveMaximum(1) // limite à 1 data dans le buffer, nécessite mqtt 5
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
            if (topics_subscribed != null && topics_subscribed.Count > 0)
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
            if (mqttClient == null)
            {
                MessageBox.Show("You are not connected.");
                return;
            }
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
        void CaptureVideo_PlayAndPublish()
        {
            Publish_CapturedVideo();
        }

        void Publish_CapturedVideo()
        {
            cts = new CancellationTokenSource();
            Task task = new Task(() => { CaptureVideo_Thread(); }, cts.Token);
            task.Start();
        }

        //-------------

        void Video_PlayAndPublish()
        {
            Publish_videofile(videofile);
        }

        void SelectVideoFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (videofile != "")
            {
                System.IO.FileInfo fi = new FileInfo(videofile);
                if (fi.Exists)
                    openFileDialog.InitialDirectory = fi.DirectoryName;
            }

            if (openFileDialog.ShowDialog() == true)
                videofile = openFileDialog.FileName;
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

                //is_video_pausing = true;
                Dispatcher.BeginInvoke(() =>
                {
                    btn_video_play.Visibility = Visibility.Visible;
                    slider_video.Value = slider_video.Minimum;
                });
            }
        }

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
            try
            {

                while (is_video_playing && !cts.IsCancellationRequested)
                {
                    while (is_video_pausing || userIsDraggingSlider)
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
                        if (videofile_loop != true)
                        {
                            break;
                        }

                        //rembobinage & lecture
                        nbframe = 0;
                        _videoCapture.Read(frame);

                        if (frame.Empty())
                            break;
                    }

                    //conversion en JPEG puis en byte array
                    Cv2.ImEncode(".jpg", frame, out byte[] data);

                    //DateTime t = DateTime.Now;
                    //ajout des metadatas
                    string metadatas = "{" +
                        '"' + "frame" + '"' + ": " + nbframe +
                        ", " + '"' + "time_s" + '"' + ": " + (nbframe / _videoCapture.Fps).ToString("0.000").Replace(",", ".") +
                        "}";
                    metadatas = nbframe.ToString();

                    data = AddMetadatas(data, metadatas);

                    if (sendNextFrame)
                    {
                        sendNextFrame = false;
                        //publie l'image
                        MQTT_Publish(topic_FrameSended, data);
                    }

                    //Mise à jour de l'ihm : affichage du nbr de frame
                    UpdateFrames(nbframe + " / " + _videoCapture.FrameCount);

                    //attente "temps réel"
                    watch.Stop();
                    double ms_to_wait = periode_entre_frame_ms - watch.ElapsedMilliseconds;
                    if (ms_to_wait > 0)
                        Thread.Sleep((int)ms_to_wait);
                }

            }
            catch (Exception ex)
            {

                throw;
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

        void CaptureVideo_Thread()
        {
            is_video_playing = true;

            //webcam
            _videoCapture = new VideoCapture(capture_device_index);
            _videoCapture.FrameWidth = res.FrameSize.Width;
            _videoCapture.FrameHeight = res.FrameSize.Height;
            _videoCapture.Fps = res.FrameRate;

            Mat frame = new Mat();

            //for (int i = 0; i < res.FrameRate; i++)
            //    _videoCapture.Read(frame);

            DateTime t0 = DateTime.Now;
            int nbcapturedframe = 0;

            //mesure du temps entre frame pour effectuer une lecture "temps réel" de la vidéo
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            while (is_video_playing && !cts.IsCancellationRequested)
            {
                //capture de l'image
                _videoCapture.Read(frame);
                nbcapturedframe++;

                //fin ou erreur ?
                if (frame.Empty())
                    continue;

                //conversion en JPEG puis en byte array
                Cv2.ImEncode(".jpg", frame, out byte[] data);

                //ajout des metadatas
                string metadatas = "{" +
                    '"' + "frame" + '"' + ": " + nbcapturedframe +
                    ", " + '"' + "time_s" + '"' + ": " + (nbcapturedframe / _videoCapture.Fps).ToString("0.000").Replace(",", ".") +
                    "}";
                metadatas = nbframe.ToString();
                data = AddMetadatas(data, metadatas);

                if (sendNextFrame)
                {
                    sendNextFrame = false;
                    //publie l'image
                    MQTT_Publish(topic_FrameSended, data);
                }

                UpdateCapturedFrames(nbcapturedframe.ToString());

            }
            _videoCapture.Dispose();
            PlayingVideoStop();
        }
        void UpdateCapturedFrames(string message)
        {
            Dispatcher.BeginInvoke(() =>
            {
                capture_frames.Text = message;
            });
        }

        private void Btn_TopicWaitSubscribe_Click(object sender, RoutedEventArgs e)
        {
            topics_subscribed = new Dictionary<string, Action<byte[]?>>();
            topics_subscribed.Add(topic_Wait, NextFrame);
            MQTTClient_Subscribes();

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
