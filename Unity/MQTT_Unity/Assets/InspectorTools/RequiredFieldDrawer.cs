using UnityEngine;
using UnityEditor;

//https://www.anton.website/colored-inspector-fields/
[CustomPropertyDrawer(typeof(RequiredField))]
public class RequiredFieldDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        RequiredField field = attribute as RequiredField;
        try
        {
            if (property.type == "string")
            {

            }
            else
            {
                if (property.objectReferenceValue == null)
                {
                    GUI.color = field.color; //Set the color of the GUI
                    EditorGUI.PropertyField(position, property, label); //Draw the GUI
                    GUI.color = Color.white; //Reset the color of the GUI to white
                }
                else
                    EditorGUI.PropertyField(position, property, label);
            }
        }
        catch (System.Exception)
        {

            throw;
        }
    }
}