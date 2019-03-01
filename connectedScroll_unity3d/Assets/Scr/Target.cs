using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
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

    private bool _markerUpdate = false;
    private bool _markerUpdateRecursion = false;

    private const float DELTA_ERROR = 0.5f;
    public const float MAX_MOTION = 0f;

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
        _markerUpdate = false;
        _markerUpdateRecursion = false;

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
            }
        }


        if (MotionTarget == null && Input.GetMouseButton(0))//ищем ячейку для движения
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, Input.mousePosition, null, out _localPoint);
            if(RectTransform.rect.Contains(_localPoint))
            {
                _isMotion = true;
                MotionTarget = this;
            }
        }
    }

    public void UpdatePosition(Target target, float speed, float koef)
    {
        if (_markerUpdate)
        {
            return;
        }
        _markerUpdate = true;

        if (target != null)
        {
            float delta = RectTransform.anchoredPosition.x - target.RectTransform.anchoredPosition.x; //текущее расстояние
            delta -= GetNeighborValue(target); //разница текущего и эталонного расстояния
            if (Mathf.Abs(delta) > DELTA_ERROR) //надо двигать
            {
                delta = Mathf.Clamp(delta, -koef * speed, koef * speed);
                RectTransform.anchoredPosition = new Vector2(RectTransform.anchoredPosition.x - delta, RectTransform.anchoredPosition.y);
            }
        }

        for (int i = 0; i < _neighborHorizontal.Count; i++)
        {
            if (_neighborHorizontal[i].Key != target)
            {
                _neighborHorizontal[i].Key.UpdatePosition(this, Mathf.Max(GetDeltaNeighbor(i) * Time.deltaTime, MAX_MOTION), koef * 0.7f);
            }
        }
    }

    public void UpdatePositionRecursion()
    {

    }

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

    public float GetDeltaNeighbor(int index)
    {
        return Mathf.Abs
        (RectTransform.anchoredPosition.x - _neighborHorizontal[0].Key.RectTransform.anchoredPosition.x -
        _neighborHorizontal[index].Value);
    }
}
