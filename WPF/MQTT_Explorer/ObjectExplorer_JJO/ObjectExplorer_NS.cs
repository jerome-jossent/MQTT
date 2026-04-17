using JsonDiffPatchDotNet;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

using static TreeView_JJO.ObjectExplorer_Common;
using System.Runtime.InteropServices.JavaScript;
using System.Xml.Linq;
using ObjectExplorer_JJO;

namespace TreeView_JJO
{
    internal class ObjectExplorer_NS
    {
        static Dictionary<string, JToken> objets;
        static Dictionary<string, JToken> differences;
        static Dictionary<string, TreeViewItem> tvis;

        public static void FillTreeView(TreeView trv, object objet)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(objet, Newtonsoft.Json.Formatting.Indented);
            JObject jObjet = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(json);
            FillTreeView(trv, jObjet, objet);
        }

        public static void FillTreeView(TreeView trv, JObject jObjet, object objet)
        {
            if (jObjet == null) return;
              
            tvis = new Dictionary<string, TreeViewItem>();

            TreeViewItem tvi = null;
            TreeViewItemFill_v2_NS(ref tvi, jObjet);
            string name = objet.GetType().ToString();
            ((TreeViewItem_JSON_UC)tvi.Header)._tbk_header.Text = name;

            ClearTreeView(trv);
            trv.Items.Add(tvi);
        }

        public static void FillTreeView_HighlightDifferences(TreeView trv, JObject? jsonObject, JObject? jsonObject_precedent, string name = "")
        {
            var jdp = new JsonDiffPatch();
            JToken? differences_avec_precedent = jdp.Diff(jsonObject_precedent, jsonObject);
            FillTreeView_HighlightDifferences(trv, jsonObject, differences_avec_precedent, name);
        }


        public static void FillTreeView_HighlightDifferences(TreeView trv, JObject? jsonObject, JToken? differences_avec_precedent, string name = "")
        {
            tvis = new Dictionary<string, TreeViewItem>();

            TreeViewItem tvi = null;
            TreeViewItemFill_v2_NS(ref tvi, jsonObject);

            HighlightDifferences(jsonObject, differences_avec_precedent);

            //CreateTreeViewElment(jsonObject, tvi, name, null, Datatype.INC);

            ClearTreeView(trv);
            trv.Items.Add(tvi);
        }

        private static void HighlightDifferences(JObject? jsonObject, JToken? differences_avec_precedent)
        {
            objets = new Dictionary<string, JToken>();
            if (jsonObject != null)
                objets = JsonPathFinder.FindPathsAndValues(jsonObject);

            differences = new Dictionary<string, JToken>();
            if (differences_avec_precedent != null)
                differences = JsonPathFinder.GetDifferencePaths(differences_avec_precedent);

            if (differences.Count == 0)
                return;

            List<string> paths = new List<string>();
            foreach (KeyValuePair<string, JToken> item in differences)
            {
                string path = item.Key;
                if (tvis.ContainsKey(path))
                {
                    var tvi = tvis[path];
                    JToken oldvalue_JToken = ((JToken)item.Value);
                    string oldvalue = oldvalue_JToken.Children().First().ToString();
                    ((TreeViewItem_JSON_UC)tvi.Header)._Highlight(oldvalue);
                }
                else
                {
                    paths.Add(path);
                }
            }

            if (paths.Count > 0)
            {
                string txt = string.Join('\n', paths.ToArray());
                Label label = new Label() { Content = txt };

                Window w = new Window();
                w.Content = label;
                w.Title = "Différences trouvées aussi sur ces éléments :";
                w.SizeToContent = SizeToContent.WidthAndHeight;
                w.Show();
            }
        }

        static string GetShortName(JObject jObject)
        {
            string path = jObject.Path;
            string name = path;
            if (jObject.Parent != null)
                if (path != jObject.Parent.Path && path.StartsWith(jObject.Parent.Path))
                    name = path.Substring(jObject.Parent.Path.Length);
            return name;
        }
        static string GetName(JValue jValue)
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


        static void TreeViewItemFill_v2_NS(ref TreeViewItem tvi, JToken jToken)
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

        static void CreateTreeViewElment(JToken jToken, TreeViewItem tvi, string name, string? value, Datatype type)
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
        }

        internal static void ExpandAll(TreeView trv, bool v)
        {
            ObjectExplorer_Common.ExpandAll(trv, v);
        }

        internal static void ClearTreeView(TreeView trv)
        {
            ObjectExplorer_Common.ClearTreeView(trv);
        }
    }
}
