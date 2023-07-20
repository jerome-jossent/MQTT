using UnityEngine;

public class UI_SetText : MonoBehaviour
{
    int text_version;

    UnityEngine.UI.Text text; //1
    TMPro.TMP_Text tmp_text;  //2
    TMPro.TMP_InputField tmp_inputfield;  //3

    void Start()
    {
        text = GetComponent<UnityEngine.UI.Text>();
        if (text != null)
        {
            text_version = 1;
            return;
        }

        tmp_text = GetComponent<TMPro.TMP_Text>();
        if (tmp_text != null)
        {
            text_version = 2;
            return;
        }

        tmp_inputfield = GetComponent<TMPro.TMP_InputField>();
        if (tmp_inputfield != null)
        {
            text_version = 3;
            return;
        }

        text_version = 0;
        Debug.Log(gameObject.name + " : No Support found ! Autodestroy component");
        DestroyImmediate(this);
    }

    public void _SetText(bool value) { _SetText(value.ToString()); }
    public void _SetText(bool? value) { _SetText(value.ToString()); }
    public void _SetText(int value) { _SetText(value.ToString()); }
    public void _SetText(int? value) { _SetText(value.ToString()); }
    public void _SetText(long value) { _SetText(value.ToString()); }
    public void _SetText(long? value) { _SetText(value.ToString()); }
    public void _SetText(float value) { _SetText(value.ToString()); }
    public void _SetText(float? value) { _SetText(value.ToString()); }
    public void _SetText(double value) { _SetText(value.ToString()); }
    public void _SetText(double? value) { _SetText(value.ToString()); }
    public void _SetText(Vector2 value) { _SetText(value.ToString()); }
    public void _SetText(Vector2? value) { _SetText(value.ToString()); }
    public void _SetText(Vector3 value) { _SetText(value.ToString()); }
    public void _SetText(Vector3? value) { _SetText(value.ToString()); }
    public void _SetText(Color value) { _SetText(value.ToString()); }
    public void _SetText(Color? value) { _SetText(value.ToString()); }
    public void _SetText(string txt)
    {
        if (txt == null) return;

        switch (text_version)
        {
            case 1: text.text = txt; break;
            case 2: tmp_text.text = txt; break;
            case 3: tmp_inputfield.text = txt; break;
        }
    }
}
