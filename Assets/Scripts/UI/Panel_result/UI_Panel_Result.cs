using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class ResultData : UIData
{
    public ProgramRecord programRecord;
}

public class UI_Panel_Result : PanelBase
{
    #region 上部的结果栏
    public Sprite sp_positive, sp_negetive, sp_nodisk, sp_no;
    public Image ima_result;
    public Text tt_name, tt_result, tt_remark;
    #endregion

    #region 项目信息
    public Text tt_programname, tt_info;
    #endregion

    #region 信息
    public Text tt_name_1, tt_phone,tt_indexa, tt_marka,tt_indexb, tt_markb,tt_indexc, tt_markc, tt_result_1;
    public Image ima_warninga, ima_warningb, ima_warningc;
    public Sprite sp_warning_red, sp_warning_gray;
    public GameObject toggleGroup;
    public Toggle[] toggletags;
    public ModelDataSingle dataSingle;
    #endregion
    public Button btn_back,btn_confirm;

    public override void OnInit(UIData data = null)
    {
        //btn_back.onClick.AddListener(() =>
        //{
        //    //UI_Manager.Instance.OpenPanel<UI_Panel_Match>();
        //    SceneManager.LoadScene(0);
        //});
        btn_confirm.onClick.AddListener(() =>
        {
            UI_Manager.Instance.OpenPanel<UI_Panel_record>();
            UI_Manager.Instance.ResetState();
            //SceneManager.LoadScene(0);
        });
    }
    public void ReturntoRecord(Scene scene, LoadSceneMode loadSceneMode)
    {

    }
    public override void OnOpen(UIData data = null)
    {
        ResultData resultData = (ResultData)data ?? new ResultData();
        if (resultData.programRecord == null)
        {
            UI_Manager.Instance.OpenPanel<UI_PanelMessageBox>(UILevel.pop, new MessageBoxData()
            {
                title = "没有信息",
                fadetime = 2
            });
            return;
        }
        toggleGroup.SetActive(resultData.programRecord.personInfos.Length != 1);
        tt_programname.text = resultData.programRecord.programname;
        tt_remark.text = resultData.programRecord.programname;

        tt_info.text = resultData.programRecord.programeinfo;
        
        for (int i = 0; i < resultData.programRecord.personInfos.Length; i++)
        {
            Debug.Log(resultData.programRecord.personInfos[i].name);
            if (string.IsNullOrEmpty(resultData.programRecord.personInfos[i].name))
            {
                continue;
            }
            toggletags[i].gameObject.SetActive(true);
            int index = i;
            toggletags[i].onValueChanged.AddListener(b =>
            {
                if (b)
                {
                    SetResultInfo(resultData.programRecord.personInfos[index]);
                    dataSingle.ChangePerson(index);
                }
            });
        }
        //toggletags[0].isOn = true;
        dataSingle.Init(resultData.programRecord);
        tt_indexa.text = resultData.programRecord.marka;
        tt_indexb.text = resultData.programRecord.markb;
        tt_indexc.text = resultData.programRecord.markc;
        for (int i = 0; i < resultData.programRecord.personInfos.Length; i++)
        {
            if (!string.IsNullOrEmpty(resultData.programRecord.personInfos[i].name))
            {
                SetResultInfo(resultData.programRecord.personInfos[i]);
                //dataSingle.Init(resultData.programRecord);
                toggletags[i].isOn = true;
                return;
            }
        }
    }

    private void SetResultInfo(PersonInfo person)
    {
        //顶部面板已隐藏
        //switch (person.resulta)
        //{
        //    case Result.invalid:
        //        ima_result.sprite = sp_no;
        //        break;
        //    case Result.positive:
        //        ima_result.sprite = sp_positive;
        //        break;
        //    case Result.negetive:
        //        ima_result.sprite = sp_negetive;
        //        break;
        //    case Result.nodisk:
        //        ima_result.sprite = sp_nodisk;
        //        break;
        //    case Result.none:
        //        break;
        //    default:
        //        break;
        //}
        //ima_result.sprite = positive ? sp_positive : sp_negetive;
        tt_name.text = person.name;
        Debug.Log(person.resulta + " " + (int)person.resulta + "结果 " + person.name);
        tt_result.text = UI_Manager.Instance.resultContents[(int)person.resulta];
        tt_name_1.text = person.name;
        tt_phone.text = person.phone;
        //tt_result_1.text = positive ? "<color=green>阴性</color>" : "<color=red>阳性</color>";
        switch (person.resulta)
        {
            case Result.invalid:
                tt_marka.text = "无效";
                ima_warninga.gameObject.SetActive(true);
                ima_warninga.sprite = sp_warning_gray;
                break;
            case Result.positive:
                tt_marka.text = "<color=green>阴性</color>";
                break;
            case Result.negetive:
                tt_marka.text = "<color=red>阳性</color>";
                ima_warninga.gameObject.SetActive(true);
                ima_warninga.sprite = sp_warning_red;
                break;
            case Result.nodisk:
                tt_marka.text = "未放盘片";
                break;
            case Result.none:
                break;
            default:
                break;
        }
        switch (person.resultb)
        {
            case Result.invalid:
                tt_markb.text = "无效";
                ima_warningb.gameObject.SetActive(true);
                ima_warningb.sprite = sp_warning_gray;
                break;
            case Result.positive:
                tt_markb.text = "<color=green>阴性</color>";
                break;
            case Result.negetive:
                tt_markb.text = "<color=red>阳性</color>";
                ima_warningb.gameObject.SetActive(true);
                ima_warningb.sprite = sp_warning_red;
                break;
            case Result.nodisk:
                tt_markb.text = "未放盘片";
                break;
            case Result.none:
                break;
            default:
                break;
        }
        switch (person.resultc)
        {
            case Result.invalid:
                tt_markc.text = "无效";
                ima_warningc.gameObject.SetActive(true);
                ima_warningc.sprite = sp_warning_gray;
                break;
            case Result.positive:
                tt_markc.text = "<color=green>阴性</color>";
                break;
            case Result.negetive:
                tt_markc.text = "<color=red>阳性</color>";
                ima_warningc.gameObject.SetActive(true);
                ima_warningc.sprite = sp_warning_red;
                break;
            case Result.nodisk:
                tt_markc.text = "未放盘片";
                break;
            case Result.none:
                break;
            default:
                break;
        }
    }

    public override void OnClose()
    {
        ima_result.sprite = null;
        tt_name.text = "";
        tt_result.text = "";
        tt_name_1.text = "";
        tt_phone.text = "";
        tt_marka.text = "";
        tt_markb.text = "";
        tt_markc.text = "";
        tt_result_1.text = "";
        tt_programname.text = "";
        tt_info.text = "";
        dataSingle._programRecord = null;
        ima_warninga.gameObject.SetActive(false);
        ima_warningb.gameObject.SetActive(false);
        ima_warningc.gameObject.SetActive(false);
        foreach (var item in toggletags)
        {
            item.gameObject.SetActive(false);
        }
    }

    public override void OnLoop()
    {
        
    }
}
