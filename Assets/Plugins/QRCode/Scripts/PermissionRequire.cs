using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class PermissionRequire : MonoBehaviour
{
    public static void RequireCamrea()
    {
        try
        {
            if (!Permission.HasUserAuthorizedPermission("android.permission.CAMERA"))
            {
                Permission.RequestUserPermission("android.permission.CAMERA");
            }
            else
            {
            }
        }
        catch (Exception e)
        {

        }
    }
}
