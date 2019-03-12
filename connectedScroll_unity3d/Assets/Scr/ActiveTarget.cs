using UnityEngine;
using UnityEngine.UI;

public class ActiveTarget : MonoBehaviour
{
    [SerializeField] Text _text;

    public Target Target { get; private set; }

    public void Show(Target target)
    {
        Target = target;
        _text.text = target.gameObject.name;
    }
}
