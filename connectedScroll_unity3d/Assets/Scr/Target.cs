using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public static System.Action<Target> ChangeMotionTargetEvent;

    [SerializeField] ConnectController _connectController;

    private List<KeyValuePair<Target, float>> _neighborHorizontal = new List<KeyValuePair<Target, float>>();
    //private KeyValuePair<Target, float> _neighborVertical;

    /// <summary>
    /// хотя бы один из таргетов движется, значит остальные подстраиваются под него
    /// </summary>
    public static Target MotionTarget = null;

    /// <summary>
    /// двигается ли этот таргет
    /// </summary>
    private bool _isMotion = false;

    /// <summary>
    /// смещение относительно центра при первом клике
    /// </summary>
    private Vector2 _localPoint;

    private const float DELTA_ERROR = 0.5f;
    private const float MAX_MOTION = 0f;

    public static Dictionary<Target, KeyValuePair<Target, int>> neighborSequence = new Dictionary<Target, KeyValuePair<Target, int>>();

    /// <summary>
    /// время предыдущего клика
    /// </summary>
    //private float _lastClickTime;

    private RectTransform _rectTarnsform;
    public RectTransform RectTransform
    {
        get
        {
            if (_rectTarnsform == null)
            {
                _rectTarnsform = gameObject.GetComponent<RectTransform>();
            }
            return _rectTarnsform;
        }
    }

    public void AddNeighbor(Target target)
    {
        _neighborHorizontal.Add(new KeyValuePair<Target, float>(target, RectTransform.anchoredPosition.x - target.RectTransform.anchoredPosition.x));
    }

    private void Update()
    {
        if (_isMotion) //главная ячейка для движения 
        {
            if (Input.GetMouseButton(0)) //продолжаем движение
            {
                Vector2 newPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, Input.mousePosition, null, out newPoint);
                RectTransform.anchoredPosition = RectTransform.anchoredPosition + new Vector2(newPoint.x - _localPoint.x, 0);
            }
            else
            {
                _isMotion = false;
                MotionTarget = null;
                ChangeMotionTargetEvent?.Invoke(MotionTarget);
            }
        }


        if (MotionTarget == null && Input.GetMouseButton(0))//ищем ячейку для движения
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, Input.mousePosition, null, out _localPoint);
            if(RectTransform.rect.Contains(_localPoint))
            {
                _isMotion = true;
                MotionTarget = this;

                neighborSequence.Clear();
                CreateNeighborDictionary(0);

                ChangeMotionTargetEvent?.Invoke(MotionTarget);
            }
        }
    }

    public void UpdatePosition(Target target, int index)
    {
        if (index < 0)
        {
            return;
        }

        float delta = GetDelta(target); //разница текущего и эталонного расстояния
        if (Mathf.Abs(delta) > DELTA_ERROR) //надо двигать
        {
            float max = Mathf.Abs(delta) * Time.deltaTime * 20   /*(_connectController.Distance() - index)*/;            
            delta = Mathf.Clamp(delta, -max, max);
            RectTransform.anchoredPosition = new Vector2(RectTransform.anchoredPosition.x - delta, RectTransform.anchoredPosition.y);
        }
    }

    /// <summary>
    /// получить эталонное расстояние
    /// </summary>
    private float GetNeighborValue(Target target)
    {
        for (int i = 0; i < _neighborHorizontal.Count; i++)
        {
            if (_neighborHorizontal[i].Key == target)
            {
                return _neighborHorizontal[i].Value;
            }
        }
        return 0;
    }

    /// <summary>
    /// получить разницу расстояний текущего и эталонного
    /// </summary>
    private float GetDelta(int index)
    {
        return GetDelta(_neighborHorizontal[index].Key, _neighborHorizontal[index].Value);
    }

    /// <summary>
    /// получить разницу расстояний текущего и эталонного
    /// </summary>
    private float GetDelta(Target target)
    {
        return GetDelta(target, GetNeighborValue(target));
    }

    /// <summary>
    /// получить разницу расстояний текущего и эталонного
    /// </summary>
    private float GetDelta(Target target, float etalonDistance)
    {
        return RectTransform.anchoredPosition.x - target.RectTransform.anchoredPosition.x - etalonDistance;
    }


    public void CreateNeighborDictionary(int index)
    {
        /*if (index >= _connectController.Distance())
        {
            return;
        }*/

        if (index == 0)
        {
            neighborSequence.Add(this, new KeyValuePair<Target, int>(null, -1));
        }

        for (int i = 0; i < _neighborHorizontal.Count; i++)
        {
            KeyValuePair<Target, int> targetPair;
            if (neighborSequence.TryGetValue(_neighborHorizontal[i].Key, out targetPair))
            {
                if (targetPair.Value > index)
                {
                    targetPair = new KeyValuePair<Target, int>(this, index);

                    //neighborSequence.Remove(_neighborHorizontal[i].Key);
                    //neighborSequence.Add(_neighborHorizontal[i].Key, targetPair);

                    neighborSequence[_neighborHorizontal[i].Key] = targetPair;
                }
            }
            else
            {
                targetPair = new KeyValuePair<Target, int>(this, index);
                neighborSequence.Add(_neighborHorizontal[i].Key, targetPair);
                Debug.Log(_neighborHorizontal[i].Key.gameObject.name + "  ---  " + index);

                _neighborHorizontal[i].Key.CreateNeighborDictionary(index + 1);
            }            
        }
    }
}
