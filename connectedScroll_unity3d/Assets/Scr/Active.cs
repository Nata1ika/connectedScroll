using System.Collections;
using UnityEngine;

public class Active : MonoBehaviour
{
    [SerializeField] GameObject _activeImmediatly;
    [SerializeField] TargetList _activeWait;
    
    //[SerializeField] StreamVideo _videoPrefab;  
    //private StreamVideo _video;    


    private void Start()
    {
        Target.DoubleClickEvent += Show;
    }

    private void OnDestroy()
    {
        Target.DoubleClickEvent -= Show;
    }

    private void Show(Target target, bool needWait)
    {
        _activeImmediatly.SetActive(true);
        //_video = Instantiate(_videoPrefab);
        //_video.Show(target, needWait);
        StartCoroutine(ShowCoroutine(target, needWait));
    }

    private IEnumerator ShowCoroutine(Target target, bool needWait)
    {
        if (needWait)
        {
            yield return new WaitForSeconds(CameraController.TIME_MOTION);
        }
        _activeWait.gameObject.SetActive(true);
        _activeWait.Show(target);
    }

    public void Hide()
    {        
        StopAllCoroutines();

        _activeImmediatly.SetActive(false);
        _activeWait.gameObject.SetActive(false);

        //if (_video != null)
        //{
        //    Destroy(_video.gameObject);
        //}
    }
}
