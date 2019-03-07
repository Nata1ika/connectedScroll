using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectController : MonoBehaviour
{
    /// <summary>
    /// настройки объектов по рядам для сферического расположения
    /// </summary>
    [SerializeField] Row[] _rowSphere;

    /// <summary>
    /// расстановка по рядам для размещения на плоскости
    /// </summary>
    [SerializeField] RowPlane[] _rowPlane;

    /// <summary>
    /// изменение размеров при изменении угла наклона dam
    /// </summary>
    [SerializeField] AnimationCurve _sphereScale;

    [SerializeField] Vector2 _deltaPositionPlane;
    [SerializeField] Vector2 _offsetPositionPlane;

    private Dictionary<Target, Target> _neighborSequence = new Dictionary<Target, Target>();
    private Queue<Target> _neighborQueue = new Queue<Target>();

    private PlaneTarget[] _planeTargets;
    private SphereTarget[] _sphereTargets;

    private bool _isPlane = true;

    private void Start()
    {
        //сфера
        _sphereTargets = gameObject.GetComponentsInChildren<SphereTarget>();
        if (_sphereTargets.Length != SummCount(_rowSphere))
        {
            Debug.LogErrorFormat("count obj SPHERE = {0}, infoCount = {1}", _sphereTargets.Length, SummCount(_rowSphere));
        }

        SetPositionSphere(true);

        Screed(_sphereTargets, _rowSphere);
        ScreedSphere();


        //плоскость
        _planeTargets = gameObject.GetComponentsInChildren<PlaneTarget>();
        if (_planeTargets.Length != SummCount(_rowPlane))
        {
            Debug.LogErrorFormat("count obj PLANE = {0}, infoCount = {1}", _planeTargets.Length, SummCount(_rowPlane));
        }

        SetPositionPlane(true);
        Screed(_planeTargets, _rowPlane);      
        

        Target.ChangeMotionTargetEvent += ChangeTarget;
        Target.ClickEvent += TargetClick;
        Target.DoubleClickEvent += DoubleClick;
    }

    private void OnDestroy()
    {
        Target.ChangeMotionTargetEvent -= ChangeTarget;
        Target.ClickEvent -= TargetClick;
        Target.DoubleClickEvent -= DoubleClick;
    }

    private void Screed(Target[] targets, RowPlane[] row)
    {
        //по каждому ряду стяжка
        for (int i = 0; i < row.Length; i++) //i ряд
        {
            for (int j = 0; j < row[i].count - 1; j++) //j элемент в ряду
            {
                int index = GetIndex(i, j, row);

                targets[index].AddNeighbor(targets[index + 1]);
                targets[index + 1].AddNeighbor(targets[index]);
            }
        }

        //стяжка между рядами
        for (int i = 0; i < row.Length - 1; i++) //i ряд
        {
            for (int j = 0; j < row[i].count; j++) //j элемент в ряду
            {
                if (j < row[i + 1].count)
                {
                    int index = GetIndex(i, j, row);
                    int next = GetIndex(i + 1, j, row);

                    targets[index].AddNeighbor(targets[next]);
                    targets[next].AddNeighbor(targets[index]);
                }

                if (row[i].count != row[i + 1].count && row[i + 1].count - j - 1 >= 0) //с конца
                {
                    int index = GetIndex(i, row[i].count - j - 1, row);
                    int next = GetIndex(i + 1, row[i + 1].count - j - 1, row);

                    targets[index].AddNeighbor(targets[next]);
                    targets[next].AddNeighbor(targets[index]);
                }
            }
        }
    }

    private void ScreedSphere()
    {
        //сферическая стяжка первого и последнего элементов в каждом ряду
        for (int i = 0; i < _rowSphere.Length; i++) //i ряд
        {
            int first = GetIndex(i, 0, _rowSphere);
            int last = GetIndex(i, _rowSphere[i].count - 1, _rowSphere);

            _sphereTargets[first].AddNeighbor(_sphereTargets[last]);
            _sphereTargets[last].AddNeighbor(_sphereTargets[first]);
        }


        //стяжка для сфер по диагонали
        for (int i = 0; i < _rowSphere.Length - 1; i++) //i ряд
        {
            if (_rowSphere[i].count != _rowSphere[i + 1].count)
            {
                continue;
            }

            for (int j = 0; j < _rowSphere[i].count; j++) //j элемент в ряду
            {
                int index = GetIndex(i, j, _rowSphere);
                int next;
                if (i % 2 == 0)
                {
                    next = GetIndex(i + 1, j - 1 >= 0 ? j - 1 : _rowSphere[i + 1].count - 1, _rowSphere);
                }
                else
                {
                    next = GetIndex(i + 1, j + 1 < _rowSphere[i + 1].count ? j + 1 : 0, _rowSphere);
                }

                _sphereTargets[index].AddNeighbor(_sphereTargets[next]);
                _sphereTargets[next].AddNeighbor(_sphereTargets[index]);
            }
        }
    }

    private void SetPositionPlane(bool immediatly)
    {
        _neighborSequence.Clear();
        _neighborQueue.Clear();

        PlaneTarget.Enable = true;
        SphereTarget.Enable = false;

        _isPlane = true;

        float middleI = _rowPlane.Length / 2f;
        for (int i = 0; i < _rowPlane.Length; i++) //i ряд
        {            
            float y = (middleI - i) * _deltaPositionPlane.y;

            float middleX = _rowPlane[i].count / 2f;
            for (int j = 0; j < _rowPlane[i].count; j++) //j элемент в ряду
            {
                float x = (middleX - j) * _deltaPositionPlane.x;
                int index = GetIndex(i, j, _rowPlane);
                _planeTargets[index].SetPosition(x + _offsetPositionPlane.x, y + _offsetPositionPlane.y);
            }
        }
    }

    private void SetPositionSphere(bool immediatly)
    {
        _neighborSequence.Clear();
        _neighborQueue.Clear();

        PlaneTarget.Enable = false;
        SphereTarget.Enable = true;

        _isPlane = false;

        for (int i = 0; i < _rowSphere.Length; i++) //i ряд
        {
            for (int j = 0; j < _rowSphere[i].count; j++) //j элемент в ряду
            {
                int index = GetIndex(i, j, _rowSphere);
                _sphereTargets[index].SetPosition((j + 0.5f * (i % 2)) * 360f / _rowSphere[i].count, _rowSphere[i].damAngle, _rowSphere[i].zRotation);
            }
        }
    }
    
    public Vector3 GetScale(float dam)
    {
        return _sphereScale.Evaluate(dam) * Vector3.one;
    }

    private int SummCount(RowPlane[] row)
    {
        int summ = 0;
        for (int i = 0; i < row.Length; i++)
        {
            summ += row[i].count;
        }
        return summ;
    }


    /// <summary>
    /// получить индекс объекта в общей нумерации
    /// </summary>
    /// <param name="i">ряд</param>
    /// <param name="j">элемент в ряду</param>
    /// <returns></returns>
    private int GetIndex(int i, int j, RowPlane[] row)
    {
        int index = 0;
        for (int r = 0; r < i; r++)
        {
            index += row[r].count;
        }
        index += j;
        return index;
    }

    private void ChangeTarget(Target obj)
    {
        if (obj == null)
        {
            return;
        }

        _neighborSequence.Clear();
        _neighborQueue.Clear();

        _neighborSequence.Add(obj, null);

        while (obj != null)
        {
            obj.CreateNeighborDictionary();
            if (_neighborQueue.Count != 0)
            {
                obj = _neighborQueue.Dequeue();
            }
            else
            {
                break;
            }
        }
    }

    public void AddNeighborSequence(Target first, Target second)
    {
        if (_neighborSequence.ContainsKey(first))
        {
            return;
        }
        _neighborSequence.Add(first, second);
        _neighborQueue.Enqueue(first);
    }

    private void LateUpdate()
    {
        foreach(var obj in _neighborSequence)
        {
            obj.Key.UpdatePosition(obj.Value);
        }
    }

    private void TargetClick(Target target)
    {
        Debug.Log("CLICK  " + target.gameObject.name);
        if (_isPlane)
        {
            SetPositionSphere(false);
        }
        else
        {
            SetPositionPlane(false);
        }
    }

    private void DoubleClick(Target target)
    {
        Debug.Log("DOUBLE  " + target.gameObject.name);
        StopClick();
    }

    public void StartClick()
    {
        if (_isPlane)
        {
            PlaneTarget.Enable = true;
            SphereTarget.Enable = false;
        }
        else
        {
            PlaneTarget.Enable = false;
            SphereTarget.Enable = true;
        }
    }

    private void StopClick()
    {
        _neighborSequence.Clear();
        _neighborQueue.Clear();

        PlaneTarget.Enable = false;
        SphereTarget.Enable = false;
    }
}

[Serializable]
public class Row : RowPlane
{
    public int damAngle;
    public float zRotation;
}

[Serializable]
public class RowPlane
{
    public int count;
}
