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
        string _parameter_name = "parameter name";

        public enum DataType { _boolean, _integer, _long, _float, _double, _string, _image, _image_with_metadatas, _image_with_json_in_metadata, _vector3, _color }
        public DataType dataType
        {
            get => _dataType;
            set
            {
                if (value == _dataType) return;
                _dataType = value;
                OnPropertyChanged();

                UpdateValuesVISIBILITY();
            }
        }
        DataType _dataType;

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

        #region VALUE
        public string value_string
        {
            get => _value_string;
            set
            {
                if (value == _value_string) return;
                _value_string = value;
                OnPropertyChanged();
                ValueChange();
            }
        }
        string _value_string;

        public bool? value_bool
        {
            get => _value_bool;
            set
            {
                if (value == _value_bool) return;
                _value_bool = value;
                OnPropertyChanged();
                ValueChange();
            }
        }
        bool? _value_bool;

        public int? value_integer
        {
            get => _value_integer;
            set
            {
                if (value == _value_integer) return;
                _value_integer = value;
                OnPropertyChanged();
                ValueChange();
            }
        }
        int? _value_integer;

        public int? value_long
        {
            get => _value_long;
            set
            {
                if (value == _value_long) return;
                _value_long = value;
                OnPropertyChanged();
                ValueChange();
            }
        }
        int? _value_long;

        public double? value_double
        {
            get => _value_double;
            set
            {
                if (value == _value_double) return;
                _value_double = value;
                OnPropertyChanged();
                ValueChange();
            }
        }
        double? _value_double;
        #endregion

        public MQTT_Topic_UC()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void UpdateValuesVISIBILITY()
        {
            grid_string.Visibility = Visibility.Hidden;
            grid_boolean.Visibility = Visibility.Hidden;
            grid_integer.Visibility = Visibility.Hidden;
            grid_long.Visibility = Visibility.Hidden;
            grid_double.Visibility = Visibility.Hidden;

            switch (dataType)
            {
                case DataType._boolean:
                    grid_boolean.Visibility = Visibility.Visible;
                    break;
                case DataType._integer:
                    grid_integer.Visibility = Visibility.Visible;
                    break;
                case DataType._long:
                    grid_long.Visibility = Visibility.Visible;
                    break;
                // case DataType._float:                    break;
                case DataType._double:
                    grid_double.Visibility = Visibility.Visible;
                    break;
                case DataType._string:
                    grid_string.Visibility = Visibility.Visible;
                    break;
                case DataType._image:
                    break;
                case DataType._image_with_metadatas:
                    break;
                case DataType._image_with_json_in_metadata:
                    break;
                case DataType._vector3:
                    break;
                case DataType._color:
                    break;
                default:
                    break;
            }
        }

        private void ValueChange()
        {
            throw new NotImplementedException();
        }

        private void Send_Click(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
