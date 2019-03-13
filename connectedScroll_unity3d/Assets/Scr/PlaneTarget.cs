using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneTarget : Target
{
    public static bool Enable = false;

    private Vector2 _borderMin;
    private Vector2 _borderMax;

    protected override void Update()
    {
        if (Enable)
        {
            base.Update();
        }
    }

    protected override void MotionActive()
    {
        if (!Enable)
        {
            return;
        }

        Vector2 newPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, _camera, out newPoint);
        Vector2 delta = newPoint - _localPoint;
        _deltaClcik += new Vector2(Mathf.Abs(delta.x), Mathf.Abs(delta.y));

        Vector2 position = rectTransform.anchoredPosition + delta;
        position.x = Mathf.Clamp(position.x, _borderMin.x, _borderMax.x);
        position.y = Mathf.Clamp(position.y, _borderMin.y, _borderMax.y);
        rectTransform.anchoredPosition = position;
    }

    public void SetBorder(Vector2 min, Vector2 max)
    {
        _borderMin = min;
        _borderMax = max;
    }

    public override void UpdatePosition(Target target)
    {
        if (Enable)
        {
            base.UpdatePosition(target);
        }
    }

    public void SetPosition(float x, float y, bool immediatly)
    {
        if (immediatly)
        {
            rectTransform.localPosition = new Vector3(x, y, 0);
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;

            _child.localPosition = Vector3.zero;
            _child.localRotation = Quaternion.identity;
            _child.localScale = Vector3.one;

            Enable = true;
        }
        else
        {
            StartCoroutine(Smooth.SmoothMotion(new Vector3(x, y, 0), transform, TIME_SMOOTH, -1f, null, () => SetPosition(x, y, true)));
            StartCoroutine(Smooth.SmoothRotation(Vector3.zero, _child, TIME_SMOOTH));
            StartCoroutine(Smooth.SmoothScale(Vector3.one, _child, TIME_SMOOTH));
        }
    }
}
