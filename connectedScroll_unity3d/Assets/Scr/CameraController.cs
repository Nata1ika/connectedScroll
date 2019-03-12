using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float _planeZ = -160f;
    [SerializeField] Parent[] _camPositions;

    /// <summary>
    /// положение камеры в обычном режиме
    /// </summary>
    private Vector3 _defaultPosition;
    private Vector3 _defaultRotation;

    float _verticalRotation = 0f;
    private const float VERTICAL_MAX = 40f;
    private const float VERTICAL_MIN = -40f;


    private bool _isPlane;


    public const float TIME_MOTION = 1f;

    private void Start()
    {
        _defaultPosition = transform.position;
        _defaultRotation = transform.eulerAngles;

        Target.DoubleClickEvent += ToTarget;
        SphereTarget.DeltaRotationVerticalEvent += RotationVertical;
        Target.ClickEvent += ChangePlaneSphere;
    }

    private void OnDestroy()
    {
        Target.DoubleClickEvent -= ToTarget;
        SphereTarget.DeltaRotationVerticalEvent -= RotationVertical;
        Target.ClickEvent -= ChangePlaneSphere;
    }

    private void RotationVertical(float currentInput, float last)
    {
        float angle = Mathf.Clamp(last - currentInput, -60f * Time.deltaTime, 60f * Time.deltaTime);
        if (_verticalRotation + angle > VERTICAL_MAX)
        {
            angle = VERTICAL_MAX - _verticalRotation;
        }
        else if (_verticalRotation + angle < VERTICAL_MIN)
        {
            angle = VERTICAL_MIN - _verticalRotation;
        }

        _verticalRotation += angle;
        transform.RotateAround(Vector3.zero, Vector3.right, angle);
    }

    public void ToTarget(Target target, bool needWait)
    {
        StopAllCoroutines();
        if (target is PlaneTarget)
        {
            _isPlane = true;
            StartCoroutine(Smooth.SmoothMotion(new Vector3(target.transform.position.x, target.transform.position.y, _planeZ), transform, TIME_MOTION));
        }
        else
        {
            var sphere = target as SphereTarget;
            for (int i = 0; i < _camPositions.Length; i++)
            {
                if (sphere.dam < _camPositions[i].value)
                {
                    StartCoroutine(Smooth.SmoothMotion(_camPositions[i].transform.position, transform, TIME_MOTION));
                    StartCoroutine(Smooth.SmoothRotation(_camPositions[i].transform.eulerAngles, transform, TIME_MOTION));
                    _verticalRotation = _camPositions[i].transform.eulerAngles.x;
                    return;
                }
            }
        }
    }

    private void ChangePlaneSphere(Target target)
    {
        Default();
    }

    public void Default()
    {
        StopAllCoroutines();
        StartCoroutine(Smooth.SmoothMotion(_defaultPosition, transform, TIME_MOTION));
        StartCoroutine(Smooth.SmoothRotation(_defaultRotation, transform, TIME_MOTION));
        _verticalRotation = _defaultRotation.x;
    } 

}
