using System;
using System.Windows.Controls;
//using System.Windows.Controls; //ref à Assemblies > Framework > PresentationFramework + WindowsBase
using System.IO.Ports;

namespace Communication_Serie_.NET7
{
    public class Communication_Serie
    {
        public static string[] _Separators_RetourchariotNewLine = new string[] { "\r\n" };

        #region "Parametres"
        private SerialPort _mySerialPort;
        private string _port;
        private string _baud;
        private SerialDataReceivedEventHandler _DataReceived;

        public SerialPort MySerialPort
        {
            get { return _mySerialPort; }
            set { _mySerialPort = value; }
        }
        public string Port
        {
            get { return _port; }
            set
            {
                if (value.ToLower().Substring(0, 2).Contains("com"))
                {
                    _port = value;
                }
                else
                {
                    _port = "COM" + System.Text.RegularExpressions.Regex.Match(value, @"\d+").Value;
                }

            }
        }
        public string Baud
        {
            get { return _baud; }
            set { _baud = value; }
        }

        public bool IsOpen
        {
            get
            {
                if (MySerialPort == null)
                    return false;
                return MySerialPort.IsOpen;
            }
        }
        #endregion

        #region "Constructeurs"
        public Communication_Serie(string port, string baud, SerialDataReceivedEventHandler datareceived)
        {
            Port = port;
            Baud = baud;
            _DataReceived = datareceived;
        }
        #endregion

        #region "Procédures
        public bool PortCom_ON()
        {
            try
            {
                //utilisation des paramètres
                MySerialPort = new SerialPort(Port);
                MySerialPort.BaudRate = Int32.Parse(Baud);
                //attribution en interne des autres paramètres nécessaires
                MySerialPort.Parity = Parity.None;
                MySerialPort.StopBits = StopBits.One;
                MySerialPort.DataBits = 8;
                MySerialPort.Handshake = Handshake.None;
                //abonnement à la méthode qui est chargée de gérer l'arrivée de données
                MySerialPort.DataReceived += new SerialDataReceivedEventHandler(_DataReceived);
                //connection
                MySerialPort.Open();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool PortCom_OFF()
        {
            if (MySerialPort == null) return true;
            try
            {
                MySerialPort.DataReceived -= _DataReceived;
                MySerialPort.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Envoyer(string texte)
        {
            try
            {
                MySerialPort.Write(texte);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void PortCom_Fill(ComboBox combobox_ports)
        {
            combobox_ports.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                ComboBoxItem cbxi = new ComboBoxItem();
                cbxi.Content = port;
                combobox_ports.Items.Add(cbxi);
            }
        }

        public static void Bauds_Fill(ComboBox combobox_bauds)
        {
            combobox_bauds.ItemsSource = new string[] { "300", "1200", "2400", "4800", "9600", "19200", "38400", "57600", "115200", "230400" };
        }
        #endregion
    }
}