using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Portik_Data_Explorer
{
    public abstract class Fichier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public System.Windows.Media.Brush color { get; set; }

        public string extension { get => fileInfo.Extension; }
        public FileInfo fileInfo { get; set; }
        public string name { get; set; }
        public int index;
        public long t_absolute;
        public long t;
        public double t_s { get; set; }

        public bool processed { get; set; }

        public abstract string mqtt_topic { get; }
        internal byte[] bytes
        {
            get
            {
                byte[] data = System.IO.File.ReadAllBytes(fileInfo.FullName);
                return data;
            }
        }

        public void ComputeTime(long T0)
        {
            t = t_absolute - T0;
            t_s = (double)t / 10_000_000;
        }

        internal void Process(MainWindow mainWindow, SendDataSetByMQTT.MQTT_Parameters mqtt_Parameters)
        {
            SendDataSetByMQTT.Send(mqtt_topic, bytes, mqtt_Parameters);
            mainWindow.Dispatcher.Invoke(new Action(() =>
            {
                mainWindow.Title = name;
            }));
        }
    }
}
