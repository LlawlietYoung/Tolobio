using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BLEButton : MonoBehaviour
{
    private bool connected;

    public bool Connected { 
        get => connected;
        set
        {
            connected = value;
            text.text = value ? "蓝牙已连接" : "蓝牙未连接\n请点击";
            button.interactable = !value;
            grayMask.SetActive(value);
        }
    }

    public GameObject grayMask;
    public Text text;
    public Button button;

    //public void Init()
    //{
    //    button = GetComponent<Button>();
    //    text = GetComponent<Text>();
    //    grayMask = transform.Find("Background/checkmark").gameObject;
    //}
}
