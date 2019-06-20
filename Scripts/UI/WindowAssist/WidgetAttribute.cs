// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-11-02      13:31
//  *LastModify：2019-02-20      11:45
//  ***************************************************************************************************/

#region

using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

#endregion

[AttributeUsage(AttributeTargets.Field)]
public class WidgetAttribute : Attribute
{
    public bool CheckEnable;
    public bool Enable;
    public string Path;

    public WidgetAttribute(string rPath, bool rEnable)
    {
        Path = rPath;
        Enable = rEnable;
        CheckEnable = true;
    }

    public WidgetAttribute(string rPath)
    {
        Path = rPath;
        CheckEnable = false;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class ArrayWidgetAttribute : Attribute
{
    public bool Child;
    public string[] Path;

    public ArrayWidgetAttribute(params string[] paths)
    {
        Path = paths;
        Child = false;
    }

    public ArrayWidgetAttribute(bool child, params string[] paths)
    {
        Path = paths;
        Child = child;
    }
}


public abstract class Window_BindWidget : LogicObject
{
    public static void BindWidget(object rObj, Transform t)
    {
        if (!t)
        {
            Logger.LogError("Bind Widget Root is null");
            return;
        }
        var fields = rObj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        for (var i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            var att = field.GetCustomAttribute<WidgetAttribute>();
            if (att != null)
            {
                var node = t.Find(att.Path);
                if (node == null)
                {
                    Logger.LogError("cannot find path = {0}", att.Path);
                    continue;
                }
                var com = node.GetComponent(field.FieldType);
                if (!com)
                {
                    Logger.LogError("the path  has no Widget, path = {0}  type = {1} ", att.Path, field.FieldType);
                    return;
                }
                if (att.CheckEnable)
                {
                    com.SafeSetActive(att.Enable);
                }
                field.SetValue(rObj, com);
            }
            else if (typeof(IList).IsAssignableFrom(field.FieldType))
            {
                var array = field.GetCustomAttribute<ArrayWidgetAttribute>();
                if (array != null)
                {
                    var ilist = (field.FieldType.IsArray ? Activator.CreateInstance(field.FieldType, array.Path.Length) : Activator.CreateInstance(field.FieldType)) as IList;
                    for (var m = 0; m < array.Path.Length; m++)
                    {
                        var node = t.Find(array.Path[m]);
                        if (node == null)
                        {
                            Logger.LogError("cannot find path = {0}", array.Path[m]);
                            continue;
                        }
                        var type = field.FieldType.GetElementType();
                        if (type == null) type = field.FieldType.GetGenericArguments()[0];

                        if (array.Child)
                        {
                            for (var n = 0; n < node.childCount; n++)
                            {
                                var com = node.GetChild(n).GetComponent(type);
                                if (com)
                                {
                                    if (field.FieldType.IsArray)
                                        ilist[m] = com;
                                    else
                                        ilist?.Add(com);
                                }
                            }
                        }
                        else
                        {
                            var com = node.GetComponent(type);
                            if (com)
                            {
                                if (field.FieldType.IsArray)
                                    ilist[m] = com;
                                else
                                    ilist?.Add(com);
                            }
                        }
                    }
                    field.SetValue(rObj, ilist);
                }
            }
        }
    }
}