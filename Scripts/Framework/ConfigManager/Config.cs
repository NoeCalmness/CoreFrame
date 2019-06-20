/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Base config definition.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-14
 * 
 ***************************************************************************************************/

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[AttributeUsage(AttributeTargets.Class)]
public class ConfigSortOrder : Attribute { public ConfigSortOrder(int _order) { order = _order; } public int order = 0; }

public abstract class ConfigItem
{
    public int ___source = -1;

    public int hash { get; set; }
    public int ID;

    /// <summary>
    /// Called when ConfigItem created from asset
    /// </summary>
    public virtual void OnLoad() { }
    /// <summary>
    /// Called before ConfigItem.OnInitialize, used to create and initialize static datas
    /// </summary>
    public virtual void InitializeOnce() { }
    /// <summary>
    /// Called after all ConfigItem.InitializeOnce
    /// </summary>
    public virtual void Initialize() { }

    public static implicit operator bool(ConfigItem c)
    {
        return c != null;
    }

    public virtual ConfigItem Clone()
    {
        return MemberwiseClone() as ConfigItem;
    }

    public T Clone<T>() where T : ConfigItem
    {
        if (typeof(T) != GetType()) return null;
        return MemberwiseClone() as T;
    }
}

public abstract class Config : ScriptableObject
{
    public virtual void CopyTo(Config c, bool merge = false)
    {
        if (!c || GetType() != c.GetType())
            return;

        var items = GetItemsBase();

        if (merge)
        {
            var oi = c.GetItemsBase();
            foreach (var o in oi)
            {
                if (FindItemBase(i => i.___source == o.___source)) continue;
                items.Add(o);
            }
        }
        c.name   = name;

        items.Sort((a, b) => a.ID < b.ID ? -1 : 1);

        c.SetItems(items);
    }

    public virtual int GetFreeID()
    {
        for (int i = 1, c = int.MaxValue; i < c; ++i)
        {
            if (FindItemBase(i)) continue;
            return i;
        }
        return -1;
    }

    /// <summary>
    /// Sort order order when initialize, default 0, lower first
    /// </summary>
    public virtual int orderID{ get; private set; }

    public abstract int hash { get; }
    public abstract Type itemType { get; }
    public abstract ConfigItem FindItemBase(int id);
    public abstract ConfigItem FindItemBase(Predicate<ConfigItem> match);
    public abstract List<ConfigItem> GetItemsBase();
    public abstract void SetItems(IEnumerable _items);
    public abstract void ForEach(Action<ConfigItem> action);

    public Config()
    {
        var t = itemType.GetCustomAttribute<ConfigSortOrder>();
        orderID = t != null ? t.order : 0;
    }
}

public abstract class Config<T> : Config where T : ConfigItem
{
    public List<T> items = new List<T>();

    public override int hash { get { return itemType.GetHashCode(); } }
    public override Type itemType { get { return typeof(T); } }
    public override ConfigItem FindItemBase(int id) { return items.Find(i => i.ID == id); }
    public override ConfigItem FindItemBase(Predicate<ConfigItem> match) { return items.Find(match); }
    public override List<ConfigItem> GetItemsBase() { return items.ConvertAll(c => c as ConfigItem); }

    public override void SetItems(IEnumerable _items)
    {
        items.Clear();
        foreach (var _item in _items)
        {
            var item = _item as T;
            items.Add(item);
        }
    }

    public override void ForEach(Action<ConfigItem> action)
    {
        items.ForEach(action);
    }

    public T FindItem(int id) { return items.Find(i => i.ID == id); }
    public T FindItem(Predicate<T> match) { return items.Find(match); }
    public List<T> GetItems() { return items; }
}
