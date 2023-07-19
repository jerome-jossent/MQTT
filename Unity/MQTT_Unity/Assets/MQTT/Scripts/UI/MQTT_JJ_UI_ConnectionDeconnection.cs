using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MQTT_JJ_UI_ConnectionDeconnection : MonoBehaviour
{
    [SerializeField] MQTT_JJ client;
    [SerializeField] MQTT_JJ_UI_Parameters ui_parameters;
    [SerializeField] Button btn_connection;
    [SerializeField] Button btn_deconnection;

    void Start()
    {
        client._statusChanged.AddListener(_ButtonsManageVisibility);
        //StartCoroutine(WaitToExecute(10));
        ButtonsManageVisibility();
    }

    private void _ButtonsManageVisibility(MQTT_JJ.StatusType arg0)
    {
        ButtonsManageVisibility();
    }

    IEnumerator WaitToExecute(int tentative)
    {
        while (client == null && tentative > 0)
        {
            yield return new WaitForSeconds(0.1f);
            tentative--;
        }
        if (!(tentative > 0))        
            Debug.Log("MQTT_JJ_UI_ConnectionDeconnection Failed to init");        
    }

    public void ButtonsManageVisibility()
    {
        //btn_connection.gameObject.SetActive(!client.isConnected);
        btn_deconnection.gameObject.SetActive(client.isConnected);
    }


    public void _Connection()
    {
        ui_parameters._Use_MQTT_Parameters();
        client.Connect();
    }

    public void _Disconnection()
    {
        client.Disconnect();
    }

}
