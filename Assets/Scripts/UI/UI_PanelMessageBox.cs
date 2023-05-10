using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class MessageBoxData:UIData
{
    public string title;
    public string content;
    public float fadetime = 0;
    public Action onconfirm;
    public int bgindex = 0;
    public Action oncancel;
}

public class UI_PanelMessageBox : PanelBase
{
    public CanvasGroup canvasGroup;
    public Text tt_title,tt_content;
    public Button btn_confirm,btn_cancel;
    public Sprite ima_nor, ima_stop;
    public Image bg;
    public override void OnInit(UIData data = null)
    {
        
    }

    public override void OnOpen(UIData data = null)
    {
        MessageBoxData message = (MessageBoxData)data ?? new MessageBoxData();
        canvasGroup.DOFade(1, 0.2f);
        tt_title.text = message.title;

        tt_content.text = message.content;

        if(message.bgindex == 0)
        {
            bg.sprite = ima_nor;
        }
        else
        {
            bg.sprite = ima_stop;
        }

        if(message.fadetime != 0)
        {
            StartCoroutine(Wait(message.fadetime));
        }
        if(message.onconfirm != null)
        {
            btn_confirm.gameObject.SetActive(true);
        }
        btn_confirm.onClick.AddListener(() =>
        {
            message.onconfirm?.Invoke();
            StartCoroutine(Wait(0));
        });
        btn_cancel.onClick.AddListener(() =>
        {
            Debug.Log("取消");
            message.oncancel?.Invoke();
            StartCoroutine(Wait(0));
        });
    }

    private IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time);
        canvasGroup.DOFade(0, 0.2f);
        yield return new WaitForSeconds(0.2f);
        Close();
    }
    public override void OnClose()
    {
        btn_confirm.onClick.RemoveAllListeners();
        btn_cancel.onClick.RemoveAllListeners();
        tt_title.text = "";
        tt_content.text = "";
        btn_confirm.gameObject.SetActive(false);
    }

    public override void OnLoop()
    {
        
    }
}
