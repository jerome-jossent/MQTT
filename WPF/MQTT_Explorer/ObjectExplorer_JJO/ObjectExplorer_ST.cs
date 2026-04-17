using System.Windows.Controls;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Text.Json;
using System.Text.Json.Nodes;

using static TreeView_JJO.ObjectExplorer_Common;

namespace TreeView_JJO
{
    internal class ObjectExplorer_ST
    {
        public static void FillTreeView(TreeView trv, object objet)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(objet, new JsonSerializerOptions { WriteIndented = true });
            JsonObject jsonObject  = System.Text.Json.JsonSerializer.Deserialize<JsonObject>(json);
            FillTreeView(trv, jsonObject, objet);
        }

        public static void FillTreeView(TreeView trv, JsonObject jsonObject, object objet)
        {
            if (jsonObject == null) return;

            TreeViewItem tvi = new TreeViewItem();
            ListViewItemHeaderName(tvi, objet.GetType().ToString());
            TreeViewItemAddType(tvi, Datatype.OBJ);
            TreeViewItemFill_v2(tvi, jsonObject);

            ClearTreeView(trv);
            trv.Items.Add(tvi);
        }

        static void TreeViewItemFill_v2(TreeViewItem tvi, object value)
        {
            string typ = value.GetType().ToString();
            switch (typ)
            {
                case "System.Text.Json.Nodes.JsonObject":
                    JsonObject valueJsonObject = (JsonObject)value;
                    foreach (KeyValuePair<string, JsonNode?> item in valueJsonObject)
                    {
                        TreeViewItemFill_v2(tvi, item);
                    }
                    break;

                case "System.Text.Json.Nodes.JsonArray":
                    TreeViewItemAddType(tvi, Datatype.ARY);

                    JsonArray jsonArray = (JsonArray)value;
                    foreach (JsonNode? item in jsonArray)
                    {
                        TreeViewItem tvi_item = new TreeViewItem();
                        ListViewItemVide(tvi_item);
                        //ListViewItemHeaderName(tvi_item, "[]");

                        TreeViewItemFill_v2(tvi_item, item);
                        tvi.Items.Add(tvi_item);
                    }
                    break;

                case "System.Collections.Generic.KeyValuePair`2[System.String,System.Text.Json.Nodes.JsonNode]":
                    KeyValuePair<string, JsonNode?> keyValuePair = (KeyValuePair<string, JsonNode?>)value;
                    if (keyValuePair.Value == null) break;

                    TreeViewItem tvi_kv = new TreeViewItem();
                    ListViewItemHeaderName(tvi_kv, keyValuePair.Key);

                    tvi.Items.Add(tvi_kv);
                    TreeViewItemFill_v2(tvi_kv, keyValuePair.Value);
                    break;

                case "System.Text.Json.Nodes.JsonValueTrimmable`1[System.Text.Json.JsonElement]":
                case "System.Text.Json.Nodes.JsonValuePrimitive`1[System.Text.Json.JsonElement]":
                    bool entreguillemets = false;
                    switch (GetValueType((JsonValue)value).Name)
                    {
                        case "Bool": TreeViewItemAddType(tvi, Datatype.BOL); break;
                        case "Double": TreeViewItemAddType(tvi, Datatype.DBL); break;
                        case "String": TreeViewItemAddType(tvi, Datatype.STR); entreguillemets = true; break;
                        default:
                            TreeViewItemAddType(tvi, Datatype.STR); break;
                    }
                    ListViewItemHeaderValue(tvi, value.ToString(), entreguillemets: entreguillemets);
                    break;

                //TreeViewItemAddType(tvi, Datatype.STR);
                //ListViewItemHeaderValue(tvi, value.ToString(), entreguillemets: true);
                //break;

                default:
                    tvi.Header += "type non pris en charge : \"" + typ + "\"";
                    break;
            }
        }

        static Type GetValueType(JsonValue jsonValue)
        {
            var value = jsonValue.GetValue<object>();
            if (value is JsonElement element)
            {
                return element.ValueKind switch
                {
                    JsonValueKind.False => typeof(bool),
                    JsonValueKind.True => typeof(bool),
                    JsonValueKind.Number => typeof(double),
                    JsonValueKind.String => typeof(string),
                    var _ => typeof(JsonElement),
                };
            }
            return value.GetType();
        }

        static void ListViewItemVide(TreeViewItem tvi)
        {
            StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal };
            sp.Height = 17;
            tvi.Header = sp;
        }

        static void ListViewItemHeaderName(TreeViewItem tvi, string text, Color? color = null)
        {
            StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal };
            sp.Height = 17;

            TextBlock tbk = new TextBlock() { Text = "\"" + text + "\" : ", FontWeight = FontWeights.Bold };
            if (color != null)
                tbk.Foreground = new SolidColorBrush(color.Value);

            sp.Children.Add(tbk);
            tvi.Header = sp;
        }

        static void TreeViewItemAddType(TreeViewItem tvi, Datatype dt)
        {
            Ellipse bullet = new Ellipse()
            {
                Width = 5,
                Height = 5,
                StrokeThickness = 0,
                Margin = new Thickness(0, 0, 3, 0)
            };

            bullet.Fill = GetColorFromType(dt);
            StackPanel sp = (StackPanel)tvi.Header;
            sp.Children.Insert(0, bullet);
        }


        static void ListViewItemHeaderValue(TreeViewItem tvi, string text, Color? color = null, bool entreguillemets = false)
        {
            StackPanel sp = (StackPanel)tvi.Header;

            if (entreguillemets)
                text = "\"" + text + "\"";

            TextBlock tbk = new TextBlock() { Text = text };
            if (color != null)
                tbk.Foreground = new SolidColorBrush(color.Value);

            sp.Children.Add(tbk);
            tvi.Header = sp;
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