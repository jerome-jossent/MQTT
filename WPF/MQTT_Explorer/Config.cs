using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;

namespace MQTT_Explorer
{
    public class Config
    {
        public string name { get; set; }
        public string mqtt_broker_ip { get; set; }
        public int mqtt_broker_port { get; set; }
        public string mqtt_ID { get; set; }
        public bool mqtt_anonymous { get; set; }
        public string mqtt_login { get; set; }
        public string mqtt_password { get; set; }

        const string mqtt_explorer_configs_filename = "configs.cfg";

        public Config() { }

        //initialise un objet à partir des valeurs d'un autre
        public Config(Config other)
        {
            foreach (PropertyInfo property in other.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                if (property.CanWrite)
                    property.SetValue(this, property.GetValue(other));

            foreach (FieldInfo field in other.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                field.SetValue(this, field.GetValue(other));
        }

        public Config DeepCopy()
        {
            var newT = Activator.CreateInstance<Config>();
            var fields = newT.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var field in fields)
            {
                var value = field.GetValue(this);
                field.SetValue(newT, value);
            }
            return newT;
        }

        public static List<Config> Load()
        {
            if (System.IO.File.Exists(mqtt_explorer_configs_filename))
            {
                string json = System.IO.File.ReadAllText(mqtt_explorer_configs_filename);
                return JsonSerializer.Deserialize<List<Config>>(json);
            }
            else
                return new List<Config>();
        }

        public void Save()
        {
            List<Config> cfgs = (System.IO.File.Exists(mqtt_explorer_configs_filename)) ? Load() : new List<Config>();

            //écrasement ?
            bool ecrase = false;
            for (int i = 0; i < cfgs.Count; i++)
            {
                Config cfg = cfgs[i];
                if (cfg.name == this.name)
                {
                    cfgs[i] = new Config(this);
                    ecrase = true;
                    break;
                }
            }
            if (!ecrase)
                cfgs.Add(this);
            Save(cfgs);
        }

        internal static void Save(ObservableCollection<Config> cfgs)
        {
            Save(cfgs.ToList());
        }

        internal static void Save(List<Config> cfgs)
        {
            string jsonString = JsonSerializer.Serialize(cfgs, options: new JsonSerializerOptions()
            {
                WriteIndented = true
            });
            System.IO.File.WriteAllText(mqtt_explorer_configs_filename, jsonString);
        }
    }
}
