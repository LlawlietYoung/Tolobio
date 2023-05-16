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
            float temp = Mathf.Clamp(value, 0, 1);
            tt_progress.text = temp.ToString("p2");
            ima_progress.fillAmount = temp;
        }
    }
    public void ResetValue()
    {
        ProgressValue = 0;
    }
}
