using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Toggle_JJ : MonoBehaviour
{
    [SerializeField] Sprite on, off;
    Image image;

    public bool _isChecked;
    public UnityEvent<bool> onChange;

    void Start()
    {
        image = GetComponent<Image>();
        GetComponent<Button>().onClick.AddListener(clic);
    }

    private void Update()
    {
        image.sprite = _isChecked ? on : off;
    }

    void clic()
    {
        _isChecked = !_isChecked;
        onChange.Invoke(_isChecked);
    }

    public void _SetIsChecked(bool? value)
    {
        if (value == null) return;
        _isChecked = (bool)value;
    }
}