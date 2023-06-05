using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using Image = System.Drawing.Image;

namespace MQTT_Manager_jjo
{
    public class MQTT_One_Topic_Subscribed
    {
        public enum DataType { _boolean, _integer, _long, _float, _double, _string, _image, _image_with_metadatas, _image_with_json_in_metadata, _vector3, _color }
        public DataType dataType;

        MQTT_One_Topic_Subscribed_UC _uc;
        public MQTT_Manager_UC mqtt_uc;

        public MQTT_One_Topic_Subscribed(MQTT_Manager_UC mqtt_uc)
        {
            this.mqtt_uc = mqtt_uc;
        }

        internal void _Link(MQTT_One_Topic_Subscribed_UC mQTT_One_Topic_Subscribed_UC)
        {
            _uc = mQTT_One_Topic_Subscribed_UC;
        }

        public void ManageIncomingData(byte[]? data)
        {
            if (data == null) return;

            DateTime dateTime = DateTime.Now;

            string txt;
            switch (dataType)
            {
                case DataType._boolean:
                    txt = Encoding.Default.GetString(data);
                    bool val_bool = bool.Parse(txt);
                    txt = val_bool.ToString();
                    _uc.AddToMessageList(txt);
                    break;

                case DataType._integer:
                    txt = Encoding.Default.GetString(data);
                    int val_int = int.Parse(txt);
                    txt = val_int.ToString();
                    _uc.AddToMessageList(txt);
                    break;

                case DataType._long:
                    txt = Encoding.Default.GetString(data);
                    long val_long = long.Parse(txt);
                    txt = val_long.ToString();
                    _uc.AddToMessageList(txt);
                    break;

                case DataType._float:
                    txt = Encoding.Default.GetString(data);
                    float val_float = float.Parse(txt);
                    txt = val_float.ToString();
                    _uc.AddToMessageList(txt);
                    break;

                case DataType._double:
                    txt = Encoding.Default.GetString(data);
                    double val_double = double.Parse(txt);
                    txt = val_double.ToString();
                    _uc.AddToMessageList(txt);
                    break;

                case DataType._string:
                    txt = Encoding.Default.GetString(data);
                    _uc.AddToMessageList(txt);
                    break;

                case DataType._image:
                    _uc.DisplayImage(data);
                    break;

                case DataType._image_with_metadatas:
                    _uc.DisplayImage(data);
                    _uc.DisplayMetaData(data);
                    break;

                case DataType._image_with_json_in_metadata:
                    _uc.DisplayImage(data);
                    _uc.DisplayJsonFromMetaData(data);
                    break;

                case DataType._vector3:
                    float[] vec3 = GetVector3FromByteArray(data);
                    txt = vec3[0].ToString() + " ; " +
                          vec3[1].ToString() + " ; " +
                          vec3[2].ToString();
                    _uc.AddToMessageList(txt);
                    break;

                case DataType._color:

                    break;

                default:
                    break;
            }
        }

        float[] GetVector3FromByteArray(byte[] data)
        {
            float[] _valeur = new float[3];
            //float : 4 octets
            byte[] x_b = new byte[4];
            byte[] y_b = new byte[4];
            byte[] z_b = new byte[4];

            Array.Copy(data, 0, x_b, 0, 4);
            Array.Copy(data, 4, y_b, 0, 4);
            Array.Copy(data, 8, z_b, 0, 4);

            _valeur[0] = BitConverter.ToSingle(x_b, 0);
            _valeur[1] = BitConverter.ToSingle(y_b, 0);
            _valeur[2] = BitConverter.ToSingle(z_b, 0);

            return _valeur;
        }

        public static Image ImageFromByteArray(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            using (Image image = Image.FromStream(ms, true, true))
                return (Image)image.Clone();
        }

        public BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

    }
}