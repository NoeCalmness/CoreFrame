// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-01-22      13:12
//  *LastModify：2019-02-11      13:06
//  ***************************************************************************************************/

#region

using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using DG.Tweening;

#endregion

[Serializable]
public struct CameraData : ICloneable
{
    public Vector3 position;
    public Vector3 euler;
    public float   fov;
    public float   leftBound;
    public float   rightBound;
    public float   upBound;
    public float   downBound;
    [NonSerialized] public NpcNode node;

    public object Clone()
    {
        return MemberwiseClone();
    }
}

public class NpcNode : ControlNodeBase, ISliderHandle
{
    public static List<NpcNode> nodeList = new List<NpcNode>();

    private const float     Speed01 = 3;

    public CameraData       cameraData;

    private Star            current;
    public  Transform       effectNode;
    public  LineRenderer    lineGray;
    public  LineRenderer    lineLight;
    public  NpcTypeID       npcId;
    private INpcMessage     npcInfo;
    private PreviewSlider   previewSlider;
    public  GameObject      starRoot;
    public  Star[]          stars;
    private ParticleSystemRenderer actorRenderer;
    private Transform       animEffect;
    private Color           highLightColor;
    private Color           originColor;
    private readonly int    TintColor = Shader.PropertyToID("_TintColor");
    private Tweener         _tween;

    private void Start()
    {
        if (effectNode == null)
            effectNode = transform.GetComponent<Transform>("stars/thumb");
        if (lineLight == null)
            lineLight = transform.GetComponent<LineRenderer>("stars/line/Particle System");

        actorRenderer = transform.GetComponent<ParticleSystemRenderer>("juese");
        animEffect = transform.GetComponent<Transform>("juese/ani");

        if (actorRenderer)
        {
            originColor = actorRenderer.sharedMaterial.GetColor(TintColor);
            highLightColor = new Color(originColor.r, originColor.g, originColor.b, 0.9f);
//            Logger.LogError($"{npcId}  {originColor.ToString()}   {highLightColor.ToString()}");
        }
        animEffect.SafeSetActive(false);

        cameraData.node = this;
        npcInfo = Module_Npc.instance?.GetTargetNpc(npcId);
        starRoot?.SafeSetActive(false);
        previewSlider = new PreviewSlider(this, null);


        DrawGrayLine();
    }

    public void DrawGrayLine()
    {
        if (lineGray == null)
            lineGray = transform.GetComponent<LineRenderer>("stars/line/Particle System (1)");
        if (lineGray != null && stars.Length > 0)
        {
            lineGray.enabled = true;
            lineGray.positionCount = stars.Length;
            for (var i = 0; i < stars.Length; i++)
                lineGray.SetPosition(i, stars[i].transform.position);
        }
    }

    private void OnDestroy()
    {
        RemoveListener();
        SetParticalColor(originColor, false);
    }

    private void OnEnable()
    {
        if(!nodeList.Contains(this))
            nodeList.Add(this);
        Module_Npc.instance.AddEventListener(Module_Npc.FocusNpcEvent, OnFocusNpc);
    }

    private void OnDisable()
    {
        nodeList.Remove(this);
        Module_Npc.instance.RemoveEventListener(Module_Npc.FocusNpcEvent, OnFocusNpc);
    }

    protected override void OnClick(GameObject data)
    {
        Controler?.CloseTo(cameraData);
    }

    /// <summary>
    ///     当摄像机靠近时触发
    /// </summary>
    public void OnCameraCloseTo()
    {
        var watcher = TimeWatcher.Watch("OnCameraCloseTo");
        watcher.See("lineGray");
        RefreshStarState();
        watcher.See("RefreshStarState");
        starRoot?.SafeSetActive(true);
        AddListener();

        Module_Awake.instance?.DispatchEvent(Module_Awake.Event_CloseToNode, Event_.Pop(npcId));
        watcher.See("Event_CloseToNode");
        watcher.Stop();
        watcher.UnWatch();

        animEffect.SafeSetActive(true);
    }

    public void OnCameraStartCloseTo()
    {
        Module_Awake.instance?.DispatchEvent(Module_Awake.Event_StartCloseToNode, Event_.Pop(npcId));
        SetParticalColor(originColor);
    }

    private void SetParticalColor(Color rColor, bool rFade = true)
    {
        _tween?.Kill();
        _tween = null;
        if (rFade)
            _tween =
                actorRenderer?.sharedMaterial?.DOColor(rColor, "_TintColor", 1)
                    .SetEase(Ease.Linear)
                    .OnComplete(OnTweenComplete);
        else
            actorRenderer?.sharedMaterial?.SetColor(TintColor, rColor);
    }

    private void OnTweenComplete()
    {
        _tween = null;
    }

    /// <summary>
    ///     当获得相机焦点
    /// </summary>
    public void OnGetFocus()
    {
        Module_Awake.instance?.DispatchEvent(Module_Awake.Event_NodeGetFocus, Event_.Pop(npcId));

        SetParticalColor(highLightColor);
    }

    /// <summary>
    ///     当失去相机焦点
    /// </summary>
    public void OnLoseFocus()
    {
        Module_Awake.instance?.DispatchEvent(Module_Awake.Event_NodeLoseFocus, Event_.Pop(npcId));

        SetParticalColor(originColor);
    }

    private void RefreshStarState()
    {
        for (var i = 0; i < stars.Length; i++)
        {
            if (stars[i].level == npcInfo.starLv)
            {
                stars[i].StartActive();
            }
            else if (stars[i].level < npcInfo.starLv)
                stars[i].Active();
            else
                stars[i].DeActive();
        }

        OnStarLevelUp(npcInfo.starLv);
        previewSlider.SetCurrentUniformAnim(npcInfo.starLv + npcInfo.starProcess, Speed01*0.1f);
    }

    /// <summary>
    ///     当摄像机远离时
    /// </summary>
    public void OnCameraFarAway()
    {
        Module_Awake.instance?.DispatchEvent(Module_Awake.Event_FarAwayNode, Event_.Pop(npcId));
        SetParticalColor(highLightColor);
    }

    /// <summary>
    ///     当摄像机开始远离时
    /// </summary>
    public void OnCameraStartFarAway()
    {
        starRoot?.SafeSetActive(false);
        RemoveListener();
        Module_Awake.instance?.DispatchEvent(Module_Awake.Event_StartFarAwayNode, Event_.Pop(npcId));
        animEffect.SafeSetActive(false);
    }

    private void AddListener()
    {
        Module_Npc.instance.AddEventListener(Module_Npc.NpcPerfusionChangeEvent, OnPerfusionChange);
    }

    private void RemoveListener()
    {
        Module_Npc.instance.RemoveEventListener(Module_Npc.NpcPerfusionChangeEvent, OnPerfusionChange);
    }

    private void OnPerfusionChange()
    {
        previewSlider.SetCurrentUniformAnim(npcInfo.starLv + npcInfo.starProcess, Speed01, null);
    }

    private void OnFocusNpc(Event_ e)
    {
        var npc = (NpcTypeID) e.param1;
        if (npc != npcId)
            return;
        Controler.FocusNpcNode(this);
    }

    #region 线条动画相关

    private float sliderValue;

    public Transform Root
    {
        get { return transform; }
    }

    public float SliderValue
    {
        get { return sliderValue; }
        set
        {
            sliderValue = value;
            var v = Mathf.FloorToInt(value);
            OnStarLevelUp(v);
            if (v < 1 || v > stars.Length - 1 || lineLight.positionCount < 2)
                return;
            lineLight.SetPosition(lineLight.positionCount - 1,
                Vector3.Lerp(stars[v - 1].transform.position, stars[v].transform.position, Mathf.Repeat(value, 1)));
            if (effectNode) effectNode.transform.position = lineLight.GetPosition(lineLight.positionCount - 1);
        }
    }

    public AnimToInteger OnAnimToInteger
    {
        get { return null; }
    }

    public void OnStarLevelUp(int rLevel)
    {
        if (current?.level == rLevel)
            return;
        current?.Active();
        current = Array.Find(stars, s => s.level == rLevel);
        current?.StartActive();
        lineLight.positionCount = Mathf.Max(2, Mathf.Min(rLevel + 1, stars.Length + 1));
        for (var i = 0; i < lineLight.positionCount - 1; i++)
            lineLight.SetPosition(i, stars[i].transform.position);
        lineLight.SetPosition(lineLight.positionCount - 1, lineLight.GetPosition(lineLight.positionCount - 2));
    }

    #endregion

    #region Editor Helper

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (null == stars || stars.Length <= 1)
            return;

        for (var i = 1; i < stars?.Length; i++)
        {
            Handles.DrawLine(stars[i - 1].transform.position, stars[i].transform.position);
        }
    }

    [ContextMenu("收集星星")]
    private void AutoCollectStar()
    {
        stars = GetComponentsInChildren<Star>();
        Array.Sort(stars, (a, b) => a.level.CompareTo(b.level));
    }
#endif

    #endregion
}