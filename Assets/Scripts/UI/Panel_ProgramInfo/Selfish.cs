using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Selfish : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Text tt_title, tt_content;
    public Button btn_close;
    public void Init(string title,string content)
    {
        tt_title.text = title;
        tt_content.text = "\n\n" + content;
        btn_close.onClick.AddListener(() =>
        {
            Close();
        });
    }
    public void Open()
    {
        gameObject.SetActive(true);
        canvasGroup.DOFade(1, 0.2f);
    }
    public void Close()
    {
        gameObject.SetActive(false);
        canvasGroup.DOFade(0, 0.2f);
    }
}
