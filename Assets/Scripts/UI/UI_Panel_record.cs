using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Panel_record : PanelBase
{
    public RecordItem recordItem;
    public Transform container;
    public ScrollRect scrollRect;
    public Button btn_back;

    public override void OnInit(UIData data = null)
    {
        
        btn_back.onClick.AddListener(() =>
            {
                UI_Manager.Instance.OpenPanel<UI_Panel_Match>();
            });
    }

    public override void OnOpen(UIData data = null)
    {
        List<ProgramRecord> records = UI_Manager.Instance.recordJsons.Records;
        for (int i = records.Count - 1; i >= 0; i--)
        {
            RecordItem rd = Instantiate<RecordItem>(recordItem, container);
            rd.Init(records[i]);
        }
        scrollRect.verticalNormalizedPosition = 0;
    }
    public override void OnClose()
    {
        foreach (var item in container.GetComponentsInChildren<RecordItem>())
        {
            Destroy(item.gameObject);
        }
    }

    public override void OnLoop()
    {
        
    }
}
