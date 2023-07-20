using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetColor : MonoBehaviour
{
    [SerializeField] Renderer renderer;

    public void _SetColor(Color? color_nullable)
    {
        if (color_nullable == null) return;
        Color color = (Color)color_nullable;
        renderer.sharedMaterial.color = color;
    }
}
