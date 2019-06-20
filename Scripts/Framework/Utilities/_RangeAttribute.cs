/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Custom editor range get/set serializeable field
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-01-19
 * 
 ***************************************************************************************************/

using UnityEngine;

public sealed class _RangeAttribute : PropertyAttribute
{
    public bool dirty;
    public readonly string name;
    public float min;
	public float max;

    public _RangeAttribute(float _min, float _max, string _name = "")
    {
        name = _name;

		min = _min;
		max = _max;
    }
}