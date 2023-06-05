using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace MQTT_Manager_jjo
{
    public class MQTT_Enums
    {
        public enum DataType { _boolean, _integer, _long, _float, _double, _string, _image, _image_with_metadatas, _image_with_json_in_metadata, _vector3, _vector4}


        //public static MQTT_Manager_UC FindChild(DependencyObject parent)
        //{
        //    // Confirm parent and childName are valid. 
        //    if (parent == null) return null;

        //    MQTT_Manager_UC foundChild = null;

        //    int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        //    for (int i = 0; i < childrenCount; i++)
        //    {
        //        var child = VisualTreeHelper.GetChild(parent, i);
        //        // If the child is not of the request child type child
        //        MQTT_Manager_UC childType = child as MQTT_Manager_UC;
        //        if (childType == null)
        //        {
        //            // recursively drill down the tree
        //            foundChild = FindChild(child);

        //            // If the child is found, break so we do not overwrite the found child. 
        //            if (foundChild != null) 
        //                break;
        //        }
        //        else
        //        {
        //            var frameworkElement = child as FrameworkElement;

        //            string typ = child.GetType().Name;

        //            // If the child's name is set for search
        //            if (frameworkElement != null && typ == "MQTT_Manager_UC")
        //            {
        //                // if the child's name is of the request name
        //                foundChild = (MQTT_Manager_UC)child;
        //                break;
        //            }

        //            // recursively drill down the tree
        //            foundChild = FindChild(child);

        //            // If the child is found, break so we do not overwrite the found child. 
        //            if (foundChild != null) 
        //                break;
        //            else
        //            {
        //                // child element found.
        //                foundChild = (MQTT_Manager_UC)child;
        //                break;
        //            }
        //        }
        //    }
        //    return foundChild;
        //}
    }
}
