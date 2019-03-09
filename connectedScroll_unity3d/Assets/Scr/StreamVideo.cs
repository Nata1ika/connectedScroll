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

    private bool _isShow = false;

    private void Start()
    {
        Target.DoubleClickEvent += Show;
    }

    private void OnDestroy()
    {
        Target.DoubleClickEvent -= Show;
    }

    private void Show(Target obj)
    {
        _isShow = true;
        StartCoroutine(PlayVideo());
    }

    public void Hide()
    {
        _isShow = false;
        _video.Stop();
        _audio.Stop();

        _raw.gameObject.SetActive(false);
    }

    private IEnumerator PlayVideo()
    {
        _video.Prepare();
        yield return new WaitForSeconds(CameraController.TIME_MOTION);

        if (! _isShow)
        {
            yield break;
        }

        var wait = new WaitForSeconds(1f);
        while (!_video.isPrepared)
        {
            yield return wait;
        }

        if (!_isShow)
        {
            yield break;
        }

        _raw.gameObject.SetActive(true);
        _raw.texture = _video.texture;
        _video.Play();
        _audio.Play();
    }
}
