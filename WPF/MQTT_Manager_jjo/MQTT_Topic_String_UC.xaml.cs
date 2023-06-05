using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using static MQTT_Manager_jjo.MQTT_Enums;

namespace MQTT_Manager_jjo
{
    public partial class MQTT_Topic_String_UC : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        MQTT_Manager_UC mqtt_uc;

        public string parameter_name
        {
            get => _parameter_name;
            set
            {
                if (value == _parameter_name) return;
                _parameter_name = value;
                OnPropertyChanged();
            }
        }
        string _parameter_name = "parameter name";
        public DataType dataType { get => DataType._string; }

        public bool? sendToMQTTifValueChange
        {
            get => _sendToMQTTifValueChange;
            set
            {
                if (value == _sendToMQTTifValueChange) return;
                _sendToMQTTifValueChange = value;
                if (value == true)
                    btn_send.Visibility = Visibility.Hidden;
                else
                    btn_send.Visibility = Visibility.Visible;

                OnPropertyChanged();
            }
        }
        bool? _sendToMQTTifValueChange = false;

        public string value
        {
            get => _value;
            set
            {
                if (value == _value) return;
                _value = value;
                OnPropertyChanged();
            }
        }
        string _value;

        public MQTT_Topic_String_UC()
        {
            InitializeComponent();
            DataContext = this;
        }
        public void _Link(MQTT_Manager_UC mqtt_uc)
        {
            this.mqtt_uc = mqtt_uc;
        }

        private void Send_Click(object sender, MouseButtonEventArgs e)
        {

        }

    }
}
