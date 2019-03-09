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
            StartCoroutine(Smooth.SmoothMotion(new Vector3(target.transform.position.x, target.transform.position.y, _planeZ), transform, TIME_MOTION));
        }
        else
        {
            var sphere = target as SphereTarget;
            for (int i = 0; i < _camPositions.Length; i++)
            {
                if (sphere.dam < _camPositions[i].dam)
                {
                    StartCoroutine(Smooth.SmoothMotion(_camPositions[i].target.position, transform, TIME_MOTION));
                    StartCoroutine(Smooth.SmoothRotation(_camPositions[i].target.eulerAngles, transform, TIME_MOTION));
                    return;
                }
            }
        }
    }

    public void Default()
    {
        StopAllCoroutines();
        StartCoroutine(Smooth.SmoothMotion(_defaultPosition, transform, TIME_MOTION));
        StartCoroutine(Smooth.SmoothRotation(_defaultRotation, transform, TIME_MOTION));
    } 

}


[System.Serializable]
public class CameraPosition
{
    public int dam;
    public Transform target;
}
