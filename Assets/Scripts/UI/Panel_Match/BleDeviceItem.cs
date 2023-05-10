using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BleDeviceItem : MonoBehaviour
{
    public Text tt_name,tt_uuid;
    public Button btn_connect;

    public void Init(string name,string uuid)
    {
        tt_name.text = name;
        tt_uuid.text = uuid;
        btn_connect.onClick.AddListener(() =>
        {
            BLEManager.Instance.ConnectToBLE(uuid);
            UI_Manager.Instance.PlayClickSound();
        });
    }
}
