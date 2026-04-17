using System.Windows.Controls;
using System.Windows.Media;

namespace TreeView_JJO
{
    public class ObjectExplorer_Common
    {
        public enum Datatype
        {
            INC,
            NUL,
            BOL, DBL, STR, DAT,
            ARY,
            OBJ
        }

        static Dictionary<Datatype, SolidColorBrush> ColorAndDataType;
        static void ColorAndDataType_Init()
        {
            ColorAndDataType = new Dictionary<Datatype, SolidColorBrush>
        {
            { Datatype.INC, new SolidColorBrush(Colors.Transparent) },
            { Datatype.NUL, new SolidColorBrush(Colors.LightGray) },
            { Datatype.BOL, new SolidColorBrush(Colors.Red) },
            { Datatype.DBL, new SolidColorBrush(Colors.GreenYellow) },
            { Datatype.STR, new SolidColorBrush(Colors.Blue) },
            { Datatype.DAT, new SolidColorBrush(Colors.Bisque) },
            { Datatype.ARY, new SolidColorBrush(Colors.Orange) },
            { Datatype.OBJ, new SolidColorBrush(Colors.Magenta) },
            //{ Datatype.NUL, new SolidColorBrush(Colors.LightGray) },
        };
        }
        public static SolidColorBrush GetColorFromType(Datatype dt)
        {
            if (ColorAndDataType == null) ColorAndDataType_Init();
            return ColorAndDataType[dt];
        }


        public static void ExpandAll(ItemsControl items, bool expand)
        {
            foreach (ItemsControl childControl in items.Items)
            {
                if (childControl != null)
                    ExpandAll(childControl, expand);

                TreeViewItem? tvi = childControl as TreeViewItem;
                if (tvi != null)
                    tvi.IsExpanded = true;
            }
        }

        internal static void ClearTreeView(TreeView trv)
        {
            trv.Items.Clear();
        }
    }
}
