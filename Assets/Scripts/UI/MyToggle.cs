using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MyToggle : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public void SetAlpha(bool b)
    {
        canvasGroup.DOFade(b ? 1 : 0, 0.25f);
    }
}
