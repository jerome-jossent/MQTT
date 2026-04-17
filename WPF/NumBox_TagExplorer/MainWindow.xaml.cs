using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NumBox_TagExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string tags_file = @"C:\DATA\NumBox\JN\taglist.txt";
        string templates_file = @"C:\DATA\NumBox\JN\templates.txt";
        string tags_json, templates_json;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadFiles();
            Refresh();
        }

        private void LoadFiles()
        {
            tags_json = System.IO.File.ReadAllText(tags_file);
            templates_json = System.IO.File.ReadAllText(templates_file);
        }

        private void Refresh()
        {
            JArray tags_obj = (JArray)Newtonsoft.Json.JsonConvert.DeserializeObject(tags_json);
            object? templates_obj = Newtonsoft.Json.JsonConvert.DeserializeObject(templates_json);
            JObject templates_obj2 = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(templates_json);

            List<string> tags_fullname = new List<string>();
            foreach (var tag in tags_obj)
            {
                string name = tag["name"].Value<string>();
                JToken? type = tag["type"];
                JToken? type_code = type["code"];
                string code = type_code.Value<string>();
                List<string> subNames = GetSubNames(code, templates_obj2);

                if (subNames == null)
                {
                    string typename = GetTypeName(type);
                    tags_fullname.Add(name + " [" + typename + "]");
                }
                else
                {
                    foreach (string subName in subNames)
                    {
                        string fullname = name + "." + subName;
                        tags_fullname.Add(fullname);
                    }
                }
            }

            tags_fullname.Sort();

            Dispatcher.BeginInvoke(() =>
            {

                foreach (string tag_fullname in tags_fullname)
                {
                    list.Items.Add(tag_fullname);
                }
                Title = tags_fullname.Count.ToString();
            });
        }

        string GetTypeName(JToken? templates_obj2)
        {
            return templates_obj2["typeName"].Value<string>();
        }

        List<string> GetSubNames(string code, JObject? templates_obj2)
        {
            List<string> subnames = new List<string>();

            var template = templates_obj2[code];

            if (template == null)
            {
                return null;
            }

            foreach (var member in template["_members"])
            {
                string sn = "";
                var name = member["name"];
                sn = name.Value<string>();


                string typename = member["type"]["string"].Value<string>();
                subnames.Add(sn + " [" + typename + "]");
            }

            return subnames;
        }
    }
}