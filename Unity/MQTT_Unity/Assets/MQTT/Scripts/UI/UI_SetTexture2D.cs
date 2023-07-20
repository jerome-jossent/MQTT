using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_SetTexture2D : MonoBehaviour
{
    UnityEngine.UI.RawImage rawImage;
    RectTransform rect;
    Vector2 initialsize;

    void Start()
    {
        rawImage = GetComponent<UnityEngine.UI.RawImage>();

        rect = rawImage.gameObject.GetComponent<RectTransform>();
        initialsize = new Vector2(rect.sizeDelta.x, rect.sizeDelta.y);
    }

    public void _SetTexture(Texture2D value)
    {
        rawImage.texture = value;

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
