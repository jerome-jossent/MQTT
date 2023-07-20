using UnityEngine;
using UnityEngine.EventSystems;

public class UI_SetSliderValue : MonoBehaviour
{
    UnityEngine.UI.Slider slider;
    [SerializeField, ReadOnly] bool isDragging;
    EventTrigger eventTrigger;

    void Start()
    {
        slider = GetComponent<UnityEngine.UI.Slider>();

        #region isDragging with EventTrigger
        eventTrigger = gameObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
            eventTrigger = gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry beginDrag = new EventTrigger.Entry();
        beginDrag.eventID = EventTriggerType.BeginDrag;
        beginDrag.callback.AddListener((eventData) => { _Drag(true); });
        eventTrigger.triggers.Add(beginDrag);

        EventTrigger.Entry endDrag = new EventTrigger.Entry();
        endDrag.eventID = EventTriggerType.EndDrag;
        endDrag.callback.AddListener((eventData) => { _Drag(false); });
        eventTrigger.triggers.Add(endDrag);
        #endregion
    }

    public void _Drag(bool isDragging)
    {
        this.isDragging = isDragging;
    }

    public void _SetValue(float? value)
    {
        if (isDragging) return;

        if (value == null) return;

        //pour ne pas lever l'évènement "OnValueChanged"
        slider.interactable = false;
        slider.value = (float)value;
        slider.interactable = true;
    }
}