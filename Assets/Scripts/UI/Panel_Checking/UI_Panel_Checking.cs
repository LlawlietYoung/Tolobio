using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UI_Panel_Checking : PanelBase
{
    public Text tt_name, tt_info;
    public Button btn_back,btn_stop,btn_next;
    public Progress progress;
    private Coroutine cor;
    public ModelData model;
    //public GifPlayer loading;

    public override void OnClose()
    {
        //loading.Stop();
        if(cor != null)
        {
            StopCoroutine(cor);
            cor = null;
        }
    }

    public override void OnInit(UIData data = null)
    {

        //btn_next.interactable = false;
        btn_back.onClick.AddListener(() =>
        {
            UI_Manager.Instance.OpenPanel<UI_PanelMessageBox>(UILevel.pop, new MessageBoxData()
            {
                title = "终止检测",
                content = "即将终止检测，点击空白处可取消终止操作。",
                onconfirm = () =>
                {
                    BLEManager.Instance.SendMessageToMCU(CMDType.Stop_Experiment);
                    //UI_Manager.Instance.OpenPanel<UI_Panel_Match>();
                },
                bgindex = 1
            });
        });
        btn_stop.onClick.AddListener(() =>
        {
            UI_Manager.Instance.OpenPanel<UI_PanelMessageBox>(UILevel.pop, new MessageBoxData()
            {
                title = "终止检测",
                content = "即将终止检测，点击空白处可取消终止操作。",
                onconfirm = () =>
                {
                    BLEManager.Instance.SendMessageToMCU(CMDType.Stop_Experiment);
                    //UI_Manager.Instance.OpenPanel<UI_Panel_Match>();
                },
                bgindex = 1
            });
        });
        btn_next.onClick.AddListener(() =>
        {
            UI_Manager.Instance.OpenPanel<UI_Panel_Result>(UILevel.common, new ResultData()
            {
                //保存结果
                programRecord = UI_Manager.Instance.SaveRecord(BLEManager.Instance.bLESevice.deviceAddress, 1, true,false)
            });
        });
        //收到检测完成
        UI_Manager.Instance.OnCheckDone += (result) =>
        {
            btn_stop.interactable = false;
            btn_next.interactable = true;
            for (int i = 0; i < UI_Manager.Instance.personInfos.Length; i++)
            {
                if (!string.IsNullOrEmpty(UI_Manager.Instance.personInfos[i].name))
                {
                    //Debug.Log("得到了结果" + (Result)(result[i * 3 + 0]));
                    UI_Manager.Instance.personInfos[i].resulta = (Result)(result[i * 3 + 0]);
                    UI_Manager.Instance.personInfos[i].resultb = (Result)(result[i * 3 + 1]);
                    UI_Manager.Instance.personInfos[i].resultc = (Result)(result[i * 3 + 2]);
                }
            }
            //UI_Manager.Instance.SaveRecord(BLEManager.Instance.bLESevice.deviceAddress,1, true);
            if(cor != null)
            {
                StopCoroutine(cor);
                progress.ProgressValue = 1;
            }
        };
        model.Init();
    }

    public override void OnLoop()
    {
        
    }

    public override void OnOpen(UIData data = null)
    {
        tt_name.text = UI_Manager.Instance.programData.name;
        tt_info.text = UI_Manager.Instance.programData.remark;
        //loading.SetGifResourse(Path.Combine(Application.streamingAssetsPath, "loding.gif"));
        //loading.Play();
        cor = StartCoroutine(CheckProgress());
    }
    private IEnumerator CheckProgress()
    {
        double timer = 0;
        double timer2 = 0;
        double timer3 = 0;
        if (BLEManager.Instance.@continue)
        {
            DateTime savedate = DateTime.Parse(UI_Manager.Instance.currentRecord.date);

            timer = UI_Manager.Instance.currentRecord.progress * UI_Manager.Instance.checktime_total + (DateTime.Now - savedate).TotalSeconds;
            UI_Manager.Instance.SaveRecord(BLEManager.Instance.bLESevice.deviceAddress, timer / UI_Manager.Instance.checktime_total, false, false);
        }
        else
            UI_Manager.Instance.SaveRecord(BLEManager.Instance.bLESevice.deviceAddress, timer / UI_Manager.Instance.checktime_total, false, true);
        Debug.Log(UI_Manager.Instance.checktime_check1 + "离心时间");
        Debug.Log(UI_Manager.Instance.checktime_total + "总时间");
        while (true)
        {
            yield return new WaitForSecondsRealtime(1);
            timer += 1;
            float pro = (float)(timer / (double)UI_Manager.Instance.checktime_total);
            progress.ProgressValue = pro;
            Debug.Log(pro);
            if(timer >= UI_Manager.Instance.checktime_check1 + 3)
            {
                //每隔30秒获取数据
                timer2 += 1;
                if(timer2 >= UI_Manager.Instance.programData.gap)
                {
                    timer2 = 0;
                    Debug.Log("请求监测数据！");
                    BLEManager.Instance.SendMessageToMCU(CMDType.GET_TEST_DATA);
                    Debug.Log("获取检测数据..................");
                }
            }
            if (timer >= UI_Manager.Instance.checktime_total)
            {
                //最后获取一次数据
                Debug.Log("请求监测数据！");
                BLEManager.Instance.SendMessageToMCU(CMDType.GET_TEST_DATA);
                Debug.Log("获取检测数据..................");
                UI_Manager.Instance.SaveRecord(BLEManager.Instance.bLESevice.deviceAddress, timer / UI_Manager.Instance.checktime_total, false,false);
                break;
            }
            timer3 += 1;
            if(timer3 >= 10)
            {
                timer3 = 0;
                UI_Manager.Instance.SaveRecord(BLEManager.Instance.bLESevice.deviceAddress, timer / UI_Manager.Instance.checktime_total, false,false);
            }
        }
        btn_next.interactable = true;
    }

    private void LoadLoadingGif()
    {

    }
}
