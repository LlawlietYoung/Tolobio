using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class Rectangle_channel : MonoBehaviour
{
    public Button btn_cancel, btn_submit;
    public Toggle[] tgs;

    public int channelcount = 1;

    public Action<int> onSelect;



    private void Start()
    {
        channelcount = 1;
        for (int i = 0; i < tgs.Length; i++)
        {
            int a = i + 1;
            tgs[i].onValueChanged.AddListener(b =>
            {
                if(b)
                {
                    channelcount = a;
                }
            });
        }
        btn_submit.onClick.AddListener(() =>
        {
            onSelect?.Invoke(channelcount);
            Close();
        });
        btn_cancel.onClick.AddListener(() =>
        {
            Close();
        });
    }
    public void Open()
    {
        transform.DOLocalMoveY(-1080, 0.25f);
    }

    public void Close()
    {
        transform.DOLocalMoveY(-1000 - 1080, 0.2f);
    }
}
