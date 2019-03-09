using System.Collections;
using System;
using UnityEngine;

public static class Smooth
{
    /// <summary>
    /// плавное движение
    /// </summary>
    public static IEnumerator SmoothMotion(Vector3 target, Transform transform, float TIME, float wait = -1f, Action everFrame = null, Action onEnd = null) 
    {
        if (wait > 0)
        {
            yield return new WaitForSeconds(wait);
        }

        float timeMotion = TIME;
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
            everFrame?.Invoke();
            yield return null;
        }

        yield return null;
        onEnd?.Invoke();
    }

    /// <summary>
    /// плавное вращение
    /// </summary>
    public static IEnumerator SmoothRotation(Vector3 target, Transform transform, float TIME, float wait = -1f, Action everFrame = null, Action onEnd = null) 
    {
        if (wait > 0)
        {
            yield return new WaitForSeconds(wait);
        }       

        float timeRotation = TIME;
        Vector3 currentRotation = transform.rotation.eulerAngles;

        Vector3 delta = target - currentRotation;
        delta.x = Angle180(delta.x) / TIME;
        delta.y = Angle180(delta.y) / TIME;
        delta.z = Angle180(delta.z) / TIME;

        while (timeRotation > Time.deltaTime)
        {
            currentRotation += delta * Time.deltaTime;
            transform.eulerAngles = currentRotation;

            timeRotation -= Time.deltaTime;
            everFrame?.Invoke();
            yield return null;
        }

        transform.eulerAngles = target;

        yield return null;
        onEnd?.Invoke();
    }

    public static IEnumerator SmoothScale(Vector3 target, Transform transform, float TIME, Action everFrame = null, Action onEnd = null)
    {
        float time = TIME;
        float delta;
        Vector3 current;

        while (time > 0)
        {
            delta = Time.deltaTime;
            if (time > delta)
            {
                current = transform.localScale;
                current = current + (target - current) * delta / time;
            }
            else
            {
                current = target;
            }

            transform.localScale = current;

            time -= delta;
            everFrame?.Invoke();
            yield return null;
        }

        yield return null;
        onEnd?.Invoke();
    }

    public static float Angle360(float angle)
    {
        while (angle > 360f)
        {
            angle -= 360f;
        }
        while (angle < 0)
        {
            angle += 360f;
        }
        return angle;
    }

    public static float Angle180(float angle)
    {
        while (angle > 180f)
        {
            angle -= 360f;
        }
        while (angle < -180)
        {
            angle += 360f;
        }
        return angle;
    }
}
