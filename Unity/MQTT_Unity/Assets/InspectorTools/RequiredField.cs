using UnityEngine;
//https://www.anton.website/colored-inspector-fields/

public class RequiredField : PropertyAttribute
{
    public Color color;
    public enum FieldColor
    {
        Red,
        Green,
        Blue,
        Yellow
    }

    public RequiredField(FieldColor _color = FieldColor.Red)
    {
        switch (_color)
        {
            case FieldColor.Red:
                color = Color.red;
                break;
            case FieldColor.Green:
                color = Color.green;
                break;
            case FieldColor.Blue:
                color = Color.blue;
                break;
            case FieldColor.Yellow:
                color = Color.yellow;
                break;
            default:
                color = Color.red;
                break;
        }
    }
}