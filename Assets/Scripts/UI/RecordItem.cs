using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class RecordItem : MonoBehaviour
{
    public Text tt_programname, tt_time;

    public GameObject[] members;
    public Button btn_degital,btn_delete;

    public void Init(ProgramRecord record)
    {
        for (int i = 0; i < members.Length; i++)
        {
            if(!string.IsNullOrEmpty(record.personInfos[i].name))
            {
                members[i].SetActive(true);
                members[i].transform.Find("Text").GetComponent<Text>().text = record.personInfos[i].name;
                members[i].transform.Find("Text_result").GetComponent<Text>().text = UI_Manager.Instance.resultContents[(int)record.personInfos[i].result];
            }
            else
            {
                members[i].SetActive(false);
            }
        }
        tt_programname.text = record.programname;
        tt_time.text = record.date;
        btn_degital.onClick.AddListener(() =>
        {
            //打开检测结果页面
            UI_Manager.Instance.OpenPanel<UI_Panel_Result>(UILevel.common, new ResultData()
            {
                programRecord = record
            });
            UI_Manager.Instance.PlayClickSound();
        });
        btn_delete.onClick.AddListener(() =>
        {
            UI_Manager.Instance.DeleteRecord(record);
            Destroy(gameObject);
        });
    }
}
