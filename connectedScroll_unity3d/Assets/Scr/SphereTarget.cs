using System.Collections;
using System;
using UnityEngine;

public class SphereTarget : Target
{
    public static Action<float> DeltaRotationEvent;
    public static Action<float, float> DeltaRotationVerticalEvent;

    [SerializeField] Parent[] _parents;
    [SerializeField] float _radius;

    public float angle;
    public float dam;
    private static float Z_ANGLE = 45f;

    protected override float DELTA_ERROR => 0.1f;

    private Vector3 _lastMousePosition;

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

    public void SetPosition(float setAngle, float setDam, bool immediatly)
    {
        if (immediatly)
        {
            angle = setAngle;
            dam = setDam;

            SetPosition();
            Enable = true;
        }
        else
        {
            StartCoroutine(Smooth.SmoothMotion(Position(_radius, setDam, setAngle), transform, TIME_SMOOTH, -1, UpdateParent, () => SetPosition(setAngle, setDam, true)));
            StartCoroutine(Smooth.SmoothRotation(ChildRotation(setAngle, setDam, Z_ANGLE), _child, TIME_SMOOTH));
            StartCoroutine(Smooth.SmoothScale(_connectController.GetScale(setDam), _child, TIME_SMOOTH));
        }
    }

    private void SetPosition()
    {
        rectTransform.localPosition = Position(_radius, dam, angle);

        _child.localEulerAngles = ChildRotation(angle, dam, Z_ANGLE);
        _child.localScale = _connectController.GetScale(dam);

        UpdateParent();
    }

    private void UpdateParent()
    {
        for (int i = 0; i < _parents.Length; i++)
        {
            if (rectTransform.localPosition.z < _parents[i].value)
            {
                transform.SetParent(_parents[i].transform);
                return;
            }
        }
    }

    private static Vector3 Position(float radius, float dam, float angle)
    {
        float hRadius = radius * Mathf.Cos(dam * Mathf.Deg2Rad);
        return new Vector3(
            hRadius * Mathf.Cos(angle * Mathf.Deg2Rad),
            radius * Mathf.Sin(dam * Mathf.Deg2Rad),
            hRadius * Mathf.Sin(angle * Mathf.Deg2Rad));
    }

    private static Vector3 ChildRotation(float angle, float dam, float z)
    {
        return new Vector3(dam, 270 - angle, z);
    }

    public override void AutoRotate()
    {
        float delta = 3 * Time.deltaTime;
        DeltaRotationEvent?.Invoke(delta);
        angle += delta;
        SetPosition();
    }

    /// <summary>
    /// повернуть так чтоб этот элемент был в центре
    /// </summary>
    public IEnumerator Rotate(float time)
    {
        float currAngle = angle;
        currAngle = Smooth.Angle360(currAngle);

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
            float delta = -Time.deltaTime * speed;
            DeltaRotationEvent?.Invoke(delta);

            angle += delta;
            SetPosition();
            time -= Time.deltaTime;
            yield return null;
        }

        angle = 270f;
        SetPosition();
    }


    protected override bool CanBeActive()
    {
        if (Vector3.Angle(_camera.gameObject.transform.forward, _child.forward) > 50f)
        {
            return false;
        }

        Vector2 childPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_child, Input.mousePosition, _camera, out childPoint);
        if (_child.rect.Contains(childPoint))
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, _camera, out _localPoint);
            _lastMousePosition = Input.mousePosition;
            return true;
        }

        return false;
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

        DeltaRotationVerticalEvent?.Invoke(Input.mousePosition.y, _lastMousePosition.y);
        _lastMousePosition = Input.mousePosition;

        float lastAngle = angle;
        angle = 360 - Mathf.Acos(x / _hRadius) * Mathf.Rad2Deg;
        float delta = angle - lastAngle;
        _deltaClcik += new Vector2(Mathf.Abs(delta), 0);

        DeltaRotationEvent?.Invoke(delta);

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

    protected override Vector2 GetDelta(Target target)
    {
        var delta = base.GetDelta(target);
        delta.x = Smooth.Angle180(delta.x);
        return delta;
    }

    protected override Vector2 GetCurrentValue(Target target)
    {
        var sphere = target as SphereTarget;
        return new Vector2(Smooth.Angle180(angle - sphere.angle), 0 /*dam - sphere.dam*/);
    }

    protected override Vector2 GetMinHorizontal(Target target)
    {
        return new Vector2(Mathf.Abs(GetNeighborValue(target).x * 0.8f), 0);
    }

    [ContextMenu("DebugForward")]
    private void DebugForward()
    {
        var obj = Instantiate(new GameObject("Debug"));
        obj.transform.position = _child.position - 160 * _child.forward;
    }
}
