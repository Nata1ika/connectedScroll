using UnityEngine;
using UnityEngine.UI;

public class Active : MonoBehaviour
{
    [SerializeField] GameObject _active;
    [SerializeField] GameObject _videoPrefab;

    private GameObject _video;

    private void Start()
    {
        Target.DoubleClickEvent += Show;
    }

    private void Show(Target target)
    {
        _active.SetActive(true);
        _video = Instantiate(_videoPrefab);
    }

    public void Hide()
    {
        _active.SetActive(false);
        if (_video != null)
        {
            Destroy(_video);
        }
    }
}
