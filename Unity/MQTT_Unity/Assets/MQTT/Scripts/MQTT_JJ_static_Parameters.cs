using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class MQTT_JJ_static_Parameters
{

    public enum PlayerPrefNames
    {
        MQTT_ID,
        MQTT_IP,
        MQTT_IPs,
        MQTT_Port,
    }

    public static char[] playerPrefSeparator = new char[1] { ',' };

    #region Save & Load Parameters
    internal static string _GetString(PlayerPrefNames ppn, string defaultvalue = null)
    {
        if (defaultvalue != null)
            return PlayerPrefs.GetString(ppn.ToString(), defaultvalue);
        else
            return PlayerPrefs.GetString(ppn.ToString());
    }

    internal static bool _GetBool(PlayerPrefNames ppn, bool? defaultvalue = null)
    {
        if (defaultvalue != null)
        {
            string a = PlayerPrefs.GetString(ppn.ToString());
            if (a == "")
                return (bool)defaultvalue;
            else
                return a.ToLower() == "true";
        }
        else
            return PlayerPrefs.GetString(ppn.ToString()).ToLower() == "true";
    }

    internal static void _Save(PlayerPrefNames ppn, bool val)
    {
        PlayerPrefs.SetString(ppn.ToString(), val.ToString());
    }
    internal static void _Save(PlayerPrefNames ppn, string val)
    {
        PlayerPrefs.SetString(ppn.ToString(), val);
    }

    internal static void _Fill(TMP_InputField txt, PlayerPrefNames ppn, string defaultvalue = null)
    {
        _Fill(txt, _GetString(ppn, defaultvalue));
    }
    internal static void _Fill(TMP_InputField txt, string value)
    {
        txt.text = value;
    }

    internal static void _Fill(Toggle tgl, PlayerPrefNames ppn, bool? defaultvalue = null)
    {
        _Fill(tgl, _GetBool(ppn, defaultvalue));
    } 
    internal static void _Fill(Toggle tgl, bool value)
    {
        tgl.isOn = value;
    }

    internal static void _Fill(TMP_Dropdown dd, PlayerPrefNames ppn, string defaultvalue = null, string valselected = "", bool valChangeEventRise = false)
    {
        string list_concat = _GetString(ppn, defaultvalue);
        string[] list = list_concat.Split(playerPrefSeparator);
        dd.options.Clear();
        foreach (string option in list)
            dd.options.Add(new TMP_Dropdown.OptionData(option));

        if (valselected == "")
            return;

        int index = FindIndexWithVal(dd, valselected);
        if (index < 0)
            return;

        bool isinterectable = dd.interactable;

        if (!valChangeEventRise)
            dd.interactable = false;

        dd.value = index;
        dd.RefreshShownValue();

        if (!valChangeEventRise)
            dd.interactable = isinterectable;
    }

    internal static int FindIndexWithVal(TMP_Dropdown dd, string val)
    {
        for (int i = 0; i < dd.options.Count; i++)
            if (dd.options[i].text == val)
                return i;
        return -1;
    }

    internal static void _Save(TMP_InputField txt, PlayerPrefNames ppn)
    {
        PlayerPrefs.SetString(ppn.ToString(), txt.text);
        PlayerPrefs.Save();
    }

    internal static void _Save(Toggle tgl, PlayerPrefNames ppn)
    {
        PlayerPrefs.SetString(ppn.ToString(), tgl.isOn.ToString());
        PlayerPrefs.Save();
    }
    internal static void _Save(TMP_Dropdown dd, PlayerPrefNames ppn)
    {
        List<string> ips_string = new List<string>();
        foreach (TMP_Dropdown.OptionData option in dd.options)
            ips_string.Add(option.text);
        string ips_string2 = string.Join(playerPrefSeparator[0], ips_string);
        PlayerPrefs.SetString(ppn.ToString(), ips_string2);
        PlayerPrefs.Save();
    }
    #endregion
}