using MQTT_Manager_jjo;
using System;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MQTT_Image_Processing
{
    public partial class MainWindow : Window
    {
        MQTT_One_Topic_Subscribed mqtt_crop;
        MQTT_One_Topic_Subscribed mqtt_image;
        float[] crop;
        BitmapImage image;
        OpenCvSharp.Mat mat;

        public MainWindow()
        {
            InitializeComponent();
            INITS();
        }

        private void INITS()
        {
            mqtt_crop = new MQTT_One_Topic_Subscribed(mqtt_uc);
            mqtt_image = new MQTT_One_Topic_Subscribed(mqtt_uc);

            //mqtt_crop_uc._Link(mqtt_crop);
            mqtt_image_uc._Link(mqtt_image);

            mqtt_image.newData += Mqtt_image_crop_newData;
            mqtt_crop.newData += Mqtt_image_crop_newData;
        }

        void Mqtt_image_crop_newData(object? sender, EventArgs e)
        {
            if (sender.GetType() == typeof(BitmapImage))
            {
                image = (BitmapImage)sender;
                mat = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToMat(image);
            }
            if (sender.GetType() == typeof(float[]))
                crop = (float[])sender;

            if (image == null || crop == null) return;

            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                CropImage(mat, crop);
            }, null);
        }

        void CropImage(OpenCvSharp.Mat mat, float[] crop)
        {
            //            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate            {

            float left = crop[0];
            float top = crop[1];
            float right = crop[2];
            float bottom = crop[3];

            int W = mat.Cols;
            int H = mat.Rows;

            int x = (int)(left * W);
            int y = (int)(top * H);

            int w = (int)((right - left) * W);
            int h = (int)((bottom - top) * H);

            OpenCvSharp.Mat croppedMat;
            if (w < 1 || h < 1)
                //error image
                croppedMat = new OpenCvSharp.Mat(32, 32, OpenCvSharp.MatType.CV_8UC3, new OpenCvSharp.Scalar(1, 0, 0));
            else
            {
                OpenCvSharp.Rect roi = new OpenCvSharp.Rect(x, y, w, h);
                croppedMat = new OpenCvSharp.Mat(mat, roi);
            }

            BitmapSource bmp_src = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(croppedMat);
            byte[] data = BitmapSource2ByteArray(bmp_src);

            //            BitmapImage bmp_img = BitmapSource2BitmapImage(bmp_src);

            mqtt_uc.MQTT_Publish("cropped", data);
            //           }, null);
        }

        static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        static BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            BitmapSource i = Imaging.CreateBitmapSourceFromHBitmap(
                           bitmap.GetHbitmap(),
                           IntPtr.Zero,
                           Int32Rect.Empty,
                           BitmapSizeOptions.FromEmptyOptions());
            return (BitmapImage)i;
        }

        static BitmapImage BitmapSource2BitmapImage(BitmapSource bitmapSource)
        {

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();
            BitmapImage bImg = new BitmapImage();

            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(memoryStream);

            memoryStream.Position = 0;
            bImg.BeginInit();
            bImg.StreamSource = memoryStream;
            bImg.EndInit();

            memoryStream.Close();

            return bImg;
        }

        static byte[] BitmapSource2ByteArray(BitmapSource bitmapSource)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(memoryStream);
            return memoryStream.GetBuffer();
        }
    }
}
