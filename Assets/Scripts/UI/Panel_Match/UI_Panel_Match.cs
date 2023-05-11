using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;



public class UI_Panel_Match : PanelBase
{
    #region 检测页
    public QRCodeScanner qRCodeScanner;

    public Button btn_scan;
    public Button tg_bluetooth;
    public BLEButton bLEButton;
    public BLEDevices bLEDevices;
    #endregion
    #region 我的页
    public Button btn_record;

    public override void OnClose()
    {
        
    }
    #endregion

    public override void OnInit(UIData data = null)
    {
        qRCodeScanner.Init();
        //bLEButton.Init();
        bLEButton.button.onClick.AddListener(() =>
        {
            if(BLEManager.Instance.connected)
            {
                //UI_Manager.Instance.OpenPanel<UI_PanelMessageBox>(UILevel.pop, new MessageBoxData()
                //{
                //    content = "断开已连接蓝牙"
                //});
            }
            else
            {
                BLEManager.Instance.ScanBLEDevice();
            }
        });
        BLEManager.Instance.onScanDevice += () =>
        {
            //btn_scan.interactable = false;
            bLEDevices.Open();
        };
        btn_scan.onClick.AddListener(() =>
        {
            if (!BLEManager.Instance.IsNetwork)
            {
                UI_Manager.Instance.OpenPanel<UI_PanelMessageBox>(UILevel.pop, new MessageBoxData()
                {
                    content = "请先连接网络再扫码！",
                    fadetime = 3
                });
                return;
            }
#if UNITY_EDITOR
            DecodeController_onQRScanFinished("8");
            Debug.Log("开始扫描");
#else
            qRCodeScanner.StartWork();
            Debug.Log("开始扫描");
#endif
        });
        BLEManager.Instance.onConnectDevice += () =>
          {
              //tg_bluetooth.GetComponentInChildren<Text>().text = "蓝牙已连接";
              bLEButton.Connected = true;
              btn_scan.interactable = true;
          };

        qRCodeScanner.decodeController.onQRScanFinished += (result) =>
        {
            qRCodeScanner.StopWork();
        };
        bLEDevices.Init();
        BLEManager.Instance.onHandshake += (b) =>
          {
              btn_scan.interactable = true;

              if (b)
              {
                  UI_Manager.Instance.OpenPanel<UI_Panel_Checking>();
                  UI_Manager.Instance.OpenPanel<UI_PanelMessageBox>(UILevel.pop, new MessageBoxData()
                  {
                      content = "蓝牙已连接，获取数据中，请稍等！",
                      fadetime = 3
                  });
              }
              else
              {
                  UI_Manager.Instance.OpenPanel<UI_PanelMessageBox>(UILevel.pop, new MessageBoxData()
                  {
                      content = "蓝牙已连接，请开始扫码！",
                      fadetime = 3
                  });
              }

          };
        qRCodeScanner.decodeController.onQRScanFinished += DecodeController_onQRScanFinished;
        btn_record.onClick.AddListener(() =>
        {
            UI_Manager.Instance.OpenPanel<UI_Panel_record>();
        });
    }

    public override void OnLoop()
    {
        
    }

    public override void OnOpen(UIData data = null)
    {
        UI_Manager.Instance.ResetState();
        btn_scan.interactable = BLEManager.Instance.connected;
        bLEButton.Connected = BLEManager.Instance.connected;
    }


    private void DecodeController_onQRScanFinished(string id)
    {
        HttpHelper.GetProgramInfo(id, (b, res) =>
         {
             if (b)
             {
                 Debug.Log(res.data.name);
                 UI_Manager.Instance.qrcode = id;
                 UI_Manager.Instance.programData = res.data;
                 UI_Manager.Instance.OpenPanel<UI_Panel_ProgramInfo>();

                 Debug.Log(UI_Manager.Instance.programData.num + "    " + UI_Manager.Instance.programData.gap);
                 //设置参数，给设备发送Setting_Parmeters
                 string temp = res.data.lxParams.Replace("[", "").Replace("]", "").Replace("(","").Replace(")","");
                 string[] datas = temp.Split(',');
                 byte[] param = new byte[datas.Length];
                 
                 for (int i = 1; i < datas.Length; i+=3)
                 {
                     if (UI_Manager.Instance.programData.flag - 1 == (i - 1)/3)
                        UI_Manager.Instance.checktime_check1+=int.Parse(datas[i])*60;
                     else
                        UI_Manager.Instance.checktime_check1 += int.Parse(datas[i]);

                     //Debug.Log(datas[i]);
                 }
                 Debug.Log(UI_Manager.Instance.checktime_check1 + "离心时间");
                 
                 UI_Manager.Instance.checktime_total = UI_Manager.Instance.checktime_check1 + res.data.gap * res.data.num;

                 Debug.Log(UI_Manager.Instance.checktime_total + "总时间");

                 for (int i = 0; i < UI_Manager.Instance.personInfos.Length; i++)
                 {
                     UI_Manager.Instance.personInfos[i].avages = new double[res.data.num];
                     UI_Manager.Instance.personInfos[i].dataas = new double[res.data.num];
                     UI_Manager.Instance.personInfos[i].databs = new double[res.data.num];
                     UI_Manager.Instance.personInfos[i].datacs = new double[res.data.num];
                 }
             }
             else
             {
                 Debug.Log("没有内容");
                 //TODO:提示重新扫码
                 UI_Manager.Instance.OpenPanel<UI_PanelMessageBox>(UILevel.pop, new MessageBoxData()
                 {
                     title = "匹配失败",
                     content = "未匹配到项目，请检查二维码后重新扫码。",

                 });
             }
         });
    }
}
