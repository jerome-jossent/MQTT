using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;
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
using Advise_Common_WPF_v2;
using Advise_Common_WPF_v2.Class;
using Microsoft.Win32;
using MQTTnet;
using MQTTnet.Server;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace MQTT_Server
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        protected void OnPropertyChanged(string? name = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<DebugMessage_UC> DebugMessages
        {
            get { return debugMessages; }
            set
            {
                debugMessages = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<DebugMessage_UC> debugMessages = new ObservableCollection<DebugMessage_UC>();

        public ObservableCollection<login_password> _loginPasswords
        {
            get { return loginPasswords; }
            set
            {
                loginPasswords = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<login_password> loginPasswords = new ObservableCollection<login_password>();

        public string _port_string
        {
            get => configuration?.port.ToString();
            set
            {
                if (int.TryParse(value, out int p))
                    _port = p;
            }
        }

        public int? _port
        {
            get => configuration?.port;
            set
            {
                if (value == null)
                    return;
                configuration.port = (int)value;
                OnPropertyChanged();
                OnPropertyChanged("_port_string");
            }
        }

        public ObservableCollection<Client> _clients
        {
            get { return clients; }
            set
            {
                clients = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<Client> clients = new ObservableCollection<Client>();

        public bool? _allow_anonymous
        {
            get => configuration?.allow_anonymous;
            set
            {
                if (value == null)
                    return;
                configuration.allow_anonymous = (bool)value;
                if (serverMQTTinterne != null)
                    serverMQTTinterne.allowAnonymous = configuration.allow_anonymous;
                OnPropertyChanged();
            }
        }


        Advise_Common_WPF_v2.MQTT_Server serverMQTTinterne;
        Configuration configuration;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Configuration_Init())
                SERVER_MQTT_START();
            else
                _tb_Settings.IsSelected = true;
        }

        void Parameters_Init() //CHARGER UN FICHIER DE CONFIGURATION
        {
            loginPasswords.Clear();
            foreach (var item in configuration.logins_passwords)
            {
                var lp = new login_password(this, item.Key, item.Value);
                loginPasswords.Add(lp);
            }
            OnPropertyChanged("_port_string");
            OnPropertyChanged("_allow_anonymous");
            DEBUG_PRINT("Configuration loaded", Colors.Pink);
        }


        bool Configuration_Init()
        {
            string lastConfigPath = Properties.Settings.Default.last_configuration_file_path;
            if (string.IsNullOrEmpty(lastConfigPath) || !System.IO.File.Exists(lastConfigPath))
                return false;
            try
            {
                configuration = Configuration.Load(lastConfigPath);
                Parameters_Init();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Load error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            SERVER_STOP();
        }

        void SERVER_MQTT_START()
        {
            Dictionary<string, string> lm = new Dictionary<string, string>();

            foreach (login_password lp in loginPasswords)
                if (lp._on)
                    lm.Add(lp._login, lp._password);

            serverMQTTinterne = new Advise_Common_WPF_v2.MQTT_Server((int)_port, lm, DEBUG_PRINT);
            serverMQTTinterne.onClientConnected += Client_Hi;
            serverMQTTinterne.onClientDisconnected += Client_ByeBye;
            serverMQTTinterne.START();

            serverMQTTinterne.allowAnonymous =             configuration.allow_anonymous ;


            DEBUG_PRINT("Server started", Colors.Pink);
        }

        void Client_ByeBye(object? sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                ClientDisconnectedEventArgs cde = e as ClientDisconnectedEventArgs;
                Client client = null;
                foreach (var c in clients)
                {
                    if (cde.ClientId == c.ClientId)
                    {
                        client = c;
                        break;
                    }
                }
                if (client != null)
                    clients.Remove(client);
            });
        }

        void Client_Hi(object? sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                ValidatingConnectionEventArgs vce = e as ValidatingConnectionEventArgs;
                Client client = new Client(vce.ClientId, vce.Username);
                clients.Add(client);
            });
        }

        internal void DEBUG_PRINT(string v = null, System.Windows.Media.Color? color = null)
        {
            string method_name = (new StackTrace()).GetFrame(1).GetMethod().Name;
            _ = System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                DEBUG_PRINT(new DebugMessage_UC(v,
                    method_name,
                    color));
            }));
        }
        internal void DEBUG_PRINT(DebugMessage_UC d)
        {
            _ = System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                DebugMessages.Insert(0, d);
                while (DebugMessages.Count > 300)
                    DebugMessages.RemoveAt(DebugMessages.Count - 1);
                OnPropertyChanged("DebugMessages");
            }));
        }

        void SERVER_STOP()
        {
            serverMQTTinterne?.STOP();
        }

        void btn_login_mdp_add(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(_tbx_login.Text) || string.IsNullOrEmpty(_tbx_password.Text))
                return;

            login_password lp = new login_password(this, _tbx_login.Text, _tbx_password.Text, true);
            loginPasswords.Add(lp);

        }

        void btn_reset(object sender, MouseButtonEventArgs e)
        {
            SERVER_STOP();
            Thread.Sleep(500);
            SERVER_MQTT_START();
        }

        internal void Delete(login_password login_password)
        {
            loginPasswords.Remove(login_password);
        }

        void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            login_password lp = e.AddedItems[0] as login_password;
            _tbx_login.Text = lp._login;
            _tbx_password.Text = lp._password;
        }

        void btn_save(object sender, MouseButtonEventArgs e)
        {
            //Save as
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Fichiers configuration (*.cfg)|*.cfg",
                DefaultExt = "cfg",
                Title = "Sauvegarder la configuration de MQTT Server"
            };

            if (saveFileDialog.ShowDialog() != true)
                return;

            //            configuration.logins_passwords_uncrypted = loginPasswords.ToList();           

            configuration.logins_passwords.Clear();
            foreach (login_password lp in loginPasswords)
            {
                configuration.logins_passwords.Add(lp._login, lp._password);
            }

            configuration.Save(saveFileDialog.FileName);

            Properties.Settings.Default.last_configuration_file_path = saveFileDialog.FileName;
            Properties.Settings.Default.Save();
            DEBUG_PRINT("Configuration saved", Colors.Pink);
        }

        void btn_load(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Fichiers configuration (*.cfg)|*.cfg",
                Title = "Charger la configuration de MQTT Server"
            };

            if (openFileDialog.ShowDialog() != true)
                return;

            configuration = Configuration.Load(openFileDialog.FileName);
            Parameters_Init();

            Properties.Settings.Default.last_configuration_file_path = openFileDialog.FileName;
            Properties.Settings.Default.Save();

            SERVER_STOP();
            Thread.Sleep(500);
            SERVER_MQTT_START();
        }
    }
}