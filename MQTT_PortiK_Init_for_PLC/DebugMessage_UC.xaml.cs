using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

namespace MQTT_PortiK_Init_for_PLC
{
    /// <summary>
    /// Logique d'interaction pour DebugMessage_UC.xaml
    /// </summary>
    public partial class DebugMessage_UC : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string _message
        {
            get { return message; }
            set
            {
                if (message == value) return;
                message = value;
                OnPropertyChanged("_message");
            }
        }
        string message;

        public string _dateTime
        {
            get { return dateTime; }
            set
            {
                if (dateTime == value) return;
                dateTime = value;
                OnPropertyChanged("_dateTime");
            }
        }
        string dateTime;


        public DebugMessage_UC(string message)
        {
            DateTime t0 = DateTime.Now;
            InitializeComponent();
            DataContext = this;

            _dateTime = t0.ToString("HH:mm:ss.fff");
             this._message = message;

        }
    }
}
