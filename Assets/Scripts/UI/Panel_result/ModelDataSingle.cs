using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelDataSingle : MonoBehaviour
{
    public Color[] colors;
    private float height;
    public Transform lineh, linel;
    public Transform[] nums;
    public LineRenderer line;
    [HideInInspector]
    private double[] avages;
    [HideInInspector]
    public ProgramRecord _programRecord;
    public void Init(ProgramRecord programRecord)
    {
        _programRecord = programRecord;
        height = lineh.position.y - linel.position.y;
        for (int i = 0; i < programRecord.personInfos.Length; i++)
        {
            if(!string.IsNullOrEmpty(programRecord.personInfos[i].name))
            {
                for (int j = 0; j < programRecord.personInfos[i].avages.Length; j++)
                {
                    nums[j].gameObject.SetActive(true);
                }
                return;
            }
        }
        //line.positionCount = UI_Manager.Instance.programData.num;
    }
    public void Test()
    {
        avages = new double[]
    {
        1121,5152,1152,11333,31333
    };
        height = lineh.position.y - linel.position.y;
        for (int i = 0; i < avages.Length; i++)
        {
            nums[i].gameObject.SetActive(true);
        }
        //line.positionCount = UI_Manager.Instance.programData.num;
        //ChangePerson(0);
        line.positionCount = 0;
        line.startColor = colors[0];
        line.endColor = colors[0];
        for (int i = 0; i < avages.Length; i++)
        {
            SetData(i, avages[i]);
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            //Test();
        }
    }

    public void ChangePerson(int id)
    {
        line.positionCount = 0;
        line.startColor = colors[id];
        line.endColor = colors[id];
        for (int i = 0; i < _programRecord.personInfos[id].avages.Length; i++)
        {
            SetData(i, _programRecord.personInfos[id].avages[i]);
        }
    }


    public void SetData(int num , double avage)
    {
        Debug.Log("平均数" + avage + "   " + num);
        if (avage < 10) return;
        float x = nums[num].position.x;
        float y = (float)avage / 40000f * height + linel.position.y;
        Vector3 pos = new Vector3(x, y, 0);
        line.positionCount++;
        line.SetPosition(num, pos);
    }

    public void OnDisable()
    {
        line.positionCount = 0;
        for (int i = 0; i < UI_Manager.Instance.programData.num; i++)
        {
            nums[i].gameObject.SetActive(false);
        }
    }
}
