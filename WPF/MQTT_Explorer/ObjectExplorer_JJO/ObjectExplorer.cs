using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;
using static TreeView_JJO.ObjectExplorer_Common;

namespace ObjectExplorer_JJO
{
    public class ObjectExplorer
    {
        public object objet { get => _objet; set => _objet = value; }
        object _objet;

        public string json { get => _json; set => _json = value; }
        string _json;

        public JObject jObject { get => _jObject; set => _jObject = value; }
        JObject _jObject;

        public ObjectItem root;

        string type_name;

        public Dictionary<string, TreeViewItem_JSON_UC> oi_ucs;

        public ObjectExplorer(object objet)
        {
            this.objet = objet;
            if (objet == null)
                return;

            type_name = objet.GetType().ToString();

            if (objet.GetType() == typeof(JObject))
                jObject = objet as JObject;
            else
            {
                json = Newtonsoft.Json.JsonConvert.SerializeObject(objet, Newtonsoft.Json.Formatting.Indented);
                jObject = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(json);
            }

            //init dicos

            //root object            
            root = null;// new ObjectItem(type_name);

            Explore(ref root, jObject);
        }

        void Explore(ref ObjectItem object_current, JToken jToken)
        {
            if (jToken == null)
                return;

            JValue jValue;
            string name;
            string value;
            ObjectItem oi_v;

            switch (jToken.Type)
            {
                case JTokenType.Object:
                    ObjectItem oi_obj = new ObjectItem();
                    JObject jObject = (JObject)jToken;

                    name = GetShortName(jObject);

                    if (object_current == null)
                        object_current = oi_obj;
                    else
                        object_current.enfants.Add(oi_obj);

                    CreateTreeViewElment(jToken, oi_obj, name, null, Datatype.OBJ);
                    List<JToken> objs_lst = jObject.Children().ToList();
                    foreach (JToken jtoken in objs_lst)
                        Explore(ref oi_obj, jtoken);
                    break;

                case JTokenType.Array:
                    ObjectItem oi_array = new ObjectItem();
                    JArray jArray = (JArray)jToken;

                    name = jToken.Path;

                    CreateTreeViewElment(jToken, oi_array, name, null, Datatype.ARY);
                    List<JToken> array_lst = jArray.Children().ToList();
                    foreach (JToken jtoken in array_lst)
                        Explore(ref oi_array, jtoken);
                    object_current.enfants.Add(oi_array);
                    break;

                case JTokenType.Property:
                    JProperty jProperty = (JProperty)jToken;
                    List<JToken> properties_lst = jProperty.Children().ToList();
                    foreach (JToken jtoken in properties_lst)
                        Explore(ref object_current, jtoken);

                    break;

                case JTokenType.Boolean:
                    oi_v = new ObjectItem();
                    jValue = (JValue)jToken;

                    name = GetName(jValue);

                    value = jValue.Value.ToString();

                    CreateTreeViewElment(jToken, oi_v, name, value, Datatype.BOL);

                    object_current.enfants.Add(oi_v);
                    break;

                case JTokenType.Integer:
                    oi_v = new ObjectItem();
                    jValue = (JValue)jToken;
                    name = GetName(jValue);
                    value = jValue.Value.ToString();

                    CreateTreeViewElment(jToken, oi_v, name, value, Datatype.DBL);

                    object_current.enfants.Add(oi_v);
                    break;

                case JTokenType.Float:
                    oi_v = new ObjectItem();
                    jValue = (JValue)jToken;
                    name = GetName(jValue);
                    value = jValue.Value.ToString();

                    CreateTreeViewElment(jToken, oi_v, name, value, Datatype.DBL);

                    object_current.enfants.Add(oi_v);
                    break;

                case JTokenType.String:
                    oi_v = new ObjectItem();
                    jValue = (JValue)jToken;
                    name = GetName(jValue);
                    value = jValue.Value.ToString();

                    CreateTreeViewElment(jToken, oi_v, name, value, Datatype.STR);

                    object_current.enfants.Add(oi_v);
                    break;

                case JTokenType.Date:
                    oi_v = new ObjectItem();
                    jValue = (JValue)jToken;
                    name = GetName(jValue);
                    value = jValue.Value.ToString();

                    CreateTreeViewElment(jToken, oi_v, name, value, Datatype.DAT);

                    object_current.enfants.Add(oi_v);
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

        void CreateTreeViewElment(JToken jToken, ObjectItem oi, string name, string? value, Datatype type)
        {
            oi.path = JsonPathFinder.GetPath(jToken);
            oi.displayname = name;
            if (value != null) oi.value = value;
            oi.type = type;
            oi.color = GetColorFromType(type);

            //TreeViewItem_JSON_UC oi_uc = new TreeViewItem_JSON_UC(
            //    oi,
            //    name,
            //    type,
            //    value,
            //    bulletcolor: GetColorFromType(type)//,
            //    );
            ////value_entreguillemets: type == Datatype.STR);

            //string fullpath = JsonPathFinder.GetPath(jToken);

            //ois.Add(fullpath, oi);
            //oi_ucs.Add(fullpath, oi_uc);
        }

    }

}
