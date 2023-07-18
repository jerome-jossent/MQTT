using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearAllChildren : MonoBehaviour
{
    [SerializeField] bool clearAtStart;
    public void Start()
    {
        if (clearAtStart)
            _ClearAllChildren(gameObject);
    }

    public static void _ClearAllChildren(GameObject Parent)
    {
        while (Parent.transform.childCount > 0)
            GameObject.DestroyImmediate(Parent.transform.GetChild(0).gameObject);
    }
}
