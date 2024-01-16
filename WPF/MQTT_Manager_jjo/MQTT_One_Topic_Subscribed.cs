using System;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;
using static MQTT_Manager_jjo.MQTT_Enums;
using Image = System.Drawing.Image;

namespace MQTT_Manager_jjo
{
    public class MQTT_One_Topic_Subscribed
    {
        public DataType dataType;

        MQTT_One_Topic_Subscribed_UC _uc;
        public MQTT_Manager_UC mqtt_uc;
        public bool _display;
        public string _topic;

        public event EventHandler newData;

        public MQTT_One_Topic_Subscribed(MQTT_Manager_UC mqtt_uc)
        {
            this.mqtt_uc = mqtt_uc;
        }

        public void _Link(MQTT_One_Topic_Subscribed_UC mQTT_One_Topic_Subscribed_UC)
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
                case DataType._bytearray:
                    newData?.Invoke(data, new EventArgs());
                    break;

                case DataType._boolean:
                    txt = Encoding.Default.GetString(data);
                    bool val_bool = bool.Parse(txt);
                    txt = val_bool.ToString();
                    if (_display) _uc.AddToMessageList(txt);
                    newData?.Invoke(val_bool, new EventArgs());
                    break;

                case DataType._integer:
                    txt = Encoding.Default.GetString(data);
                    int val_int = int.Parse(txt);
                    txt = val_int.ToString();
                    if (_display) _uc.AddToMessageList(txt);
                    newData?.Invoke(val_int, new EventArgs());
                    break;

                case DataType._long:
                    txt = Encoding.Default.GetString(data);
                    long val_long = long.Parse(txt);
                    txt = val_long.ToString();
                    if (_display) _uc.AddToMessageList(txt);
                    newData?.Invoke(val_long, new EventArgs());
                    break;

                case DataType._float:
                    txt = Encoding.Default.GetString(data);
                    float val_float = float.Parse(txt);
                    txt = val_float.ToString();
                    if (_display) _uc.AddToMessageList(txt);
                    newData?.Invoke(val_float, new EventArgs());
                    break;

                case DataType._double:
                    txt = Encoding.Default.GetString(data);
                    double val_double = double.Parse(txt);
                    txt = val_double.ToString();
                    if (_display) _uc.AddToMessageList(txt);
                    newData?.Invoke(val_double, new EventArgs());
                    break;

                case DataType._string:
                    txt = Encoding.Default.GetString(data);
                    if (_display) _uc.AddToMessageList(txt);
                    newData?.Invoke(txt, new EventArgs());
                    break;

                case DataType._image:
                    BitmapImage image0 = ToImage(data);
                    if (_display) _uc.DisplayImage(data);
                    newData?.Invoke(image0, new EventArgs());
                    break;

                case DataType._image_with_metadatas:
                    BitmapImage image1 = ToImage(data);
                    if (_display) _uc.DisplayImage(data);

                    if (_display) _uc.DisplayMetaData(data);
                    newData?.Invoke(image1, new EventArgs());
                    break;

                case DataType._image_with_json_in_metadata:
                    BitmapImage image2 = ToImage(data);
                    if (_display) _uc.DisplayImage(data);

                    if (_display) _uc.DisplayJsonFromMetaData(data);
                    newData?.Invoke(image2, new EventArgs());
                    break;

                case DataType._vector3:
                    float[] vec3 = GetVector3FromByteArray(data);
                    txt = vec3[0].ToString() + " ; " +
                          vec3[1].ToString() + " ; " +
                          vec3[2].ToString();
                    if (_display) _uc.AddToMessageList(txt);
                    newData?.Invoke(vec3, new EventArgs());
                    break;

                case DataType._vector4:
                    float[] vec4 = GetVector4FromByteArray(data);
                    txt = vec4[0].ToString() + " ; " +
                          vec4[1].ToString() + " ; " +
                          vec4[2].ToString() + " ; " +
                          vec4[3].ToString();
                    if (_display) _uc.AddToMessageList(txt);
                    newData?.Invoke(vec4, new EventArgs());
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

        float[] GetVector4FromByteArray(byte[] data)
        {
            float[] _valeur = new float[4];
            //float : 4 octets
            byte[] x_b = new byte[4];
            byte[] y_b = new byte[4];
            byte[] z_b = new byte[4];
            byte[] w_b = new byte[4];

            Array.Copy(data, 0, x_b, 0, 4);
            Array.Copy(data, 4, y_b, 0, 4);
            Array.Copy(data, 8, z_b, 0, 4);
            Array.Copy(data, 12, w_b, 0, 4);

            _valeur[0] = BitConverter.ToSingle(x_b, 0);
            _valeur[1] = BitConverter.ToSingle(y_b, 0);
            _valeur[2] = BitConverter.ToSingle(z_b, 0);
            _valeur[3] = BitConverter.ToSingle(w_b, 0);

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
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

    }
}