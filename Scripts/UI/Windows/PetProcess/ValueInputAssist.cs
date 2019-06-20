// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-08-30      18:20
//  * LastModify：2018-10-31      10:04
//  ***************************************************************************************************/

#region

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#endregion

public class ValueInputAssist : LogicObject
{
    private Button          _add;
    private InputField      _input;
    private Button          _minus;
    private Text            _text;
    private bool            addPress;

    private AnimationCurve  animCurve;
    private float           fValue;
    private int             limit_min;
    private bool            minusPress;

    /// <summary>
    /// 值溢出回调
    /// </summary>
    public Action           OnOverFlow;

    public UnityAction<int> OnValueChange;
    private float           timer;
    private int             value;
    private ValueInputAssist() { }
    public uint Value { get { return (uint)value; } }
    public int LimitMax { get; private set; }

    private void Initialize()
    {
        if (_minus)
        {
            _minus.onPressed().AddListener(OnMinusPress);
            _minus.onClick.AddListener(OnMinusClick);
        }
        if (_add)
        {
            _add.onPressed().AddListener(OnAddPress);
            _add.onClick.AddListener(OnAddClick);
        }
        if(_input)
            _input.onValueChanged.AddListener(OnInputValueChange);

        InitAnimationCurve();
        Process(0);
    }

    public override void OnRootUpdate(float diff)
    {
        base.OnRootUpdate(diff);
        if (minusPress || addPress)
        {
            var v = animCurve.Evaluate(Time.time - timer);
            if(minusPress)
                Process(-v);
            else if(addPress)
                Process(v);
        }
    }

    /// <summary>
    /// 设定初始值
    /// </summary>
    /// <param name="rValue">值</param>
    public void SetValue(int rValue)
    {
        fValue = Mathf.Clamp(rValue, limit_min, LimitMax);
        value = (int)fValue;

        if (_input)
            _input.text = value.ToString();
        else
        {
            if (_text)
                _text.text = value.ToString();
            OnValueChange?.Invoke(value);
        }
    }

    public void SetValue(int rValue, int rMin, int rMax)
    {
        limit_min = Mathf.Min(rMin, rMax);
        LimitMax = Mathf.Max(rMin, rMax);
        fValue = Mathf.Clamp(rValue, limit_min, LimitMax);
        value = (int)fValue;

        if (_input)
            _input.text = value.ToString();
        else
        {
            if (_text)
                _text.text = value.ToString();
            OnValueChange?.Invoke(value);
        }
    }

    private void OnAddClick()
    {
        Process(1);
    }

    private void OnMinusClick()
    {
        Process(-1);
    }

    public void SetLimit(int min, int max)
    {
        LimitMax = Mathf.Max(min, max);
        limit_min = Mathf.Min(min, max);
        fValue = value = Mathf.Clamp(value, min, max);
        ProcessFixed(fValue);
    }

    private void InitAnimationCurve()
    {
        animCurve = new AnimationCurve();
        animCurve.AddKey(0, 0.1f);
        animCurve.AddKey(1, 0.5f);
        animCurve.AddKey(2, 1.0f);
        animCurve.AddKey(4, 5.0f);
    }

    private void OnInputValueChange(string s)
    {
        //数据有效检验
        value = Convert.ToInt32(s);
        ProcessFixed(value);
        if (s != Value.ToString())
            _input.textComponent.text = Value.ToString();
        else
            OnValueChange?.Invoke((int)Value);
    }

    private void OnAddPress(bool rPress)
    {
        addPress = rPress;
        if (rPress)
            timer = Time.time;
        else
            return;
        var v = animCurve.Evaluate(Time.time - timer);
        Process(v);
    }

    private void OnMinusPress(bool rPress)
    {
        minusPress = rPress;
        if (rPress)
            timer = Time.time;
        else
            return;
        var v = animCurve.Evaluate(Time.time - timer);
        Process(-v);
    }

    private void ProcessFixed(float v)
    {
        fValue = v;
        fValue = Mathf.Clamp(fValue, limit_min, LimitMax);
        if (fValue < v)
            OnOverFlow?.Invoke();

        value = (int)fValue;
        //input不为null就由input去回调
        if (_input)
            _input.text = value.ToString();
        else
        {
            if (_text)
                _text.text = value.ToString();
            OnValueChange?.Invoke(value);
        }
    }

    private void Process(float v)
    {
        fValue += v;
        ProcessFixed(fValue);
    }


    public static ValueInputAssist Create(Button minus, Button add, InputField input)
    {
        var valueInput = _Create<ValueInputAssist>();
        valueInput._minus   = minus;
        valueInput._add     = add;
        valueInput._input   = input;
        valueInput._text    = input?.textComponent;
        valueInput.limit_min = int.MinValue;
        valueInput.LimitMax = int.MaxValue;
        valueInput.value    = 0;
        valueInput.Initialize();
        return valueInput;
    }

    public static ValueInputAssist Create(Button minus, Button add, Text text)
    {
        var valueInput = _Create<ValueInputAssist>();
        valueInput._minus   = minus;
        valueInput._add     = add;
        valueInput._input   = null;
        valueInput._text    = text;
        valueInput.limit_min = int.MinValue;
        valueInput.LimitMax = int.MaxValue;
        valueInput.value    = 0;
        valueInput.Initialize();
        return valueInput;
    }
}
