using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static TreeView_JJO.ObjectExplorer_Common;

namespace ObjectExplorer_JJO
{
    public partial class TreeViewItem_JSON_UC : UserControl
    {
        string name;
        Datatype type;
        public string? _value;
        SolidColorBrush bulletcolor;
        //bool value_entreguillemets;

        public TreeViewItem_JSON_UC()
        {
            InitializeComponent();
        }

        public TreeViewItem_JSON_UC(TreeViewItem tvi, string name, Datatype type, string? value, SolidColorBrush bulletcolor)//, bool value_entreguillemets)
        {
            InitializeComponent();

            tvi.Header = this;

            this.name = name;
            this.type = type;
            this._value = value;
            this.bulletcolor = bulletcolor;
            //this.value_entreguillemets = value_entreguillemets;

            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(value) ||
                bulletcolor.Color.A == 0)
            {
                _elp.Visibility = Visibility.Collapsed;
            }
            else
            {
                _elp.Fill = bulletcolor;
            }
            _tbk_header.Text = name + " : ";
            //_tbk_header.Text = "\"" + name + "\" : ";

            if (value != null)
            {
                //if (value_entreguillemets)
                //    value = "\"" + value + "\"";
                _tbk_value.Text = value;

                //if (type == Datatype.BOL)
                //{
                //    if (bool.TryParse(value, out bool v))
                //        _tbk_value.Foreground = v ? Brushes.Green : Brushes.Red;
                //}
            }
            else
            {

                _tbk_value.Text = "";
            }
        }

        public void _Highlight(string oldvalue)
        {
            //_tbk_value.Foreground = Brushes.Blue;
            this.Background = Brushes.LightYellow;
            this.ToolTip = oldvalue;
        }

        internal void _SetBackGroundColor(SolidColorBrush col)
        {
            _tbk_value.Background = col;
        }
    }
}
