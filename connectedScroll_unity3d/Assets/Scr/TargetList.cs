using UnityEngine;

public class TargetList : MonoBehaviour
{
    [SerializeField] ConnectController _connect;
    [SerializeField] ActiveTarget _activeTargetPrefab;

    [SerializeField] Transform _currentParent;
    [SerializeField] Transform _nextParent;
    [SerializeField] Transform _prevParent;

    private ActiveTarget[] _activeTarget;

    private float TIME = 0.3f;

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void Show(Target target)
    {
        Create();
        _activeTarget[2].Show(target);
        _activeTarget[1].Show(_connect.GetPrev(target));
        _activeTarget[0].Show(_connect.GetPrev(_activeTarget[1].Target));
        _activeTarget[3].Show(_connect.GetNext(target));
        _activeTarget[4].Show(_connect.GetNext(_activeTarget[3].Target));
    }

    public void Prev()
    {
        StopAllCoroutines();

        StartCoroutine(Smooth.SmoothMotion(GetPosition(1), _activeTarget[0].gameObject.transform, TIME, -1f, null, FinishPrev));
        for (int i = 1; i < _activeTarget.Length; i++)
        {
            StartCoroutine(Smooth.SmoothMotion(GetPosition(i + 1), _activeTarget[i].gameObject.transform, TIME));
        }
    }

    public void Next()
    {
        StopAllCoroutines();
        StartCoroutine(Smooth.SmoothMotion(GetPosition(-1), _activeTarget[0].gameObject.transform, TIME, -1f, null, FinishNext));
        for (int i = 1; i < _activeTarget.Length; i++)
        {
            StartCoroutine(Smooth.SmoothMotion(GetPosition(i - 1), _activeTarget[i].gameObject.transform, TIME));
        }
    }


    private void Create()
    {
        if (_activeTarget != null)
        {
            SetPosition();
            return;
        }

        _activeTarget = new ActiveTarget[5];

        for (int i = 0; i < _activeTarget.Length; i++)
        {
            _activeTarget[i] = Instantiate(_activeTargetPrefab, GetPosition(i), Quaternion.identity, _currentParent);
        }
    }

    private void SetPosition()
    {
        for (int i = 0; i < _activeTarget.Length; i++)
        {
            _activeTarget[i].gameObject.transform.position = GetPosition(i);
        }
    }

    private Vector3 GetPosition(int index)
    {
        switch (index)
        {
            case -1: return 3 * _prevParent.position - 2 * _currentParent.position;
            case 0: return 2 * _prevParent.position - _currentParent.position;
            case 1: return _prevParent.position;
            case 2: return _currentParent.position;
            case 3: return _nextParent.position;
            case 4: return 2 * _nextParent.position - _currentParent.position;
            default: return 3 * _nextParent.position - 2 * _currentParent.position;
        }
    }

    private void FinishPrev()
    {
        var empty = _activeTarget[_activeTarget.Length - 1];
        for (int i = _activeTarget.Length - 1; i > 0; i--)
        {
            _activeTarget[i] = _activeTarget[i - 1];
        }
        _activeTarget[0] = empty;
        _activeTarget[0].Show(_connect.GetPrev(_activeTarget[1].Target));
        SetPosition();
    }

    private void FinishNext()
    {
        var empty = _activeTarget[0];
        for (int i = 0; i < _activeTarget.Length - 1; i++)
        {
            _activeTarget[i] = _activeTarget[i + 1];
        }
        _activeTarget[_activeTarget.Length - 1] = empty;
        _activeTarget[_activeTarget.Length - 1].Show(_connect.GetNext(_activeTarget[_activeTarget.Length - 2].Target));
        SetPosition();
    }
}
