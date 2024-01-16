using CommunityToolkit.Mvvm.Input;
using CompactExifLib;
using Geoimage;
using Google.Protobuf;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Windows.Forms.AxHost;

namespace Portik_Data_Explorer
{
    public partial class MainWindow : System.Windows.Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ObservableCollection<DataSet> dataSets
        {
            get { return _dataSets; }
            set
            {
                _dataSets = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<DataSet> _dataSets = new ObservableCollection<DataSet>();

        public DataSet selectedDataSet
        {
            get => _selectedDataSet; set
            {
                if (value == null || value == _selectedDataSet)
                    return;
                _selectedDataSet = value;
                OnPropertyChanged();
            }
        }
        DataSet _selectedDataSet;

        public string path
        {
            get => _path; set
            {
                if (_path != value)
                {
                    _path = value;
                    OnPropertyChanged();
                    Properties.Settings.Default["path"] = _path;
                    Properties.Settings.Default.Save();
                }
            }
        }
        string _path;

        bool replayIsRunning;
        bool replayPlay;
        bool replayModePasAPas;
        bool replayNext;
        Thread thread;
        CancellationTokenSource thread_cancel;

        SendDataSetByMQTT.MQTT_Parameters mqtt_Parameters;
        public string ip_Broker
        {
            get => _ip_Broker; set
            {
                _ip_Broker = value;
                OnPropertyChanged();
            }
        }
        string _ip_Broker = "127.0.0.1";

        public string port_Broker
        {
            get => _port_Broker; set
            {
                _port_Broker = value;
                OnPropertyChanged();
            }
        }
        string _port_Broker = "1883";
        private bool play_GeoImages;
        private bool play_Inferences;
        private bool play_GeoObjets;
        private bool play_Flips;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {       
            path = Properties.Settings.Default["path"].ToString();
            SearchDatasSet();
        }

        [RelayCommand] //ajouté pour que dans le xaml on puisse mettre DeleteFolderCommand (avec Command de concatener) comme binding
        void DeleteFolder(DataSet ds)
        {
            ds.DeleteFolder();
            dataSets.Remove(ds);
        }

        void DeleteSelection_MouseDown(object sender, MouseButtonEventArgs e)
        {
            List<DataSet> dataSetsToDelete = new List<DataSet>();
            foreach (var item in dataGrid.SelectedItems)
                dataSetsToDelete.Add((DataSet)item);

            if (dataSetsToDelete.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show("Suppression définitive de " + dataSetsToDelete.Count + " dossiers.\n" +
                                       "Confirmer la suppression.", "Attention",
                                       MessageBoxButton.OKCancel,
                                       MessageBoxImage.Warning);
                if (result != MessageBoxResult.OK)
                    return;
            }

            for (int i = 0; i < dataSetsToDelete.Count; i++)
            {
                DataSet ds = dataSetsToDelete[i];
                ds.DeleteFolder(false);
                dataSets.Remove(ds);
            }
        }

        [RelayCommand] //ajouté pour que dans le xaml on puisse mettre DeleteFolderCommand (avec Command de concatener) comme binding
        void ExploreFile(DataSet ds)
        {
            ds.ExploreFile();
        }

        void SelectFolder_MouseDown(object sender, MouseButtonEventArgs e) { SelectFolder(); }
        void SearchDataSet_MouseDown(object sender, MouseButtonEventArgs e) { SearchDatasSet(); }

        void TBX_port_Broker_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }

        void SelectFolder()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    path = dialog.SelectedPath;
                    SearchDatasSet();
                }
            }
        }

        void SearchDatasSet()
        {
            if (!Directory.Exists(path))
                return;

            dataSets.Clear();

            foreach (string folder in Directory.GetDirectories(path))
            {
                DataSet ds = new DataSet(path, folder);
                dataSets.Add(ds);
            }
        }

        private void ReplaySelection_STOP_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (replayIsRunning)
                Stop();
        }

        private void ReplaySelection_PLAY_MouseDown(object sender, MouseButtonEventArgs e)
        {
            replayPlay = true;
            replayModePasAPas = false;
            replayNext = false;
            SetExtensionFiles();
            if (!replayIsRunning)
                Start(selectedDataSet);
        }

        private void ReplaySelection_PAUSE_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (replayIsRunning)
                replayPlay = false;
        }

        private void ReplaySelection_NEXT_MouseDown(object sender, MouseButtonEventArgs e)
        {
            replayModePasAPas = true;
            replayNext = true;
            replayPlay = true;
            SetExtensionFiles();
            if (!replayIsRunning)
                Start(selectedDataSet);
        }

        void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = e.Source as ListView;
            listView.ScrollIntoView(listView.SelectedItem);
        }

        void SetExtensionFiles()
        {
            play_GeoImages = ckb_GeoImages.IsChecked == true;
            play_Inferences = ckb_Inferences.IsChecked == true;
            play_GeoObjets = ckb_GeoObjets.IsChecked == true;
            play_Flips = ckb_Flips.IsChecked == true;
        }

        void Stop()
        {
            thread_cancel.Cancel();
            replayIsRunning = false;
            replayPlay = false;
            replayModePasAPas = false;
            replayNext = false;
        }

        void Start(DataSet dataSet)
        {
            if (thread != null) Stop();
            thread_cancel = new CancellationTokenSource();
            ThreadPool.QueueUserWorkItem(new WaitCallback(Play), new object[] { thread_cancel.Token, dataSet });
        }

        void Play(object state)
        {
            replayIsRunning = true;
            replayPlay = true;

            object[] array = state as object[];
            CancellationToken token = (CancellationToken)array[0];
            DataSet dataSet = (DataSet)array[1];

            if (dataSet == null) return;

            List<Fichier> filesToProcess = new List<Fichier>();
            foreach (Fichier file in dataSet.fichiers)
            {
                switch (file.extension)
                {
                    case FichierGeoImage.extension: if (!play_GeoImages) continue; break;
                    case FichierInferences.extension: if (!play_Inferences) continue; break;
                    case FichierGeoObjets.extension: if (!play_GeoObjets) continue; break;
                    case FichierGeoFlips.extension: if (!play_Flips) continue; break;
                }
                filesToProcess.Add(file);
            }

            DateTime t0 = DateTime.Now;

            mqtt_Parameters = new SendDataSetByMQTT.MQTT_Parameters() { IP = ip_Broker, port = int.Parse(port_Broker) };

            //démarrage de la file de lecture des fichiers
            for (int i = 0; i < dataSet.fichiers.Count - 1; i++)
            {
                Fichier f = dataSet.fichiers[i];
                dataSet.activeFichier = f; //permet la sélection/surbrillance dans la liste

                //mesure du temps avant le prochain fichier
                Fichier f_next = dataSet.fichiers[i + 1];
                double deltaT = f_next.t_s - f.t_s;

                //action sur le fichier (ici envoit par mqtt)
                f.Process(this, mqtt_Parameters);

                //save to jpeg to disk
                Dispatcher.Invoke(() =>
                {
                    bool convert_geoimage_to_jpeg_and_save_to_disk = this.convert_geoimage_to_jpeg_and_save_to_disk.IsChecked == true;
                    if (convert_geoimage_to_jpeg_and_save_to_disk && ".portik" + f.extension == FichierGeoImage.extension)
                    {
                        FichierGeoImage fgi = (FichierGeoImage)f;

                        GeoImage geoImage = FichierGeoImage.LoadGeoImage(f.bytes);
                        Mat mat = FichierGeoImage.GeoImage_to_Mat(geoImage);
                        Cv2.ImEncode(".jpg", mat, out byte[] data);

                        //suppression des bytes "images"
                        geoImage = SimplifyImageData(geoImage);
                        //geoImage.ImageData = null;

                        //création des metadatas
                        string json = System.Text.Json.JsonSerializer.Serialize(geoImage, new JsonSerializerOptions { WriteIndented = true });

                        // metadatas placé dans 
                        data = AddMetadatas(data, json);

                        File.WriteAllBytes(f.fileInfo.FullName + ".jpg", data);
                        //FichierGeoImage.SaveToDiskImage(f.bytes, f.fileInfo.FullName + ".jpg");
                    }
                });

                //calcul du temps passé depuis le début du replay (avec prise en compte des pauses/nexts ?)
                dataSet.currenttime = GetDeltaTimeInFloat(t0);

                if (replayModePasAPas)
                {
                    while (!replayNext)
                    {
                        Thread.Sleep(100);
                        if (token.IsCancellationRequested)
                        {
                            dataSet.currenttime = 0;
                            return;
                        }
                    }
                    replayNext = false;
                }
                else
                {
                    //attente entre 2 fichiers
                    while (f_next.t_s > dataSet.currenttime)
                    {
                        if (deltaT < 0.001)
                            break;
                        else
                        {
                            Thread.Sleep(1);
                            deltaT -= 0.001;
                        }

                        if (replayPlay == false) //mise en pause
                        {
                            DateTime t0_pause = DateTime.Now;
                            while (!replayPlay)
                            {
                                Thread.Sleep(100);
                                if (token.IsCancellationRequested)
                                {
                                    dataSet.currenttime = 0;
                                    return;
                                }
                            }
                            //reprise
                            TimeSpan t_pause = DateTime.Now - t0_pause;
                            t0 += t_pause;
                        }

                        if (token.IsCancellationRequested)
                        {
                            dataSet.currenttime = 0;
                            return;
                        }
                        //update time in the wait loop
                        dataSet.currenttime = GetDeltaTimeInFloat(t0);
                    }
                }
            }
            Fichier f_last = dataSet.fichiers[dataSet.fichiers.Count - 1];
            f_last.Process(this, mqtt_Parameters);
            dataSet.activeFichier = f_last;

            Stop();
        }

        float GetDeltaTimeInFloat(DateTime t0)
        {
            return (float)(DateTime.Now - t0).TotalSeconds;
        }

        GeoImage SimplifyImageData(GeoImage geoImage)
        {
            GeoImage geoImageLight = geoImage.Clone();

            byte[] bytes_full = geoImageLight.ImageData.ToByteArray();
            byte[] bytes_light = new byte[2];
            for (int i = 0; i < 2; i++)
                Array.Copy(bytes_full, 2, bytes_light, 0, 2);

            geoImageLight.ImageData = ByteString.CopyFrom(bytes_light);
            return geoImageLight;
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

        private void ckb_data_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
