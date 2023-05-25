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

namespace MQTT_Manager_jjo
{
    /// <summary>
    /// Logique d'interaction pour MQTT_Topic_UC.xaml
    /// </summary>
    public partial class MQTT_Topic_UC : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
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
        string _parameter_name;

        public enum dataType

        public string value_string
        {
            get => _value_string;
            set
            {
                if (value == _value_string) return;
                _value_string = value;
                OnPropertyChanged();
            }
        }
        string _value_string;

        public int? value_integer
        {
            get=>_value_integer;            
            set
            {
                if (value == _value_integer) return;
                _value_integer = value;
                OnPropertyChanged();
            }
        }
        int? _value_integer;

        

        public MQTT_Topic_UC()
        {
            InitializeComponent();
            DataContext = this;
        }

    }
}
