using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Net.NetworkInformation;

public class BLEDevice
{
    public string deviceName;
    public string deviceAddress;
}
public class BLESevice
{
    public string deviceAddress;
    public string serviceUUID;
    public string characteristicUUID;
}

public class BLEManager : Singleton<BLEManager>
{
    private bool isInited = false;
    public bool IsInited
    {
        get => isInited;
        set
        {
            isInited = value;
        }
    }
    public bool @continue = false;//是否有正在检测的
    public Action onBleInited;
    public Action<string> onBleInitFalse;
    public Action onStartConnect;
    public Action onConnectDevice;
    public Action onConnectDeviceFailed;
    public Action onScanDevice;
    public Action onCloseDeviceList;
    public Action<string,string> onDiscoveredDevice;
    public Action<bool> onHandshake;
    public Action<List<float[]>> onGetData;
    public bool connected = false;
    private bool handshake = false;

    private BluetoothDeviceScript deviceScript;

    public BLEDevice bLEDevice = new BLEDevice();
    public BLESevice bLESevice = new BLESevice();

    private Dictionary<string, string> deviceDict = new Dictionary<string, string>();

    public bool IsNetwork => IsNetworkReachability();
    /// <summary>
    /// 初始化蓝牙模块
    /// </summary>
    public void InitBLE()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndriodPermission.RequireLocationPermission();
        if (AndriodPermission.RequireBluetoothPermission())
#endif
        {
            Debug.Log("初始化蓝牙");
            deviceScript = BluetoothLEHardwareInterface.Initialize(true, false, delegate
            {
                IsInited = true;
                onBleInited?.Invoke();
            }, (errorInfo) =>
            {
                onBleInitFalse?.Invoke(errorInfo);
            });

        }
    }


    /// <summary>网络可达性</summary> 
    /// <returns>网络可达性状态</returns>
    protected bool IsNetworkReachability()
    {
        switch (Application.internetReachability)
        {
            case NetworkReachability.ReachableViaLocalAreaNetwork:
                Debug.Log("当前使用的是：WiFi！");
                return true;
            case NetworkReachability.ReachableViaCarrierDataNetwork:
                Debug.Log("当前使用的是：移动网络！");
                return true;
            default:
                Debug.Log("当前没有联网，请您先联网后再进行操作！");
                return false;
        }
    }

    /// <summary>
    /// 扫描设备
    /// </summary>
    public void ScanBLEDevice()
    {
        Debug.Log("扫描蓝牙");
        onScanDevice?.Invoke();
        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, (address, name) =>
         {
             if (name == "NoName" || name == "No Name" || name == "Noname" || name == "No name") return;
             if (!deviceDict.Keys.Contains(address))
             {
                 onDiscoveredDevice?.Invoke(name,address);
                 deviceDict.Add(address, name);
             }
         });
        
    }

    /// <summary>
    /// 链接设备
    /// </summary>
    /// <param name="address"></param>
    public void ConnectToBLE(string address)
    {
        try
        {
            onStartConnect?.Invoke();
            Debug.Log("连接蓝牙" + address);
            BluetoothLEHardwareInterface.ConnectToPeripheral(address, (a) =>
            {
                
            }, (a, uuid) =>
            {
            }, (a, serviceUUID, characteristicUUID) =>
            {
                Debug.Log("蓝牙连接成功");
                //if (connected) return;
                connected = true;
                BluetoothLEHardwareInterface.StopScan();
                bLESevice.deviceAddress = address;
                bLESevice.serviceUUID = serviceUUID;
                bLESevice.characteristicUUID = characteristicUUID;
                
                StartCoroutine(DelayToHandshake());
            });
        }
        catch (Exception e)
        {
            Debug.Log("连接蓝牙失败" + e.Message);
            ScanBLEDevice();
            onConnectDeviceFailed?.Invoke();
        }
        
    }
    private IEnumerator DelayToHandshake()
    {
        yield return new WaitForSeconds(1f);
        SubscribeCharacteristicWithDeviceAddress();
        yield return new WaitForSeconds(1f);
        
            SendMessageToMCU(CMDType.HELLO);
        
    }

    private bool recieving = false;
    private byte[] recievingData = new byte[] { };
    private short length = 0;
    /// <summary>
    /// 订阅蓝牙服务，可以和蓝牙模块通信
    /// </summary>
    private void SubscribeCharacteristicWithDeviceAddress()
    { 
        BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(bLESevice.deviceAddress, bLESevice.serviceUUID, bLESevice.characteristicUUID, (a, b) =>
           {

           }, (deviceAddress, characteristic, data) =>
         {
             //TODO:解析数据
             Debug.Log("蓝牙发来了数据" + BitConverter.ToString(data));
             if(!recieving)
             if(data[0] == 0xaa)//帧头
             {
                 length = BitConverter.ToInt16(new byte[] {data[4],data[3]},0);//长度
                 recieving = true;
             }
             if(recieving)
             {
                 Debug.Log("开始接收数据");
                 recievingData = recievingData.Combine(data);
                 Debug.Log(recievingData.Length + "      " + length + "数据长度");
                 if(recievingData.Length >= length)
                 {
                     Debug.Log("长度够了");
                     //再判断校验位
                     Debug.Log(recievingData.GetByteSumLow8IgnoreLast() + "    " + data[data.Length - 1]);
                     if (recievingData.GetByteSumLow8IgnoreLast() == data[data.Length - 1])
                     {
                         Debug.Log("接收数据完成" +BitConverter.ToString(recievingData));
                         
                         //接收正确
                         Data result = new Data(recievingData);
                         switch (result.cmd)
                         {
                             case CMDType.HELLO_g:
                                 ParseStatus(result.STATUS, () =>
                                 {
                                     Debug.Log("握手成功");
                                     if (handshake) return;
                                     handshake = true;
                                     onHandshake?.Invoke(@continue);
                                     onConnectDevice?.Invoke();
                                     StartCoroutine(BLEHeartBeat());
                                 });
                                 break;
                             case CMDType.SET_Temperature_g:
                                 break;
                             case CMDType.GET_Actual_Temperature_g:
                                 break;
                             case CMDType.MT_RESET_g:
                                 break;
                             case CMDType.MT_MOVE_TO_g:
                                 break;
                             case CMDType.SET_ROTATION_g:
                                 break;
                             case CMDType.GET_SIG_g:
                                 break;
                             case CMDType.SET_PARAMETERS_g:
                                 ParseStatus(result.STATUS, () =>
                                 {
                                     Debug.Log("设置参数成功");
                                     //UI_Manager.Instance.OpenPanel<UI_Panel_OperatingTips>();
                                 });
                                 break;
                             case CMDType.START_EXPERIMENT_g:
                                 ParseStatus(result.STATUS, () =>
                                 {
                                     Debug.Log("开始检测");
                                     UI_Manager.Instance.OpenPanel<UI_Panel_Checking>();
                                 });
                                 break;
                             case CMDType.Stop_Experiment_g:
                                 ParseStatus(result.STATUS, () =>
                                 {
                                     Debug.Log("终止检测");
                                     UI_Manager.Instance.OnStopCheck();
                                 });
                                 break;
                             case CMDType.GET_TEST_DATA_g:
                                 Debug.Log("获取到了数据");
                                 byte[] DataX_Y = result.DATA.GetArea(12, UI_Manager.Instance.programData.num * 32);
                                 List<float[]> datasavages = new List<float[]>();
                                 List<int[]> datas = new List<int[]>();
                                 for (int i = 0; i < UI_Manager.Instance.programData.num; i++)
                                 {
                                     byte[] linedata = new byte[32];
                                     int[] linedata2 = new int[16];//高位和低位转成int
                                     float[] linedata3 = new float[4];//计算平均数 用于画线  暂时不用
                                     for (int j = 0; j < linedata.Length; j++)
                                     {
                                         linedata[j] = DataX_Y[i * 32 + j];
                                     }
                                     for (int j = 0; j < linedata2.Length; j++)
                                     {
                                         linedata2[j] = BitConverter.ToUInt16(new byte[] { linedata[j * 2 + 1], linedata[j * 2] }, 0);
                                     }
                                     for (int j = 0; j < linedata3.Length; j++)
                                     {
                                         float r = (linedata2[j * 4] + linedata2[j * 4 + 1] + linedata2[j * 4 + 2] + linedata2[j * 4 + 3]) / 4f;
                                         linedata3[j] = r;
                                     }
                                     //按照平均数计算，暂时弃用
                                     datasavages.Add(linedata3);
                                     datas.Add(linedata2);
                                 }
                                 //荧光数据每次获得数据 通道1-16 每4个一组 分别对应指标DABC  D指标不显示  不用保存  不用上传
                                 for (int i = 0; i < UI_Manager.Instance.personInfos.Length; i++)
                                 {
                                     for (int j = 0; j < datas.Count; j++)
                                     {
                                         UI_Manager.Instance.personInfos[i].dataas[j] = datas[j][i * 4 + 1];
                                         UI_Manager.Instance.personInfos[i].databs[j] = datas[j][i * 4 + 2];
                                         UI_Manager.Instance.personInfos[i].datacs[j] = datas[j][i * 4 + 3];
                                     }
                                 }
                                 for (int i = 0; i < UI_Manager.Instance.personInfos.Length; i++)
                                 {
                                     for (int j = 0; j < datasavages.Count; j++)
                                     {
                                         //if (datas[j][i] < 10) continue;
                                         UI_Manager.Instance.personInfos[i].avages[j] = datasavages[j][i];
                                     }
                                 }
                                 Debug.Log("接受到了数据即将绘图！");
                                 onGetData?.Invoke(datasavages);
                                 if (result.ID == 0)//得到了结果
                                 {
                                     Debug.Log(BitConverter.ToString(new byte[] { result.DATA[0], result.DATA[1], result.DATA[2], result.DATA[3] }));
                                     UI_Manager.Instance.OnCheckDone(new byte[] { result.DATA[0], result.DATA[1], result.DATA[2], result.DATA[3], result.DATA[4], result.DATA[5], result.DATA[6], result.DATA[7], result.DATA[8], result.DATA[9], result.DATA[10], result.DATA[11] });
                                 }
                                 break;
                             default:
                                 break;
                         }
                         recieving = false;
                         recievingData = new byte[] { };
                         length = 0;
                     }
                     else
                     {
                         Debug.Log("接收数据失败");
                         recieving = false;
                         recievingData = new byte[] { };
                         length = 0;
                     }
                 }
             }

             
         });
    }
    private void ParseStatus(byte STATUS, Action action)
    {
        switch (STATUS)
        {
            case 0x00:
                action?.Invoke();
                break;
            case 0x01:
                Debug.Log("操作失败");
                UI_Manager.Instance.OpenPanel<UI_PanelMessageBox>(UILevel.pop, new MessageBoxData()
                {
                    content = "操作失败",
                    fadetime = 3
                });
                break;
            case 0x03:
                Debug.Log("错误指令或错误数据");
                UI_Manager.Instance.OpenPanel<UI_PanelMessageBox>(UILevel.pop, new MessageBoxData()
                {
                    content = "错误指令或错误数据",
                    fadetime = 3
                });
                break;
            default:
                break;
        }
    }

    public void SendMessageToMCU(byte[] data)
    {
        Debug.Log($"{bLESevice.deviceAddress}....{bLESevice.serviceUUID}....{bLESevice.characteristicUUID}");
        BluetoothLEHardwareInterface.WriteCharacteristic(bLESevice.deviceAddress, bLESevice.serviceUUID, bLESevice.characteristicUUID,
            data, data.Length, true, (re) =>
               {
                   Debug.Log(re);
               });
    }

    /// <summary>
    /// 按照指令发送消息
    /// </summary>
    /// <param name="cmdType"></param>
    /// <param name="id"></param>
    /// <param name="data"></param>
    public void SendMessageToMCU(CMDType cmdType,byte id = 0,params byte[] data)
    {
        byte[] mess = new Data(cmdType, id, data).ToData();
        Debug.Log(BitConverter.ToString(mess));
        SendMessageToMCU(mess);
    }


    private IEnumerator BLEHeartBeat()
    {
        while (connected)
        {
            SendMessageToMCU(CMDType.CONNECTION, 0);
            yield return new WaitForSeconds(5);
        }
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            //Test();
        }
    }

    public void Test()
    {
        string test = "AA-9E-00-03-CB-03-03-03-03-0C-99-0C-6D-0C-5C-0C-1C-0B-F8-0C-87-0C-EB-0C-55-0C-B8-0C-B9-0C-98-0C-B3-0C-20-0B-DA-0C-02-0C-8B-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-60";
        string[] splits = test.Split('-');
        byte[] data = new byte[splits.Length];
        for (int i = 0; i < splits.Length; i++)
        {
            splits[i] = splits[i].ToLower();
            data[i] = (byte)Convert.ToInt32(splits[i],16);
        }
        byte[] DataX_Y = data.GetArea(10, 5 * 32);
        
        List<float[]> datas = new List<float[]>();
        for (int i = 0; i < 5; i++)
        {
            byte[] linedata = new byte[32];
            int[] linedata2 = new int[16];//高位和低位转成int
            float[] linedata3 = new float[4];
            for (int j = 0; j < linedata.Length; j++)
            {
                linedata[j] = DataX_Y[i * 32 + j];
            }
            for (int j = 0; j < linedata2.Length; j++)
            {
                linedata2[j] = BitConverter.ToUInt16(new byte[] {linedata[j * 2 + 1], linedata[j * 2] }, 0);
            }
            for (int j = 0; j < linedata3.Length; j++)
            {
                float r = (linedata2[j * 4] + linedata2[j * 4 + 1] + linedata2[j * 4 + 2] + linedata2[j * 4 + 3]) / 4f;
                linedata3[j] = r;
                Debug.Log(linedata3[j]);
            }
            datas.Add(linedata3);
        }
        
        onGetData?.Invoke(datas);
    }
    #region 耦合函数
    ///// <summary>
    ///// 握手
    ///// </summary>
    //public void HandShake()
    //{
    //    byte[] data = new Data(CMDType.HELLO, 0, null).ToData();
    //    SendMessage(data);
    //}
    ///// <summary>
    ///// 设置目标温度
    ///// </summary>
    ///// <param name="temp_select"></param>
    ///// <param name="temp_h"></param>
    ///// <param name="temp_l"></param>
    //public void SetTempreature(byte temp_select,byte temp_h,byte temp_l)
    //{
    //    byte[] data = new Data(CMDType.SET_Temperature, temp_select, new byte[] { temp_h, temp_l }).ToData();
    //    SendMessage(data);
    //}
    ///// <summary>
    ///// 获取实际温度
    ///// </summary>
    ///// <param name="temp_select"></param>
    //public void GET_Actual_Temperature(byte temp_select)
    //{
    //    byte[] data = new Data(CMDType.GET_Actual_Temperature, temp_select, null).ToData();
    //    SendMessage(data);
    //}
    ///// <summary>
    ///// 顶针电机复位指令
    ///// </summary>
    //public void MT_RESET()
    //{
    //    byte[] data = new Data(CMDType.MT_RESET,0, null).ToData();
    //    SendMessage(data);
    //}
    ///// <summary>
    ///// 顶针电机移动到指令
    ///// </summary>
    ///// <param name="h"></param>
    ///// <param name="m"></param>
    ///// <param name="l"></param>
    //public void MT_MOVE_TO(byte h,byte m,byte l)
    //{
    //    byte[] data = new Data(CMDType.MT_RESET, 0, new byte[] { h,m,l}).ToData();
    //    SendMessage(data);
    //}
    ///// <summary>
    ///// 设置盘片转动参数
    ///// </summary>
    ///// <param name="spd_h"></param>
    ///// <param name="spd_l"></param>
    //public void SET_ROTATION(byte spd_h,byte spd_l)
    //{
    //    byte[] data = new Data(CMDType.MT_RESET, 0, new byte[] { spd_h, spd_l }).ToData();
    //    SendMessage(data);
    //}
    //public void GET_SIG()
    //{
    //    byte[] data = new Data(CMDType.GET_SIG, 0, null).ToData();
    //    SendMessage(data);
    //}
    ///// <summary>
    ///// 开始检测
    ///// </summary>
    //public void StartCheck()
    //{
    //    byte[] data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
    //    SendMessage(data);
    //}
    #endregion
}
