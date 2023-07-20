using UnityEngine;

public class UI_SetSliderValue : MonoBehaviour
{
    UnityEngine.UI.Slider slider;

    void Start()
    {
        slider = GetComponent<UnityEngine.UI.Slider>();
    }

    public void _SetValue(float? value)
    {
        if (value == null) return;
        //pour ne pas lever l'évènement "OnValueChanged"
        slider.interactable = false;
        slider.value = (float)value;
        slider.interactable = true;
    }
}