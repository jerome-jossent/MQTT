using Communication_Serie_.NET7;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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

namespace MQTT_Serial_to_MQTT
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string data_separator
        {
            get => _data_separator;
            set
            {
                if (value == _data_separator) return;
                _data_separator = value;
                OnPropertyChanged();
            }
        }
        string _data_separator = ",";

        public string dataKeyValue_separator
        {
            get => _dataKeyValue_separator;
            set
            {
                if (value == _dataKeyValue_separator) return;
                _dataKeyValue_separator = value;
                OnPropertyChanged();
            }
        }
        string _dataKeyValue_separator = "=";

        public string topicPrefix
        {
            get => _topicPrefix;
            set
            {
                if (value == _topicPrefix) return;
                _topicPrefix = value;
                OnPropertyChanged();
            }
        }
        string _topicPrefix = "veri/test_arduino/";

        Communication_Serie_.NET7.Communication_Serie cs;
        Thread threadProcessing;
        CancellationTokenSource threadProcessing_cts;

        object serialIncome_lock = new object();
        List<string> serialIncome = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            INIT_Serial();
            INIT_Processing_Serial2MQTT();
        }

        private void INIT_Serial()
        {
            serial._Link(cs, newDataReceived);
            serial._SetPort(115200);
        }

        private void INIT_Processing_Serial2MQTT()
        {
            threadProcessing_cts = new CancellationTokenSource();
            threadProcessing = new Thread(() => Serial2MQTT(threadProcessing_cts.Token));
            threadProcessing.Start();
        }

        void Processing_Serial2MQTT_Stop()
        {
            threadProcessing_cts.Cancel();
        }

        private void Serial2MQTT(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                    return;

                if (serialIncome.Count > 0)
                {
                    lock (serialIncome_lock)
                    {
                        string dataset = serialIncome[0];
                        SendDataSetToMQTT(dataset);
                        serialIncome.RemoveAt(0);
                    }
                }
                Thread.Sleep(1);
            }
        }

        private void SendDataSetToMQTT(string dataset)
        {
            if (mqtt_client.mqttClient == null || !mqtt_client.mqttClient.IsConnected) return;

            string[] data = dataset.Split(data_separator, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < data.Length; i++)
            {
                string[] keyvalue = data[i].Split(dataKeyValue_separator);
                string topic = topicPrefix + keyvalue[0];
                string val = keyvalue[1];

                mqtt_client.MQTT_Publish(topic, val);

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    lbx_sended_to_broker.Items.Add(topic + " : " + val);
                }));
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                while (lbx_sended_to_broker.Items.Count > 10 * data.Length)
                    lbx_sended_to_broker.Items.RemoveAt(0);
            }));

        }

        string derniermorceau = "";

        private void newDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort comm = (SerialPort)sender;
            string incoming_Data = comm.ReadExisting();

            //affiche
            tbx_serial_in._displaytext(incoming_Data);

            //udpate fifo
            lock (serialIncome_lock)
            {
                incoming_Data = derniermorceau + incoming_Data;
                string[] morceaux = incoming_Data.Split(Communication_Serie._Separators_RetourchariotNewLine, StringSplitOptions.TrimEntries);
                for (int i = 0; i < morceaux.Length - 1; i++)
                {
                    string morceau = morceaux[i];
                    serialIncome.Add(morceau);
                }
                derniermorceau = morceaux[morceaux.Length - 1];
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Processing_Serial2MQTT_Stop();
        }

        //tbx_sended_to_broker

    }
}
