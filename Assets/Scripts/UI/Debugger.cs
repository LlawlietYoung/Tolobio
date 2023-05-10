using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Debugger : Singleton<Debugger>
{
    public ScrollRect scrollRect;
    public Text content;

    public void Log(object info)
    {
        content.text += info.ToString() + "\n";
        scrollRect.verticalNormalizedPosition = 1;
    }
}
