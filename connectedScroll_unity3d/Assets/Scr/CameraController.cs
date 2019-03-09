using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float _planeZ = -160f;
    [SerializeField] CameraPosition[] _camPositions;

    /// <summary>
    /// положение камеры в обычном режиме
    /// </summary>
    private Vector3 _defaultPosition;
    private Vector3 _defaultRotation;


    private bool _isPlane;


    public const float TIME_MOTION = 1f;

    private void Start()
    {
        _defaultPosition = transform.position;
        _defaultRotation = transform.eulerAngles;

        Target.DoubleClickEvent += ToTarget;
    }

    private void OnDestroy()
    {
        Target.DoubleClickEvent -= ToTarget;
    }

    public void ToTarget(Target target)
    {
        StopAllCoroutines();
        if (target is PlaneTarget)
        {
            _isPlane = true;
            StartCoroutine(SmoothMotion(new Vector3(target.transform.position.x, target.transform.position.y, _planeZ)));
        }
        else
        {
            var sphere = target as SphereTarget;
            for (int i = 0; i < _camPositions.Length; i++)
            {
                if (sphere.dam < _camPositions[i].dam)
                {
                    StartCoroutine(SmoothMotion(_camPositions[i].target.position));
                    StartCoroutine(SmoothRotation(_camPositions[i].target.eulerAngles));
                    return;
                }
            }
        }
    }

    public void Default()
    {
        StopAllCoroutines();
        StartCoroutine(SmoothMotion(_defaultPosition));
        StartCoroutine(SmoothRotation(_defaultRotation));
    }

    IEnumerator SmoothMotion(Vector3 target) //плавное движение 
    {
        float timeMotion = TIME_MOTION;
        float delta;
        Vector3 position;

        while (timeMotion > 0)
        {
            delta = Time.deltaTime;
            if (timeMotion > delta)
            {
                position = transform.position;
                position += (target - position) * delta / timeMotion;
            }
            else
            {
                position = target;
            }

            transform.position = position;

            timeMotion -= delta;
            yield return null;
        }
    }

    IEnumerator SmoothRotation(Vector3 target) //плавное вращение 
    {
        float timeRotation = TIME_MOTION;
        float delta;
        Vector3 currentRotation;

        while (timeRotation > 0)
        {
            delta = Time.deltaTime;
            if (timeRotation > delta)
            {
                currentRotation = transform.rotation.eulerAngles;
                currentRotation = currentRotation + (target - currentRotation) * delta / timeRotation;
            }
            else
            {
                currentRotation = target;
            }

            transform.rotation = Quaternion.Euler(currentRotation);

            timeRotation -= delta;
            yield return null;
        }
    }

}


[System.Serializable]
public class CameraPosition
{
    public int dam;
    public Transform target;
}
