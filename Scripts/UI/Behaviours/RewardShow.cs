// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 奖励展示
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-09-20      14:40
//  * LastModify：2018-09-20      14:40
//  ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public interface ISubRewardShow
{
    void Show(PItem2 rItem);

    Action OnBack { get; set; }
}

public interface ITemplateBind
{
    void Bind(Transform rRoot, PItem2 rItem);
}

public class NormalTemplateBind : ITemplateBind
{
    public void Bind(Transform rRoot, PItem2 rItem)
    {
        if (rItem == null)
            return;
        var prop = ConfigManager.Get<PropItemInfo>(rItem.itemTypeId);

        Util.SetItemInfo(rRoot, prop, rItem.level, (int)rItem.num, true, rItem.star);
    }
}

public class SummonTempleteBind : ITemplateBind
{
    public void Bind(Transform rRoot, PItem2 rItem)
    {
        if (rItem == null)
            return;
        var prop = ConfigManager.Get<PropItemInfo>(rItem.itemTypeId);
        var isPet = prop?.itemType == PropType.Pet;
        var petNode = rRoot.Find("pet");
        var propNode = rRoot.Find("prop");
        propNode?.SafeSetActive(!isPet);
        petNode ?.SafeSetActive(isPet);

        if (!isPet)
            Util.SetItemInfo(propNode, prop, rItem.level, (int) rItem.num, true, rItem.star);
        else
        {
            var petInfo = Module_Pet.instance.GetPet(rItem.itemTypeId);
            if (petInfo == null) return;
            Util.SetPetInfo(petNode, petInfo);
        }
    }
}

public class RewardShow : MonoBehaviour
{
    public Transform  Content;
    public GameObject Template;

    public Button CloseButton;
    [HideInInspector]
    public Action OnClose;

    public UnityEvent onAnimEnd;

    public Transform EffectNode;

    public Button SkipEffectAnimationButton;

    public float EffAnimLength;

    private List<Tuple<Func<PItem2, bool>, ISubRewardShow>> handler = new List<Tuple<Func<PItem2, bool>, ISubRewardShow>>();

    private Queue<PItem2> showQueue = new Queue<PItem2>();

    private bool isPlaying;
    private float timer;
    private PItem2 currentShowItem;
    private ITemplateBind binder;
    private bool isShowItems;

    private Queue <TweenBase> itemTweenQueue = new Queue<TweenBase>();
    private List<GameObject> items = new List<GameObject>();

    public void Start()
    {
        Template.SafeSetActive(false);
        SkipEffectAnimationButton?.onClick.AddListener(SkipEffectAnimation);
    }

    public void SetBinder(ITemplateBind rBinder)
    {
        binder = rBinder;
    }

    public void OnReturn()
    {
        gameObject.SafeSetActive(false);
        foreach (var go in items)
            if (go) Destroy(go);
        OnClose?.Invoke();
    }

    public void Clear()
    {
        foreach (var go in items)
            if (go) Destroy(go);
        EffectNode.gameObject.SetActive(false);
    }

    public void ClearHandler()
    {
        handler.Clear();
    }

    public void Regirest(Func<PItem2, bool> rFunc, ISubRewardShow rHandle)
    {
        handler.Add(Tuple.Create(rFunc, rHandle));
    }

    public void Show(List<PItem2> rRewards, bool canGoto = true, bool skipAnim = false)
    {
        Show(rRewards?.ToArray(), canGoto, skipAnim);
    }

    public void SkipEffectAnimation()
    {
        EffectNode.SafeSetActive(false);
        timer = 0;
        SkipEffectAnimationButton.SafeSetActive(false);
    }

    public void Show(PItem2[] rReward, bool canGoto = true, bool skipAnim = false)
    {
        if (rReward == null || rReward.Length == 0)
            return;

        if (showQueue == null) showQueue = new Queue<PItem2>();
        else showQueue.Clear();

        gameObject.SetActive(true);
        Content.SafeSetActive(false);
        enabled = true;

        if (binder == null)
            binder = new NormalTemplateBind();
        Util.ClearChildren(Content);
        for (var i = 0; i < rReward.Length; i++)
        {
            var item = rReward[i];
            var t = Content.AddNewChild(Template);
            t.name = i.ToString();
            t.SafeSetActive(true);
            showQueue.Enqueue(item);
            binder?.Bind(t, item);
            itemTweenQueue.Enqueue(t.GetComponent<TweenFillAmount>("card"));
            t.GetComponent<Transform>("card").SafeSetActive(true);
            items.Add(t?.gameObject);
            t?.GetComponent<Button>()?.onClick.AddListener(() =>
            {
                if(!enabled)
                    Module_Global.instance.UpdateGlobalTip(item, false);
            });

            foreach (var tupe in handler)
            {
                var b = tupe.Item1(item);
                t.GetComponent<Transform>("eff_putong").SafeSetActive(!b);
                t.GetComponent<Transform>("eff_jipin").SafeSetActive(b);
                if (b) break;
            }
        }

        EffectNode.SafeSetActive(!skipAnim);
        SkipEffectAnimationButton.SafeSetActive(!skipAnim);
        if (!skipAnim)
            timer = EffAnimLength;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0)
            return;
        if (Content && !Content.gameObject.activeSelf)
            Content.gameObject.SetActive(true);
        ShowItems();

        if (!isPlaying)
        {
            if (showQueue.Count > 0)
            {
                var item = showQueue.Dequeue();
                ShowItem(item);
            }
            else if (showQueue.Count <= 0)
            {
                Module_Global.instance.OnGlobalTween(false, 1);
                enabled = false;
                onAnimEnd.Invoke();
            }
        }
    }

    private void ItemShowEnd()
    {
        if (itemTweenQueue.Count > 0)
        {
            var tween = itemTweenQueue.Dequeue();
            if (tween != null)
            {
                tween.Play();
                tween.onComplete.AddListener((b) => isPlaying = false);
                return;
            }
        }
        isPlaying = false;
    }

    private void ShowItem(PItem2 item)
    {
        AudioManager.PlaySound(AudioInLogicInfo.audioConst.cardOpen);
        currentShowItem = item;
        isPlaying = true;
        foreach (var tupe in handler)
        {
            if (tupe.Item1(currentShowItem))
            {
                if (tupe.Item2 != null)
                {
                    tupe.Item2.Show(currentShowItem);
                    tupe.Item2.OnBack = ItemShowEnd;
                    return;
                }
            }
        }
        ItemShowEnd();
    }

    private void ShowItems()
    {
        if (isShowItems) return;
        isShowItems = true;
        foreach (var item in items)
        {
            var ts = item.GetComponentsInChildren<TweenAlpha>();
            if (null == ts || ts.Length == 0)
                continue;
            for (var i = 0; i < ts.Length; i++)
            {
                ts[i].enabled = true;
            }
        }
    }
}
