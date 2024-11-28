using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Net;
public class GetCurrentIP : MonoBehaviour
{
    TMP_InputField inputField;
    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        var strHostName = Dns.GetHostName();
        var ipEntry = Dns.GetHostEntry(strHostName);
        var addresses = ipEntry.AddressList;
        inputField.text = addresses[addresses.Length - 1].ToString();
    }
}
