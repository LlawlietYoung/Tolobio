using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;



public class VideoPlayerPanel : MonoBehaviour
{
    public Text tt_step, tt_stepcontent;
    public Button btn_back, btn_next,btn_done;

    public MediaPlayer vp;
    public VideoPlayer vp1;
    public Action onPlayEnd;
    public Action onNext;
    private void Start()
    {
        vp.Loop = true;
        btn_back.onClick.AddListener(StopPlay);
        btn_next.onClick.AddListener(() =>
        {
            vp.Stop();
            onNext?.Invoke();
        });
        btn_done.onClick.AddListener(() =>
        {
            Vp_loopPointReached();
        });
    }

    private void Vp_loopPointReached()
    {
        StopPlay();
        onPlayEnd?.Invoke();
    }

    public void PlayVideoByURL(string url,string[] stepinfo,bool next = true)
    {
        gameObject.SetActive(true);
        tt_step.text = stepinfo[0];
        tt_stepcontent.text = stepinfo[1];

        //if (next) btn_next.GetComponentInChildren<Text>().text = "下一步";
        //else btn_next.GetComponentInChildren<Text>().text = "已完成";
        btn_next.gameObject.SetActive(next);
        btn_done.gameObject.SetActive(!next);

        vp.OpenMedia(new MediaPath(HttpHelper.scope + url, MediaPathType.AbsolutePathOrURL), true);
        vp.Play();

        //vp1.url = HttpHelper.scope + url;
        //vp1.Play();
    }
    public void StopPlay()
    {
        //vp1.Stop();
        vp.Stop();
        gameObject.SetActive(false);
    }
}
