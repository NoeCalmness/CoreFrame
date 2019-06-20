// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-10-24      13:45
//  * LastModify：2018-10-24      13:46
//  ***************************************************************************************************/
#region

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#endregion

[RequireComponent(typeof(EventTriggerListener))]
[RequireComponent(typeof(LineRenderer))]
public class AwakeController : MonoBehaviour
{
    #region public functions

    public void RefreshState()
    {
        state = Module_Awake.instance.CheckAwakeState(info);
        if (state == 0 || state == 2)
        {
            transform.localScale = Vector3.one * 1.5f;
            if (layerController)
                layerController.Watch(this);

            PlayLineAnim();
        }else
            transform.localScale = Vector3.one * 1.5f;

        if (text)
        {
            if (state == 0)
            {
                text.transform.parent.SafeSetActive(true);
                text.text = info.NameString;
            }
            else
            {
                text.transform.parent.SafeSetActive(false);
                text.text = string.Empty;
            }
        }

        if (state == 0)
        {
            if (!specialEffect)
            {
                var effName = "eff_xinghun_w_1";
                var assets = new System.Collections.Generic.List<string>();
                assets.Add(effName);
                Level.PrepareAssets(assets, (b) =>
                {
                    if (!b)
                        return;
                    specialEffect = SceneObject.Create<SceneObject>(effName, effName);
                    specialEffect.transform.SetParent(transform);
                    specialEffect.transform.localPosition = Vector3.zero;
                    specialEffect.transform.localScale = Vector3.one;
                });
            }
        }
        else if (specialEffect) specialEffect.Destroy();


        if (state < 0)
        {
            if (effect != null)
            {
                if (effect.name.Equals(info.unlockEffect))
                    return;
                effect.Destroy();
            }

            var assets = new System.Collections.Generic.List<string>();
            assets.Add(info.unlockEffect);
            Level.PrepareAssets(assets, (b) =>
            {
                if (!b)
                    return;
                effect = SceneObject.Create<SceneObject>(info.unlockEffect, info.unlockEffect);

                if (effect)
                {
                    effect.transform.SetParent(transform);
                    effect.transform.localPosition = Vector3.zero;
                    effect.transform.localScale = Vector3.one;
                }
            });
        }
        else if (state >= 0)
        {
            if (effect != null)
            {
                if (effect.name.Equals(info.lockEffect))
                    return;
                effect.Destroy();
            }
            var assets = new System.Collections.Generic.List<string>();
            assets.Add(info.lockEffect);
            Level.PrepareAssets(assets, (b) =>
            {
                if (!b)
                    return;
                effect = SceneObject.Create<SceneObject>(info.lockEffect, info.lockEffect);

                if (effect)
                {
                    effect.transform.SetParent(transform);
                    effect.transform.localPosition = Vector3.zero;
                    effect.transform.localScale = Vector3.one;
                }
            });
        }

    }

    public void Init(AwakeInfo rInfo)
    {
        info = rInfo;
    }

    public void DrawLine(AwakeHandle handle)
    {
        if (!gameObject)
            return;
        if (lineRenderer == null)
        {
            lineRenderer = this.GetComponentDefault<LineRenderer>();
        }

        var prevInfo = Module_Awake.PrevAwakeInfo(info);
        if (prevInfo && handle)
        {
            prev = handle.GetAwakeController(prevInfo.layer, prevInfo.index);
        }

        RefreshState();
    }

    #endregion

    #region private functions

    private void PlayLineAnim()
    {
        isPlaying = true;
        timer = 0;
    }

    private void Start()
    {
        var trigger = this.GetComponentDefault<EventTriggerListener>();
        trigger.onClick += OnClick;
        trigger.onPressBegin += OnBeginDrag;
        trigger.onPressMove += OnDrag;
        trigger.onPressEnd += OnDragEnd;

        if (!lineRenderer)
            lineRenderer = GetComponent<LineRenderer>();
        if (!lineAnim)
            lineAnim = transform.Find("Line")?.GetComponent<LineRenderer>();

        if (text)
        {
            var b = text.transform.parent.GetComponentDefault<Board>();
            var level = Level.current as Level_Home;
            b.target = level?.awakeCamera;
        }

        pageController = transform.GetComponentInParent<AwakePageController>();
    }

    private void OnDragEnd(PointerEventData go)
    {
        pageController?.OnEndDrag(go);
    }

    private void OnDrag(PointerEventData go)
    {
        pageController?.OnDrag(go);
    }

    private void OnBeginDrag(PointerEventData go)
    {
        pageController?.OnBeginDrag(go);
    }

    private void OnClick(GameObject go)
    {
        if (state == 2)
            Module_Global.instance.ShowMessage(9802);
        else
            Module_Awake.instance.DispatchModuleEvent(Module_Awake.Notice_ShowSkillOpenPanel, info, state);
    }

    private void LateUpdate()
    {
        if (prev != null)
        {
            if (!lineRenderer.enabled)
                lineRenderer.enabled = true;
            if (isPlaying && lineAnim)
            {
                timer += Time.deltaTime;
                if (timer > curve[curve.length - 1].time)
                    isPlaying = false;

                lineAnim.SetPositions(new[] { prev.transform.position, Vector3.Lerp(prev.transform.position, transform.position, curve.Evaluate(timer)) });
            }
            else
                lineAnim.SetPositions(new[] { prev.transform.position, transform.position });

            lineRenderer.SetPositions(new[] { prev.transform.position, transform.position });

            if (lineAnim && lineAnim.enabled ^ (state != 1))
                lineAnim.enabled = state != 1;
        }
        else
        {
            if (lineRenderer.enabled)
                lineRenderer.enabled = false;
            if (lineAnim.enabled)
                lineAnim.enabled = false;
        }

    }

    #endregion

    #region public Fields

    public AnimationCurve       curve;
    public AwakeInfo            info;
    public AwakeLayerController layerController;
    public AwakePageController  pageController;
    public LineRenderer         lineAnim;
    public LineRenderer         lineRenderer;
    public AwakeController      prev;
    public Text                 text;
    private SceneObject         effect;
    private SceneObject         specialEffect;
    // -1 已经点亮 0 可以点亮 1 不可以点亮
    public int state;

    #endregion

    #region private Fields

    private bool isPlaying;
    private float timer;

    #endregion
}
