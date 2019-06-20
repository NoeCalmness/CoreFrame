/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Used for wish window.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-02-028
 * 
 ***************************************************************************************************/

using System;
using UnityEngine;

public abstract class WishItemInfo : MonoBehaviour, ISubRewardShow
{
    protected static PItem2 emptyItem
    {
        get
        {
            if (m_emptyItem == null) m_emptyItem = PacketObject.Create<PItem2>();
            return m_emptyItem;
        }
    }
    private static PItem2 m_emptyItem = null;

    public Action OnBack { get; set; }

    public PItem2 item
    {
        get { return m_item; }
        set
        {
            if (value == null) emptyItem.CopyTo(ref m_item);
            else value.CopyTo(ref m_item);

            itemInfo = ConfigManager.Get<PropItemInfo>(m_item.itemTypeId);

            UpdateItemInfo();
        }
    }
    protected PItem2 m_item;

    public ScEquipWeaponDecompose decomposeInfo
    {
        set
        {
            if (value == null) m_di = m_dp = m_dc = 0;
            else
            {
                m_di = value.weaponId;
                m_dp = value.pieceId;
                m_dc = value.pieceNum;
            }
        }
    }
    protected int m_di, m_dp, m_dc; // Decompose item, piece, count

    public bool isNewItem { get; set; }

    public PropItemInfo itemInfo { get; protected set; }

    public TweenAlpha parentTween = null;
    public TweenAlphaParticle effect = null;

    public System.Action onHide = null;

    protected virtual void Awake()
    {
        if (m_item == null) m_item = emptyItem.Clone();

        UpdateTexts();
        UpdateItemInfo();
    }

    protected virtual void OnDestroy()
    {
        m_item.Destroy();
        itemInfo = null;
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);

        if (parentTween) parentTween.PlayForward();
        if (effect) effect.PlayForward();
    }

    public void Show(PItem2 i)
    {
        item = i;
        Show();
    }

    public virtual void Hide()
    {
        if (m_di > 0 && Module_Global.instance)
        {
            var i = m_di; m_di = 0;
            Module_Global.instance.ShowItemDecomposeInfo(i, m_dp, m_dc, () => Hide());
            return;
        }

        if (parentTween) parentTween.PlayReverse();
        if (effect) effect.PlayReverse();

        onHide?.Invoke();
        OnBack?.Invoke();
    }

    public virtual void UpdateTexts() { }

    public virtual void UpdateItemInfo() { }
}
