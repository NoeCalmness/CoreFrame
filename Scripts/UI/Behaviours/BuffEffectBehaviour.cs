// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-10-13      9:40
//  * LastModify：2018-10-13      9:40
//  ***************************************************************************************************/

using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BuffEffectBehaviour : MonoBehaviour
{
    public Buff buff;

    public Image icon;
    public Image duration;
    public Text  desc;
    public Text  overrideNum;
    public TweenBase destoryTween;
    public TweenBase activeTween;

    public Action<Buff> onClick; 
    private int overlapNumber;
    private int buffID;
    private Creature creature;

    private int OverlapNumber
    {
        get { return overlapNumber; }
        set
        {
            overlapNumber = value;
            Util.SetText(overrideNum, value.ToString());
            overrideNum?.SafeSetActive(value > 1);
        }
    }


    private bool isInited = false;

    private void Start()
    {
        Initelize();
    }
    private void Initelize()
    {
        if (isInited) return;
        isInited = true;
        icon        = transform.GetComponent<Image>("icon");
        duration    = transform.GetComponent<Image>("duration");
        desc        = transform.GetComponent<Text>("des");
        overrideNum = transform.GetComponent<Text>("count");
        destoryTween= transform.GetComponent<TweenBase>();
        activeTween = transform.GetComponent<TweenBase>("icon/Image");

        desc        ?.SafeSetActive(false);

        icon?.GetComponent<Button>()?.onClick.AddListener(() => onClick?.Invoke(buff));
    }

    public void ShowDesc(Buff rBuff)
    {
        desc.SafeSetActive(buff?.ID == rBuff?.ID);
    }

    public void Init(Buff rBuff, Creature rCreature)
    {
        Initelize();
        buff = rBuff;
        buffID = rBuff.ID;
        OverlapNumber = 1;
        creature = rCreature;

        if(!string.IsNullOrEmpty(buff.info.icon))
            AtlasHelper.SetIcons(icon, buff.info.icon);
        Util.SetText(desc, buff.info.BuffDesc(buff.BuffLevel));
    }

    public void OnTrigger()
    {
        activeTween?.Play();
    }

    public bool Destory()
    {
        OverlapNumber--;
        if (OverlapNumber > 0)
        {
            //如果还有相同ID 的buff。需要在此buff销毁后，重新替换一个相同ID的buff
            if (buff.pendingDestroy || buff.destroyed)
            {
                buff = creature.GetBuffList().Find(item => item.ID == buffID && !item.pendingDestroy && !item.destroyed);
            }
            return false;
        }
        if (!destoryTween)
            GameObject.Destroy(this.gameObject);
        else
        {
            destoryTween.onComplete.AddListener((b) => Destroy(gameObject));
            destoryTween.Play();
        }
        return true;
    }

    private void Update()
    {
        if (null != duration)
        {
            if (buff.length <= 0)
                duration.fillAmount = 0;
            else
                duration.fillAmount = 1 - Mathf.Clamp01((float)buff.duration/buff.length);
        }
    }

    public void Overlap(Buff rBuff)
    {
        if (rBuff.duration > buff.duration)
            buff = rBuff;
        OverlapNumber++;
    }
}
