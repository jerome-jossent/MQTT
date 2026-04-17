using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace MQTT_Server
{
    public class Configuration
    {
        public int port { get; set; }
        public bool allow_anonymous { get; set; }
        public Dictionary<string, string> logins_passwords { get; set; }
            = new Dictionary<string, string>();

        public List<login_password> logins_passwords_uncrypted { get; set; } //relou parce que c'est directement des usercontrols !?

        public Configuration() { }

        public Configuration(int port, bool allow_anonymous, Dictionary<string, string> logins_passwords)
        {
            this.port = port;
            this.allow_anonymous = allow_anonymous;
            this.logins_passwords = logins_passwords;
        }

        internal string ToJSON()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true // Pour un JSON formaté
            });
        }

        internal static Configuration Load(string fileName)
        {
            // Lire le contenu du fichier
            string json = File.ReadAllText(fileName);

            // Désérialiser le JSON en objet Configuration
            return JsonSerializer.Deserialize<Configuration>(json);
        }

        internal void Save(string fileName)
        {
            string json = ToJSON();
            File.WriteAllText(fileName, json);
        }
    }
}
