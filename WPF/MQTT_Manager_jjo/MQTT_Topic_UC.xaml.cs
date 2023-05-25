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

        public enum DataType { _boolean, _integer, _long, _float, _double, _string, _image, _image_with_metadatas, _image_with_json_in_metadata, _vector3, _color }
        public DataType dataType
        {
            get => _dataType;
            set
            {
                if (value == _dataType) return;
                _dataType = value;
                OnPropertyChanged();





                //Update VISIBILITY
                grid_string.Visibility = Visibility.Hidden;
                grid_integer.Visibility = Visibility.Hidden;

                switch (_dataType)
                {
                    case DataType._boolean:
                        break;
                    case DataType._integer: 
                        grid_integer.Visibility= Visibility.Visible;
                        break;
                    case DataType._long:
                        break;
                    case DataType._float:
                        break;
                    case DataType._double:
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
        }
        DataType _dataType;


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
            get => _value_integer;
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
