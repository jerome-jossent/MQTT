using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using static MQTT_JJ;

public class MQTT_JJ_Subscribe_GameObject : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text txt;
    [SerializeField] Collider2D col;
    MQTT_JJ_Subscribe sub;
    public GameObject selectedObject;

    public void _Link(MQTT_JJ mqtt, string topic, DataType dataType)
    {
        sub = GetComponent<MQTT_JJ_Subscribe>();
        sub.mqtt = mqtt;
        sub.topic = topic;
        sub.dataType = dataType;
        sub.onNewMessage.AddListener(NewString);
    }

    void NewString(MQTT_JJ_Message stringpayload)
    {
        txt.text = (string)stringpayload.val;
    }

    private void OnDestroy()
    {
        sub?.onNewMessage.RemoveAllListeners();
    }

    public void Delete()
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //if (Input.GetMouseButtonDown(0))
        {
            Collider2D targetObject = Physics2D.OverlapPoint(mousePosition);
            if (targetObject)
            {
                selectedObject = targetObject.transform.gameObject;
                Debug.Log(selectedObject.name);
            }
        }


        //if (Input.touchCount > 0)
        //{
        //    Vector3 wp = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
        //    if (col.OverlapPoint(wp))
        //    {
        //        //your code
        //        Debug.Log("Hello");
        //    }
        //}
        //else
        //{
        //    Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //    if (col.OverlapPoint(point))
        //    {
        //        //your code
        //        Debug.Log("Hello 2");
        //    }
        //}
    }

}