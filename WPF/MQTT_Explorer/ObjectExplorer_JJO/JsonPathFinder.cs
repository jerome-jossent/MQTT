using Newtonsoft.Json.Linq;

namespace ObjectExplorer_JJO
{
    public class JsonPathFinder
    {
        public static Dictionary<string, JToken> FindPathsAndValues(JObject jObject)
        {
            var pathsAndValues = new Dictionary<string, JToken>();
            FindPathsAndValuesRecursive(jObject, "", pathsAndValues);
            return pathsAndValues;
        }

        public static string GetPath(JToken token)
        {
            if (token == null)
                return null;

            var path = new List<string>();
            var current = token;

            while (current != null)
            {
                if (current.Parent is JProperty prop)
                {
                    path.Add(prop.Name);
                    current = prop.Parent;
                }
                else if (current.Parent is JArray array)
                {
                    var index = array.IndexOf(current);
                    //path.Add($"[{index}]");
                    path.Add($"{index}");
                    path.Add(current.Parent.Path);
                    current = array.Parent;
                }
                else
                {
                    current = current.Parent;
                }
            }

            path.Reverse();
            return string.Join(".", path.Select(p => p.StartsWith("[") ? p : p.Contains(".") ? $"['{p}']" : p));
        }

        static void FindPathsAndValuesRecursive(JToken token, string currentPath, Dictionary<string, JToken> pathsAndValues)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (var property in token.Children<JProperty>())
                    {
                        string newPath = string.IsNullOrEmpty(currentPath)
                            ? property.Name
                            : currentPath + "." + property.Name;
                        FindPathsAndValuesRecursive(property.Value, newPath, pathsAndValues);
                    }
                    break;

                case JTokenType.Array:
                    for (int i = 0; i < token.Count(); i++)
                    {
                        //string newPath = currentPath + "[" + i + "]";
                        string newPath = currentPath +  i ;
                        FindPathsAndValuesRecursive(token[i], newPath, pathsAndValues);
                    }
                    break;

                default:
                    pathsAndValues[currentPath] = token;
                    break;
            }
        }


        public static Dictionary<string, JToken> GetDifferencePaths(JToken diffResult)
        {
            var paths = new Dictionary<string, JToken>();
            TraverseDiff(diffResult, "", paths);
            return paths;
        }

        static void TraverseDiff(JToken token, string currentPath, Dictionary<string, JToken> paths)
        {
            if (token.Type == JTokenType.Array && token.Count() > 0)
            {
                // C'est un tableau de différences
                paths.Add(currentPath, token);
            }
            else if (token.Type == JTokenType.Object)
            {
                foreach (var property in token.Children<JProperty>())
                {
                    string newPath = string.IsNullOrEmpty(currentPath) ?
                        property.Name : currentPath + "." + property.Name;

                    TraverseDiff(property.Value, newPath, paths);
                }
            }
        }
    }
}
