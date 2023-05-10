using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UI_Panel_OperatingTips_data :UIData
{
    public int channelcount = 1;

}
public class UI_Panel_OperatingTips : PanelBase
{
    public Text tt_channel, tt_step2;
    public Button btn_step1, btn_step2, btn_step3;
    public VideoPlayerPanel vpp;
    public Button btn_back, btn_devicecheck, btn_startcheck;

    [HideInInspector]
    public string[] urls_1channel;
    public string[] urls_2channel;
    public string[] urls_3channel;
    public string[] urls_4channel;
    public int currentstep = 0;
    private string[][] stepinfos = new string[][] {
        new string[] {"第一步","试剂加样并盖装" },
        new string[] {"第二步","将封装好的芯片放入对应检测槽内并配平" },
        new string[] {"第三步","自检芯片已安装到位盖好仪器盖板" },
    };
    public override void OnOpen(UIData data = null)
    {
        btn_startcheck.interactable = false;
        currentstep = 0;

    }
    public override void OnInit(UIData data = null)
    {
        UI_Panel_OperatingTips_data _data = (UI_Panel_OperatingTips_data)data?? new UI_Panel_OperatingTips_data();
        urls_1channel = new string[]
        {
            UI_Manager.Instance.programData.videoPath1,
            UI_Manager.Instance.programData.videoPath2,
            UI_Manager.Instance.programData.videoPath3
        };
        urls_2channel = new string[]
        {
            UI_Manager.Instance.programData.videoPath4,
            UI_Manager.Instance.programData.videoPath5,
            UI_Manager.Instance.programData.videoPath6
        };
        urls_3channel = new string[]
        {
            UI_Manager.Instance.programData.videoPath7,
            UI_Manager.Instance.programData.videoPath8,
            UI_Manager.Instance.programData.videoPath9
        };
        urls_4channel = new string[]
        {
            UI_Manager.Instance.programData.videoPath10,
            UI_Manager.Instance.programData.videoPath11,
            UI_Manager.Instance.programData.videoPath12
        };
        vpp.onNext += () =>
        {
            PlayVideo(++currentstep, _data.channelcount);
            if(currentstep == 2) btn_startcheck.interactable = true;
        };
        //播放第一步视频
        btn_step1.onClick.AddListener(() =>
        {
            PlayVideo(0, _data.channelcount);
        });
        //播放第二部视频
        btn_step2.onClick.AddListener(() =>
        {
            PlayVideo(1, _data.channelcount);
        });
        //播放第三步视频
        btn_step3.onClick.AddListener(() =>
        {
            PlayVideo(2, _data.channelcount);
        });

        //仪器检查
        //btn_devicecheck.onClick.AddListener(() =>
        //{
        //    //蓝牙通知仪器开始检查仪器
        //    //BLEManager.Instance.StartCheckDevice();
        //    btn_startcheck.interactable = true;
        //});

        btn_startcheck.onClick.AddListener(() =>
        {
            BLEManager.Instance.SendMessageToMCU(CMDType.START_EXPERIMENT);
        });
        btn_back.onClick.AddListener(() =>
        {
            UI_Manager.Instance.OpenPanel<UI_Panel_ProgramInfo>();
        });
    }

    private void PlayVideo(int step,int channelcount)
    {
        currentstep = step;
        string url = "";
        switch (channelcount)
        {
            case 1:
                url = urls_1channel[currentstep];
                break;
            case 2:
                url = urls_2channel[currentstep];
                break;
            case 3:
                url = urls_3channel[currentstep];
                break;
            case 4:
                url = urls_4channel[currentstep];
                break;
            default:
                break;
        }
        vpp.PlayVideoByURL(url,stepinfos[currentstep],step == 2?false:true);
        vpp.onPlayEnd += () =>
        {
            //仪器检查按钮可用
            btn_devicecheck.interactable = true;
            vpp.onPlayEnd = null;
        };
    }

    public override void OnLoop()
    {
        
    }

    public override void OnClose()
    {
        
    }
}
