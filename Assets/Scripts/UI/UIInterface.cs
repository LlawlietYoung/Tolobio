
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 所有panel基类，都要继承这个
/// </summary>
public abstract class PanelBase : MonoBehaviour
{
    internal bool isInited = false;

    public void Close()
    {
        gameObject.SetActive(false);
        OnClose();
    }

    public void Init(UIData data = null)
    {
        if (isInited)
        {
            Open(data);
            return;
        }
        isInited = true;
        OnInit(data);
        Open(data);
    }

    public void Open(UIData data = null)
    {
        gameObject.SetActive(true);
        OnOpen(data);
    }

    public abstract void OnInit(UIData data = null);
    public abstract void OnOpen(UIData data = null);
    public abstract void OnLoop();
    public abstract void OnClose();
    private void Update()
    {
        OnLoop();
    }
}

public enum UILevel
{
    common,
    pop
}
