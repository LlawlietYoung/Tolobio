using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UI_Panel_ProgramInfo : PanelBase
{
    #region 项目信息
    public class ProgramInfo:UIData
    {
        public string name;
        public string introduce;
    }

    public Text tt_programname, tt_programintroduce;

    public void SetProgramInfo(ProgramInfo info)
    {
        tt_programname.text = info.name;
        tt_programintroduce.text = info.introduce;
    }

    #endregion

    #region UI Fields
    public GameObject con_tags;
    public Toggle[] tg_tages;
    public InputField if_name, if_phone;
    public Toggle tg_sync;
    public Button btn_back,btn_backtop, btn_next,btn_channel,btn_superlink;
    public Rectangle_channel rectangle_Channel;
    public Selfish selfish;//隐私协议
    #endregion


    private int channelcount;
    public int ChannelCount
    {
        get => channelcount;
        set
        {
            channelcount = value;
            switch (value)
            {
                case 1:
                    tg_tages[0].interactable = true;
                    tg_tages[1].interactable = false;
                    tg_tages[2].interactable = false;
                    tg_tages[3].interactable = false;
                    break;
                case 2:
                    tg_tages[0].interactable = true;
                    tg_tages[1].interactable = false;
                    tg_tages[2].interactable = true;
                    tg_tages[3].interactable = false;
                    break;
                case 3:
                    tg_tages[0].interactable = true;
                    tg_tages[1].interactable = true;
                    tg_tages[2].interactable = true;
                    tg_tages[3].interactable = false;
                    break;
                case 4:
                    tg_tages[0].interactable = true;
                    tg_tages[1].interactable = true;
                    tg_tages[2].interactable = true;
                    tg_tages[3].interactable = true;
                    break;
                default:
                    break;
            }
        }
    }

    private int selectchannelcount;
    public int SelectedChannelCount
    {
        get => selectchannelcount;
        set
        {
            selectchannelcount = value;
            
        }
    }

    //当前正在编辑的通道号
    private int currentchaanel = 0;

    #region 个人信息逻辑
    

    /// <summary>
    /// 显示当前通道个人信息
    /// </summary>
    /// <param name="info"></param>
    private void ShowPersonInfo(PersonInfo info)
    {
        if_name.text = info.name;
        if_phone.text = info.phone;
    }
    #endregion

    public override void OnInit(UIData data = null)
    {
        btn_channel.onClick.AddListener(() =>
        {
            rectangle_Channel.Open();
        });

        rectangle_Channel.onSelect += (c) =>
        {
            ChannelCount = c;
            string t = "";
            switch (c)
            {
                case 1:
                    t = "一";
                    break;
                case 2:
                    t = "二";
                    break;
                case 3:
                    t = "三";
                    break;
                case 4:
                    t = "四";
                    break;
                default:
                    break;
            }
            tg_tages[0].isOn = true;
            btn_channel.GetComponentInChildren<Text>().text = t + "人份";
        };
        ChannelCount = 1;
        if_name.onEndEdit.AddListener(t =>
        {
            UI_Manager.Instance.personInfos[currentchaanel].name = t;
        });
        if_phone.onEndEdit.AddListener(t =>
        {
            if(t.Length != 11 || !t.StartsWith("1"))
            {
                if_phone.text = string.Empty;
                if_phone.placeholder.GetComponent<Text>().text = "<color=red>请输入正确的手机号码</color>";
            }
            UI_Manager.Instance.personInfos[currentchaanel].phone = t;
        });

        tg_sync.onValueChanged.AddListener(b =>
        {
            //TODO: 是否同步
            btn_superlink.interactable = true;
            btn_superlink.GetComponent<Text>().color = Color.green;
            UI_Manager.Instance.UploadRecord = true;
        });

        btn_back.onClick.AddListener(() =>
        {
            UI_Manager.Instance.OpenPanel<UI_Panel_Match>();
        });
        btn_backtop.onClick.AddListener(() =>
        {
            UI_Manager.Instance.OpenPanel<UI_Panel_Match>();
        });

        btn_next.onClick.AddListener(() =>
        {
            //TODO: 下一步，上传信息
            //判断几通道，是否每个可用通道都添加了信息
            byte[] pos = new byte[4] { 0, 0, 0, 0};

            bool allempty = true;
            for (int i = 0; i < 4; i++)
            {
                //全是空的
                if(!string.IsNullOrEmpty(UI_Manager.Instance.personInfos[i].name)||!string.IsNullOrEmpty(UI_Manager.Instance.personInfos[i].phone))
                {
                    allempty = false;
                }
                //不完整
                if((string.IsNullOrEmpty(UI_Manager.Instance.personInfos[i].name)&&!string.IsNullOrEmpty(UI_Manager.Instance.personInfos[i].phone))||(!string.IsNullOrEmpty(UI_Manager.Instance.personInfos[i].name) && string.IsNullOrEmpty(UI_Manager.Instance.personInfos[i].phone)))
                {
                    UI_Manager.Instance.OpenPanel<UI_PanelMessageBox>(UILevel.pop, new MessageBoxData()
                    {
                        content = "信息填写不完整！",
                        fadetime = 3
                    });
                    return;
                }
            }
            if(allempty)
            {
                UI_Manager.Instance.OpenPanel<UI_PanelMessageBox>(UILevel.pop, new MessageBoxData()
                {
                    content = "未填写信息！",fadetime = 3

                });
                return;
            }
            for (int i = 0; i < 4; i++)
            {
                if(!string.IsNullOrEmpty(UI_Manager.Instance.personInfos[i].name)&&!string.IsNullOrEmpty(UI_Manager.Instance.personInfos[i].phone))
                {
                    pos[i] = 1;
                    SelectedChannelCount++;
                }
            }
            Debug.Log(BitConverter.ToString(pos));
            UI_Manager.Instance.OpenPanel<UI_PanelMessageBox>(UILevel.pop, new MessageBoxData()
            {
                content = "是否已确认人员对应的通道位置！",
                onconfirm = () =>
                {
                    //设置参数，给设备发送Setting_Parmeters
                    string temp = UI_Manager.Instance.programData.lxParams.Replace("[", "").Replace("]", "").Replace("(", "").Replace(")", "");
                    string[] datas = temp.Split(',');
                    //前一段的离心参数
                    byte[] param = new byte[60];

                    for (int i = 0; i < 60; i++)
                    {
                        if (i < datas.Length)
                            param[i] = (byte)int.Parse(datas[i]);
                        else
                            param[i] = 0x00;
                    }
                    short wd = (short)(float.Parse(UI_Manager.Instance.programData.speed) * 100);//温度乘以100
                    byte[] wdbyte = BitConverter.GetBytes(wd);
                    BitConverter.GetBytes(wd);
                    byte[] times = new byte[] { (byte)UI_Manager.Instance.programData.gap, (byte)UI_Manager.Instance.programData.num, wdbyte[1], wdbyte[0], (byte)int.Parse(UI_Manager.Instance.programData.standard1), (byte)((int)(float.Parse(UI_Manager.Instance.programData.threshold1) * 10)) };
                    byte[] result = param.Combine(pos).Combine(times);
                    BLEManager.Instance.SendMessageToMCU(CMDType.SET_PARAMETERS, (byte)UI_Manager.Instance.programData.flag, result);
                    UI_Manager.Instance.OpenPanel<UI_Panel_OperatingTips>(UILevel.common, new UI_Panel_OperatingTips_data()
                    {
                        channelcount = this.SelectedChannelCount
                    });
                }
            });

            
        });
        btn_superlink.onClick.AddListener(() =>
        {
            HttpHelper.GetNotice((title, content) =>
            {
                selfish.Init(title,content);
                selfish.Open();
            });

        });
    }

    public override void OnOpen(UIData data = null)
    {
        ProgramInfo programInfo = new ProgramInfo()
        {
            name = UI_Manager.Instance.programData.name,
            introduce =  UI_Manager.Instance.programData.remark
        };

        

        SetProgramInfo(programInfo);
        //默认单通道
        SelectedChannelCount = 0;
        UI_Manager.Instance.personInfos = new PersonInfo[]
        {
            new PersonInfo(),
            new PersonInfo(),
            new PersonInfo(),
            new PersonInfo()
        };
        //切换通道数
        for (int i = 0; i < UI_Manager.Instance.personInfos.Length; i++)
        {
            //UI_Manager.Instance.personInfos[i] = new PersonInfo();
            //PersonInfo temp = UI_Manager.Instance.personInfos[i];
            int a = i;
            tg_tages[i].onValueChanged.RemoveAllListeners();
            tg_tages[i].onValueChanged.AddListener(b =>
            {
                if (b)
                {
                    if_phone.placeholder.GetComponent<Text>().text = "<color=#32323277>输入手机号码</color>";
                    ShowPersonInfo(UI_Manager.Instance.personInfos[a]);
                    currentchaanel = a;
                }
            });
        }
        tg_sync.onValueChanged.RemoveAllListeners();
        tg_sync.onValueChanged.AddListener(b =>
        {
            UI_Manager.Instance.UploadRecord = b;
        });
    }

    public override void OnClose()
    {
        SetProgramInfo(new ProgramInfo());
        if_name.text = "";
        if_phone.text = "";

    }

    public override void OnLoop()
    {
        
    }
}
