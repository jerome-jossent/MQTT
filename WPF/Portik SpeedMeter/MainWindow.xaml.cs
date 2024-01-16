using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using MQTT_Manager_jjo;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using OpenCvSharp.XFeatures2D;
using Portik_Data_Explorer;
using ScottPlot.Plottable;
using ScottPlot.Renderable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace Portik_SpeedMeter
{
    public partial class MainWindow : System.Windows.Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public float resolution_mm_by_pixel
        {
            get => _resolution_mm_by_pixel;

            set
            {
                if (_resolution_mm_by_pixel == value) return;
                _resolution_mm_by_pixel = value;
                OnPropertyChanged();
            }
        }
        float _resolution_mm_by_pixel = 0.53f;

        public float speed
        {
            get => _speed;

            set
            {
                if (_speed == value) return;
                _speed = value;
                OnPropertyChanged();
            }
        }
        float _speed = 1.23f;

        MQTT_Manager_jjo.MQTT_One_Topic_Subscribed incoming_portik_image;

        Thread thread;
        CancellationTokenSource cts;
        List<Geoimage.GeoImage> gisToProcess = new List<Geoimage.GeoImage>();
        object matProcess_lock = new object();

        Mat previous_mat;
        DateTime t0;
        DateTime previous_t;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            cts.Cancel();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            mqtt._connected += Mqtt__connected;

            cts = new CancellationTokenSource();
            thread = new Thread(ThreadProcess);
            thread.Start();

            //test();

            Debug_graph_INIT();

            //ReadGeoImagesFromFolder();
        }

        void ReadGeoImagesFromFolder()
        {
            string folder = @"D:\DATA\Portik\2023-12-08 17-00\";
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(folder);
            var files = di.GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo fi = files[i];
                var gi = FichierGeoImage.LoadGeoImage(fi.FullName);
                lock (matProcess_lock)
                {
                    gisToProcess.Add(gi);
                }
            }
        }

        ScottPlot.Plottable.DataLogger debug_log_02;

        void Debug_graph_INIT()
        {
            graph.Plot.Style(new ScottPlot.Styles.Gray1());

            graph.Plot.YAxis.IsVisible = false;

            debug_log_02 = graph.Plot.AddDataLogger();
            debug_log_02.MarkerSize = 5;
            debug_log_02.Color = System.Drawing.Color.DodgerBlue;
            // Create another axis to the left and give it an index
            var secondYAxis = graph.Plot.AddAxis(Edge.Left, axisIndex: 2, "distancepixel", color: debug_log_02.Color);
            debug_log_02.YAxisIndex = 2;
        }

        void Debug_New(float x, float y)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    debug_log_02.Add(x, y);
                    graph.Plot.AxisAuto();
                    graph.Refresh();
                }
                catch (Exception)
                {

                }
            }), priority: DispatcherPriority.Background);
        }

        private void Mqtt__connected(object? sender, EventArgs e)
        {
            mqtt.topics_subscribed = new Dictionary<string, Action<byte[]?>>();
            mqtt.topics_subscribed.Add("ackisition/geoimage/viewer", NewGeoImage);
            mqtt.MQTTClient_Subscribes();
        }

        private void NewGeoImage(byte[]? obj)
        {
            var gi = Portik_Data_Explorer.FichierGeoImage.LoadGeoImage(obj);

            Mat new_mat = Portik_Data_Explorer.FichierGeoImage.GeoImage_to_Mat(gi);

            Display(new_mat);

            lock (matProcess_lock)
            {
                gisToProcess.Add(gi);
            }
        }

        void ThreadProcess()
        {
            while (!cts.IsCancellationRequested)
            {
                #region wait & get new GeoImage
                while (gisToProcess.Count == 0 || cts.IsCancellationRequested)
                    Thread.Sleep(10);

                Geoimage.GeoImage gi;
                lock (matProcess_lock)
                {
                    gi = gisToProcess[0];
                    gisToProcess.RemoveAt(0);
                }
                #endregion

                Process(gi);
            }
        }

        void Process(Geoimage.GeoImage gi)
        {
            Mat new_mat = Portik_Data_Explorer.FichierGeoImage.GeoImage_to_Mat(gi);
            Timestamp new_tt = gi.Geodata.Data.Timestamp;
            DateTime new_t = new_tt.ToDateTime();
            Measure(new_t, new_mat);
        }

        void Display(Mat new_mat)
        {
            Dispatcher.Invoke(() =>
            {
                if (new_mat.Empty()) return;
                image.Source = new_mat.ToWriteableBitmap();
            });
        }

        void Measure(DateTime t, Mat new_mat)
        {
            Cv2.CvtColor(new_mat, new_mat, ColorConversionCodes.BGR2GRAY);

            if (previous_mat != null)
            {
                Mat resultImage = new Mat();
                Cv2.MatchTemplate(previous_mat, new_mat, resultImage, TemplateMatchModes.CCoeffNormed);

                Cv2.MinMaxLoc(resultImage, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);
                OpenCvSharp.Rect matchRect = new OpenCvSharp.Rect(maxLoc, new OpenCvSharp.Size(previous_mat.Width, previous_mat.Height));
                Cv2.Rectangle(new_mat, matchRect, Scalar.Red, 2);

                OpenCvSharp.Point centresource = new OpenCvSharp.Point(new_mat.Width / 2, 100 + (int)(0.4 * previous_mat.Height));
                OpenCvSharp.Point centretemplate = new OpenCvSharp.Point(new_mat.Width / 2, matchRect.Y + matchRect.Height / 2);

                float distance_pixel = centretemplate.Y - centresource.Y;
                float distance_mm = distance_pixel * resolution_mm_by_pixel;
                double duree_sec = (t - previous_t).TotalSeconds;
                speed = (float)(distance_mm / 1000 * 1 / duree_sec);

                Debug_New((float)(t - t0).TotalSeconds, speed);
            } else
                t0 = t;

            //création de l'image template qui servira lors de l'arrivée de l'image suivante
            previous_mat = new Mat(new_mat, new OpenCvSharp.Rect((int)(0.2 * new_mat.Width), 100, (int)(0.6 * new_mat.Width), (int)(0.4 * new_mat.Height)));
            previous_t = t;
        }

        void test()
        {
            Mat templateImage = Cv2.ImRead(@"D:\DATA\Portik\17020547353173100.portik.geoimage.jpg", ImreadModes.Color);
            Mat sourceImage = Cv2.ImRead(@"D:\DATA\Portik\17020547358190440.portik.geoimage.jpg", ImreadModes.Color);
            Mat templateImageROI = new Mat(templateImage, new OpenCvSharp.Rect((int)(0.2 * templateImage.Width), 100, (int)(0.6 * templateImage.Width), (int)(0.4 * templateImage.Height)));



            Mat graySourceImage = new Mat();
            Mat grayTemplateImage = new Mat();

            Cv2.CvtColor(sourceImage, graySourceImage, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(templateImageROI, grayTemplateImage, ColorConversionCodes.BGR2GRAY);

            Mat resultImage = new Mat();
            Cv2.MatchTemplate(graySourceImage, grayTemplateImage, resultImage, TemplateMatchModes.CCoeffNormed);

            Cv2.MinMaxLoc(resultImage, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);
            OpenCvSharp.Rect matchRect = new OpenCvSharp.Rect(maxLoc, new OpenCvSharp.Size(templateImageROI.Width, templateImageROI.Height));
            Cv2.Rectangle(sourceImage, matchRect, Scalar.Red, 2);
            OpenCvSharp.Point centresource = new OpenCvSharp.Point(sourceImage.Width / 2, 100 + (int)(0.4 * templateImageROI.Height));
            OpenCvSharp.Point centretemplate = new OpenCvSharp.Point(sourceImage.Width / 2, matchRect.Y + matchRect.Height / 2);

            Cv2.Line(sourceImage, centresource, centretemplate, new Scalar(255, 0, 0));

            Cv2.ImShow("templateImageROI", templateImageROI);
            Cv2.ImShow("templateImage", templateImage);
            Cv2.ImShow("Matching Result", sourceImage);
            Cv2.WaitKey(0);
        }
    }
}