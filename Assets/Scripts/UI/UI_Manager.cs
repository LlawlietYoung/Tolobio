using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using System.Text;
using System;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public interface UIData
{

}
public enum Result
{
    invalid = 0,//无效
    positive,//阴性
    negetive,//阳性
    nodisk,//未放盘片
    none
}

public class PersonInfo
{
    public string name="";
    public string phone="";
    public Result resulta=Result.none;
    public Result resultb=Result.none;
    public Result resultc=Result.none;
    public double[] dataas, databs, datacs;
    public double[] avages;
}

public class ProgramRecord
{
    public string programname;
    public string programeinfo;
    public string date;
    public PersonInfo[] personInfos;
    public double totaltime;
    public double progress;
    public string bleadress;
    public bool done;//是否完成
    public string qrcode;
    public bool upload;
    public string savedate;
}



public class RecordJson
{
    public RecordJson()
    {
        Records = new List<ProgramRecord>();
    }
    public List<ProgramRecord> Records;
}

public class UI_Manager : Singleton<UI_Manager>
{
    private string path;
    [SerializeField]
    private List<PanelBase> panels = new List<PanelBase>();
    [SerializeField]
    private PanelBase messagebox;

    public ProgramData programData;
    [HideInInspector]
    public PersonInfo[] personInfos = new PersonInfo[4]
        {
            new PersonInfo(),
            new PersonInfo(),
            new PersonInfo(),
            new PersonInfo()
        };

    [HideInInspector]
    public string[] resultContents;


    /// <summary>
    /// 历史记录，转成json保存到本地
    /// </summary>
    public RecordJson recordJsons = new RecordJson();

    public bool UploadRecord = false;
    public string qrcode;
    public int checktime_total = 0;
    public int checktime_check1 = 0;

    public ProgramRecord currentRecord;


    public Action<byte[]> OnCheckDone;

    public AudioSource audio;
    public AudioClip audioClip;


    protected override void Awake()
    {
        base.Awake();
        path = Path.Combine(Application.persistentDataPath, "record.json");
    }

    private void Test()
    {
        recordJsons.Records.Add(new ProgramRecord());
        Debug.Log(JsonMapper.ToJson(recordJsons));
    }
    private void Start()
    {
        resultContents = new string[]
        {
            "无效","<color=green>结果阴性</color>","<color=red>结果阳性</color>","未放盘片"
        };
        programData = new ProgramData();
        if (!BLEManager.Instance.IsInited) BLEManager.Instance.InitBLE();
        OpenPanel<UI_Panel_Match>();
        foreach (var item in transform.GetComponentsInChildren<Button>(true))
        {
            item.onClick.AddListener(() =>
            {
                PlayClickSound();
            });
        }
        foreach (var item in transform.GetComponentsInChildren<Toggle>(true))
        {
            item.onValueChanged.AddListener((b) =>
            {
                PlayClickSound();
            });
        }
        GetRecord();
    }
    public void OpenPanel<T>(UILevel uILevel = UILevel.common,UIData data = null) where T:PanelBase
    {
        switch (uILevel)
        {
            case UILevel.common:
                foreach (var item in panels)
                {
                    if (item is T)
                    {
                        item.Init(data);
                    }
                    else
                    {
                        item.Close();
                    }
                }
                break;
            case UILevel.pop:
                messagebox.Init(data);
                break;
            default:
                break;
        }
    }

    public void ClosePanel<T>()
    {
        foreach (var item in panels)
        {
            if (item is T)
            {
                item.Close();
            }
        }
    }

    public void ResetProgram()
    {
        BluetoothLEHardwareInterface.DisconnectPeripheral(BLEManager.Instance.bLESevice.deviceAddress, str =>
        {
            SceneManager.LoadScene(0);
        });
    }

    public void GetRecord()
    {
        //string path = Path.Combine(Application.persistentDataPath, "record.json");
        if(!File.Exists(path))
        {
            recordJsons = new RecordJson()
            {
                Records = new List<ProgramRecord>()
            };
            FileStream fs = File.Create(path);
            fs.Close();
            Debug.Log(File.Exists(path) + "是否创建成功");
        }
        else
        {
            string json = File.ReadAllText(path);
            Debug.Log(json + "记录内容");
            if (string.IsNullOrEmpty(json)) return;
            Debug.Log(path);
            recordJsons = JsonMapper.ToObject<RecordJson>(json);
            if (recordJsons.Records == null) return;
            if (recordJsons.Records.Count == 0) return;
            ProgramRecord item = recordJsons.Records.Last();
                Debug.Log(item.programeinfo + "记录 ");
                DateTime checkdate = DateTime.Parse(item.date);

                Debug.Log((DateTime.Now - checkdate).TotalSeconds + "       " + item.totaltime * 2 +"检测时间");
            if (!item.done)
            {
                if ((DateTime.Now - checkdate).TotalSeconds < item.totaltime * 2)
                {
                    UI_Manager.Instance.OpenPanel<UI_PanelMessageBox>(UILevel.pop, new MessageBoxData()
                    {
                        content = "未检测完的项目点击确定继续，结束检测请重启设备！",
                        onconfirm = () =>
                        {
                            personInfos = item.personInfos;
                            qrcode = item.qrcode;
                            UploadRecord = item.upload;
                            currentRecord = item;
                            HttpHelper.GetProgramInfo(item.qrcode, (b, res) =>
                             {
                                 if (b)
                                 {
                                     programData = res.data;
                                     string temp = res.data.lxParams.Replace("[", "").Replace("]", "").Replace("(", "").Replace(")", "");
                                     string[] datas = temp.Split(',');
                                     byte[] param = new byte[datas.Length];

                                     for (int i = 1; i < datas.Length; i += 3)
                                     {
                                         if (UI_Manager.Instance.programData.flag - 1 == (i - 1) / 3)
                                             UI_Manager.Instance.checktime_check1 += int.Parse(datas[i]) * 60;
                                         else
                                             UI_Manager.Instance.checktime_check1 += int.Parse(datas[i]);
                                     }
                                     UI_Manager.Instance.checktime_total = UI_Manager.Instance.checktime_check1 + res.data.gap * res.data.num;

                                 }
                             });
                            BLEManager.Instance.ScanBLEDevice();
                            BLEManager.Instance.@continue = true;
                        },
                        oncancel = () =>
                        {
                            Debug.Log("取消检测");
                            recordJsons.Records.Remove(item);
                            string newjson = JsonMapper.ToJson(recordJsons);
                            Debug.Log(newjson + "新json");
                            //File.WriteAllText(path, newjson);
                            if (File.Exists(path)) File.Delete(path);
                            using (FileStream fs = new FileStream(path, FileMode.CreateNew))
                            {
                                byte[] buffer = Encoding.Default.GetBytes(newjson);
                                fs.Write(buffer, 0, buffer.Length);
                                fs.Flush();
                            }
                            //GetRecord();
                        }
                    });
                }
                else
                {
                    UI_Manager.Instance.OpenPanel<UI_PanelMessageBox>(UILevel.pop, new MessageBoxData()
                    {
                        content = "上次检测未完成，已失效，请重新扫码！",
                        oncancel = () =>
                        {
                            Debug.Log("取消检测");
                            recordJsons.Records.Remove(item);
                            string newjson = JsonMapper.ToJson(recordJsons);
                            Debug.Log(newjson + "新json");
                            if (File.Exists(path)) File.Delete(path);
                            using (FileStream fs = new FileStream(path, FileMode.CreateNew))
                            {
                                byte[] buffer = Encoding.Default.GetBytes(newjson);
                                fs.Write(buffer, 0, buffer.Length);
                                fs.Flush();
                            }
                            //GetRecord();
                        }
                    });
                }

            }
        }
    }
    /// <summary>
    /// 取消检测未完成项目
    /// </summary>
    public void CancelCheckUnDonePrograme()
    {
        Debug.Log("取消检测");
        BLEManager.Instance.@continue = false;
        recordJsons.Records.Remove(recordJsons.Records.Last());
        string newjson = JsonMapper.ToJson(recordJsons);
        Debug.Log(newjson + "新json");
        if (File.Exists(path)) File.Delete(path);
        using (FileStream fs = new FileStream(path, FileMode.CreateNew))
        {
            byte[] buffer = Encoding.Default.GetBytes(newjson);
            fs.Write(buffer, 0, buffer.Length);
            fs.Flush();
        }
    }

    /// <summary>
    /// 清除当前的检测记录
    /// </summary>
    public void OnStopCheck()
    {
        Debug.Log("删除当前检测的项目");
        if(File.Exists(path))
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(json)) return;
            Debug.Log(path);
            recordJsons = JsonMapper.ToObject<RecordJson>(json);
            if (recordJsons.Records == null) return;
            if (recordJsons.Records.Count == 0) return;
            recordJsons.Records.Remove(recordJsons.Records.Last());
            string newjson = JsonMapper.ToJson(recordJsons);
            Debug.Log(newjson + "新json");
            if (File.Exists(path)) File.Delete(path);
            using (FileStream fs = new FileStream(path, FileMode.CreateNew))
            {
                byte[] buffer = Encoding.Default.GetBytes(newjson);
                fs.Write(buffer, 0, buffer.Length);
                fs.Flush();
            }
        }
        OpenPanel<UI_Panel_Match>();
    }

    /// <summary>
    /// 检测完之后保存记录
    /// </summary>
    public ProgramRecord SaveRecord(string bleadress, double progress, bool done,bool savenew)
    {

        ProgramRecord programRecord = new ProgramRecord();
        programRecord.programeinfo = programData.remark;
        programRecord.programname = programData.name;
        programRecord.date = DateTime.Now.ToString();
        //Debug.Log(programRecord.date + "时间      ");
        programRecord.bleadress = bleadress;
        programRecord.done = done;
        programRecord.totaltime = this.checktime_total;
        programRecord.progress = progress;
        programRecord.qrcode = this.qrcode;
        programRecord.upload = this.UploadRecord;
        programRecord.personInfos = personInfos;
        programRecord.date = DateTime.Now.ToString();
        string j = JsonMapper.ToJson(programRecord);
        Debug.Log(j + "一条记录");
        if (savenew)
        {
            recordJsons.Records.Add(programRecord);
        }
        else
            recordJsons.Records[recordJsons.Records.Count - 1] = programRecord;
        
        string path = Path.Combine(Application.persistentDataPath, "record.json");

        Debug.Log(recordJsons.Records.Count);
        for (int i = 0; i < recordJsons.Records.Count; i++)
        {
            Debug.Log(recordJsons.Records[i].date);
        }
        string json = JsonMapper.ToJson(recordJsons);
        Debug.Log(json + "保存内容");
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        if(File.Exists(path))
        File.Delete(path);
        using (FileStream fs = new FileStream(path, FileMode.CreateNew))
        {
            fs.Write(buffer, 0, buffer.Length);
            fs.Flush();
        }
        if (!done) return programRecord;
        #region 上传云端
        if(UploadRecord)
        {
            List<Record> records = new List<Record>();
            for (int i = 0; i < personInfos.Length; i++)
            {
                if(!string.IsNullOrEmpty(personInfos[i].name))
                {
                    string c = "";
                    switch (i)
                    {
                        case 0:
                            c = "一";
                            break;
                        case 1:
                            c = "二";
                            break;
                        case 2:
                            c = "三";
                            break;
                        case 3:
                            c = "四";
                            break;
                        default:
                            break;
                    }
                    string result = "";
                    switch (personInfos[i].resulta)
                    {
                        case Result.invalid:
                            result = "无效";
                            break;
                        case Result.positive:
                            result = "阴性";
                            break;
                        case Result.negetive:
                            result = "阳性";
                            break;
                        case Result.nodisk:
                            result = "未放盘片";
                            break;
                        case Result.none:
                            break;
                        default:
                            break;
                    }
                    string ori = "";
                    //原来上传平均数  已弃用
                    //for (int k = 0; k < personInfos[i].avages.Length; k++)
                    //{
                    //    if(k == 0)
                    //        ori += personInfos[i].avages[k];
                    //    else
                    //        ori += "-" + personInfos[i].avages[k];
                    //}
                    //上传三个指标
                    string oria = "";
                    for (int k = 0; k < personInfos[i].dataas.Length; k++)
                    {
                        if(k == 0) oria += personInfos[i].dataas[k];
                        else oria += "-" + personInfos[i].dataas[k];
                    }
                    string orib = "";
                    for (int k = 0; k < personInfos[i].databs.Length; k++)
                    {
                        if (k == 0) orib += personInfos[i].databs[k];
                        else orib += "-" + personInfos[i].databs[k];
                    }
                    string oric = "";
                    for (int k = 0; k < personInfos[i].datacs.Length; k++)
                    {
                        if (k == 0) oric += personInfos[i].datacs[k];
                        else oric += "-" + personInfos[i].datacs[k];
                    }
                    ori = $"{oria}:{orib}:{oric}";
                    records.Add(new Record()
                    {
                        checkChannel = "通道" + c,
                        checkDate = "data",
                        checkResult = result,
                        name = personInfos[i].name,
                        originData = ori,
                        phone = personInfos[i].phone
                    });
                }
            }
            UploadData uploadData = new UploadData()
            {
                checkDate = programRecord.date,
                projName = programData.name,
                records = records
            };
            HttpHelper.UpLoadRecord(uploadData, (b, res) =>
             {
                 if(b)
                 {
                     OpenPanel<UI_PanelMessageBox>(UILevel.pop, new MessageBoxData()
                     {
                         title = "上传成功",
                         content = "检测数据已与云端同步"
                     });
                 }
             });
        }
        #endregion

        return programRecord;
    }

    public void DeleteRecord(ProgramRecord record)
    {
        recordJsons.Records.Remove(record);
        string json = JsonMapper.ToJson(recordJsons);
        Debug.Log(json + "保存内容");
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        if (File.Exists(path))
            File.Delete(path);
        using (FileStream fs = new FileStream(path, FileMode.CreateNew))
        {
            fs.Write(buffer, 0, buffer.Length);
            fs.Flush();
        }
    }

    public void PlayClickSound()
    {
        audio.PlayOneShot(audioClip);
    }

    /// <summary>
    /// 清空信息，用于重新扫码，终止测试等
    /// </summary>
    public void ResetState()
    {
        personInfos = new PersonInfo[4]
        {
            new PersonInfo(),
            new PersonInfo(),
            new PersonInfo(),
            new PersonInfo()
        };
        programData = new ProgramData();
        UploadRecord = false;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            //HttpHelper.GetNotice();
            SceneManager.LoadScene(0);
        }
    }
}
