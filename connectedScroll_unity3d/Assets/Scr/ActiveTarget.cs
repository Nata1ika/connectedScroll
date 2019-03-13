using UnityEngine;
using UnityEngine.UI;

public class ActiveTarget : MonoBehaviour
{
    [SerializeField] Text _text;

    public Target Target { get; private set; }
    private GameObject _content;

    public void Show(Target target)
    {
        Target = target;
        _text.text = target.gameObject.name;

        if (_content != null)
        {
            Destroy(_content);
        }

        if (target.contentPrefab != null)
        {
            _content = Instantiate(target.contentPrefab, transform);
        }
    }
}
