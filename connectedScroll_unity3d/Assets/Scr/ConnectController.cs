using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectController : MonoBehaviour
{
    /// <summary>
    /// настройки объектов по рядам для сферического расположения
    /// </summary>
    [SerializeField] RowSphere[] _rowSphere;

    /// <summary>
    /// расстановка по рядам для размещения на плоскости
    /// </summary>
    [SerializeField] RowPlane[] _rowPlane;

    /// <summary>
    /// изменение размеров при изменении угла наклона dam
    /// </summary>
    [SerializeField] AnimationCurve _sphereScale;

    [SerializeField] Cube _sphereObj;

    [SerializeField] Vector2 _deltaPositionPlane;
    [SerializeField] Vector2 _offsetPositionPlane;

    [SerializeField] Transform _leftDown;
    [SerializeField] Transform _rightUp;
    [SerializeField] Transform _centr;

    private Dictionary<Target, Target> _neighborSequence = new Dictionary<Target, Target>();
    private Queue<Target> _neighborQueue = new Queue<Target>();

    private bool _isPlane = true;

    [SerializeField] SphereTarget _targetAuto;
    private float _timeAutoRotation = -1f;
    private float MIN_TIME_AUTO_ROTATION = 2f; //спустя это время бездействия начинаем вращение _

    private void Start()
    {
        CreatePlane();
        CreateSphere();

        //сфера
        SetPositionSphere(true);
        Screed(_rowSphere);
        ScreedSphere();

        //плоскость
        SetPositionPlane(true);
        SetPlaneBorder();
        Screed(_rowPlane);
        

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

    private void Screed(RowBase[] row)
    {
        //по каждому ряду стяжка
        for (int i = 0; i < row.Length; i++) //i ряд
        {
            for (int j = 0; j < row[i].count - 1; j++) //j элемент в ряду
            {
                row[i].GetTarget(j).AddNeighbor(row[i].GetTarget(j + 1));
                row[i].GetTarget(j + 1).AddNeighbor(row[i].GetTarget(j));
            }
        }

        //стяжка между рядами
        for (int i = 0; i < row.Length - 1; i++) //i ряд
        {
            for (int j = 0; j < row[i].count; j++) //j элемент в ряду
            {
                if (j < row[i + 1].count)
                {
                    row[i].GetTarget(j).AddNeighbor(row[i + 1].GetTarget(j));
                    row[i + 1].GetTarget(j).AddNeighbor(row[i].GetTarget(j));
                }

                if (row[i].count != row[i + 1].count && row[i + 1].count - j - 1 >= 0) //с конца
                {
                    row[i].GetTarget(row[i].count - j - 1).AddNeighbor(row[i + 1].GetTarget(row[i + 1].count - j - 1));
                    row[i + 1].GetTarget(row[i + 1].count - j - 1).AddNeighbor(row[i].GetTarget(row[i].count - j - 1));
                }
            }
        }
    }

    private void ScreedSphere()
    {
        //сферическая стяжка первого и последнего элементов в каждом ряду
        for (int i = 0; i < _rowSphere.Length; i++) //i ряд
        {
            _rowSphere[i].GetTarget(0).AddNeighbor(_rowSphere[i].GetTarget(_rowSphere[i].count - 1));
            _rowSphere[i].GetTarget(_rowSphere[i].count - 1).AddNeighbor(_rowSphere[i].GetTarget(0));
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
                var target = _rowSphere[i].GetTarget(j);
                Target next;
                if (i % 2 == 0)
                {
                    next = _rowSphere[i + 1].GetTarget(j - 1 >= 0 ? j - 1 : _rowSphere[i + 1].count - 1);
                }
                else
                {
                    next = _rowSphere[i + 1].GetTarget(j + 1 < _rowSphere[i + 1].count ? j + 1 : 0);
                }

                target.AddNeighbor(next);
                next.AddNeighbor(target);
            }
        }
    }

    private void SetPositionPlane(bool immediatly)
    {
        _sphereObj.SetActive(false);
        _timeAutoRotation = Time.deltaTime;

        _neighborSequence.Clear();
        _neighborQueue.Clear();

        PlaneTarget.Enable = false;
        SphereTarget.Enable = false;

        _isPlane = true;

        float middleI = _rowPlane.Length / 2f;
        for (int i = 0; i < _rowPlane.Length; i++) //i ряд
        {            
            float y = (middleI - i) * _deltaPositionPlane.y;

            float middleX = _rowPlane[i].count / 2f;
            for (int j = 0; j < _rowPlane[i].count; j++) //j элемент в ряду
            {
                float x = (j - middleX) * _deltaPositionPlane.x;
                _rowPlane[i].GetPlane(j).SetPosition(x + _offsetPositionPlane.x, y + _offsetPositionPlane.y, immediatly);
            }
        }
    }

    private void SetPlaneBorder()
    {
        for (int i = 0; i < _rowPlane.Length; i++)
        {
            float up = _rightUp.position.y - _centr.position.y - i * _deltaPositionPlane.y;
            float down = _leftDown.position.y - _centr.position.y + (_rowPlane.Length - i) * _deltaPositionPlane.y;

            for (int j = 0; j < _rowPlane[i].count; j++)
            {
                float left = _leftDown.position.x - _centr.position.x + j * _deltaPositionPlane.x;
                float right = _rightUp.position.x - _centr.position.x - (_rowPlane[i].count - j) * _deltaPositionPlane.x;

                _rowPlane[i].planeTargets[j].SetBorder(new Vector2(left, down), new Vector2(right, up));
            }
        }
    }

    private void SetPositionSphere(bool immediatly)
    {
        _sphereObj.SetActive(true);
        _timeAutoRotation = Time.deltaTime;

        _neighborSequence.Clear();
        _neighborQueue.Clear();

        PlaneTarget.Enable = false;
        SphereTarget.Enable = false;

        _isPlane = false;

        for (int i = 0; i < _rowSphere.Length; i++) //i ряд
        {
            for (int j = 0; j < _rowSphere[i].count; j++) //j элемент в ряду
            {
                _rowSphere[i].GetSphere(j).SetPosition((j + 0.5f * (i % 2)) * 360f / _rowSphere[i].count, _rowSphere[i].damAngle, immediatly);
            }
        }
    }
    
    public Vector3 GetScale(float dam)
    {
        return _sphereScale.Evaluate(dam) * Vector3.one;
    }

    private int SummCount(RowBase[] row)
    {
        int summ = 0;
        for (int i = 0; i < row.Length; i++)
        {
            summ += row[i].count;
        }
        return summ;
    }

    private void ChangeTarget(Target obj)
    {
        if (obj == null)
        {
            _timeAutoRotation = Time.deltaTime;
            return;
        }

        _timeAutoRotation = -1f;

        CreateNeighborSequence(obj);
    }

    private void CreateNeighborSequence(Target obj)
    {
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

    private Target GetFirstNeighborSequence()
    {
        if (_neighborSequence.Count == 0)
        {
            CreateNeighborSequence(_targetAuto);
            return _targetAuto;
        }

        foreach (var element in _neighborSequence)
        {
            if (element.Value == null)
            {
                return element.Key;
            }
        }

        return null;
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
        if (_timeAutoRotation >= 0)
        {
            _timeAutoRotation += Time.deltaTime;
        }

        if (_timeAutoRotation > MIN_TIME_AUTO_ROTATION && SphereTarget.Enable && !_isPlane)
        {
            var target = GetFirstNeighborSequence();
            if (target != null)
            {
                target.AutoRotate();
            }
        }

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

    private void DoubleClick(Target target, bool needWait)
    {
        Debug.Log("DOUBLE  " + target.gameObject.name);
        StopClick();
        if (_isPlane)
        {

        }
        else
        {
            var sphere = target as SphereTarget;
            sphere.StartCoroutine(sphere.Rotate(CameraController.TIME_MOTION));
        }
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
        //_neighborSequence.Clear();
        //_neighborQueue.Clear();

        PlaneTarget.Enable = false;
        SphereTarget.Enable = false;
    }

    public Target GetNext(Target target)
    {
        if (target is PlaneTarget)
        {
            return GetNext(target, _rowPlane);
        }
        else
        {
            return GetNext(target, _rowSphere);
        }
    }

    private static Target GetNext(Target target, RowBase[] row)
    {
        bool next = false;
        for (int i = 0; i < row.Length; i++)
        {
            for (int j = 0; j < row[i].count; j++)
            {
                if (next)
                {
                    return row[i].GetTarget(j);
                }

                if (row[i].GetTarget(j) == target)
                {
                    next = true;
                }
            }
        }
        return row[0].GetTarget(0);
    }

    public Target GetPrev(Target target)
    {
        if (target is PlaneTarget)
        {
            return GetPrev(target, _rowPlane);
        }
        else
        {
            return GetPrev(target, _rowSphere);
        }
    }

    private static Target GetPrev(Target target, RowBase[] row)
    {
        Target prev = null;
        for (int i = 0; i < row.Length; i++)
        {
            for (int j = 0; j < row[i].count; j++)
            {
                if (prev != null && row[i].GetTarget(j) == target)
                {
                    return prev;
                }

                prev = row[i].GetTarget(j);
            }
        }

        return prev;
    }    

    private void CreateSphere()
    {
        var sphere = gameObject.GetComponentsInChildren<SphereTarget>();
        int index = 0;
        for (int i = 0; i < _rowSphere.Length; i++)
        {
            _rowSphere[i].sphereTargets = new SphereTarget[_rowSphere[i].setCount];
            for (int j = 0; j < _rowSphere[i].count; j++)
            {
                _rowSphere[i].sphereTargets[j] = sphere[index];
                index++;
            }
        }
    }

    private void CreatePlane()
    {
        var plane = gameObject.GetComponentsInChildren<PlaneTarget>();
        int index = 0;
        for (int i = 0; i < _rowPlane.Length; i++)
        {
            _rowPlane[i].planeTargets = new PlaneTarget[_rowPlane[i].setCount];
            for (int j = 0; j < _rowPlane[i].count; j++)
            {
                _rowPlane[i].planeTargets[j] = plane[index];
                index++;
            }
        }
    }
}

public abstract class RowBase
{
    public abstract int count { get; }
    public abstract Target GetTarget(int index);
    public int setCount;
}

[Serializable]
public class RowPlane : RowBase
{
    [HideInInspector]
    public PlaneTarget[] planeTargets;

    public override int count => planeTargets.Length;

    public override Target GetTarget(int index)
    {
        return planeTargets[index];
    }

    public PlaneTarget GetPlane(int index)
    {
        return planeTargets[index];
    }
}

[Serializable]
public class RowSphere : RowBase
{
    public int damAngle;

    [HideInInspector]
    public SphereTarget[] sphereTargets;

    public override int count => sphereTargets.Length;

    public override Target GetTarget(int index)
    {
        return sphereTargets[index];
    }

    public SphereTarget GetSphere(int index)
    {
        return sphereTargets[index];
    }
}

