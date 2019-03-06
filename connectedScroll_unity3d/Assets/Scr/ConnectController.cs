using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectController : MonoBehaviour
{
    /// <summary>
    /// настройки объектов по рядам
    /// </summary>
    [SerializeField] Row[] _row;
    [SerializeField] AnimationCurve _sphereScale;

    private Dictionary<Target, Target> _neighborSequence = new Dictionary<Target, Target>();
    private Queue<Target> _neighborQueue = new Queue<Target>();

    private Target[] _targets;

    private void Start()
    {
        _targets = gameObject.GetComponentsInChildren<Target>();
        if (_targets.Length != SummCount())
        {
            Debug.LogErrorFormat("count obj = {0}, infoCount = {1}", _targets.Length, SummCount());
        }

        Target.ChangeMotionTargetEvent += ChangeTarget;

        if (_targets[0] is SphereTarget)
        {
            for (int i = 0; i < _row.Length; i++) //i ряд
            {
                for (int j = 0; j < _row[i].count; j++) //j элемент в ряду
                {
                    int index = GetIndex(i, j);
                    var sphere = _targets[index] as SphereTarget;
                    sphere.SetPosition((j + 0.5f * (i % 2)) * 360f / _row[i].count, _row[i].damAngle, _row[i].zRotation);
                }
            }
        }

        //по каждому ряду стяжка
        for (int i = 0; i < _row.Length; i++) //i ряд
        {
            for (int j = 0; j < _row[i].count - 1; j++) //j элемент в ряду
            {
                int index = GetIndex(i, j);

                _targets[index].AddNeighbor(_targets[index + 1]);
                _targets[index + 1].AddNeighbor(_targets[index]);
            }

            //сферическая стяжка первого и последнего элементов
            if (_targets[0] is SphereTarget)
            {
                int first = GetIndex(i, 0);
                int last = GetIndex(i, _row[i].count - 1);

                _targets[first].AddNeighbor(_targets[last]);
                _targets[last].AddNeighbor(_targets[first]);
            }
        }

        
        //стяжка между рядами
        for (int i = 0; i < _row.Length - 1; i++) //i ряд
        {
            for (int j = 0; j < _row[i].count; j++) //j элемент в ряду
            {
                if (j < _row[i + 1].count)
                {
                    int index = GetIndex(i, j);
                    int next = GetIndex(i + 1, j);

                    _targets[index].AddNeighbor(_targets[next]);
                    _targets[next].AddNeighbor(_targets[index]);
                }

                if (_row[i].count != _row[i + 1].count && _row[i + 1].count - j - 1 >= 0) //с конца
                {
                    int index = GetIndex(i, _row[i].count - j - 1);
                    int next = GetIndex(i + 1, _row[i + 1].count - j - 1);

                    _targets[index].AddNeighbor(_targets[next]);
                    _targets[next].AddNeighbor(_targets[index]);
                }

                //стяжка для сфер по диагонали
                if (_targets[0] is SphereTarget)
                {
                    int index = GetIndex(i, j);
                    int next;
                    if (i % 2 == 0)
                    {
                        next = GetIndex(i + 1, j - 1 >= 0 ? j - 1 : _row[i + 1].count - 1);
                    }
                    else
                    {
                        next = GetIndex(i + 1, j + 1 < _row[i + 1].count ? j + 1 : 0);
                    }

                    _targets[index].AddNeighbor(_targets[next]);
                    _targets[next].AddNeighbor(_targets[index]);
                }
            }
        }        
    }

    private void OnDestroy()
    {
        Target.ChangeMotionTargetEvent -= ChangeTarget;
    }
    
    public Vector3 GetScale(float dam)
    {
        return _sphereScale.Evaluate(dam) * Vector3.one;
    }

    private int SummCount()
    {
        int summ = 0;
        for (int i = 0; i < _row.Length; i++)
        {
            summ += _row[i].count;
        }
        return summ;
    }

    /// <summary>
    /// получить индекс объекта в общей нумерации
    /// </summary>
    /// <param name="i">ряд</param>
    /// <param name="j">элемент в ряду</param>
    /// <returns></returns>
    private int GetIndex(int i, int j)
    {
        int index = 0;
        for (int r = 0; r < i; r++)
        {
            index += _row[r].count;
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
}

[Serializable]
public class Row
{
    public int count;
    public int damAngle;
    public float zRotation;
}
