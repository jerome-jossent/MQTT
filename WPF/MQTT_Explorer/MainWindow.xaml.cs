using MQTTnet;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock.Layout;
using System.Runtime.InteropServices;
using ObjectExplorer_JJO;
using Microsoft.Win32;
using System.IO;
using OfficeOpenXml;
using System.Diagnostics;
using Color = System.Windows.Media.Color;
using MQTTnet.Client;
using System.Collections;
using System.Text;

namespace MQTT_Explorer
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region BINDING
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Visibility _grd_connection_visibility
        {
            get => grd_connection_visibility; set
            {
                grd_connection_visibility = value;
                OnPropertyChanged();
            }
        }
        Visibility grd_connection_visibility = Visibility.Collapsed;

        public Visibility _img_connection_visibility
        {
            get => img_connection_visibility; set
            {
                img_connection_visibility = value;
                OnPropertyChanged();
            }
        }
        Visibility img_connection_visibility = Visibility.Visible;

        public SolidColorBrush _ell_connection_color
        {
            get => ell_connection_color; set
            {
                ell_connection_color = value;
                OnPropertyChanged();
            }
        }
        SolidColorBrush ell_connection_color = System.Windows.Media.Brushes.Red;

        public ObservableCollection<Config> _configs
        {
            get => configs; set
            {
                configs = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<Config> configs;

        public Config _config
        {
            get => config; set
            {
                if (value == null) return;
                config = value;
                OnPropertyChanged();
            }
        }
        Config config;

        public string _mqtt_broker_ip
        {
            get => _config.mqtt_broker_ip; set
            {
                _config.mqtt_broker_ip = value;
                OnPropertyChanged();
            }
        }

        //public string _mqtt_broker_port_string
        //{
        //    get => _mqtt_broker_port.ToString(); set
        //    {
        //        if (!int.TryParse(value, out int res))
        //            return;
        //        _mqtt_broker_port = res;
        //    }
        //}

        public int _mqtt_broker_port
        {
            get => _config.mqtt_broker_port; set
            {
                _config.mqtt_broker_port = value;
                OnPropertyChanged();
                //OnPropertyChanged(nameof(_mqtt_broker_port_string));
            }
        }

        public string _mqtt_ID
        {
            get => _config.mqtt_ID; set
            {
                _config.mqtt_ID = value;
                OnPropertyChanged();
            }
        }

        public bool? _mqtt_anonymous
        {
            get => _config.mqtt_anonymous; set
            {
                _config.mqtt_anonymous = (bool)value;
                OnPropertyChanged();
            }
        }

        public string _mqtt_login
        {
            get => _config.mqtt_login; set
            {
                _config.mqtt_login = value;
                OnPropertyChanged();
            }
        }

        public string _mqtt_password
        {
            get => _config.mqtt_password; set
            {
                _config.mqtt_password = value;
                OnPropertyChanged();
            }
        }

        public string _config_name
        {
            get => _config.name; set
            {
                _config.name = value;
                OnPropertyChanged();
            }
        }


        public int datatotalsizemax_mo
        {
            get => _datatotalsizemax_mo;
            set
            {
                if (value < 1)
                    value = 1;
                if (value > 200)
                    value = 200;
                _datatotalsizemax_mo = value;
                OnPropertyChanged();
            }
        }
        int _datatotalsizemax_mo = (int)10;

        static public int data_total_size_max { get => mainWindow.datatotalsizemax_mo * 1000000; }

        static MainWindow mainWindow;

        #endregion

        #region Constante
        //couleur ellipse
        Color vert = Color.FromRgb(50, 200, 80);
        SolidColorBrush VERT = new SolidColorBrush(Color.FromRgb(50, 200, 80));

        //couleurs timeline
        SolidColorBrush tl_vert = new SolidColorBrush(Color.FromRgb(50, 200, 80));
        SolidColorBrush tl_rouge = new SolidColorBrush(Color.FromRgb(200, 80, 80));
        SolidColorBrush tl_gris = new SolidColorBrush(Color.FromRgb(240, 240, 240));
        SolidColorBrush tl_jaune = new SolidColorBrush(Color.FromRgb(240, 240, 0));
        SolidColorBrush tl_noir = new SolidColorBrush(Color.FromRgb(40, 40, 40));
        #endregion

        #region PARAMETERS
        int tryToJSONIfSizeUnder = 5000;
        public static bool _data_top_is_last = true;
        #endregion

        #region VARIABLES
        MQTTnet.Client.IMqttClient mqtt_explorer;

        public ObservableCollection<Topic> _topics { get; set; } = new ObservableCollection<Topic>();
        Dictionary<string, Topic> topics_dico = new Dictionary<string, Topic>();

        Dictionary<object, SolidColorBrush> coloration_rules = new Dictionary<object, SolidColorBrush>();
        List<Topic_Data_UC> topic_Data_UCs_Visible = new List<Topic_Data_UC>();

        Dictionary<string, List<TreeViewItem_JSON_UC>> valeurs_treeViewItemcorrespondant = new Dictionary<string, List<TreeViewItem_JSON_UC>>();
        Dictionary<string, SolidColorBrush> valeurs_couleurcorrespondantes = new Dictionary<string, SolidColorBrush>();

        public Topic _currenttopic
        {
            get => currenttopic;
            set
            {
                if (value == null)
                    return;

                currenttopic = value;
                OnPropertyChanged();
                if (currenttopic.datas.Count > 0)
                    _currentdata = currenttopic.datas.First(); // = last car trié dans le sens inverse
                //SelectLastData();
            }
        }
        Topic currenttopic;

        public Topic_Data _currentdata
        {
            get => currentdata;
            set
            {
                if (value == null)
                    return;

                currentdata = value;
                OnPropertyChanged();

                DisplayData(_currentdata, false);
            }
        }
        Topic_Data currentdata;

        Dictionary<DateTime, string> datas_par_topic = new Dictionary<DateTime, string>();
        List<DateTime> listeDatesSortees;

        int publish_Tab_index = 0;
        #endregion

        #region Window Events (Constructor)
        public MainWindow()
        {
            Config_INIT();

            InitializeComponent();
            DataContext = this;

            mainWindow = this;
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (config.name != "New")
                //auto connect
                MQTTExplorer_Start();

            if (mqtt_explorer == null || !mqtt_explorer.IsConnected)
                _grd_connection_visibility = Visibility.Visible;

            timer = new Timer(ExecuteTest, null, 0, 100);
        }

        void Tbk2_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Publish_UC puc = (((sender as TextBlock).Parent as TabItem).Content as Publish_UC);
            //puc.
        }


        void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            MQTTExplorer_Stop();
        }
        #endregion

        #region CONFIG
        void Config_INIT(Config? auto = null)
        {
            List<Config> cfgs = Config.Load();
            _configs = new ObservableCollection<Config>();
            foreach (Config c in cfgs)
                _configs.Add(c);

            if (cfgs.Count == 0)
            {
                Config cfg = new Config()
                {
                    name = "New",
                    mqtt_broker_ip = "127.0.0.1",
                    mqtt_broker_port = 1883,
                };

                _configs.Add(cfg);
            }
            if (auto != null)
                Config_Load(auto);
            else
                Config_Load(_configs[0]);
        }

        void img_config_save_Click(object sender, MouseButtonEventArgs e)
        {
            _config.Save();
            //update de la liste des configs ET charge la config
            Config_INIT(_config);
        }

        //void img_config_load_Click(object sender, MouseButtonEventArgs e)
        //{
        //    OpenFileDialog openFileDialog = new OpenFileDialog();
        //    openFileDialog.Filter = "MQTT Explorer config|*.cfg";
        //    if (openFileDialog.ShowDialog() == false)
        //        return;
        //    //_config = Config.Load(openFileDialog.FileName);
        //    //Config_Load

        //}

        void Config_Load(Config cfg)
        {
            _config = cfg;
            OnPropertyChanged(nameof(_mqtt_broker_ip));
            OnPropertyChanged(nameof(_mqtt_broker_port));
            OnPropertyChanged(nameof(_mqtt_ID));
            OnPropertyChanged(nameof(_mqtt_anonymous));
            OnPropertyChanged(nameof(_mqtt_login));
            OnPropertyChanged(nameof(_mqtt_password));
            OnPropertyChanged(nameof(_config_name));
        }

        void Config_new_Click(object sender, MouseButtonEventArgs e)
        {
            Config_Load(new Config());
        }

        void Config_Delete_Click(object sender, MouseButtonEventArgs e)
        {
            var config = (sender as FrameworkElement)?.DataContext as Config;
            if (config != null)
            {
                _configs.Remove(config);
                Config.Save(_configs);
            }
        }
        #endregion

        #region MQTT Management
        void Connection_Panel(object sender, RoutedEventArgs e)
        {
            //affiche / masque le pannel
            _grd_connection_visibility = (_grd_connection_visibility == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;
        }

        void img_Connection_Click(object sender, MouseButtonEventArgs e)
        {
            MQTTExplorer_Start();
        }

        void img_Disconnection_Click(object sender, MouseButtonEventArgs e)
        {
            MQTTExplorer_Stop();
        }

        void MQTTExplorer_Start()
        {
            var mqttFactory = new MqttFactory();
            mqtt_explorer = mqttFactory.CreateMqttClient();

            MQTTnet.Client.MqttClientOptions mqttClientOptions;

            if (_mqtt_anonymous == false)
                mqttClientOptions = new MQTTnet.Client.MqttClientOptionsBuilder()
                .WithClientId(_mqtt_ID)
                .WithTcpServer(_mqtt_broker_ip, _mqtt_broker_port)
                .WithCredentials(_mqtt_login, _mqtt_password)
                .WithoutThrowOnNonSuccessfulConnectResponse()
                .Build();
            else
                mqttClientOptions = new MQTTnet.Client.MqttClientOptionsBuilder()
                .WithClientId(_mqtt_ID)
                .WithTcpServer(_mqtt_broker_ip, _mqtt_broker_port)
                //.WithCredentials(_mqtt_login, _mqtt_password)
                .WithoutThrowOnNonSuccessfulConnectResponse()
                .Build();

            mqtt_explorer.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;

            mqtt_explorer.ConnectingAsync += MqttClient_ConnectingAsync;
            mqtt_explorer.ConnectedAsync += MqttClient_ConnectedAsync;
            mqtt_explorer.DisconnectedAsync += MqttClient_DisconnectedAsync;

            try
            {
                MQTTnet.Client.MqttClientConnectResult rep = mqtt_explorer.ConnectAsync(mqttClientOptions, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
                if (rep.ResultCode != MQTTnet.Client.MqttClientConnectResultCode.Success)
                    MessageBox.Show(rep.ResultCode.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void MQTTExplorer_Stop()
        {
            if (mqtt_explorer == null) return;

            var mqttClientOptions = new MQTTnet.Client.MqttClientDisconnectOptionsBuilder()
                .Build();

            mqtt_explorer.DisconnectAsync(mqttClientOptions, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
        }

        Task MqttClient_ConnectingAsync(MQTTnet.Client.MqttClientConnectingEventArgs arg)
        {
            _ell_connection_color = System.Windows.Media.Brushes.Yellow;
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        Task MqttClient_ConnectedAsync(MQTTnet.Client.MqttClientConnectedEventArgs arg)
        {
            _img_connection_visibility = Visibility.Collapsed;

            _ell_connection_color = VERT;
            MQTTClient_SubscribesToAllTopics();
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        Task MqttClient_DisconnectedAsync(MQTTnet.Client.MqttClientDisconnectedEventArgs arg)
        {
            _img_connection_visibility = Visibility.Visible;
            _ell_connection_color = System.Windows.Media.Brushes.Red;
            return MQTTnet.Internal.CompletedTask.Instance;
        }

        void MQTTClient_SubscribesToAllTopics()
        {
            var mqtt_Subscriber = new MQTTnet.Client.MqttClientSubscribeOptions();

            var filter = new MQTTnet.Packets.MqttTopicFilter()
            {
                Topic = "#",
                QualityOfServiceLevel = 0
            };
            mqtt_Subscriber.TopicFilters.Add(filter);

            mqtt_explorer.SubscribeAsync(mqtt_Subscriber, System.Threading.CancellationToken.None);
        }

        void MQTT_Publish(string topic, string payload, bool retain = false)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(payload);
            MQTT_Publish(topic, bytes, retain);
        }

        void MQTT_Publish(string topic, byte[] payload, bool retain = false)
        {
            if (mqtt_explorer == null || !mqtt_explorer.IsConnected)
                return;

            MqttApplicationMessage applicationMessage = new MqttApplicationMessageBuilder()
                                                            .WithTopic(topic)
                                                            .WithPayload(payload)
                                                            .WithRetainFlag(retain)
                                                            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                                                            .Build();

            mqtt_explorer.PublishAsync(applicationMessage, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
        }

        Task MqttClient_ApplicationMessageReceivedAsync(MQTTnet.Client.MqttApplicationMessageReceivedEventArgs arg)
        {
            string msg_topic = arg.ApplicationMessage.Topic;
            byte[]? msg_data = arg.ApplicationMessage.PayloadSegment.Array;
            ManageIncommingMessage(msg_topic, msg_data);

            return MQTTnet.Internal.CompletedTask.Instance;
        }

        void MQTT_Publish(Topic_Data topic_Data)
        {
            MQTT_Publish(topic_Data.topic.topic, topic_Data.data);
        }
        #endregion

        #region PUBLISH
        void PublishPanel_ShowOrAdd(object sender, RoutedEventArgs e)
        {
            Publish_Init();
        }

        void Publish_Init()
        {
            Publish_UC puc = new Publish_UC();
            puc._Publish_Event += Publish_Event;
            NewWindow_pub_integrated(puc);
        }

        void Publish_Event(object? sender, Publish_UC.PublishArguments e)
        {
            MQTT_Publish(e.topic, e.payload_string, e.retain);
        }

        #endregion



        void ManageIncommingMessage(string msg_topic, byte[]? msg_data)
        {
            Topic topic;

            lock (locker)
            {
                //update dictionnaries
                if (!topics_dico.ContainsKey(msg_topic))
                {
                    topic = new Topic(msg_topic, MQTT_Publish);
                    topics_dico.Add(msg_topic, topic);

                    // dans listview
                    _ = Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _topics.Add(topic);
                    }));

                }
                topic = topics_dico[msg_topic];
                Topic_Data d = topic.Data_New(msg_data, tryToJSONIfSizeUnder);

                //////////////////////////////Update_ValeursIdentiques(d);

                if (_currenttopic != null && msg_topic == _currenttopic.topic)
                    SelectLastData();

                SortTopics();
            }
            //TimeLinesUpdate();
            needtoupdate = true;
        }

        void Update_ValeursIdentiques(Topic_Data d)
        {



            foreach (TreeViewItem_JSON_UC tvi_JSON in d.objectExplorer2.tvi_ucs.Values)
            {
                string? v = tvi_JSON._value;
                if (string.IsNullOrEmpty(v)) continue;

                //est ce que cette valeur existe déjà dans le dictionnaire ?
                if (!valeurs_treeViewItemcorrespondant.ContainsKey(v))
                    valeurs_treeViewItemcorrespondant.Add(v, new List<TreeViewItem_JSON_UC>());

                valeurs_treeViewItemcorrespondant[v].Add(tvi_JSON);

                SolidColorBrush col;
                //s'agit il du 2ème élément ?
                if (valeurs_treeViewItemcorrespondant[v].Count == 2)
                {
                    //alors il faut trouver une nouvelle couleur et colorier aussi le premier !
                    List<SolidColorBrush> currentColors = valeurs_couleurcorrespondantes.Values.ToList();
                    col = ColorGenerator.GetMostDifferentColor(currentColors);

                    valeurs_couleurcorrespondantes.Add(v, col);
                    //colorier le premier
                    valeurs_treeViewItemcorrespondant[v].First()._SetBackGroundColor(col);
                }

                //le nouveau
                if (valeurs_treeViewItemcorrespondant[v].Count > 1)
                {
                    //retrouver la même couleur que les autres
                    col = valeurs_couleurcorrespondantes[v];
                    tvi_JSON._SetBackGroundColor(col);
                }
            }
        }

        object locker = new object();
        bool needtoupdate;
        Timer timer;
        private void ExecuteTest(object state)
        {
            if (!needtoupdate)
                return;

            needtoupdate = false;
            TimeLinesUpdate();
        }


        #region Timeline
        void Topics_TimeLine_Refresh(object sender, RoutedEventArgs e)
        {
            TimeLinesUpdate();
        }

        void TimeLinesUpdate()
        {
            Dispatcher.Invoke(() =>
            {
                lock (locker)
                {
                    datas_par_topic.Clear();

                    //on veut connaître la séquence de tous les messages (=topics confondus)
                    foreach (var topic in topics_dico.Values)
                        foreach (var data in topic.datas_dico.Values)
                            datas_par_topic.Add(data.date, topic.topic);
                    //toutes les data triées par date :
                    listeDatesSortees = datas_par_topic.Keys.OrderBy(date => date).ToList();

                    foreach (var topic in topics_dico.Values)
                    {
                        //création de la timeline
                        bool?[] timeline = new bool?[listeDatesSortees.Count];
                        for (int i = 0; i < listeDatesSortees.Count; i++)
                        {
                            if (datas_par_topic[listeDatesSortees[i]] == topic.topic)
                            {
                                DateTime t = listeDatesSortees[i];

                                if (topic.datas_dico[t].data_deleted)
                                    timeline[i] = false;
                                else
                                    timeline[i] = true;
                            }
                        }

                        //création des boutons
                        topic.timeline_buttons = DrawTimeLine_Buttons(timeline, topic);
                    }
                }
            });
        }

        ObservableCollection<Button> DrawTimeLine_Buttons(bool?[] timeline, Topic topic)
        {
            ObservableCollection<Button> buttons = new ObservableCollection<Button>();
            double marge = 10d / timeline.Length;
            for (int x = 0; x < timeline.Length; x++)
            {
                Button btn = new Button();
                SolidColorBrush col;
                if (timeline[x] == true) //data !
                {
                    col = tl_vert;
                    btn.Tag = topic.datas_dico[listeDatesSortees[x]];
                }
                else if (timeline[x] == false) //data effacée car trop volumineuse
                {
                    col = tl_noir;
                    btn.Tag = topic.datas_dico[listeDatesSortees[x]];
                }
                else // pas de data
                {
                    col = tl_gris;
                    btn.IsEnabled = false;
                }

                btn.Background = col;
                btn.BorderThickness = new Thickness(0);
                btn.MouseEnter += Timeline_Btn_MouseEnter;
                btn.Click += Timeline_Btn_Click;

                btn.Margin = new Thickness(marge, -2, marge, -2);

                buttons.Add(btn);
            }

            return buttons;
        }

        void Timeline_Btn_MouseEnter(object sender, MouseEventArgs e)
        {
            object obj = (sender as Button).Tag;
            if (obj == null)
                return;
            Topic_Data topic_data = obj as Topic_Data;
            _currenttopic = topic_data.topic;
            _currentdata = topic_data;
        }

        //pour supprimer les datas (et potentiellement topics) avant cette data
        void Timeline_Btn_Click(object sender, RoutedEventArgs e)
        {
            //supprimer toutes les données avant DATE ?
            object obj = (sender as Button).Tag;
            if (obj == null)
                return;
            Topic_Data topic_data = obj as Topic_Data;

            DateTime T0 = topic_data.date;

            //pour chaque topic, 
            for (int j = topics_dico.Count - 1; j >= 0; j--)
            {
                Topic topic = topics_dico.ElementAt(j).Value;

                for (int i = 0; i < topic.datas.Count; i++)
                {
                    Topic_Data td = topic.datas[i];
                    if (td.date < T0)
                    {
                        topic.Data_Del(td);
                        i--;
                    }
                }

                //si plus aucune data alors suppression des topics
                if (topic.datas.Count == 0)
                    Topic_delete(topic);
            }
            TimeLinesUpdate();
        }
        #endregion

        #region Topics
        void SortTopics()
        {
            _ = Dispatcher.BeginInvoke(new Action(() =>
            {
                //ObservableCollection<Topic> temp = new ObservableCollection<Topic>(_topics.OrderBy(p => p.topic));
                ObservableCollection<Topic> temp = new ObservableCollection<Topic>(_topics.OrderByDescending(p => p.lastDate));
                _topics.Clear();
                foreach (Topic j in temp)
                    _topics.Add(j);
            }));
        }

        void Topics_delete_all(object sender, RoutedEventArgs e)
        {
            List<string> keysToRemove = new List<string>(topics_dico.Keys);

            foreach (var key in keysToRemove)
            {
                Topic topic = topics_dico[key];
                Topic_delete(topic);
            }
        }

        void Topic_delete(object sender, MouseButtonEventArgs e)
        {
            Topic_delete(GetTopic(sender));
            TimeLinesUpdate();
        }

        void Topic_copy(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(GetTopic(sender).topic);
        }

        void Topic_delete(Topic topic)
        {
            foreach (Topic_Data data in topic.datas_dico.Values)
                Topic_deleteDataView(data);
            topic.datas.Clear();
            topic.datas_dico.Clear();

            _ = Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
             {
                 topics_dico.Remove(topic.topic);
                 _topics.Remove(topic);
             }));
        }

        Topic GetTopic(object sender)
        {
            System.Windows.Controls.Image i = sender as System.Windows.Controls.Image;
            return (Topic)i.DataContext;
        }

        void Topic_follow(object sender, MouseButtonEventArgs e)
        {
            Topic topic = GetTopic(sender);
            Topic_follow(topic);
        }

        void Topic_follow(Topic topic)
        {
            if (topic.followed)
            {
                //update only
            }
            else
            {
                //create view
                topic.followed = true;
            }

            List<Topic_Data> diffs = topic.Data_Follow();
            Topic_follow_view uc = new Topic_follow_view(diffs);

            NewWindow_data_floated(uc);
        }
        #endregion

        #region Data
        //on sélectionne la dernière donnée dans la listview
        void SelectLastData()
        {
            _ = Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                if (_data_top_is_last)
                    _lv_datas.SelectedIndex = 0;
                else
                    _lv_datas.SelectedIndex = _lv_datas.Items.Count - 1;
            }));
        }

        void TopicData_NewView(object sender, MouseButtonEventArgs e)
        {
            Topic_Data data = GetTopicData(sender);
            _currentdata = data;
            DisplayData(data, newview: true);
        }

        void SetSameColorForAllSameValues(object value, SolidColorBrush color)
        {
            if (!coloration_rules.ContainsKey(value))
                coloration_rules.Add(value, color);
            else
                coloration_rules[value] = color;

            foreach (Topic_Data_UC topic_Data_UC in topic_Data_UCs_Visible)
                topic_Data_UC._TreeViewUpdate(coloration_rules);

            //foreach (Topic topic in topics_dico.Values)
            //    foreach (Topic_Data topic_Data in topic.datas)
            //        topic_Data.SetSameColorForAllSameValues(value, color);
        }

        void DisplayData(Topic_Data data, bool newview)
        {
            if (data.type == Topic_Data.TopicType.inconnu)
                data.TryToJSON();

            //update type
            data.topic.type = data.type;
            Topic_Data_UC tduc;
            //if (data.uc == null)
            tduc = new Topic_Data_UC(data, SetSameColorForAllSameValues);
            topic_Data_UCs_Visible.Add(tduc);
            tduc = data.uc;

            //NewWindow_integrated(tduc);
            // Créer une nouvelle fenêtre
            LayoutAnchorable nouvelleFenetre = new LayoutAnchorable
            {
                CanClose = true,
                CanHide = false,
                Title = data.index.ToString(),
                Content = tduc,
            };
            Topic_Data_UC._AvalonDockWindow.Add(nouvelleFenetre, tduc);
            data.fenetre = nouvelleFenetre;
            nouvelleFenetre.Closing += TopicData_View_Closing;

            LayoutAnchorablePane nouveauPane = new LayoutAnchorablePane();
            data.pane = nouveauPane;

            nouveauPane.Children.Add(nouvelleFenetre);

            if (_datas_uc_group.Children.Count == 0 || newview)
            {
                _datas_uc_group.Children.Add(nouveauPane);
            }
            else
            {
                LayoutAnchorablePane lap = (LayoutAnchorablePane)_datas_uc_group.Children[0];


                //topic_Data_UCs_Visible


                _datas_uc_group.Children[0] = nouveauPane;
            }
            //}
        }

        private void TopicData_View_Closing(object? sender, CancelEventArgs e)
        {
            //closing existe mais quand on ferme la fenêtre avalondock, en fait on détruit le parent !
            LayoutAnchorable fenetre = (LayoutAnchorable)sender;

            if (!Topic_Data_UC._AvalonDockWindow.ContainsKey(fenetre))
                return;

            Topic_Data_UC uc = Topic_Data_UC._AvalonDockWindow[fenetre];

            topic_Data_UCs_Visible.Remove(uc);

            uc._data.uc = null; //raz vue

            if (fenetre.Parent.GetType() == typeof(LayoutDocumentPane))
            {
                e.Cancel = true;
            }
            else
            {
                LayoutAnchorablePane nouveauPane = (LayoutAnchorablePane)fenetre.Parent;
                _datas_uc_group.Children.Remove(nouveauPane);
                Topic_Data_UC._AvalonDockWindow.Remove(fenetre);
            }

            //e.Cancel = true;
        }

        //void lv_topics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (e.AddedItems.Count == 0) return;
        //    _currenttopic = (Topic)e.AddedItems[0];

        //    SelectLastData();
        //}

        //void lv_datas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (e.AddedItems.Count == 0) return;
        //    Topic_Data data = (Topic_Data)e.AddedItems[0];
        //    _currentdata = data;
        //    DisplayData(data, false, Topic_Data_UC.Json_type.newtonsoft);
        //}


        void Topic_deleteDataView(Topic_Data data)
        {
            if (data.fenetre != null)
            {
                LayoutAnchorable fenetre = data.fenetre;
                fenetre.Close();
            }
        }

        void TopicData_delete(object sender, MouseButtonEventArgs e)
        {
            Topic_Data topic_Data = GetTopicData(sender);
            Topic_deleteDataView(topic_Data);
            topic_Data.topic.datas.Remove(topic_Data);
            topic_Data.topic.datas_dico.Remove(topic_Data.date);
        }

        void TopicData_copy(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(GetTopicData(sender).texte_UTF8);
        }

        Topic_Data GetTopicData(object sender)
        {
            System.Windows.Controls.Image i = sender as System.Windows.Controls.Image;
            return (Topic_Data)i.DataContext;
        }

        #endregion

        #region AvalonDock
        void NewWindow_data_floated(Control uc)
        {
            var w = new Window();
            w.Content = uc;
            w.SizeToContent = SizeToContent.WidthAndHeight;
            w.Show();
        }

        void NewWindow_data_integrated(Control uc)
        {
            // Créer une nouvelle fenêtre
            LayoutAnchorable nouvelleFenetre = new LayoutAnchorable
            {
                CanClose = true,
                CanHide = false,
                Title = "Fenêtre Intégrée",
                Content = uc,
            };
            //nouvelleFenetre.Closing += TopicData_View_Closing;
            LayoutAnchorablePane nouveauPane = new LayoutAnchorablePane();
            nouveauPane.Children.Add(nouvelleFenetre);

            _datas_uc_group.Children.Add(nouveauPane);

            //rattache (et rend visible) s'il a été fermé !
            if (_datas_uc_group.Root == null)
                _layoutPanel.Children.Add(_datas_uc_group);

            nouvelleFenetre.IsActive = true;
        }

        void NewWindow_pub_integrated(Control uc)
        {
            LayoutAnchorable nouvelleFenetre = new LayoutAnchorable
            {
                CanClose = true,
                CanHide = false,
                Title = "Pub " + publish_Tab_index++,
                Content = uc,
            };
            LayoutAnchorablePane nouveauPane = new LayoutAnchorablePane();
            nouveauPane.Children.Add(nouvelleFenetre);

            _publish_group.Children.Add(nouveauPane);

            //rattache (et rend visible) s'il a été fermé !
            if (_publish_group.Root == null)
                _layoutPanel.Children.Add(_publish_group);
        }
        #endregion

        #region SAVE Topics data TO
        void SaveToExcelFile(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog()
            {
                Filter = "Excel File(*.xlsx)|*.xlsx"
            };
            if (dialog.ShowDialog() != true)
                return;

            System.IO.FileInfo fileInfo = new System.IO.FileInfo(dialog.FileName);

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            // Créer un nouveau fichier Excel
            using (var package = new ExcelPackage())
            {
                foreach (Topic topic in topics_dico.Values)
                {
                    string topic_filename = topic.topic;
                    topic_filename = topic_filename.Replace("/", "_");
                    topic_filename = topic_filename.Replace("\\", "_");

                    // Ajouter une nouvelle feuille et la nommer
                    var worksheet = package.Workbook.Worksheets.Add(topic_filename);

                    int col = 0;
                    int lignedeb = 5;

                    foreach (Topic_Data data in topic.datas_dico.Values)
                    {
                        col++;
                        worksheet.Cells[1, col].Value = data.index;
                        //worksheet.Cells[2, col].Value = data.date.ToString("yyyy-MM-dd HH_mm_ss.fff");
                        worksheet.Cells[2, col].Value = data.date.ToString("HH:mm:ss.fff");

                        var lignes = data.texte_UTF8.Split(new string[] { "\n" }, options: StringSplitOptions.None);
                        for (int i = 0; i < lignes.Length; i++)
                            worksheet.Cells[i + lignedeb, col].Value = lignes[i];
                    }
                }

                // Sauvegarder le fichier
                package.SaveAs(fileInfo);

                if (MessageBox.Show("Done !\n\nOpen file ?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    System.Diagnostics.Process.Start(new ProcessStartInfo(fileInfo.FullName) { UseShellExecute = true });
            }
        }

        void SaveToFolder(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            if (openFolderDialog.ShowDialog() != true)
                return;

            DirectoryInfo directoryInfo = new DirectoryInfo(openFolderDialog.FolderName);
            if (!directoryInfo.Exists)
                directoryInfo.Create();

            foreach (Topic topic in topics_dico.Values)
            {
                string topic_filename = topic.topic;
                topic_filename = topic_filename.Replace("/", "\\");
                string foldername = directoryInfo.FullName + "\\" + topic_filename;
                DirectoryInfo di = new DirectoryInfo(foldername);
                if (!di.Exists)
                    di.Create();

                foreach (Topic_Data data in topic.datas_dico.Values)
                {
                    string data_filename = "[" + data.index.ToString("D3") + "] " + data.date.ToString("yyyy-MM-dd HH_mm_ss.fff");
                    string fullfilename = di.FullName + "\\" + data_filename + ".json";
                    System.IO.File.WriteAllText(fullfilename, data.texte_UTF8);
                }
            }

            if (MessageBox.Show("Done !\n\nOpen folder ?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                System.Diagnostics.Process.Start("explorer.exe", directoryInfo.FullName);
        }
        #endregion

    }
}