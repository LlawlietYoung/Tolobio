using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BLEDevices : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public Transform content;
    public BleDeviceItem deviceItem;
    private List<BleDeviceItem> bleDeviceItems = new List<BleDeviceItem>();
    public Button btn_close;
    public Animator connecting;
   
    public void Init()
    {
        Debug.Log("inited");
        canvasGroup = GetComponent<CanvasGroup>();
        BLEManager.Instance.onConnectDeviceFailed += () =>
            {
                Open();
            };
        BLEManager.Instance.onDiscoveredDevice += (name, address) =>
        {
            Debug.Log(name + "  " + address);

            BleDeviceItem item = Instantiate(deviceItem, content);
            item.Init(name, address);
            bleDeviceItems.Add(item);
        };

        BLEManager.Instance.onStartConnect += () =>
        {
            connecting.gameObject.SetActive(true);
        };

        BLEManager.Instance.onConnectDevice += () =>
        {
            Close();
        };
        btn_close.onClick.AddListener(() =>
        {
            BluetoothLEHardwareInterface.StopScan();
            if(BLEManager.Instance.@continue)
            {
                UI_Manager.Instance.OpenPanel<UI_PanelMessageBox>(UILevel.pop, new MessageBoxData()
                {
                    content = "是否关闭未完成项目？",
                    onconfirm = () =>
                    {
                        UI_Manager.Instance.CancelCheckUnDonePrograme();
                        BLEManager.Instance.onCloseDeviceList?.Invoke();
                        Close();
                    }
                });
            }
            else
            {
                Close();
                BLEManager.Instance.onCloseDeviceList?.Invoke();
            }
        });
    }

    public void Open()
    {
        connecting.gameObject.SetActive(false);
        gameObject.SetActive(true);
        canvasGroup.DOFade(1, 0.2f);
        Debug.Log("打开设备列表");
    }
    public void Close()
    {
        connecting?.gameObject.SetActive(false);
        canvasGroup.DOFade(0, 0.2f).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
        Debug.Log("关闭设备列表");
    }
}
