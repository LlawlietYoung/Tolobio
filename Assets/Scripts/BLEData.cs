using System;
using UnityEngine;
/// <summary>
/// 数据类型，发给BLE还是BLE发过来的
/// </summary>
public enum DataType
{
    ToBLE,
    ByBLE
}


public enum CMDType:byte
{
    NONE = 0xFF,
    HELLO = 0x01,
    SET_Temperature = 0x07,
    GET_Actual_Temperature = 0X09,
    MT_RESET = 0x11,
    MT_MOVE_TO = 0x14,
    SET_ROTATION = 0x15,
    GET_SIG = 0x18,
    SET_PARAMETERS = 0x1B,
    START_EXPERIMENT = 0x1C,
    Stop_Experiment = 0x1D,
    GET_TEST_DATA = 0x1E,
    CONNECTION = 0x20,

    HELLO_g = 0x01 | 0x80,
    SET_Temperature_g = 0x07 | 0x80,
    GET_Actual_Temperature_g = 0X09 | 0x80,
    MT_RESET_g = 0x11 | 0x80,
    MT_MOVE_TO_g = 0x14 | 0x80,
    SET_ROTATION_g = 0x15 | 0x80,
    GET_SIG_g = 0x18 | 0x80,
    SET_PARAMETERS_g = 0x1B | 0x80,
    START_EXPERIMENT_g = 0x1C | 0x80,
    Stop_Experiment_g = 0x1D | 0x80,
    GET_TEST_DATA_g = 0x1E | 0x80
}


public class Data
{
    public byte head = 0XAA;
    public CMDType cmd = CMDType.NONE;
    public byte ID;
    public byte LTH_H = 0x00;
    public byte LTH_L = 0x09;
    public byte STATUS;
    public byte RCC;
    public byte[] DATA;

    public short length => BitConverter.ToInt16(new byte[] { LTH_H, LTH_L }, 0);

    /// <summary>
    /// 用于发送给BLE的消息体
    /// </summary>
    /// <param name="cmdType"></param>
    /// <param name="data"></param>
    /// <param name="id"></param>
    public Data(CMDType cmdType,byte id,byte[] data)
    {
        ID = id;
        //LTH_L = (byte)(data.Length + 6);
        short length = (short)(data.Length + 6);
        byte[] len = BitConverter.GetBytes(length);
        LTH_H = len[1];
        LTH_L = len[0];
        cmd = cmdType;
        DATA = data;
        RCC = new byte[]
            {
                head,(byte)cmd,id,LTH_H,LTH_L
            }.Combine(data).GetByteSumLow8();
    }

    /// <summary>
    /// 用于接收BLE的消息体
    /// </summary>
    /// <param name="data"></param>
    public Data(byte[] data)
    {
        cmd = (CMDType)data[1];
        ID = data[2];
        LTH_H = data[3];
        LTH_L = data[4];
        DATA = data.GetArea(5, data.Length - 6);
        STATUS = data[data.Length - 2];
        RCC = data[data.Length - 1];
    }

    /// <summary>
    /// 转换成数据
    /// </summary>
    /// <returns></returns>
    public byte[] ToData()
    {
        byte[] result = new byte[]
        {
            head,(byte)cmd,ID,LTH_H,LTH_L
        }.Combine(DATA).Combine(new byte[]{RCC});
        Debug.Log(RCC + "RCC");

        return result;
    }
}
public class DataR 
{
    public byte head = 0XAA;
    public CMDType cmd = CMDType.NONE;
    public byte ID;
    public byte LTH_H = 0x00;
    public byte LTH_L = 0x09;
    public byte STATUS;
    public byte RCC;
    public byte[] DATA;
    
}
public static class Utils
{
    /// <summary>
    /// 数组各元素的和低8位
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte GetByteSumLow8(this byte[] data)
    {
        int sum = 0;
        for (int i = 0; i < data.Length; i++)
        {
            sum += (int)data[i];
        }
        return (byte)(sum % 256);
    }
    public static byte GetByteSumLow8IgnoreLast(this byte[] data)
    {
        int sum = 0;
        for (int i = 0; i < data.Length - 1; i++)
        {
            sum += (int)data[i];
        }
        return (byte)(sum % 256);
    }
    /// <summary>
    /// 把两个数组组合起来
    /// </summary>
    /// <param name="data1"></param>
    /// <param name="data2"></param>
    /// <returns></returns>
    public static byte[] Combine(this byte[] data1,byte[] data2)
    {
        byte[] result = new byte[data1.Length + data2.Length];

        for (int i = 0; i < data1.Length; i++)
        {
            result[i] = data1[i];
        }
        for (int i = data1.Length; i < result.Length; i++)
        {
            result[i] = data2[i - data1.Length];
        }
        return result;
    }

    /// <summary>
    /// 截取数组的一段，包含start和end位
    /// </summary>
    /// <param name="data"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static byte[] GetArea(this byte[] data, int start,int length)
    {
        if (start < 0 || length >= data.Length) return null;
        byte[] result = new byte[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = data[i + start];
        }
        return result;
    }
}

