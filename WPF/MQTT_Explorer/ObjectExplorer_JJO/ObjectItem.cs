using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ObjectExplorer_JJO
{
    public class ObjectItem
    {
        public string path;
        public string displayname;

        public string value;

        public ObjectItem parent;
        public List<ObjectItem> enfants = new List<ObjectItem>();

        public TreeView_JJO.ObjectExplorer_Common.Datatype type;
        internal SolidColorBrush color;

        //public Topic_Data_UC uc;

        public ObjectItem()
        {
        }
    }
}
