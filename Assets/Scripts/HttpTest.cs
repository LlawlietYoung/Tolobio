using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HttpTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Record record = new Record()
        {
            checkChannel = "通道一",
            checkDate = DateTime.Now.ToString(),
            checkResult = "阴性",
            name = "张三",
            originData = "aaaaaaaaaa",
            phone = "13082303872"
        };
        //HttpHelper.UpLoadRecord(record);
        //HttpHelper.GetProgramInfo(1024);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
