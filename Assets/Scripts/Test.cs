using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Test : MonoBehaviour
{
    public ModelData modeldata;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1);
        modeldata.Init();
    }
}
