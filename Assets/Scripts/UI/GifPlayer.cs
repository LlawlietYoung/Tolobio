using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using static UniGif;


public class GifPlayer : MonoBehaviour
{
    [HideInInspector]
    public RawImage rawImage;
    private List<GifTexture> images = new List<GifTexture>();
    private Coroutine load,play;
    private bool inited = false;
    private bool playing = false;

    private void OnDisable()
    {
        Stop();
    }

    public void SetGifResourse(string path,bool autoplay = true)
    {
        if (inited) return;
        rawImage = GetComponent<RawImage>();
        using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            byte[] bufffer = new byte[stream.Length];
            stream.Read(bufffer, 0, bufffer.Length);
            load = StartCoroutine(GetTextureListCoroutine(bufffer, (list,count,w,h)=>
            {
                inited = true;
                Debug.Log("º”‘ÿÕÍ≥…" + list.Count + count + w + h);
                images = list;
                if(autoplay)
                {
                    Play();
                }
            }));
        }
    }

    private IEnumerator SequencePlay(List<GifTexture> gifTextures)
    {
        Debug.Log(1111111111);
        while (playing)
        {
            for (int i = 0; i < gifTextures.Count; i++)
            {
                yield return new WaitForSeconds(0.05f);
                Debug.Log(111111111);
                rawImage.texture = gifTextures[i].m_texture2d;
            }
            yield return null;
        }
    }

    public void Play()
    {
        Debug.Log("GifPlay");
        if(!inited) return;
        if (playing) return;
        gameObject.SetActive(true);
        playing = true;
        play = StartCoroutine(SequencePlay(images));
    }
    public void Stop()
    {
        if (!playing) return;
        playing = false;
        StopCoroutine(play);
        play = null;
    }
}
