using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Progress : MonoBehaviour
{
    public Text tt_progress;
    public Image ima_progress;
    public float ProgressValue
    {
        set
        {
            tt_progress.text = value.ToString("p2");
            ima_progress.fillAmount = value;
        }
    }
    public void ResetValue()
    {
        ProgressValue = 0;
    }
}
