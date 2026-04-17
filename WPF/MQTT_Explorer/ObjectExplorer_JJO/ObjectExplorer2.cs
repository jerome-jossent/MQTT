using Newtonsoft.Json.Linq;
using ObjectExplorer_JJO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using TreeView_JJO;
using static TreeView_JJO.ObjectExplorer_Common;

namespace TreeView_JJO
{
    public class ObjectExplorer2
    {
        public object objet { get => _objet; set => _objet = value; }
        object _objet;

        public string json { get => _json; set => _json = value; }
        string _json;

        public JObject jObject { get => _jObject; set => _jObject = value; }
        JObject _jObject;

        public TreeView treeView { get => _treeView; set => _treeView = value; }
        TreeView _treeView;

        public Dictionary<string, TreeViewItem> tvis;
        public Dictionary<string, TreeViewItem_JSON_UC> tvi_ucs;

        public ObjectExplorer2(object objet, TreeView treeViewSiDejaExistant = null)
        {
            this.objet = objet;
            if (objet == null)
            {
                treeView = new TreeView();
                return;
            }

            if (objet.GetType() != typeof(JObject))
            {
                json = Newtonsoft.Json.JsonConvert.SerializeObject(objet, Newtonsoft.Json.Formatting.Indented);
                jObject = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(json);
            }
            else
            {
                jObject = objet as JObject;
            }














            if (treeViewSiDejaExistant == null)
                treeView = new TreeView();
            else
                treeView = treeViewSiDejaExistant;


            FillTreeView(treeView, jObject, objet);

        }

        void FillTreeView(TreeView treeView, JObject? jObject, object objet)
        {
            if (jObject == null) return;

            tvis = new Dictionary<string, TreeViewItem>();
            tvi_ucs = new Dictionary<string, TreeViewItem_JSON_UC>();

            TreeViewItem tvi = null;
            TreeViewItemFill_v2_NS(ref tvi, jObject);
            string name = objet.GetType().ToString();
            ((TreeViewItem_JSON_UC)tvi.Header)._tbk_header.Text = name;

            ClearTreeView(treeView);
            treeView.Items.Add(tvi);
        }

        void TreeViewItemFill_v2_NS(ref TreeViewItem tvi, JToken jToken)
        {
            if (jToken == null)
                return;

            JValue jValue;
            string name;
            string value;
            TreeViewItem tvi_v;

            switch (jToken.Type)
            {
                case JTokenType.Object:
                    TreeViewItem tvi_obj = new TreeViewItem();
                    JObject jObject = (JObject)jToken;

                    name = GetShortName(jObject);

                    if (tvi == null)
                        tvi = tvi_obj;
                    else
                        tvi.Items.Add(tvi_obj);

                    CreateTreeViewElment(jToken, tvi_obj, name, null, Datatype.OBJ);
                    List<JToken> objs_lst = jObject.Children().ToList();
                    foreach (JToken jtoken in objs_lst)
                        TreeViewItemFill_v2_NS(ref tvi_obj, jtoken);
                    break;

                case JTokenType.Array:
                    TreeViewItem tvi_array = new TreeViewItem();
                    JArray jArray = (JArray)jToken;

                    name = jToken.Path;

                    CreateTreeViewElment(jToken, tvi_array, name, null, Datatype.ARY);
                    List<JToken> array_lst = jArray.Children().ToList();
                    foreach (JToken jtoken in array_lst)
                        TreeViewItemFill_v2_NS(ref tvi_array, jtoken);
                    tvi.Items.Add(tvi_array);
                    break;

                case JTokenType.Property:
                    JProperty jProperty = (JProperty)jToken;
                    List<JToken> properties_lst = jProperty.Children().ToList();
                    foreach (JToken jtoken in properties_lst)
                        TreeViewItemFill_v2_NS(ref tvi, jtoken);

                    break;

                case JTokenType.Boolean:
                    tvi_v = new TreeViewItem();
                    jValue = (JValue)jToken;

                    name = GetName(jValue);

                    value = jValue.Value.ToString();

                    CreateTreeViewElment(jToken, tvi_v, name, value, Datatype.BOL);

                    tvi.Items.Add(tvi_v);
                    break;

                case JTokenType.Integer:
                    tvi_v = new TreeViewItem();
                    jValue = (JValue)jToken;
                    name = GetName(jValue);
                    value = jValue.Value.ToString();

                    CreateTreeViewElment(jToken, tvi_v, name, value, Datatype.DBL);

                    tvi.Items.Add(tvi_v);
                    break;

                case JTokenType.Float:
                    tvi_v = new TreeViewItem();
                    jValue = (JValue)jToken;
                    name = GetName(jValue);
                    value = jValue.Value.ToString();

                    CreateTreeViewElment(jToken, tvi_v, name, value, Datatype.DBL);

                    tvi.Items.Add(tvi_v);
                    break;

                case JTokenType.String:
                    tvi_v = new TreeViewItem();
                    jValue = (JValue)jToken;
                    name = GetName(jValue);
                    value = jValue.Value.ToString();

                    CreateTreeViewElment(jToken, tvi_v, name, value, Datatype.STR);

                    tvi.Items.Add(tvi_v);
                    break;

                case JTokenType.Date:
                    tvi_v = new TreeViewItem();
                    jValue = (JValue)jToken;
                    name = GetName(jValue);
                    value = jValue.Value.ToString();

                    CreateTreeViewElment(jToken, tvi_v, name, value, Datatype.DAT);

                    tvi.Items.Add(tvi_v);
                    break;

                case JTokenType.Constructor:
                case JTokenType.Comment:
                case JTokenType.None:
                case JTokenType.Null:
                case JTokenType.Undefined:
                case JTokenType.Raw:
                case JTokenType.Bytes:
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:

                default:
                    break;
            }
        }


        string GetShortName(JObject jObject)
        {
            string path = jObject.Path;
            string name = path;
            if (jObject.Parent != null)
                if (path != jObject.Parent.Path && path.StartsWith(jObject.Parent.Path))
                    name = path.Substring(jObject.Parent.Path.Length);
            return name;
        }

        string GetName(JValue jValue)
        {
            string path = jValue.Path;
            string name = path;
            try
            {
                JProperty jP = (JProperty)jValue.Parent;
                name = jP.Name;
            }
            catch (Exception)
            {
                //   name = GetName(jValue);
                if (jValue.Parent != null)
                    if (path != jValue.Parent.Path && path.StartsWith(jValue.Parent.Path))
                        name = path.Substring(jValue.Parent.Path.Length);
            }
            return name;
        }

        void CreateTreeViewElment(JToken jToken, TreeViewItem tvi, string name, string? value, Datatype type)
        {
            TreeViewItem_JSON_UC tvi_uc = new TreeViewItem_JSON_UC(
                tvi,
                name,
                type,
                value,
                bulletcolor: GetColorFromType(type)//,
                );
            //value_entreguillemets: type == Datatype.STR);

            string fullpath = JsonPathFinder.GetPath(jToken);

            tvis.Add(fullpath, tvi);
            tvi_ucs.Add(fullpath, tvi_uc);
        }

        Dictionary<Datatype, SolidColorBrush> ColorAndDataType;
        void ColorAndDataType_Init()
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

        public SolidColorBrush GetColorFromType(Datatype dt)
        {
            if (ColorAndDataType == null) ColorAndDataType_Init();
            return ColorAndDataType[dt];
        }

        public void ExpandAll(ItemsControl items, bool expand)
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

        internal void ClearTreeView(TreeView trv)
        {
            trv.Items.Clear();
        }

        internal void TreeViewUpdate(Dictionary<object, SolidColorBrush> coloration_rules)
        {
            string txt = "";
            string txt_ok = "";
            foreach (var rule in coloration_rules)
                foreach (TreeViewItem_JSON_UC tvi_uc in tvi_ucs.Values)
                {
                    if (tvi_uc._value == null)
                        continue;

                    if (rule.Key.ToString() == tvi_uc._value.ToString())
                    {
                        tvi_uc._tbk_value.Background = rule.Value;
                        txt_ok += rule.Key + "\t==\t" + tvi_uc._value;
                        txt_ok += "\n";
                    }
                    else
                    {
                        txt += rule.Key + "\t!=\t" + tvi_uc._value;
                        txt += "\n";
                    }
                }


        }
    }
}
