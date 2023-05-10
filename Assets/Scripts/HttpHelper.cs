using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP;
using System;
using System.Text;
using LitJson;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using BestHTTP.Forms;

[Serializable]
public class Record
{
    public string checkChannel;
    public string checkDate;
    public string checkResult;
    public string name;
    public string originData;
    public string phone;
}
public class UploadData
{
    public string checkDate;
    public string projName;
    public List<Record> records;
}



public class UploadResponse
{
    public int code;
    public bool data;
    public string msg;
}


public class GetResponse
{
    public int code;
    public ProgramData data;
    public string msg;
}

public class GetNotciData
{
    public int code;
    public NoticeData data;
    public string msg;
}
public class ProgramData
{
    public string name;
    public string remark;
    public string lxParams;
    public string index1;
    public string index2;
    public string index3;
    public string index4;
    public string standard1;
    public string standard2;
    public string standard3;
    public string standard4;
    public string videoPath1 = "105.mp4";
    public string videoPath10 = "105.mp4";
    public string videoPath11 = "105.mp4";
    public string videoPath12 = "105.mp4";
    public string videoPath2 = "105.mp4";
    public string videoPath3 = "105.mp4";
    public string videoPath4 = "105.mp4";
    public string videoPath5 = "105.mp4";
    public string videoPath6 = "105.mp4";
    public string videoPath7 = "105.mp4";
    public string videoPath8 = "105.mp4";
    public string videoPath9 = "105.mp4";
    public string path;
    public int flag;
    public string threshold1;
    public string threshold2;
    public string threshold3;
    public string threshold4;
    public int gap;
    public int num;
    public string speed;
    public string id;
    public long createTime;
    //public string selfish;
}
public class NoticeData
{
    public Notice[] list;
    public string total;
}
public class Notice
{
    public string title;
    public int type;
    public string content;
    public int status;
    public string id;
    public long creatTime;
}


public class HttpHelper
{
    public static string scope = "http://124.222.137.187:48080";
    public static string scope1 = "https://124.222.137.187:48080";
    public static string UpLoadRecordUrl = $"{scope}/app-api/vv/record/update";
    public static string GetProgramUrl = $"{scope}/app-api/vv/proj/get";
    public static string GetNoticeUrl = $"{scope}/app-api/system/notice/page";
    /// <summary>
    /// 上床检测记录
    /// </summary>
    public static void UpLoadRecord(UploadData records,Action<bool, UploadResponse> onresponse)
    {
        HTTPRequest request = new HTTPRequest(new Uri(UpLoadRecordUrl), HTTPMethods.Post, (req, res) =>
          {
              if(res != null)
              {
                  Debug.Log(res.DataAsText);
                  UploadResponse response = JsonMapper.ToObject<UploadResponse>(res.DataAsText);
                  if(response.code == 200)
                  {
                      onresponse?.Invoke(response.data, response);
                  }
                  else
                  {
                      onresponse?.Invoke(false, response);
                  }
              }
          });
        request.SetHeader("Content-Type", "application/json");
        request.RawData = Encoding.UTF8.GetBytes(JsonMapper.ToJson(records));
        request.Send();
    }
    public static byte[] ObjectToBytes(object obj)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            return ms.GetBuffer();
        }
    }


    public static void GetProgramInfo(string id, Action<bool, GetResponse> onresponse)
    {
        HTTPRequest request = new HTTPRequest(new Uri(GetProgramUrl), HTTPMethods.Get, (req, res) =>
          {
              Debug.Log(res.DataAsText);
              GetResponse response = JsonMapper.ToObject<GetResponse>(res.DataAsText);
              if (response.code == 200)
              {
                  Debug.Log(response.data);
                  if(response.data != null)
                  //获取成功
                    onresponse?.Invoke(true, response);
                  else
                  {
                      onresponse?.Invoke(false, response);
                  }
              }
              else
              {
                  onresponse?.Invoke(false, response);
              }
          });
        HTTPMultiPartForm form = new HTTPMultiPartForm();
        form.AddField("id", id);
        request.SetForm(form);
        request.Send();

    }

    public static void GetNotice(Action<string,string> onAction)
    {
        HTTPRequest request = new HTTPRequest(new Uri(GetNoticeUrl), HTTPMethods.Get, (req, res) =>
          {
              Debug.Log(res.DataAsText);
              GetNotciData notciData = JsonMapper.ToObject<GetNotciData>(res.DataAsText);
              if(notciData.code == 200)
              {
                  if(notciData.data != null)
                  {
                      onAction?.Invoke(notciData.data.list[0].title, notciData.data.list[0].content);
                  }
              }
          });
        HTTPMultiPartForm form = new HTTPMultiPartForm();
        form.AddField("pageNo", "1");
        form.AddField("pageSize", "10");
        request.SetForm(form);
        request.Send();
    }
}
