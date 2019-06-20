/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Extended methods.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-01
 * 
 ***************************************************************************************************/

using System.Security.Cryptography;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System;
using System.IO;

public static partial class ExtendedMethods
{
    private static MD5CryptoServiceProvider m_md5Provider = new MD5CryptoServiceProvider();

    public static bool isUpperCase(this char c)
    {
        return c >= 'A' && c <= 'Z';
    }

    public static TValue GetDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TValue : new()
    {
        TValue value;
        if (!dict.TryGetValue(key, out value))
        {
            value = new TValue();
            dict.Add(key, value);
        }
        return value;
    }

    public static TValue GetDefault<TKey, TValue>(this SortedDictionary<TKey, TValue> dict, TKey key) where TValue : new()
    {
        TValue value;
        if (!dict.TryGetValue(key, out value))
        {
            value = new TValue();
            dict.Add(key, value);
        }
        return value;
    }

    public static TValue Get<Tkey, TValue>(this Dictionary<Tkey, TValue> dict, Tkey key)
    {
        TValue value;
        dict.TryGetValue(key, out value);
        return value;
    }

    public static TValue Get<Tkey, TValue>(this Dictionary<Tkey, TValue> dict, Tkey key, TValue def)
    {
        TValue value;
        return dict.TryGetValue(key, out value) ? value : def;
    }

    public static TValue Get<TValue>(this Dictionary<string, TValue> dict, string key, bool ignoreCase)
    {
        if (!ignoreCase)
        {
            TValue value;
            dict.TryGetValue(key, out value);
            return value;
        }
        foreach (var pair in dict) if (string.Compare(pair.Key, key, StringComparison.OrdinalIgnoreCase) == 0) return pair.Value;
        return default(TValue);
    }

    public static TValue Get<TValue>(this Dictionary<string, TValue> dict, string key, TValue def, bool ignoreCase)
    {
        if (!ignoreCase)
        {
            TValue value;
            return dict.TryGetValue(key, out value) ? value : def;
        }
        foreach (var pair in dict) if (string.Compare(pair.Key, key, StringComparison.Ordinal) == 0) return pair.Value;
        return def;
    }

    public static void Set<Tkey, TValue>(this Dictionary<Tkey, TValue> dict, Tkey key, TValue val)
    {
        if (dict.ContainsKey(key)) dict[key] = val;
        else dict.Add(key, val);
    }

    public static void CopyTo<T>(this T[] arr, ref T[] tar) where T : PacketObject<T>
    {
        if (tar == null) tar = new T[arr.Length];
        else
        {
            PacketObject.BackArray(tar);
            Array.Resize(ref tar, arr.Length);
        }

        for (var i = 0; i < arr.Length; ++i) tar[i] = arr[i].Clone();
    }

    public static bool pressed(this Button button)
    {
        if (!button) return false;
        var press = button.GetComponent<UIPressButton>();
        return press && press.pressed;
    }

    public static void SetPressDelay(this Button button, float delay)
    {
        if (!button) return;
        var press = button.GetComponent<UIPressButton>();
        if (!press) press = button.gameObject.AddComponent<UIPressButton>();
        press.pressDelay = delay;
    }

    public static void SetPressEnabled(this Button button, bool enabled, float delay = 0.5f)
    {
        if (!button) return;
        var press = button.GetComponent<UIPressButton>();
        if (!press && enabled) press = button.gameObject.AddComponent<UIPressButton>();
        if (press)
        {
            if (enabled) press.pressDelay = delay;
            else press.enabled = false;
        }
    }

    public static UIPressButton.ButtonPressedEvent onPressed(this Button button)
    {
        if (!button) return null;
        var press = button.GetComponent<UIPressButton>();
        if (!press) press = button.gameObject.AddComponent<UIPressButton>();
        return press.onPressed;
    }

    public static UIInputListener.KeyEvent keyEvent(this Selectable sel)
    {
        if (!sel) return null;
        var ls = sel.GetComponent<UIInputListener>();
        if (!ls) ls = sel.gameObject.AddComponent<UIInputListener>();
        return ls.keyEvent;
    }

    public static string GetMD5(this string str, bool upper = false)
    {
#if UNITY_EDITOR
        m_md5Provider = new MD5CryptoServiceProvider();
#endif
        var hash = m_md5Provider.ComputeHash(Encoding.Default.GetBytes(str));

        var md5 = "";
        for (var i = 0; i < hash.Length; ++i)
            md5 += hash[i].ToString(upper ? "X" : "x").PadLeft(2, '0');

        return md5;
    }

    public static string GetMD5(this FileStream file, bool upper = false)
    {
#if UNITY_EDITOR
        m_md5Provider = new MD5CryptoServiceProvider();
#endif
        var hash = m_md5Provider.ComputeHash(file);

        var md5 = "";
        for (var i = 0; i < hash.Length; ++i)
            md5 += hash[i].ToString(upper ? "X" : "x").PadLeft(2, '0');

        return md5;
    }

    public static string GetMD5(this byte[] data, bool upper = false, int offset = 0, int count = -1)
    {
#if UNITY_EDITOR
        m_md5Provider = new MD5CryptoServiceProvider();
#endif
        var hash = m_md5Provider.ComputeHash(data, offset, count < 0 ? data.Length : count);

        var md5 = "";
        for (var i = 0; i < hash.Length; ++i)
            md5 += hash[i].ToString(upper ? "X" : "x").PadLeft(2, '0');

        return md5;
    }

    public static int FindIndex<T>(this T[] arr, Predicate<T> match)
    {
        for (var i = 0; i < arr.Length; ++i)
            if (match(arr[i])) return i;
        return -1;
    }

    public static int FindIndex<T>(this T[] arr, int start, Predicate<T> match)
    {
        if (start < 0) start = 0;
        for (; start < arr.Length; ++start)
            if (match(arr[start])) return start;
        return -1;
    }

    public static int FindIndex<T>(this T[] arr, int start, int end, Predicate<T> match)
    {
        if (start < 0) start = 0;
        if (end > arr.Length) end = arr.Length;
        for (var i = start; i < end; ++i)
            if (match(arr[i])) return i;
        return -1;
    }

    public static int FindLastIndex<T>(this T[] arr, Predicate<T> match)
    {
        for (int i = arr.Length - 1; i > -1; --i)
            if (match(arr[i])) return i;
        return -1;
    }

    public static T[] SimpleClone<T>(this T[] arr)
    {
        var r = new T[arr.Length];
        for (var i = 0; i < arr.Length; ++i) r[i] = arr[i];
        return r;
    }

    public static void Foreach<T>(this T[] arr, Func<T, bool> call, int count = -1)
    {
        if (count < 0 || count > arr.Length) count = arr.Length;
        for (var i = 0; i < count; ++i)
            if (call(arr[i])) return;
    }

    public static bool Contains<T>(this T[] arr, T element)
    {
        foreach (var e in arr)
            if (e.Equals(element)) return true;
        return false;
    }

    public static bool Contains(this string[] arr, string value, bool ignoreCase = false)
    {
        foreach (var s in arr)
            if (s.Equals(value, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture)) return true;
        return false;
    }

    public static string PrettyPrint<T>(this T[] arr, char split = ' ')
    {
        if (arr == null || arr.Length < 1) return string.Empty;
        var ss = string.Empty;
        for (var i = 0; i < arr.Length; ++i) { ss += arr[i] != null ? arr[i].ToString() : "null"; if (i != arr.Length - 1) ss += split; }
        return ss;
    }

    public static string PrettyPrint<T>(this List<T> arr, char split = ' ')
    {
        if (arr == null || arr.Count < 1) return string.Empty;
        var ss = string.Empty;
        for (var i = 0; i < arr.Count; ++i) { ss += arr[i] != null ? arr[i].ToString() : "null"; if (i != arr.Count - 1) ss += split; }
        if (ss.Length > 0) ss.Remove(ss.Length - 1);
        return ss;
    }

    public static void Clear(this Array arr)
    {
        Array.Clear(arr, 0, arr.Length);
    }

    public static void Clear<T>(this T[] arr, bool destroy) where T : IDestroyable
    {
        if (arr == null || arr.Length < 1) return;

        if (destroy)
        {
            foreach (var o in arr)
            {
                if (o == null) continue;
                o.Destroy();
            }
        }

        Array.Clear(arr, 0, arr.Length);
    }

    public static void Distinct<T>(this List<T> list)
    {
        for (int i = 0, c = list.Count; i < c; ++i)
        {
            var n = list[i];
            var ii = -1;
            while ((ii = list.LastIndexOf(n)) != i && ii > -1) { list.RemoveAt(ii); --c; }
        }
    }

    public static void Clear<T>(this List<T> list, bool destroy) where T : IDestroyable
    {
        if (list == null || list.Count < 1) return;

        if (destroy)
        {
            foreach (var o in list)
            {
                if (o == null) continue;
                o.Destroy();
            }
        }

        list.Clear();
    }

    public static int RemoveAll<T>(this List<T> list, Predicate<T> match, bool destroy) where T : IDestroyable
    {
        if (list == null || list.Count < 1) return 0;

        if (destroy)
        {
            var c = 0;
            for (var i = list.Count - 1; i > -1; --i)
            {
                var o = list[i];
                if (!match(o)) continue;

                list.RemoveAt(i);
                o?.Destroy();

                ++c;
            }
            return c;
        }
        else return list.RemoveAll(match);
    }

    public static bool Remove<T>(this List<T> list, T item, bool destroy) where T : IDestroyable
    {
        if (list == null || list.Count < 1) return false;
        var r = list.Remove(item);
        if (destroy) item?.Destroy();
        return r;
    }

    public static void RemoveAt<T>(this List<T> list, int index, bool destroy) where T : IDestroyable
    {
        if (list == null || list.Count < 1 || index < 0 || index >= list.Count) return;

        var item = list[index];
        list.RemoveAt(index);

        if (destroy) item?.Destroy();
    }

    public static void RemoveRange<T>(this List<T> list, int index, int count, bool destroy) where T : IDestroyable
    {
        if (list == null || list.Count < 1) return;

        if (destroy)
        {
            for (var i = index + count - 1; i >= index; --i)
            {
                if (i < 0 || i >= list.Count) break;
                var item = list[i];
                list.RemoveAt(i);
                item?.Destroy();
            }
        }
        else list.RemoveRange(index, count);
    }

    public static void AddRange(this List<string> list, IEnumerable<string> elements, bool lower = false)
    {
        if (elements == null) return;
        if (!lower) list.AddRange(elements);
        else
        {
            foreach (var element in elements)
            {
                if (string.IsNullOrEmpty(element)) continue;
                list.Add(element.ToLower());
            }
        }
    }

    public static bool SequenceEqual<T> (this List<T> list, List<T> list1)
    {
        if (list == list1) return true;

        if (list.Count != list1.Count) return false;
        for (int i = 0, c = list.Count; i < c; ++i) if (!list[i].Equals(list1[i])) return false;
        return true;
    }

    public static T[] Distinct<T>(this T[] arr)
    {
        var c = arr.Length;
        for (int i = 0; i < c; ++i)
        {
            var n = arr[i];
            var ii = -1;
            while ((ii = Array.LastIndexOf(arr, n)) != i && ii > -1 && ii < c)
            {
                for (var j = ii; j < c - 1; ++j) arr[j] = arr[j + 1];
                --c;
            }
        }
        if (c != arr.Length) Array.Resize(ref arr, c);
        return arr;
    }

    public static T[] Remove<T>(this T[] arr, T element)
    {
        if (arr == null || arr.Length < 1) return arr;
        var idx = Array.IndexOf(arr, element);
        if (idx < 0) return arr;

        for (int i = idx, c = arr.Length - 1; i < c; ++i)
            arr[i] = arr[i + 1];

        Array.Resize(ref arr, arr.Length - 1);

        return arr;
    }

    public static T[] Add<T>(this T[] arr, T element)
    {
        if (arr == null) return arr;
        Array.Resize(ref arr, arr.Length + 1);
        arr[arr.Length - 1] = element;
        return arr;
    }

    public static void Push<T>(this T[] arr, T element)
    {
        if (arr == null) return;

        for (var i = arr.Length - 1; i > 0; --i) arr[i] = arr[i - 1];
        arr[0] = element;
        return;
    }

    public static bool PushDistinct<T>(this T[] arr, T element, int tail)
    {
        if (arr == null) return false;

        if (tail < 0) tail = arr.Length - 1;
        var idx = Array.IndexOf(arr, element, 0, tail);
        if (idx > -1) tail = idx;

        for (; tail > 0; --tail) arr[tail] = arr[tail - 1];
        arr[0] = element;

        return idx < 0;
    }

    public static bool SequenceEqual<T> (this T[] arr, T[] arr1)
    {
        if (arr == arr1) return true;

        if (arr.Length != arr1.Length) return false;
        for (int i = 0, c = arr.Length; i < c; ++i) if (!arr[i].Equals(arr1[i])) return false;
        return true;
    }

    public static RectTransform rectTransform(this GameObject obj)
    {
        return obj.transform as RectTransform;
    }

    public static RectTransform rectTransform(this Component component)
    {
        return component.transform as RectTransform;
    }

    public static string GetPath(this Component component)
    {
        var path = component.name;
        var node = component.transform.parent;
        while (node)
        {
            path = node.name + "/" + path;
            node = node.parent;
        }
        return path;
    }

    public static string GetPath(this Component component, string root)
    {
        var path = component.name;
        var node = component.transform.parent;
        while (node && node.name != root)
        {
            path = node.name + "/" + path;
            node = node.parent;
        }
        return path;
    }

    public static T GetComponentDefault<T>(this Component component) where T : Component
    {
        var c = component.GetComponent<T>();
        if (!c) c = component.gameObject.AddComponent<T>();
        return c;
    }

    public static T GetComponentDefault<T>(this GameObject obj) where T : Component
    {
        var c = obj.GetComponent<T>();
        if (!c) c = obj.AddComponent<T>();
        return c;
    }

    public static T GetComponentDefault<T>(this Transform t) where T : Component
    {
        var c = t?.gameObject.GetComponent<T>();
        if (!c) c = t.gameObject.AddComponent<T>();
        return c;
    }

    public static Component GetComponentDefault(this Component component, Type type)
    {
        var c = component.GetComponent(type);
        if (!c) c = component.gameObject.AddComponent(type);
        return c;
    }

    public static Component GetComponentDefault(this GameObject obj, Type type)
    {
        var c = obj.GetComponent(type);
        if (!c) c = obj.AddComponent(type);
        return c;
    }

    public static T GetComponentDefault<T>(this GameObject obj, string path) where T : Component
    {
        var child = obj.transform.Find(path);
        if (child) return child.GetComponentDefault<T>();
        return null;
    }

    public static T GetComponentDefault<T>(this Transform transform, string path) where T : Component
    {
        var obj = transform.Find(path);
        if (obj) return obj.GetComponentDefault<T>();
        return null;
    }

    public static void SafeSetActive(this Component target, bool enable)
    {
        if (!target || !target.gameObject)
            return;
        target.gameObject.SetActive(enable);
    }

    public static void SafeSetActive(this GameObject go, bool enable)
    {
        if (!go) return;
        go.SetActive(enable);
    }

    public static T Random<T>(this T[] rArr)
    {
        if (rArr == null || rArr.Length == 0)
            return default(T);
        var dom = UnityEngine.Random.Range(0, rArr.Length);
        return rArr[dom];
    }

    public static T GetComponent<T>(this GameObject obj, string path) where T : Component
    {
        var child = obj.transform.Find(path);
        if (child) return child.GetComponent<T>();
        return null;
    }

    public static T GetComponent<T>(this Transform transform, string path) where T : Component
    {
        var obj = transform.Find(path);
        if (obj) return obj.GetComponent<T>();
        return null;
    }

    public static void SetKeyWord(this Material material, string keyword, bool enable)
    {
        if (enable) material.EnableKeyword(keyword);
        else material.DisableKeyword(keyword);
    }

    public static Sprite ToSprite(this Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    /// <summary>
    /// 实例化预制并添加到固定父物体下
    /// </summary>
    /// <param name="parent">预制父对象</param>
    /// <param name="prefab">预制游戏物体</param>
    /// <returns></returns>
    static public Transform AddNewChild(this Transform parent, GameObject prefab)
    {
        Transform trans = null;
        GameObject go = GameObject.Instantiate(prefab) as GameObject;

        if (go != null && parent != null)
        {
            trans = go.GetComponent<Transform>();
            if (trans == null)
                trans = go.GetComponent<RectTransform>();
            trans.SetParent(parent);
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;
            trans.localScale = Vector3.one;
        }
        return trans;
    }

    /// <summary>
    /// 实例化预制并添加到固定父物体下
    /// </summary>
    /// <param name="parent">预制父对象</param>
    /// <param name="prefab">预制游戏物体</param>
    /// <returns></returns>
    static public Transform AddNewChild(this Transform parent, Transform prefab)
    {
        return parent.AddNewChild(prefab.gameObject);
    }

    static public Transform AddNewChild(this Transform parent)
    {
        Transform trans = null;
        GameObject go = new GameObject("child");

        if (go != null && parent != null)
        {
            trans = go.GetComponent<Transform>();
            if (trans == null)
                trans = go.GetComponent<RectTransform>();
            trans.SetParent(parent);
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;
            trans.localScale = Vector3.one;
        }
        return trans;
    }

    public static RectTransform AddUINode(this Transform root, string name = null)
    {
        return AddUINode(root, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
    }

    public static RectTransform AddUINodeStrech(this Transform root, string name = null)
    {
        return AddUINode(root, name, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
    }

    public static RectTransform AddUINode(this Transform root, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 offsetMin, Vector2 offsetMax)
    {
        var rc = root as RectTransform;
        if (!rc)
        {
            Logger.LogWarning("Transform::AddUINode: root is {0}", root ? "not an UI element" : "null");
            return null;
        }
        
        var node = new GameObject(name ?? "uiNode");

        var t = node.GetComponentDefault<RectTransform>();
        Util.AddChild(root, t);

        t.pivot = pivot;
        t.anchorMin = anchorMin;
        t.anchorMax = anchorMax;
        t.offsetMin = offsetMin;
        t.offsetMax = offsetMax;

        return t;
    }

    public static void Strech(this Transform node)
    {
        if (!node) return;

        var rc = node.rectTransform();
        if (!rc)
        {
            Logger.LogWarning("Transform::Strech: node is not an UI element.");
            return;
        }

        rc.anchorMin = Vector2.zero;
        rc.anchorMax = Vector2.one;
        rc.sizeDelta = Vector2.zero;
        rc.anchoredPosition3D = Vector3.zero;
        rc.localScale = Vector3.one;
    }

    static public void SetInteractable(this Button button, bool enable)
    {
        if (!button) return;

        button.interactable = enable;
        if (!button.targetGraphic) return;

        button.targetGraphic.raycastTarget = enable;
    }

    static public void SetSaturation(this GameObject t, float saturation, bool containChild = true)
    {
        if (!t) return;

        Graphic[] gs = null;
        if (containChild) gs = t.GetComponentsInChildren<Graphic>(true); 
        else gs = t.GetComponents<Graphic>();

        if (gs != null && gs.Length > 0)
        {
            foreach (var item in gs)
            {
                item.saturation = saturation;
            }
        }
    }

    static public void SetSaturation(this Transform t, float saturation, bool containChild = true)
    {
        t?.gameObject.SetSaturation(saturation, containChild);
    }

    static public List<Transform> GetChildList(this Transform t)
    {
        if (!t) return null;

        var children = new List<Transform>();
        for (int i = 0, count = t.childCount; i < count; i++)
            children.Add(t.GetChild(i));

        return children;
    }

    #region Bitmask operations

    /// <summary>
    /// Check if chek's bit value is true
    /// e.g:<para></para>
    /// 15 = 0x01 | 0x02 | 0x04 | 0x08; (flag: 0, 1, 2, 3)<para></para>
    /// Mask 15 contains flag 0, 1, 2, 3<para></para>
    /// Bit 0 1 2 3 is true
    /// </summary>
    /// <param name="check"></param>
    /// <param name="bit"></param>
    /// <returns></returns>
    public static bool BitMask(this int check, int bit)
    {
        if (bit < 0 || bit > 31) return false;
        return (check >> bit & 0x01) == 1;
    }

    /// <summary>
    /// Check if chek's bit value is true
    /// e.g:<para></para>
    /// 15 = 0x01 | 0x02 | 0x04 | 0x08; (flag: 0, 1, 2, 3)<para></para>
    /// Mask 15 contains flag 0, 1, 2, 3<para></para>
    /// Bit 0 1 2 3 is true
    /// </summary>
    /// <param name="check"></param>
    /// <param name="bit"></param>
    /// <returns></returns>
    public static bool BitMask(this long check, int bit)
    {
        if (bit < 0 || bit > 63) return false;
        return (check >> bit & 0x01) == 1;
    }

    /// <summary>
    /// Set chek's bit value
    /// </summary>
    /// <param name="check"></param>
    /// <param name="bit"></param>
    /// <returns></returns>
    public static int BitMask(this int check, int bit, bool value)
    {
        if (bit < 0 || bit > 31) return check;
        int mask = 1 << bit;
        return value ? check | mask : check & (check ^ mask);
    }

    /// <summary>
    /// Set chek's bit value
    /// </summary>
    /// <param name="check"></param>
    /// <param name="bit"></param>
    /// <returns></returns>
    public static long BitMask(this long check, int bit, bool value)
    {
        if (bit < 0 || bit > 63) return check;
        long mask = (long)1 << bit;
        return value ? check | mask : check & (check ^ mask);
    }

    /// <summary>
    /// Check if mask contains flags
    /// e.g:
    /// 15 = 0x01 | 0x02 | 0x04 | 0x08; (flag: 0, 1, 2, 3)
    /// Mask 15 contains flag 0, 1, 2, 3
    /// </summary>
    /// <returns>If mask contains flag return true, else return false</returns>
    public static bool BitMasks(this int mask, params int[] flags)
    {
        if (flags == null || flags.Length < 1) return false;
        foreach (var flag in flags) if ((mask & (1 << flag)) != 0) return true;
        return false;
    }

    /// <summary>
    /// Set check's bit values
    /// </summary>
    public static int BitMasks(this int check, bool value, params int[] flags)
    {
        if (flags == null || flags.Length < 1) return check;
        foreach (var flag in flags) check = check.BitMask(flag, value);
        return check;
    }

    /// <summary>
    /// Get flag if mask contains unique flag
    /// e.g:
    /// 15 = 0x01 | 0x02 | 0x04 | 0x08; (flag: 0, 1, 2, 3)
    /// 16 = 0x10; (flag: 4)
    /// Mask 16 contains unique flag 4
    /// </summary>
    /// <returns>If mask contains unique flag return flag, else return -1</returns>
    public static int Unique(this int mask)
    {
        for (var i = 0; i < 32; ++i)
        {
            if ((mask >> i & 0x01) == 1)
                return (mask ^ 1 << i) == 0 ? i : -1;
        }
        return -1;
    }

    /// <summary>
    /// Get flag if mask contains unique flag
    /// e.g:
    /// 15 = 0x01 | 0x02 | 0x04 | 0x08; (flag: 0, 1, 2, 3)
    /// 16 = 0x10; (flag: 4)
    /// Mask 16 contains unique flag 4
    /// </summary>
    /// <returns>If mask contains unique flag return flag, else return -1</returns>
    public static int Unique(this long mask)
    {
        for (var i = 0; i < 64; ++i)
        {
            if ((mask >> i & 0x01) == 1)
                return (mask ^ 1 << i) == 0 ? i : -1;
        }
        return -1;
    }

    /// <summary>
    /// Get the bitmask of flag
    /// </summary>
    /// <param name="flag"></param>
    /// <returns></returns>
    public static int ToMask(this int flag)
    {
        return 1 << flag;
    }

    /// <summary>
    /// Get the bitmask of flag
    /// </summary>
    /// <param name="flag"></param>
    /// <returns></returns>
    public static long ToLongMask(this int flag)
    {
        return (long)1 << flag;
    }

    /// <summary>
    /// Convert from a formatted array string to int mask
    /// <para></para>
    /// e.g: "0;1;2" => 0x01 | 0x02 | 0x04 = 7
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static int ToMask(this string s)
    {
        var arr = Util.ParseString<int>(s);
        if (arr.Length < 1) return 0;

        var mask = 0;
        foreach (var i in arr) mask = mask.BitMask(i, true);

        return mask;
    }

    /// <summary>
    /// Convert from a formatted array string to long mask
    /// <para></para>
    /// e.g: "0;1;2" => 0x01 | 0x02 | 0x04 = 7
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static long ToLongMask(this string s)
    {
        var arr = Util.ParseString<int>(s);
        if (arr.Length < 1) return 0;

        var mask = 0L;
        foreach (var i in arr) mask = mask.BitMask(i, true);

        return mask;
    }

    /// <summary>
    /// Get this low 8 bit part
    /// </summary>
    /// <param name="mask"></param>
    /// <returns></returns>
    public static int Low8(this int mask)
    {
        return mask & 0xFF;
    }

    /// <summary>
    /// Get this low 16 bit part
    /// </summary>
    /// <param name="mask"></param>
    /// <returns></returns>
    public static int Low16(this int mask)
    {
        return mask & 0xFFFF;
    }

    /// <summary>
    /// Get this high 8 bit part
    /// </summary>
    /// <param name="mask"></param>
    /// <returns></returns>
    public static int High8(this int mask)
    {
        return mask >> 8 & 0xFF;
    }

    /// <summary>
    /// Get this high 16 bit part
    /// </summary>
    /// <param name="mask"></param>
    /// <returns></returns>
    public static int High16(this int mask)
    {
        return mask >> 16 & 0xFF;
    }

    #endregion

    /// <summary>
    /// Set color alpha and return a new color
    /// </summary>
    /// <param name="color"></param>
    /// <param name="alpha"></param>
    /// <returns></returns>
    public static Color SetAlpha(this Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    public static Vector3 ClampAngle(this Vector3 vec3)
    {
        vec3.x = (float)Mathd.ClampAngle(vec3.x);
        vec3.y = (float)Mathd.ClampAngle(vec3.y);
        vec3.z = (float)Mathd.ClampAngle(vec3.z);

        return vec3;
    }

    public static T GetValue<T>(this T[] array,int index)
    {
        if (array == null || index < 0 || index >= array.Length)
        {
            Logger.LogError("array get value failed, error is:[{0}],index:[{1}]",array == null ? "array is null" : Util.Format("array len is {0}",array.Length),index);
            return default(T);
        }
        return array[index];
    }

    public static T GetValue<T>(this List<T> list, int index)
    {
        if (list == null || index < 0 || index >= list.Count)
        {
            Logger.LogWarning("list get value failed,error is:[{0}],index:[{1}]", list == null ? "list is null" : Util.Format("list count is {0}", list.Count), index);
            return default(T);
        }
        return list[index];
    }

    public static T Find<T>(this IReadOnlyList<T> list, Predicate<T> match)
    {
        if (list == null || list.Count == 0)
            return default(T);
        for (int i = 0; i < list.Count; i++)
        {
            if (match != null && match(list[i]))
                return list[i];
        }
        return default(T);
    }

    public static int FindIndex<T>(this IReadOnlyList<T> list, Predicate<T> match)
    {
        if (list == null || list.Count == 0)
            return -1;
        for (int i = 0; i < list.Count; i++)
        {
            if (match != null && match(list[i]))
                return i;
        }
        return -1;
    }

    public static void SetGray(this Transform t, bool gray)
    {
        if (!t) return;
        var graphics = t.GetComponentsInChildren<Graphic>();
        foreach (var graphic in graphics)
        {
            graphic.saturation = gray ? 0 : 1;
            graphic.SetAllDirty();
        }
    }

    #region Vector3 helpers

    public const float Epsilon = 0.001f;

    /// <summary>
    /// Is the vector within Epsilon of zero length?
    /// </summary>
    /// <param name="v"></param>
    /// <returns>True if the square magnitude of the vector is within Epsilon of zero</returns>
    public static bool IsZero(this Vector3 v)
    {
        return v.sqrMagnitude < Epsilon * Epsilon;
}

    /// <summary>
    /// Returns a non-normalized projection of the supplied vector onto a plane as described by its normal
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="planeNormal">The normal that defines the plane.  Cannot be zero-length.</param>
    /// <returns>The component of the vector that lies in the plane</returns>
    public static Vector3 Project(this Vector3 vector, Vector3 planeNormal)
    {
        return (vector - Vector3.Dot(vector, planeNormal) * planeNormal);
    }

    /// <summary>
    /// Calculate the shortest difference between target.
    /// </summary>
    /// <param name="vec3"></param>
    /// <param name="to">Target angle</param>
    /// <returns></returns>
    public static Vector3 DeltaAnglesTo(this Vector3 vec3, Vector3 to)
    {
        return new Vector3(Mathf.DeltaAngle(vec3.x, to.x), Mathf.DeltaAngle(vec3.y, to.y), Mathf.DeltaAngle(vec3.z, to.z));
    }

    #endregion

    #region Vector4 helpers

    public static Quaternion ToQuaternion(this Vector4 vec4)
    {
        return new Quaternion(vec4.x, vec4.y, vec4.z, vec4.w);
    }

    #endregion

    #region Quaternion helpers

    public static Vector4 ToVector4(this Quaternion rotation)
    {
        return new Vector4(rotation.x, rotation.y, rotation.z, rotation.w);
    }

    #endregion
}
