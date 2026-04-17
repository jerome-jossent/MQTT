using System;
using System.Collections.Generic;
using System.Linq;
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
using TreeView_JJO;

namespace MQTT_Explorer
{
    /// <summary>
    /// Logique d'interaction pour Topic_follow_view.xaml
    /// </summary>
    public partial class Topic_follow_view : UserControl
    {
        public Topic_follow_view(List<Topic_Data> diffs)
        {
            InitializeComponent();
            foreach (Topic_Data item in diffs)
            {

                StackPanel sp_ind = new StackPanel() { Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center};

                TextBlock tbk = new TextBlock()
                {
                    Text = "[" + item.index + "]" + item.date.ToString("HH:mm:ss.fff"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                sp_ind.Children.Add(tbk);

                TreeView trv = new TreeView();
                //if (item.differences_avec_precedent == null)
                //    ObjectExplorer.FillTreeView(trv, item.jsonObject);
                //else
                ObjectExplorer_NS.FillTreeView_HighlightDifferences(trv, item.jsonObject_NS, item.differences_avec_precedent, item.GetType().ToString());

                ObjectExplorer_NS.ExpandAll(trv, true);
                sp_ind.Children.Add(trv);
                _sp.Children.Add(sp_ind);
            }
        }
    }
}
