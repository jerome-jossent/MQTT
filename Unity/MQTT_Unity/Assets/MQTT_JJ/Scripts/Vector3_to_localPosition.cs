using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3_to_localPosition : MonoBehaviour
{
    public MQTT_Subscribe_Vector3_ByteArray value;

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = value._valeur;
    }
}
