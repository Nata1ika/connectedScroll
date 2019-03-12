using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    [SerializeField] Transform[] _obj;
    [SerializeField] Parent[] _parents;

    private void Start()
    {
        SphereTarget.DeltaRotationEvent += DeltaRotation;
        UpdateParent();
    }

    private void OnDestroy()
    {
        SphereTarget.DeltaRotationEvent -= DeltaRotation;
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    private void DeltaRotation(float obj)
    {
        transform.eulerAngles = transform.eulerAngles + obj * Vector3.up;
        UpdateParent();
    }

    private void UpdateParent()
    {
        for (int i = 0; i < _obj.Length; i++)
        {
            for (int j = 0; j < _parents.Length; j++)
            {
                if (_obj[i].transform.position.z < _parents[j].value)
                {
                    _obj[i].SetParent(_parents[j].transform);
                    break;
                }
            }
        }
    }

    /*
    private void Update()
    {
        UpdateParent();
    }
    */
}
