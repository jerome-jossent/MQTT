using MQTT_Manager_jjo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static MQTT_Manager_jjo.MQTT_One_Topic_Subscribed;

namespace MQTT_StringToFile
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // ─────────────────────────────────────────────
        //  Logger partagé
        // ─────────────────────────────────────────────
        String2Disk _logger = new String2Disk("Logs");

        // ─────────────────────────────────────────────
        //  Dictionnaire : topic ──► fichier
        // ─────────────────────────────────────────────
        private readonly Dictionary<MQTT_One_Topic_Subscribed, string> _topicFileMap = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _baseDirectory = "Logs";
        }

        // ─────────────────────────────────────────────
        //  Propriétés bindées
        // ─────────────────────────────────────────────
        public string _baseDirectory
        {
            get => __baseDirectory;
            set
            {
                if (__baseDirectory == value) return;
                __baseDirectory = value;
                OnPropertyChanged();
            }
        }
        string __baseDirectory;

        public bool? _dateTime
        {
            get => _logger?._timestamp;
            set
            {
                if (_logger == null) return;
                _logger._timestamp = value ?? false;
                OnPropertyChanged();
            }
        }

        public string _dateTime_format
        {
            get => _logger?._dateTime_format;
            set
            {
                if (_logger == null) return;
                _logger._dateTime_format = value;
                OnPropertyChanged();
            }
        }

        public string _separation
        {
            get => _logger?._separation;
            set
            {
                if (_logger == null) return;
                _logger._separation = value;
                OnPropertyChanged();
            }
        }

        // ─────────────────────────────────────────────
        //  Choisir le répertoire de base
        // ─────────────────────────────────────────────
        private void _btn_selectDirectory_Click(object sender, MouseButtonEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _baseDirectory = dialog.SelectedPath;

                // Recrée le logger avec le nouveau répertoire
                _logger?.Dispose();
                _logger = new String2Disk(_baseDirectory);
                OnPropertyChanged(nameof(_dateTime));
                OnPropertyChanged(nameof(_dateTime_format));
                OnPropertyChanged(nameof(_separation));
            }
        }

        // ─────────────────────────────────────────────
        //  Ajouter un topic
        // ─────────────────────────────────────────────
        private void _btn_addTopic_Click(object sender, RoutedEventArgs e)
        {
            // 1. Créer le topic
            var newTopic = new MQTT_One_Topic_Subscribed(mqtt_client);

            // 2. Créer la ligne UC
            var row = new TopicFileRow(newTopic);

            // 3. Quand un fichier est choisi → enregistrer dans le dictionnaire
            row.FilePathChanged += (s, filePath) =>
            {
                _topicFileMap[newTopic] = filePath;
            };

            // 4. Quand le topic reçoit des données → écrire dans le bon fichier
            newTopic.newData += (s, e) =>
            {
                if (!_topicFileMap.TryGetValue(newTopic, out string? filePath)) return;
                if (string.IsNullOrEmpty(filePath)) return;

                MQTTDataArgs args = e as MQTTDataArgs;
                string stringData = Encoding.Default.GetString(args.data);
                _logger._WriteToDisk(filePath, stringData);
            };

            // 5. Bouton ✕ → retirer la ligne
            row.RemoveRequested += (s, _) => RemoveRow(row, newTopic);

            // 6. Ajouter dans le panel
            _topicsPanel.Children.Add(row);
        }

        // ─────────────────────────────────────────────
        //  Supprimer un topic
        // ─────────────────────────────────────────────
        private void RemoveRow(TopicFileRow row, MQTT_One_Topic_Subscribed topic)
        {
            _topicFileMap.Remove(topic);
            _topicsPanel.Children.Remove(row);
        }

        // ─────────────────────────────────────────────
        //  Fermeture
        // ─────────────────────────────────────────────
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _logger.Dispose();
        }
    }

}