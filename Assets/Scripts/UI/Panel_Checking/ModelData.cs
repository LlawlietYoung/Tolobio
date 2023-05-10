using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModelData : MonoBehaviour
{
    public Color[] colors;

    private float height;
    private int width;

    public Transform lineh, linel;
    public Transform[] nums;
    public Transform[] dotparents;

    public LineRenderer line;
    
    private LineRenderer[] lines = new LineRenderer[4];
    private void Start()
    {
        //Init();
    }
    public void Init()
    {
        height = lineh.position.y - linel.position.y;
        for (int i = 0; i < UI_Manager.Instance.programData.num; i++)
        {
            nums[i].gameObject.SetActive(true);
        }
        //生成四条线代表四个人的折线图
        for (int i = 0; i < 4; i++)
        {
            lines[i] = Instantiate(line, dotparents[i]);
            lines[i].startColor = colors[i];
            lines[i].endColor = colors[i];
        }
        BLEManager.Instance.onGetData += data =>
        {
            Debug.Log("调用");
            for (int i = 0; i < lines.Length; i++)
            {
                for (int j = 0; j < lines[i].positionCount; j++)
                {
                    lines[i].SetPosition(j, Vector3.zero);
                }
                lines[i].positionCount = 0;
            }
            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    SetData(j,i,data[i][j]);
                }
            }
        };
    }

    public void SetData(int id,int num,float avage)  //就是荧光信号应该只会40000左右，所以是这里改一下吗，图表显示看着太低也不舒服，他是一个3000-27000的过程，一般其实到27000就很高了!
    {
        if (avage < 10) return;
        float x = nums[num].position.x;
        float y = avage / 35000f * height + linel.position.y;
        Vector3 pos = new Vector3(x,y,0);
        Debug.Log(pos + "   " + num + "  " + avage + "绘制线");
        lines[id].positionCount++;
        //if (avage > 45000)
        //{
        //    lines[id].SetPosition(num, lines[id].GetPosition(num - 1));
        //}

        lines[id].SetPosition(num,pos);
    }

    public void OnDisable()
    {
        for (int i = 0; i < nums.Length; i++)
        {
            nums[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i].positionCount = 0;
        }
    }
}
