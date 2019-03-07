using UnityEngine;
using UnityEngine.UI;

public class Active : MonoBehaviour
{
    [SerializeField] GameObject _active;

    private void Start()
    {
        Target.DoubleClickEvent += Show;
    }

    private void Show(Target target)
    {
        _active.SetActive(true);
    }

    public void Hide()
    {
        _active.SetActive(false);
    }
}
