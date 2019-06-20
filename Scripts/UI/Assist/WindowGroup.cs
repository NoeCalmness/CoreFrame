// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 用于管理Toggle切换子窗口
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-12-15      13:54
//  *LastModify：2018-12-15      17:37
//  ***************************************************************************************************/

#region

using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;

#endregion

public class WindowGroup : IDisposable
{
    public WindowSwitch onWindowSwitch;

    private readonly Dictionary<ValueType, Tuple<Toggle, SubWindowBase, object[]>> _enumWindowMap = new Dictionary<ValueType, Tuple<Toggle, SubWindowBase, object[]>>();
    private readonly Dictionary<Toggle, ValueType>                                 _toggleTypeMap = new Dictionary<Toggle, ValueType>();

    private ValueType _currentType;

    public WindowGroup(ToggleGroup rGroup)
    {
        rGroup?.onAnyToggleStateOn.AddListener(OnAnyToggleOn);
        _toggleTypeMap.Clear();
        _enumWindowMap.Clear();
    }

    /// <summary>
    /// 销毁所有窗口
    /// </summary>
    public void Dispose()
    {
        foreach (var kv in _enumWindowMap)
        {
            kv.Value?.Item2?.Destroy();
        }

        _enumWindowMap.Clear();
        _toggleTypeMap.Clear();
    }

    public T GetWindow<T>() where T : SubWindowBase
    {
        foreach (var kv in _enumWindowMap)
        {
            if (kv.Value?.Item2?.GetType() == typeof (T))
                return kv.Value.Item2 as T;
        }
        return default(T);
    }

    public SubWindowBase GetWindow(ValueType rEnum)
    {
        if (_enumWindowMap.ContainsKey(rEnum))
            return _enumWindowMap[rEnum].Item2;
        return null;
    }

    public void Registe(ValueType rEnum, Toggle rToggle, SubWindowBase rWindow, params object[] args)
    {
        if (null == rToggle)
        {
            Logger.LogError("Toggle cannot be null");
            return;
        }

        if (null == rWindow)
        {
            Logger.LogError("Window cannot be null");
            return;
        }

        if (_toggleTypeMap.ContainsKey(rToggle))
        {
            Logger.LogError("Toggle 重复注册了！，每个Toggle只能注册一个Window");
            return;
        }

        _toggleTypeMap.Add(rToggle, rEnum);
        _enumWindowMap.Add(rEnum, Tuple.Create(rToggle, rWindow, args));
    }

    public bool UnRegiste(ValueType rEnum)
    {
        if (null == rEnum)
            return false;

        foreach (var kv in _toggleTypeMap)
        {
            if (Equals(kv.Value, rEnum))
            {
                _toggleTypeMap.Remove(kv.Key);
                break;
            }
        }

        return _enumWindowMap.Remove(rEnum);
    }

    public void SwitchWindow(ValueType rEnum)
    {
        if (_currentType != null && _enumWindowMap.ContainsKey(_currentType))
        {
            var t = _enumWindowMap[_currentType];
            t?.Item2.UnInitialize();
        }

        if (_enumWindowMap.ContainsKey(rEnum))
        {
            var t = _enumWindowMap[rEnum];
            if (t != null)
            {
                t.Item1.isOn = true;
                t.Item2.Initialize(t.Item3);
                onWindowSwitch?.Invoke(t.Item2);
            }
        }
        _currentType = rEnum;
    }

    private void OnAnyToggleOn(Toggle rToggle)
    {
        if (_toggleTypeMap.ContainsKey(rToggle))
            SwitchWindow(_toggleTypeMap[rToggle]);
    }

    public class WindowSwitch : UnityEvent<SubWindowBase>
    {
        
    }
}
