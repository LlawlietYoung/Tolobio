using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(VerticalLayoutGroup))]
public class VerticalLayoutAdapter : MonoBehaviour
{
    public RectTransform[] childs;
    private void Start()
    {
        foreach (var item in childs)
        {
            Vector2 size = item.sizeDelta;
            size.x = UI_Manager.Instance.GetComponent<RectTransform>().sizeDelta.x;
            item.sizeDelta = size;
        }
    }
}
