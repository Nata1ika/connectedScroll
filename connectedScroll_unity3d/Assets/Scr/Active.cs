using UnityEngine;
using UnityEngine.UI;

public class Active : MonoBehaviour
{
    [SerializeField] Animation _animation;
    [SerializeField] Text _text;

    public void Show(Target target)
    {
        gameObject.SetActive(true);
        _animation.Play("ActiveTarget");
        _text.text = target.gameObject.name;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        _animation.Stop();
    }
}
