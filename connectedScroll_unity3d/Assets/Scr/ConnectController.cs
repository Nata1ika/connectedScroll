using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectController : MonoBehaviour
{
    /// <summary>
    /// количество объектов в рядах
    /// </summary>
    [SerializeField] int[] _count;

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

        //по каждому ряду стяжка
        for (int i = 0; i < _count.Length; i++) //i ряд
        {
            for (int j = 0; j < _count[i] - 1; j++) //j элемент в ряду
            {
                int index = GetIndex(i, j);

                _targets[index].AddNeighbor(_targets[index + 1]);
                _targets[index + 1].AddNeighbor(_targets[index]);
            }
        }

        //стяжка между рядами
        for (int i = 0; i < _count.Length - 1; i++) //i ряд
        {
            for (int j = 0; j < _count[i]; j++) //j элемент в ряду
            {
                if (j < _count[i + 1])
                {
                    int index = GetIndex(i, j);
                    int next = GetIndex(i + 1, j);

                    _targets[index].AddNeighbor(_targets[next]);
                    _targets[next].AddNeighbor(_targets[index]);
                }

                if (_count[i] != _count[i + 1] && _count[i + 1] - j - 1 >= 0) //с конца
                {
                    int index = GetIndex(i, _count[i] - j - 1);
                    int next = GetIndex(i + 1, _count[i + 1] - j - 1);

                    _targets[index].AddNeighbor(_targets[next]);
                    _targets[next].AddNeighbor(_targets[index]);
                }
            }
        }

        if (_targets[0] is SphereTarget)
        {
            for (int i = 0; i < _count.Length; i++) //i ряд
            {
                for (int j = 0; j < _count[i]; j++) //j элемент в ряду
                {
                    int index = GetIndex(i, j);
                    var sphere = _targets[index] as SphereTarget;
                    sphere.SetPosition(j * 360f / _count[i], i * 180f / _count.Length - 90);
                }
            }
        }
    }

    private void OnDestroy()
    {
        Target.ChangeMotionTargetEvent -= ChangeTarget;
    }    

    private int SummCount()
    {
        int summ = 0;
        for (int i = 0; i < _count.Length; i++)
        {
            summ += _count[i];
        }
        return summ;
    }

    public int Distance()
    {
        int max = 0;
        for (int i = 0; i < _count.Length; i++)
        {
            if (max < _count[i])
            {
                max = _count[i];
            }
        }
        max += _count.Length + 2;
        return max;
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
            index += _count[r];
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
