using UnityEngine;
using TMPro;
using System;

public class MQTT_JJ_UI_Parameters : MonoBehaviour
{
    [SerializeField] MQTT_JJ mqtt;

    [SerializeField] TMP_InputField if_id;
    [SerializeField] TMP_InputField if_ip;
    [SerializeField] TMP_Dropdown dd_ips;
    [SerializeField] TMP_InputField if_port;

    public void Start()
    {
        _Load_MQTT_Parameters();
    }

    public void _Load_MQTT_Parameters()
    {
        if (mqtt._ClientID_JJ != "")
            MQTT_JJ_static_Parameters._Fill(if_id, mqtt._ClientID_JJ);
        else
            MQTT_JJ_static_Parameters._Fill(if_id, MQTT_JJ_static_Parameters.PlayerPrefNames.MQTT_ID);

        if (mqtt.brokerAddress != "")
            MQTT_JJ_static_Parameters._Fill(if_ip, mqtt.brokerAddress);
        else
            MQTT_JJ_static_Parameters._Fill(if_ip, MQTT_JJ_static_Parameters.PlayerPrefNames.MQTT_IP, "localhost");

        MQTT_JJ_static_Parameters._Fill(dd_ips, MQTT_JJ_static_Parameters.PlayerPrefNames.MQTT_IPs, if_ip.text);

        if (mqtt.brokerPort != 0)
            MQTT_JJ_static_Parameters._Fill(if_port, mqtt.brokerPort.ToString());
        else
            MQTT_JJ_static_Parameters._Fill(if_port, MQTT_JJ_static_Parameters.PlayerPrefNames.MQTT_Port, "1883");
    }

    public void _Save_MQTT_Parameters()
    {
        MQTT_JJ_static_Parameters._Save(if_id, MQTT_JJ_static_Parameters.PlayerPrefNames.MQTT_ID);
        MQTT_JJ_static_Parameters._Save(if_ip, MQTT_JJ_static_Parameters.PlayerPrefNames.MQTT_IP);
        MQTT_JJ_static_Parameters._Save(dd_ips, MQTT_JJ_static_Parameters.PlayerPrefNames.MQTT_IPs);
        MQTT_JJ_static_Parameters._Save(if_port, MQTT_JJ_static_Parameters.PlayerPrefNames.MQTT_Port);
    }

    public void _IP_Add()
    {
        string ip_to_add = if_ip.text;
        dd_ips.options.Add(new TMP_Dropdown.OptionData(ip_to_add));
        MQTT_JJ_static_Parameters._Save(dd_ips, MQTT_JJ_static_Parameters.PlayerPrefNames.MQTT_IPs);
        MQTT_JJ_static_Parameters._Fill(dd_ips, MQTT_JJ_static_Parameters.PlayerPrefNames.MQTT_IPs);
        //select
        dd_ips.value = dd_ips.options.Count - 1;
        dd_ips.RefreshShownValue();
    }

    public void _IP_Remove()
    {
        string ip_to_remove = dd_ips.options[dd_ips.value].text;
        int index = MQTT_JJ_static_Parameters.FindIndexWithVal(dd_ips, ip_to_remove);
        if (index < 0)
            return;

        dd_ips.options.RemoveAt(index);
        MQTT_JJ_static_Parameters._Save(dd_ips, MQTT_JJ_static_Parameters.PlayerPrefNames.MQTT_IPs);
        MQTT_JJ_static_Parameters._Fill(dd_ips, MQTT_JJ_static_Parameters.PlayerPrefNames.MQTT_IPs);
        dd_ips.value = index - 1;
        dd_ips.RefreshShownValue();
    }

    public void _IP_Set_FromDropDown(int index)
    {
        if_ip.text = dd_ips.options[index].text;
    }

    internal void _Use_MQTT_Parameters()
    {
        if (if_id.text != "")
            mqtt._ClientID_JJ = if_id.text;

        mqtt.brokerAddress = if_ip.text;
        mqtt.brokerPort = int.Parse(if_port.text.Trim());
    }
}
