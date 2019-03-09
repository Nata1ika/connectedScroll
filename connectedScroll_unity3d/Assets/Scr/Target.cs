using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Target : MonoBehaviour
{    
    public static Action<Target> ChangeMotionTargetEvent;
    public static Action<Target> ClickEvent;
    public static Action<Target> DoubleClickEvent;

    public const float TIME_SMOOTH = 0.5f;

    [SerializeField] protected ConnectController _connectController;
    [SerializeField] protected Camera _camera;

    [SerializeField] protected Transform _child;

    private List<KeyValuePair<Target, Vector2>> _neighborHorizontal = new List<KeyValuePair<Target, Vector2>>();
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
    protected Vector2 _localPoint;

    protected virtual float DELTA_ERROR => 0.5f;
    public static float KOEF_MOTION = 40f;

    /// <summary>
    /// время прошедшее с предыдущего клика
    /// </summary>
    private float _lastClick = 10f;

    /// <summary>
    /// расстояние, на которое подвинули активный объект. нужно для определения клика и движения
    /// </summary>
    protected Vector2 _deltaClcik;

    /// <summary>
    /// был ли клик - находимся в ожидании второго
    /// </summary>
    private bool _wasFirstClick = false;

    /// <summary>
    /// если расстояние движения активного объекта не превышает заданное значение по осям, то считаем это за клик
    /// </summary>
    protected virtual float DELTA_CLICK => 10f;

    /// <summary>
    /// интервал между двойными кликами
    /// </summary>
    private const float TIME_DOUBLE_CLICK = 0.2f;

    /// <summary>
    /// длительность для долгого клика
    /// </summary>
    private const float LONG_CLICK = 0.5f;
    

    private RectTransform _rectTarnsform;
    protected RectTransform rectTransform
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
        _neighborHorizontal.Add(new KeyValuePair<Target, Vector2>(target, GetCurrentValue(target)));
    }

    protected virtual void Start()
    {
        gameObject.GetComponentInChildren<UnityEngine.UI.Text>().text = gameObject.name.Substring(6);
        ClickEvent += ClickTarget;
    }

    private void OnDestroy()
    {
        ClickEvent -= ClickTarget;
    }    

    protected virtual void Update()
    {
        _lastClick += Time.deltaTime;

        if (_isMotion) //главная ячейка для движения 
        {
            if (Input.GetMouseButton(0)) //продолжаем движение
            {
                MotionActive();
            }
            else
            {                
                SetActive(false);                
            }
        }


        if (MotionTarget == null && Input.GetMouseButton(0))//ищем ячейку для движения
        {            
            if(CanBeActive())
            {
                SetActive(true);
            }
        }

        if (_wasFirstClick && MotionTarget != null && MotionTarget != this)
        {
            _wasFirstClick = false;
        }

        if (_wasFirstClick && MotionTarget == null && _lastClick > TIME_DOUBLE_CLICK)
        {
            _wasFirstClick = false;
            ClickEvent?.Invoke(this);
        }
    }

    private void ClickTarget(Target obj)
    {
        _wasFirstClick = false;
    }

    /// <summary>
    /// может ли элемент быть выбран в качестве активного
    /// </summary>
    protected virtual bool CanBeActive()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, _camera, out _localPoint);
        return rectTransform.rect.Contains(_localPoint);
    }

    /// <summary>
    /// текущий объект активен, двигаем его в зависимости от полжения мыши
    /// </summary>
    protected virtual void MotionActive()
    {
        Vector2 newPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, _camera, out newPoint);
        Vector2 delta = newPoint - _localPoint;
        _deltaClcik += new Vector2(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
        rectTransform.anchoredPosition = rectTransform.anchoredPosition + delta; //new Vector2(newPoint.x - _localPoint.x, 0);
    }

    public virtual void UpdatePosition(Target target)
    {
        if (target == null)
        {
            return;
        }

        Vector2 delta = GetDelta(target); //разница текущего и эталонного расстояния
        if (Mathf.Abs(delta.x) > DELTA_ERROR || Mathf.Abs(delta.y) > DELTA_ERROR) //надо двигать
        {
            Vector2 max = Time.deltaTime * KOEF_MOTION * new Vector2(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
            
            Vector2 curr = GetCurrentValue(target);
            curr.x = Mathf.Abs(curr.x);
            curr.y = Mathf.Abs(curr.y);

            Vector2 min = GetMinHorizontal(target);

            min -= curr;

            max.x = Mathf.Max(max.x, min.x);
            max.y = Mathf.Max(max.y, min.y);
            
            delta.x = Mathf.Clamp(delta.x, -max.x, max.x);
            delta.y = Mathf.Clamp(delta.y, -max.y, max.y);      
            
            rectTransform.anchoredPosition = rectTransform.anchoredPosition - delta;
        }
    }

    /// <summary>
    /// получить минимальное расстояние между объектами
    /// </summary>
    protected virtual Vector2 GetMinHorizontal(Target target)
    {
        float x1 = (_child.GetComponent<RectTransform>().rect.width + _child.GetComponent<RectTransform>().rect.width) / 2f;
        float x2 = Mathf.Abs(GetNeighborValue(target).x) - 20f;

        float y1 = 130f;
        float y2 = Mathf.Abs(GetNeighborValue(target).y);

        return new Vector2(Mathf.Min(x1, x2), Mathf.Min(y1, y2));
    }

    /// <summary>
    /// получить эталонное расстояние
    /// </summary>
    protected Vector2 GetNeighborValue(Target target)
    {
        for (int i = 0; i < _neighborHorizontal.Count; i++)
        {
            if (_neighborHorizontal[i].Key == target)
            {
                return _neighborHorizontal[i].Value;
            }
        }
        return Vector2.zero;
    }

    /// <summary>
    /// текущее расстояние между объектами
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    protected virtual Vector2 GetCurrentValue(Target target)
    {
        return rectTransform.anchoredPosition - target.rectTransform.anchoredPosition;
    }

    /// <summary>
    /// получить разницу расстояний текущего и эталонного
    /// </summary>
    private Vector2 GetDelta(int index)
    {
        return GetDelta(_neighborHorizontal[index].Key, _neighborHorizontal[index].Value);
    }

    /// <summary>
    /// получить разницу расстояний текущего и эталонного
    /// </summary>
    protected virtual Vector2 GetDelta(Target target)
    {
        return GetDelta(target, GetNeighborValue(target));
    }

    /// <summary>
    /// получить разницу расстояний текущего и эталонного
    /// </summary>
    private Vector2 GetDelta(Target target, Vector2 etalonDistance)
    {
        return GetCurrentValue(target) - etalonDistance;
    }


    public void CreateNeighborDictionary()
    {
        for (int i = 0; i < _neighborHorizontal.Count; i++)
        {
            _connectController.AddNeighborSequence(_neighborHorizontal[i].Key, this);            
        }
    }

    private void SetActive(bool active)
    {
        _isMotion = active;
        MotionTarget = active ? this : null;

        transform.localScale = (active ? 0.9f : 1) * Vector3.one;

        if (active)
        {            
            if (_wasFirstClick && _lastClick > TIME_DOUBLE_CLICK)
            {
                _wasFirstClick = false;
            }

            _deltaClcik = Vector2.zero;
            _lastClick = 0f;
        }
        else if (_lastClick < LONG_CLICK && _deltaClcik.x < DELTA_CLICK && _deltaClcik.y < DELTA_CLICK)
        {
            if (_wasFirstClick)
            {
                DoubleClickEvent?.Invoke(this);
                _wasFirstClick = false;
            }
            else
            {
                _wasFirstClick = true;
                _lastClick = 0;
            }
        }

        ChangeMotionTargetEvent?.Invoke(MotionTarget);
    }

    [ContextMenu("Neighbor")]
    private void DebugNeighbor()
    {
        string text = string.Format("Count = {0};", _neighborHorizontal.Count);
        for (int i = 0; i < _neighborHorizontal.Count; i++)
        {
            text += string.Format(" {0} = {1}; ", i, _neighborHorizontal[i].Key.gameObject.name);
        }
        Debug.Log(text);
    }
}
