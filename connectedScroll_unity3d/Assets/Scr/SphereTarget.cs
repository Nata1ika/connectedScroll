using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereTarget : Target
{
    [SerializeField] Transform _parent_m100;
    [SerializeField] Transform _parent_m50;
    [SerializeField] Transform _parent_0;
    [SerializeField] Transform _parent_50;
    [SerializeField] Transform _parent_100;
    [SerializeField] Transform _parent_p100;

    [SerializeField] float _radius;

    public float angle;
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

    protected override float DELTA_CLICK => 5f;

    public static bool Enable = false;

    protected override void Update()
    {
        if (Enable)
        {
            base.Update();
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

        if (rectTransform.localPosition.z < -100f)
        {
            transform.SetParent(_parent_m100);
        }
        else if (rectTransform.localPosition.z < -50f)
        {
            transform.SetParent(_parent_m50);
        }
        else if (rectTransform.localPosition.z < 0)
        {
            transform.SetParent(_parent_0);
        }
        else if (rectTransform.localPosition.z < 50)
        {
            transform.SetParent(_parent_50);
        }
        else if (rectTransform.localPosition.z < 100)
        {
            transform.SetParent(_parent_100);
        }
        else
        {
            transform.SetParent(_parent_p100);
        }
    }

    public IEnumerator Rotate(float time)
    {
        float currAngle = angle;
        while (currAngle > 360f)
        {
            currAngle -= 360f;
        }
        while (currAngle < 0)
        {
            currAngle += 360f;
        }

        //делта угло между текущим и тем чтоб стать 270
        float deltaAngle;
        if (currAngle < 90)
        {
            deltaAngle = 90 + currAngle;
        }
        else
        {
            deltaAngle = currAngle - 270;
        }

        float speed = deltaAngle / time; //скорость движения в секунду

        while (time > Time.deltaTime)
        {
            angle -= Time.deltaTime * speed;
            SetPosition();
            time -= Time.deltaTime;
            yield return null;
        }

        angle = 270f;
        SetPosition();
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
        if (! Enable)
        {
            return;
        }

        Vector2 newPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, _camera, out newPoint);

        float x = rectTransform.localPosition.x + newPoint.x - _localPoint.x;
        x = Mathf.Clamp(x, -_hRadius, _hRadius);

        float lastAngle = angle;
        angle = 360 - Mathf.Acos(x / _hRadius) * Mathf.Rad2Deg;
        _deltaClcik += new Vector2(Mathf.Abs(angle - lastAngle), 0);

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

    protected override Vector2 GetCurrentValue(Target target)
    {
        var sphere = target as SphereTarget;
        return new Vector2(angle - sphere.angle, 0 /*dam - sphere.dam*/);
    }

    protected override Vector2 GetMinHorizontal(Target target)
    {
        return new Vector2(GetNeighborValue(target).x * 0.8f, 0);
    }

    [ContextMenu("DebugForward")]
    private void DebugForward()
    {
        var obj = Instantiate(new GameObject("Debug"));
        obj.transform.position = _child.position - 160 * _child.forward;
    }
}
