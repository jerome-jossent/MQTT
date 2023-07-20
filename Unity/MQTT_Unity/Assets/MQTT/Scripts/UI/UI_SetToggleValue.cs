using UnityEngine;

public class UI_SetToggleValue : MonoBehaviour
{
    UnityEngine.UI.Toggle toggle;

    void Start()
    {
        toggle = GetComponent<UnityEngine.UI.Toggle>();
    }

    public void _SetValue(bool? value)
    {
        if (value == null) return;
        //pour ne pas lever l'évènement "OnValueChanged"
        toggle.interactable = false;
        toggle.isOn = (bool)value;
        toggle.interactable = true;
    }
}
