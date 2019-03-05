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
    private float _dam;

    public void SetPosition(float angle, float dam)
    {
        _angle = angle;
        _dam = dam;

        float hRadius = _radius * Mathf.Cos(_dam * Mathf.Deg2Rad);

        rectTransform.localPosition = new Vector3(
            hRadius * Mathf.Cos(_angle * Mathf.Deg2Rad),
            _radius * Mathf.Sin(_dam * Mathf.Deg2Rad),
            hRadius * Mathf.Sin(_angle * Mathf.Deg2Rad));

        _child.eulerAngles = new Vector3(180 - _dam, 90 - _angle, 0);

        transform.SetParent(rectTransform.localPosition.z > 0 ? _parentUp : _parentDown);
    }


    protected override bool CanBeActive()
    {
        if (_angle < 180)
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
        x = Mathf.Clamp(x, -_radius, _radius);
        _angle = 360 - Mathf.Acos(x / _radius) * Mathf.Rad2Deg;
        float z = _radius * Mathf.Sin(_angle * Mathf.Deg2Rad);

        //rectTransform.localPosition = new Vector3(x, y, z);
        //_child.eulerAngles = new Vector3(_damAngle, 90 - _angle, 0);
    }

    //private void Update()
    //{
    //    RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, _camera, out _localPoint);
    //    Debug.Log(_localPoint + " --- " + rectTransform.rect.Contains(_localPoint) + " --- " + RectTransformUtility.RectangleContainsScreenPoint
    //        (rectTransform, new Vector2(Input.mousePosition.x, Input.mousePosition.y), _camera));
    //}

}
