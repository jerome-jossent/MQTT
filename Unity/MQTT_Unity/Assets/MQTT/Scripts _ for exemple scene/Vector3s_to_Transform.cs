using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3s_to_Transform : MonoBehaviour
{
    public void _SetLocalPosition(Vector3? value)
    {
        if (value != null)
            transform.localPosition = (Vector3)value;
    }
    public void _SetLocalRotation(Vector3? value)
    {
        if (value != null)
            transform.localEulerAngles = (Vector3)value;
    }
    public void _SetLocalScale(Vector3? value)
    {
        if (value != null)
            transform.localScale = (Vector3)value;
    }
    public void _SetAbsolutePosition(Vector3? value)
    {
        if (value != null)
            transform.position = (Vector3)value;
    }
    public void _SetAbsoluteRotation(Vector3? value)
    {
        if (value != null)
            transform.eulerAngles = (Vector3)value;
    }
}
