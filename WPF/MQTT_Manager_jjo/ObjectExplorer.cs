using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Text.Json;
using System.Text.Json.Nodes;
using System;
using static System.Net.Mime.MediaTypeNames;

internal class ObjectExplorer
{
    public static bool FillTreeView(TreeView trv, object objet)
    {
        TreeViewItem trvi = FillTreeView_v2(objet);
        if (trvi == null)
            return false;

        trv.Items.Clear();
        trv.Items.Add(trvi);
        return true;
    }
    public static bool FillTreeView(TreeView trv, string json, string displayName = "")
    {
        TreeViewItem trvi = FillTreeView_v2(json, displayName);
        if (trvi == null)
            return false;

        trv.Items.Clear();
        trv.Items.Add(trvi);
        return true;
    }

    public static TreeViewItem FillTreeView_v2(object objet)
    {
        string json = System.Text.Json.JsonSerializer.Serialize(objet, new JsonSerializerOptions { WriteIndented = true });
        JsonObject? jsonElement = System.Text.Json.JsonSerializer.Deserialize<JsonObject>(json);

        if (jsonElement == null) return null;

        TreeViewItem trvi = new TreeViewItem();
        ListViewItemHeaderName(trvi, objet.GetType().ToString());
        TreeViewItemAddType(trvi, Datatype.OBJ);
        TreeViewItemFill_v2(trvi, jsonElement);

        return trvi;
    }

    public static TreeViewItem FillTreeView_v2(string jsonObject, string displayName)
    {
        JsonObject? jsonElement = System.Text.Json.JsonSerializer.Deserialize<JsonObject>(jsonObject);

        if (jsonElement == null) return null;

        TreeViewItem trvi = new TreeViewItem();
        ListViewItemHeaderName(trvi, displayName);
        TreeViewItemAddType(trvi, Datatype.OBJ);
        TreeViewItemFill_v2(trvi, jsonElement);

        return trvi;
    }

    //public static TreeViewItem FillTreeView(object objet)
    //{
    //    return FillTreeView_v2(objet);


    //    //string json = JsonConvert.SerializeObject(objet, Formatting.Indented);

    //    //Dictionary<string, dynamic>? dic = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);

    //    //if (dic == null)
    //    //    return null;

    //    //TreeViewItem trvi = new TreeViewItem();
    //    //ListViewItemHeaderName(trvi, objet.GetType().ToString());
    //    //TreeViewItemAddType(trvi, Datatype.OBJ);
    //    //TreeViewItemFill(trvi, dic);

    //    //return trvi;
    //}

    //static void TreeViewItemFill(TreeViewItem trvi, object value)
    //{
    //    string typ = value.GetType().ToString();
    //    switch (typ)
    //    {
    //        case "System.Collections.Generic.Dictionary`2[System.String,System.Object]":
    //            Dictionary<string, dynamic>? v_dico = value as Dictionary<string, dynamic>;
    //            if (v_dico == null) break;
    //            foreach (var item in v_dico)
    //                TreeViewItemFill(trvi, item);
    //            break;

    //        case "System.Collections.Generic.KeyValuePair`2[System.String,System.Object]":
    //            KeyValuePair<string, object>? v_kv = value as KeyValuePair<string, object>?;
    //            if (v_kv == null) break;
    //            TreeViewItem tvi_kv = new TreeViewItem();
    //            ListViewItemHeaderName(tvi_kv, v_kv.Value.Key);

    //            trvi.Items.Add(tvi_kv);
    //            TreeViewItemFill(tvi_kv, v_kv.Value.Value);
    //            break;

    //        case "Newtonsoft.Json.Linq.JArray":
    //            Newtonsoft.Json.Linq.JArray? v_array = value as Newtonsoft.Json.Linq.JArray;
    //            if (v_array == null) break;
    //            TreeViewItemAddType(trvi, Datatype.ARY);

    //            TreeViewItem tvi_array = new TreeViewItem();
    //            ListViewItemHeaderName(tvi_array, "");

    //            string chaine = string.Join(", ", v_array);
    //            string[] arr = chaine.Split(',');

    //            foreach (string item in arr)
    //            {

    //                string s = item.Replace("\r\n", "");
    //                s = s.Replace("[", "");
    //                s = s.Replace("]", "");
    //                s = s.Trim();

    //                TreeViewItem tvi_array_val = new TreeViewItem();
    //                ListViewItemVide(tvi_array_val);
    //                //ListViewItemHeaderName(tvi_array_val, "");
    //                tvi_array.Items.Add(tvi_array_val);

    //                TreeViewItemFill(tvi_array_val, s);
    //            }

    //            //ListViewItemHeaderValue(tvi_array, "[" + chaine + "]");
    //            trvi.Items.Add(tvi_array);
    //            break;

    //        case "Newtonsoft.Json.Linq.JObject":
    //            Newtonsoft.Json.Linq.JObject? v_jobject = value as Newtonsoft.Json.Linq.JObject;
    //            string json = JsonConvert.SerializeObject(v_jobject, Formatting.Indented);
    //            Dictionary<string, dynamic>? dic = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);
    //            if (dic == null) break;
    //            TreeViewItemAddType(trvi, Datatype.OBJ);
    //            TreeViewItemFill(trvi, dic);
    //            break;

    //        //case "System.Int64":
    //        //    long? v_long = value as long?;
    //        //    TreeViewItemAddType(trvi, Datatype.LNG);
    //        //    ListViewItemHeaderValue(trvi, v_long.ToString());
    //        //    break;

    //        case "System.Double":
    //            double? v_double = value as double?;
    //            TreeViewItemAddType(trvi, Datatype.DBL);
    //            ListViewItemHeaderValue(trvi, v_double.ToString());
    //            break;

    //        case "System.String":
    //            string? v_string = value as string;
    //            TreeViewItemAddType(trvi, Datatype.STR);
    //            ListViewItemHeaderValue(trvi, "\"" + v_string + "\"".ToString());
    //            break;

    //        default:
    //            trvi.Header += "type non pris en charge : \"" + typ + "\"";
    //            break;
    //    }
    //}

    static void TreeViewItemFill_v2(TreeViewItem trvi, object value)
    {
        string typ = value.GetType().ToString();
        switch (typ)
        {
            case "System.Text.Json.Nodes.JsonObject":
                JsonObject valueJsonObject = (JsonObject)value;
                //reserializer ??
                foreach (KeyValuePair<string, JsonNode?> item in valueJsonObject)
                {
                    TreeViewItemFill_v2(trvi, item);
                }
                break;

            case "System.Text.Json.Nodes.JsonArray":
                TreeViewItemAddType(trvi, Datatype.ARY);

                JsonArray jsonArray = (JsonArray)value;
                foreach (JsonNode? item in jsonArray)
                {
                    TreeViewItem tvi_item = new TreeViewItem();
                    ListViewItemVide(tvi_item);
                    //ListViewItemHeaderName(tvi_item, "[]");

                    TreeViewItemFill_v2(tvi_item, item);
                    trvi.Items.Add(tvi_item);
                }
                break;

            case "System.Collections.Generic.KeyValuePair`2[System.String,System.Text.Json.Nodes.JsonNode]":
                KeyValuePair<string, JsonNode?> keyValuePair = (KeyValuePair<string, JsonNode?>)value;
                if (keyValuePair.Value == null) break;

                TreeViewItem tvi_kv = new TreeViewItem();
                ListViewItemHeaderName(tvi_kv, keyValuePair.Key);

                trvi.Items.Add(tvi_kv);
                TreeViewItemFill_v2(tvi_kv, keyValuePair.Value);
                break;

            case "System.Text.Json.Nodes.JsonValueTrimmable`1[System.Text.Json.JsonElement]":
                switch (GetValueType((JsonValue)value).Name)
                {
                    case "Bool": TreeViewItemAddType(trvi, Datatype.BOL); break;
                    case "Double": TreeViewItemAddType(trvi, Datatype.DBL); break;
                    case "String": TreeViewItemAddType(trvi, Datatype.STR); break;
                    default:
                        TreeViewItemAddType(trvi, Datatype.STR); break;
                }
                ListViewItemHeaderValue(trvi, value.ToString());
                break;

            default:
                trvi.Header += "type non pris en charge : \"" + typ + "\"";
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

    static void ListViewItemVide(TreeViewItem trvi)
    {
        StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal };
        sp.Height = 17;
        trvi.Header = sp;
    }

    static void ListViewItemHeaderName(TreeViewItem trvi, string text)
    {
        StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal };
        sp.Height = 17;

        TextBlock tbk = new TextBlock() { Text = "\"" + text + "\" : ", FontWeight = FontWeights.Bold };
        sp.Children.Add(tbk);
        trvi.Header = sp;
    }

    enum Datatype { BOL, DBL, STR, ARY, OBJ }

    static void TreeViewItemAddType(TreeViewItem trvi, Datatype dt)
    {
        Ellipse bullet = new Ellipse()
        {
            Width = 5,
            Height = 5,
            StrokeThickness = 0,
            Margin = new Thickness(0, 0, 3, 0)
        };
        SolidColorBrush color;
        switch (dt)
        {
            case Datatype.BOL: color = new SolidColorBrush(Colors.Red); break;
            case Datatype.DBL: color = new SolidColorBrush(Colors.GreenYellow); break;
            case Datatype.STR: color = new SolidColorBrush(Colors.Blue); break;
            case Datatype.ARY: color = new SolidColorBrush(Colors.Orange); break;
            case Datatype.OBJ: color = new SolidColorBrush(Colors.Magenta); break;
            default: color = new SolidColorBrush(Colors.Black); break;
        }
        bullet.Fill = color;
        StackPanel sp = (StackPanel)trvi.Header;
        sp.Children.Insert(0, bullet);
    }

    static void ListViewItemHeaderValue(TreeViewItem trvi, string text)
    {
        StackPanel sp = (StackPanel)trvi.Header;
        TextBlock tbk = new TextBlock() { Text = text };
        sp.Children.Add(tbk);
        trvi.Header = sp;
    }

    public static void ExpandAll(ItemsControl items, bool expand=true)
    {
        foreach (ItemsControl childControl in items.Items)
        {
            if (childControl != null)
                ExpandAll(childControl, expand);

            TreeViewItem? trvi = childControl as TreeViewItem;
            if (trvi != null)
                trvi.IsExpanded = true;
        }
    }
}