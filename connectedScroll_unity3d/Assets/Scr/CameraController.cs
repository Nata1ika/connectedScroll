using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float _planeZ = -160f;

    /// <summary>
    /// положение камеры в обычном режиме
    /// </summary>
    private Vector3 _defaultPosition;

    /// <summary>
    /// целевое положение
    /// </summary>
    private Vector3 _position;

    /// <summary>
    /// оставшееся время движения
    /// </summary>
    private float _timeMotion;

    private bool _isPlane;

    /// <summary>
    /// двигается ли сейчас линейно камера
    /// </summary>
    private bool _isMotion = false;

    private const float TIME_MOTION = 1f;

    private void Start()
    {
        _defaultPosition = gameObject.transform.position;
        Target.DoubleClickEvent += ToTarget;
    }

    private void OnDestroy()
    {
        Target.DoubleClickEvent -= ToTarget;
    }

    private void Update()
    {
        //transform.RotateAround(Vector3.zero, Vector3.up, Time.deltaTime);
    }

    public void ToTarget(Target target)
    {
        if (target is PlaneTarget)
        {
            _isPlane = true;
            StartSmoothMotion(new Vector3(target.transform.position.x, target.transform.position.y, _planeZ));
        }
        //_parentCamera.RotateAround(point, axis, angle);
    }

    public void Default()
    {
        if (_isPlane)
        {
            StartSmoothMotion(_defaultPosition);
        }
    }

    private void StartSmoothMotion(Vector3 position)
    {
        _position = position;
        _timeMotion = TIME_MOTION;
        if (!_isMotion)
        {
            StartCoroutine(SmoothMotion());
        }
    }

    IEnumerator SmoothMotion() //плавное движение к определенному положению
    {
        _isMotion = true;
        float delta;
        Vector3 position;

        while (_timeMotion > 0)
        {
            delta = Time.deltaTime;
            if (_timeMotion > delta)
            {
                position = transform.position;
                position += (_position - position) * delta / _timeMotion;
            }
            else
            {
                position = _position;
            }

            transform.position = position;

            _timeMotion -= delta;
            yield return null;
        }

        _isMotion = false;
    }
}
