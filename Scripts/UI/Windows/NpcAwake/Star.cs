// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-01-22      16:39
//  *LastModify：2019-01-22      16:39
//  ***************************************************************************************************/


using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Star : MonoBehaviour
{
    private GameObject  effectObject;
    public int          level;
    public NpcNode      node;

    private void Awake()
    {
        node = GetComponentInParent<NpcNode>();
    }

    /// <summary>
    ///     已经激活当前点
    /// </summary>
    public void Active()
    {
        Root.instance.StartCoroutine(LoadEffect_Async("effect_jiban_x01"));
    }

    private IEnumerator LoadEffect_Async(string effect)
    {
        yield return Level.PrepareAsset<GameObject>(effect);

        if (effectObject)
            Destroy(effectObject);

        effectObject = Level.GetPreloadObject<GameObject>(effect);
        effectObject.transform.SetParent(transform);
        effectObject.transform.localPosition = Vector3.zero;
        effectObject.transform.localScale = Vector3.one;

        Util.SetLayer(effectObject, Layers.NpcAwakeNode);
    }

    /// <summary>
    ///     开始激活当前点
    /// </summary>
    public void StartActive()
    {
        Root.instance.StartCoroutine(LoadEffect_Async("effect_jiban_x02"));
    }

    /// <summary>
    ///     未激活
    /// </summary>
    public void DeActive()
    {
        Root.instance.StartCoroutine(LoadEffect_Async("effect_jiban_x03"));
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        level = Util.Parse<int>(name);

        using (new Handles.DrawingScope(Color.green))
        {
            Handles.Label(transform.position + Vector3.up, "Lv:" + level);
        }
    }
#endif
}