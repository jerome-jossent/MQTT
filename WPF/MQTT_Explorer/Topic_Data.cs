using JsonDiffPatchDotNet;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Nodes;
using TreeView_JJO;
using Xceed.Wpf.AvalonDock.Layout;

namespace MQTT_Explorer
{
    public class Topic_Data : INotifyPropertyChanged
    {
        #region BINDING
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        public byte[]? data;
        public string texte_UTF8 = "";
        public JsonObject? jsonObject;
        public JObject? jsonObject_NS;

        internal Topic_Data_UC uc;
        internal LayoutAnchorablePane pane;
        internal LayoutAnchorable fenetre;

        public ObjectExplorer2 objectExplorer2;
        public ObjectExplorer_JJO.ObjectExplorer objectExplorer;

        public enum TopicType { inconnu, valeur, json, data }

        public TopicType type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }
        TopicType _type;

        public DateTime date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged();
            }
        }
        DateTime _date;

        public Topic topic
        {
            get => _topic;
            set
            {
                _topic = value;
                OnPropertyChanged();
            }
        }
        Topic _topic;

        public int index
        {
            get => _index;
            set
            {
                _index = value;
                OnPropertyChanged();
            }
        }
        int _index;

        public JToken differences_avec_precedent;

        public int datasize { get => data == null ? 0 : data.Length; }

        public bool data_deleted = false;

        public Topic_Data(byte[]? data, Topic topic, int tryToJSONIfSizeUnder = 0)
        {
            date = DateTime.Now;
            this.topic = topic;
            this.data = data;

            if (tryToJSONIfSizeUnder < 1 || data?.Length < tryToJSONIfSizeUnder)            
                if (TryToJSON())
                    objectExplorer = new ObjectExplorer_JJO.ObjectExplorer(jsonObject_NS);            
        }

        public bool TryToJSON()
        {
            if (data == null)
                return false;
            
            texte_UTF8 = Encoding.UTF8.GetString(data);
            try
            {
                jsonObject = System.Text.Json.JsonSerializer.Deserialize<JsonObject>(texte_UTF8);
                jsonObject_NS = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(texte_UTF8);
                type = Topic_Data.TopicType.json;
                return true;
            }
            catch (Exception ex)
            {
                type = Topic_Data.TopicType.data;
                return false;
            }
        }

        internal void SetDifferences(Topic_Data data_prev)
        {
            if (this.type == TopicType.inconnu)
                TryToJSON();
            if (data_prev.type == TopicType.inconnu)
                data_prev.TryToJSON();

            var jdp = new JsonDiffPatch();
            differences_avec_precedent = jdp.Diff(data_prev.jsonObject_NS, this.jsonObject_NS);
        }
    }
}