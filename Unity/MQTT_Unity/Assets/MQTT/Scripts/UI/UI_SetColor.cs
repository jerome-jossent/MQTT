using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SetColor : MonoBehaviour
{
    int version;

    Text text;           //1
    TMPro.TMP_Text tmp_text;            //2
    Image image;         //3
    RawImage raw_image;  //4

    void Start()
    {
        text = GetComponent<Text>();
        if (text != null)
        {
            version = 1;
            return;
        }

        tmp_text = GetComponent<TMPro.TMP_Text>();
        if (tmp_text != null)
        {
            version = 2;
            return;
        }

        image = GetComponent<Image>();
        if (image != null)
        {
            version = 3;
            return;
        }

        raw_image = GetComponent<RawImage>();
        if (raw_image != null)
        {
            version = 4;
            return;
        }

        version = 0;
        Debug.Log("No support found ! Autodestroy component");
        DestroyImmediate(this);
    }

    public void _SetColor(Color? color_nullable)
    {
        if (color_nullable == null) return;
        Color color = (Color)color_nullable;
        switch (version)
        {
            case 1: text.color = color; break;
            case 2: tmp_text.color = color; break;
            case 3: image.color = color; break;
            case 4: raw_image.color = color; break;
        }
    }
}
