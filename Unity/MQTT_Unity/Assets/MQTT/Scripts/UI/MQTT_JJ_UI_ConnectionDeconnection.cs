using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MQTT_JJ_UI_ConnectionDeconnection : MonoBehaviour
{
    [SerializeField] MQTT_JJ_Client client;
    [SerializeField] MQTT_JJ_UI_Parameters ui_parameters;
    [SerializeField] Button btn_connection;
    [SerializeField] Button btn_deconnection;

    void Start()
    {
        client._statusChanged.AddListener(_ButtonsManageVisibility);
        ButtonsManageVisibility();
    }

    private void _ButtonsManageVisibility(MQTT_JJ_Client.StatusType arg0)
    {
        ButtonsManageVisibility();
    }

    public void ButtonsManageVisibility()
    {
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
