using System;
using UnityEngine;
using UnityEngine.Android;

public class AndriodPermission 
{
    /// <summary>
    /// 请求蓝牙权限并打开 蓝牙
    /// </summary>
    public static bool RequireBluetoothPermission()
    {
        AndroidJavaClass bluetooth = new AndroidJavaClass("android.bluetooth.BluetoothAdapter");
        AndroidJavaObject bluetoothAdapter = bluetooth.CallStatic<AndroidJavaObject>("getDefaultAdapter");
        if (!bluetoothAdapter.Call<bool>("isEnabled"))
        {
            var isOpen = bluetoothAdapter.Call<bool>("enable");  //打开蓝牙，需要BLUETOOTH_ADMIN权限  
            return isOpen;
        }
        return true;
    }

    /// <summary>
    /// 请求定位权限，ble需要开启定位
    /// </summary>
    public static void RequireLocationPermission()
    {
        try
        {
            if (!Permission.HasUserAuthorizedPermission("android.permission.ACCESS_FINE_LOCATION"))
            {
                Permission.RequestUserPermission("android.permission.ACCESS_FINE_LOCATION");
            }
            else
            {
            }
        }
        catch (Exception e)
        {

        }
    }

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
