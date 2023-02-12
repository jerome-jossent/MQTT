using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IP_Port_UI : MonoBehaviour
{
    public int _port;
    public string _ip;
    [SerializeField] TMPro.TMP_Dropdown ip_dropdown;
    [SerializeField] TMPro.TMP_InputField ip_inputfield;
    [SerializeField] TMPro.TMP_InputField port_inputfield;

    List<string> ips;
    const string KeyName_ips = "IPs";
    const string KeyName_ip_last = "IP_last";
    const string KeyName_separator = ";";
    const string KeyName_port = "Port";

    bool loaded;

    void Start()
    {
        loaded = false;
        LoadIPsFromDisk();
        LoadPortFromDisk();
        loaded = true;
    }

    void LoadIPsFromDisk()
    {
        string s = PlayerPrefs.GetString(KeyName_ips);
        ips = s.Split(KeyName_separator).ToList<string>();
        _ip = PlayerPrefs.GetString(KeyName_ip_last);
        LoadIPs();
    }

    void LoadIPs()
    {
        ip_dropdown.ClearOptions();
        ip_dropdown.AddOptions(ips);

        int? i = findIPindex(_ip);
        if (i != null)
            ip_dropdown.value = (int)i;

        ip_inputfield.text=_ip;
    }

    private void LoadPortFromDisk()
    {
        _port = PlayerPrefs.GetInt(KeyName_port);
        port_inputfield.text = _port.ToString();
    }

    int? findIPindex(string IP)
    {
        for (int i = 0; i < ips.Count; i++)
            if (ips[i] == IP)
                return i;
        return null;
    }

    void SaveIPs()
    {
        string s = string.Join(KeyName_separator, ips);
        PlayerPrefs.SetString(KeyName_ips, s);
        SaveIP_last();
        LoadIPs();
    }
    void SaveIP_last()
    {
        PlayerPrefs.SetString(KeyName_ip_last, _ip);
    }

    void SavePort()
    {
        PlayerPrefs.SetInt(KeyName_port, _port);
    }

    public void _ip_deleteSelected()
    {
        string s = ip_inputfield.text;
        int? i = findIPindex(s);
        if (i != null)
            ips.RemoveAt((int)i);
        SaveIPs();
    }

    public void _ip_add()
    {
        string s = ip_inputfield.text;
        if (!ips.Contains(s))
            ips.Add(s);
        SaveIPs();
    }

    public void _ip_Selection_changed(int index)
    {
        if (!loaded) return;
        ip_inputfield.text = ip_dropdown.options[index].text;
    }

    public void _ip_InputFiledValueChange()
    {
        if (!loaded) return;
        _ip = ip_inputfield.text;
    }

    public void _port_InputFiledValueChange()
    {
        if (!loaded) return;
        _port = int.Parse(port_inputfield.text);
        SavePort();
        port_inputfield.text = _port.ToString();
    }

    public void _Connection()
    {
        _ip = ip_inputfield.text;
        SaveIP_last();
    }

    public void _Disconnection()
    {
    }

}
