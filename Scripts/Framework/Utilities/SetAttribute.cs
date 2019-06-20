/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Custom editor get/set serializeable field
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-09-21
 * 
 ***************************************************************************************************/

using UnityEngine;

public sealed class SetAttribute : PropertyAttribute
{
    public readonly string name;

    public SetAttribute(string _name)
    {
        name = _name;
    }
}