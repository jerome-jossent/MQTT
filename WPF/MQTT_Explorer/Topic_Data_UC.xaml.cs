using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using Xceed.Wpf.AvalonDock.Layout;
using TreeView_JJO;
using Newtonsoft.Json.Linq;
using System.Windows.Media;
using System.Drawing;
using Microsoft.VisualBasic;
using ObjectExplorer_JJO;

namespace MQTT_Explorer
{
    public partial class Topic_Data_UC : UserControl
    {        
        internal static Dictionary<LayoutAnchorable, Topic_Data_UC> _AvalonDockWindow = new Dictionary<LayoutAnchorable, Topic_Data_UC>();
        
        public Topic_Data _data { get; set; }
        public Action<object, SolidColorBrush> SetSameColorForAllSameValues { get; }

        TreeViewItem_JSON_UC current_TreeViewItem_JSON;

        public Topic_Data_UC(Topic_Data data, Action<object, SolidColorBrush> setSameColorForAllSameValues)
        {
            InitializeComponent();

            DataContext = this;


            _data = data;
            SetSameColorForAllSameValues = setSameColorForAllSameValues;
            _data.uc = this;

            _Update();

            ContextMenu_Init();
        }

       public void _Update()
        {
           switch (_data.type)
            {
                case Topic_Data.TopicType.data:
                case Topic_Data.TopicType.valeur:
                    ObjectExplorer_ST.ClearTreeView(trv);
                    DisplayTxt(_data.texte_UTF8);
                    break;

                case Topic_Data.TopicType.json:
                    DisplayTxt("");
                            DisplayTreeView(_data.jsonObject_NS);

                    break;
            }
        }

        void ContextMenu_Init()
        {
            ContextMenu cm = new ContextMenu();

            List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>()
            {
                System.Windows.Media.Color.FromArgb(255,255,55,55),
                System.Windows.Media.Color.FromArgb(255,55,255,55),
                System.Windows.Media.Color.FromArgb(255,55,55,255),

                System.Windows.Media.Color.FromArgb(255,255,255,55),
                System.Windows.Media.Color.FromArgb(255,55,255,255),
                System.Windows.Media.Color.FromArgb(255,255,55,255),
               
                System.Windows.Media.Color.FromArgb(255,100,100,100),
                System.Windows.Media.Color.FromArgb(255,255,255,255),
                System.Windows.Media.Color.FromArgb(255,200,200,200),
            };

            for (int i = 0; i < colors.Count; i++)
            {
                Label l = new Label();
                l.Margin = new Thickness(-45, 0, 0, 0);
                l.Background = new SolidColorBrush(colors[i]);
                l.Width = l.Height = 20;
                MenuItem m = new MenuItem() { Header = l };
                m.Tag = new SolidColorBrush(colors[i]);
                m.Width = m.Height = 25;
                m.Margin = new Thickness(1);
                m.Click += M_Click;
                cm.Items.Add(m);
            }
            trv.ContextMenu = cm;
        }

        void M_Click(object sender, RoutedEventArgs e)
        {
            MenuItem m = (MenuItem)sender;
            //current_TreeViewItem_JSON._tbk_value.Background = (SolidColorBrush)m.Tag;
            SetSameColorForAllSameValues(current_TreeViewItem_JSON._tbk_value.Text, (SolidColorBrush)m.Tag);
        }


        private void trv_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            current_TreeViewItem_JSON = e.Source as TreeViewItem_JSON_UC;
        }

        private void TEST(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ContextMenu_Init();
        }



        internal void _TreeViewUpdate(Dictionary<object, SolidColorBrush> coloration_rules)
        {
            if (_data.objectExplorer2 == null)
            {
                _data.objectExplorer2 = new ObjectExplorer2(this._data.jsonObject_NS, trv);
                _data.objectExplorer2.ExpandAll(trv, true);
            }
            _data.objectExplorer2.TreeViewUpdate(coloration_rules);
        }


        void DisplayTreeView(JsonObject objetFromJson)
        {
            _ = System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                if (objetFromJson != null)
                {
                    ObjectExplorer_ST.FillTreeView(trv, objetFromJson);
                    ObjectExplorer_ST.ExpandAll(trv, true);
                }
            }));
        }


        void DisplayTreeView(JObject objetFromJson) //Newtonsoft
        {
            _ = System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                if (objetFromJson != null)
                {
                    //ObjectExplorer_NS.FillTreeView(trv, objetFromJson);
                    //ObjectExplorer_NS.ExpandAll(trv, true);

                    //if (objectExplorer2 == null)
                    {
                        _data.objectExplorer2 = new ObjectExplorer2(objetFromJson, trv);
                        _data.objectExplorer2.ExpandAll(trv, true);
                    }
                    //else
                    //{

                    //}
                }
            }));
        }

        void DisplayTxt(string texte)
        {
            _ = Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(texte))
                {
                    txt.Visibility = Visibility.Collapsed;
                }
                else
                {
                    txt.Visibility = Visibility.Visible;
                    txt.Text = texte;
                }
            }));
        }

        private void Copy_to_clipboard(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Clipboard.SetText(_data.texte_UTF8);
        }

        private void Publish_again(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _data.topic.PublishAgain(_data);
        }

        private void SetFilter(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}
