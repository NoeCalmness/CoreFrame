/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Generate random event
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-11-01
 * 
 ***************************************************************************************************/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("HYLR/UI/Random Event")]
public class UIRandomEvent : MonoBehaviour
{
    [Serializable] public class RandomEventInt    : UnityEvent<int>    { public RandomEventInt() { } }   
    [Serializable] public class RandomEventFloat  : UnityEvent<float>  { public RandomEventFloat() { } } 
    [Serializable] public class RandomEventString : UnityEvent<string> { public RandomEventString() { } }

    public RandomEventInt onEventInt { get { return m_onEventInt; } }
    public RandomEventInt onEventFloat { get { return m_onEventInt; } }
    public RandomEventInt onEventString { get { return m_onEventInt; } }

    [SerializeField] private RandomEventInt    m_onEventInt    = new RandomEventInt();
    [SerializeField] private RandomEventFloat  m_onEventFloat  = new RandomEventFloat();
    [SerializeField] private RandomEventString m_onEventString = new RandomEventString();

    public float min = 0;
    public float max = 1;

    [Space(10)]
    public bool  autoStart = true;
    public bool  oneShot   = false;
    public bool  isInteger = true;
    public float delay     = 5;

    private Coroutine m_execute = null;
    private bool m_firstEnable = true;

    private void OnEnable()
    {
        if (autoStart && (m_firstEnable || !oneShot))
            Execute();

        m_firstEnable = false;
    }

    public void Execute()
    {
        if (m_execute != null) StopCoroutine(m_execute);
        m_execute = StartCoroutine(ExecuteDelay());
    }

    public void ExecuteImmediately()
    {
        if (m_execute != null) StopCoroutine(m_execute);

        var r = GenerateRandom();

        m_onEventInt.Invoke((int)r);
        m_onEventFloat.Invoke(r);
        m_onEventString.Invoke(r.ToString());
    }

    public float GenerateRandom()
    {
        var mi = min > max ? max : min;
        var ma = min > max ? min : max;
        var r = isInteger ? UnityEngine.Random.Range((int)mi, (int)ma) : UnityEngine.Random.Range(min, max);

        return r;
    }

    private IEnumerator ExecuteDelay()
    {
        if (delay > 0)
        {
            var wait = new WaitForSeconds(delay);
            yield return wait;
        }

        ExecuteImmediately();
    }
}
