using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneTarget : Target
{
    public static bool Enable = false;

    protected override void Update()
    {
        if (Enable)
        {
            base.Update();
        }
    }

    protected override void MotionActive()
    {
        if (Enable)
        {
            base.MotionActive();
        }
    }

    public override void UpdatePosition(Target target)
    {
        if (Enable)
        {
            base.UpdatePosition(target);
        }
    }

    public void SetPosition(float x, float y)
    {
        rectTransform.localPosition = new Vector3(x, y, 0);
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;

        _child.localPosition = Vector3.zero;
        _child.localRotation = Quaternion.identity;
        _child.localScale = Vector3.one;
    }
}
