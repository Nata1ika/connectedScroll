using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StreamVideo : MonoBehaviour
{
    [SerializeField] RawImage _raw;
    [SerializeField] VideoPlayer _video;
    [SerializeField] AudioSource _audio;

    public void Show(Target target, bool needWait)
    {
        StartCoroutine(PlayVideo(needWait));
    }

    private IEnumerator PlayVideo(bool needWait)
    {
        _video.Prepare();

        if (needWait)
        {
            yield return new WaitForSeconds(CameraController.TIME_MOTION);
        }
        
        var wait = new WaitForSeconds(1f);
        while (!_video.isPrepared)
        {
            yield return wait;
        }        

        _raw.texture = _video.texture;
        _raw.color = Color.white;
        _video.Play();
        _audio.Play();
    }
}
