using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MQTT_JJ_Message_Image_Texture2D))]
public class MQTT_JJ_UI_Subscribed_Image_Texture2D : MonoBehaviour
{
    public UnityEngine.UI.RawImage rawImage;
    MQTT_JJ_Message_Image_Texture2D m;

    RectTransform rect;
    Vector2 initialsize;

    private void Start()
    {
        if (rawImage == null) rawImage = GetComponent<UnityEngine.UI.RawImage>();

        rect = rawImage.gameObject.GetComponent<RectTransform>();
        initialsize = new Vector2(rect.sizeDelta.x, rect.sizeDelta.y);

        m = GetComponent<MQTT_JJ_Message_Image_Texture2D>();
        m.onNewImageTexture2D.AddListener(OnNewValue);
    }

    void OnNewValue(Texture2D? value)
    {
        rawImage.texture = m.value;

        //resize
        if (rawImage.texture.height < rawImage.texture.width)
            rect.sizeDelta = new Vector2(initialsize.x, 
                initialsize.y * 
                (float)rawImage.texture.height / rawImage.texture.width);
        else
            rect.sizeDelta = new Vector2(initialsize.x * 
                (float)rawImage.texture.width / rawImage.texture.height, 
                initialsize.y);

        //vital : évite fuite mémoire
        Resources.UnloadUnusedAssets();
    }
}