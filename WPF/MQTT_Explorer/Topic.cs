
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MQTT_Explorer
{
    public class Topic : INotifyPropertyChanged
    {
        #region BINDING
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        public string topic
        {
            get => _topic;
            set
            {
                _topic = value;
                OnPropertyChanged();
            }
        }
        string _topic;

        public Topic_Data.TopicType type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }
        Topic_Data.TopicType _type;

        public int datasize
        {
            get => _datasize;
            set
            {
                _datasize = value;
                OnPropertyChanged();
            }
        }
        int _datasize;

        public int datatotalsize
        {
            get => _datatotalsize;
            set
            {
                _datatotalsize = value;
                OnPropertyChanged();
            }
        }
        int _datatotalsize;

        public int occurence
        {
            get => _occurence;
            set
            {
                _occurence = value;
                OnPropertyChanged();
            }
        }
        int _occurence;
        internal bool followed;

        public ObservableCollection<Topic_Data> datas { get; set; } = new ObservableCollection<Topic_Data>();
        public Dictionary<DateTime, Topic_Data> datas_dico { get; set; } = new Dictionary<DateTime, Topic_Data>();

        public List<string> champs_a_masquer = new List<string>();

        public DateTime lastDate
        {
            get
            {
                try
                {
                    return datas_dico.Values.Last().date;
                }
                catch (Exception ex)
                {
                    return DateTime.MinValue;
                }
            }
        }






        public ObservableCollection<Button> timeline_buttons
        {
            get => _timeline_buttons; set
            {
                _timeline_buttons = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<Button> _timeline_buttons;





        Action<Topic_Data> MQTT_Publish;


        public Topic(string topic)
        {
            this.topic = topic;
        }

        public Topic(string topic, Action<Topic_Data> mQTT_Publish) : this(topic)
        {
            this.MQTT_Publish = mQTT_Publish;
        }

        public Topic_Data Data_New(byte[]? data, int tryToJSONIfSizeUnder)
        {
            Topic_Data d = new Topic_Data(data, this, tryToJSONIfSizeUnder);

            occurence++;
            d.index = occurence;

            datatotalsize += d.datasize;
            datas_dico.Add(d.date, d);

            //if (MainWindow.data_total_size_max > 0)
                while (datatotalsize > MainWindow.data_total_size_max)
                    DelOnlyOldestData();

            _ = System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                if (MainWindow._data_top_is_last)
                    datas.Insert(0, d);
                else
                    datas.Add(d);
            }));

            if (data != null)
                datasize = d.datasize;

            type = d.type;

            return d;
        }

        public void DelOnlyOldestData()
        {
            DateTime t_oldest = DateTime.Now;
            foreach (var data in datas_dico)
                if (!data.Value.data_deleted)
                    if (data.Key < t_oldest)
                        t_oldest = data.Key;

            Topic_Data old_data = datas_dico[t_oldest];

            _datatotalsize -= old_data.datasize;
            old_data.texte_UTF8 = "Data deleted";
            old_data.data = Encoding.UTF8.GetBytes(old_data.texte_UTF8);
            old_data.data_deleted = true;
        }

        public void Data_Del(Topic_Data data)
        {
            DateTime t = data.date;
            datas_dico.Remove(t);
            datas.Remove(data);
            _datatotalsize -= data.datasize;
        }

        internal List<Topic_Data> Data_Follow()
        {
            Dictionary<int, Topic_Data> datas_dico = new Dictionary<int, Topic_Data>();

            foreach (Topic_Data data in datas_dico.Values)
                datas_dico.Add(data.index, data);

            //produit une liste des data trié du plus petit index au plus grand
            List<Topic_Data> datats_sorted = datas_dico.OrderBy(i => i.Key).Select(i => i.Value).ToList();

            for (int i = 1; i < datats_sorted.Count; i++)
            {
                Topic_Data data = datats_sorted[i];
                Topic_Data data_prev = datats_sorted[i - 1];
                data.SetDifferences(data_prev);
            }

            return datats_sorted;
        }

        internal void PublishAgain(Topic_Data data)
        {
            MQTT_Publish(data);
        }

    }
}