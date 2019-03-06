using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereTarget : Target
{
    [SerializeField] Transform _parentUp;
    [SerializeField] Transform _parentDown;

    [SerializeField] Transform _child;
    [SerializeField] float _radius;

    private float _angle;
    public float angle;
    /*
    {
        get
        {
            return _angle;
        }
        private set
        {
            while (value < 0)
            {
                value += 360f;
            }
            while (value > 360)
            {
                value -= 360f;
            }
            _angle = value;
        }
    }
    */

    public float dam;
    private float _zAngle;

    protected override float DELTA_ERROR => 0.1f;

    private float _hRadius
    {
        get
        {
            return _radius * Mathf.Cos(dam * Mathf.Deg2Rad);
        }
    }

    public void SetPosition(float setAngle, float setDam, float z)
    {
        angle = setAngle;
        dam = setDam;
        _zAngle = z;

        SetPosition();
    }

    private void SetPosition()
    {
        rectTransform.localPosition = new Vector3(
            _hRadius * Mathf.Cos(angle * Mathf.Deg2Rad),
            _radius * Mathf.Sin(dam * Mathf.Deg2Rad),
            _hRadius * Mathf.Sin(angle * Mathf.Deg2Rad));

        _child.localEulerAngles = new Vector3(dam, 270 - angle, _zAngle);
        _child.localScale = _connectController.GetScale(dam);

        transform.SetParent(rectTransform.localPosition.z >= 0 ? _parentUp : _parentDown);
    }


    protected override bool CanBeActive()
    {
        if (rectTransform.localPosition.z > 0)
        {
            return false;
        }
        return base.CanBeActive();
    }

    /// <summary>
    /// текущий объект активен, двигаем его в зависимости от полжения мыши
    /// </summary>
    protected override void MotionActive()
    {
        Vector2 newPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, _camera, out newPoint);

        float x = rectTransform.localPosition.x + newPoint.x - _localPoint.x;
        x = Mathf.Clamp(x, -_hRadius, _hRadius);
        angle = 360 - Mathf.Acos(x / _hRadius) * Mathf.Rad2Deg;

        SetPosition();
    }

    public override void UpdatePosition(Target target)
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
            //curr.y = Mathf.Abs(curr.y);

            Vector2 min = GetMinHorizontal(target);
            min -= curr;

            max.x = Mathf.Max(max.x, min.x);
            //max.y = Mathf.Max(max.y, min.y);

            delta.x = Mathf.Clamp(delta.x, -max.x, max.x);
            delta.y = Mathf.Clamp(delta.y, -max.y, max.y);

            angle -= delta.x;
            SetPosition();
        }
    }

    //private void Update()
    //{
    //    RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, _camera, out _localPoint);
    //    Debug.Log(_localPoint + " --- " + rectTransform.rect.Contains(_localPoint) + " --- " + RectTransformUtility.RectangleContainsScreenPoint
    //        (rectTransform, new Vector2(Input.mousePosition.x, Input.mousePosition.y), _camera));
    //}

    protected override Vector2 GetCurrentValue(Target target)
    {
        var sphere = target as SphereTarget;
        return new Vector2(angle - sphere.angle, 0 /*dam - sphere.dam*/);
    }

    protected override Vector2 GetMinHorizontal(Target target)
    {
        return new Vector2(GetNeighborValue(target).x * 0.8f, 0);
    }
}
