/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * A simple toggle, fucxxxing UGUI
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-02-11
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("HYLR/UI/Toggle")]
public class UIToggle : Toggle
{
    public ToggleEvent onValueChangedInvert = new ToggleEvent();

    protected override void Awake()
    {
        base.Awake();
        onValueChanged.AddListener(b => onValueChangedInvert.Invoke(!b));
    }
}
