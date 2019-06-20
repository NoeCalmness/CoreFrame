using System;
using UnityEngine;

public class DisparkGuide : GuideBase
{
    public override EnumGuideType panelType { get { return EnumGuideType.TipGuide; } }

    //cache the temp component
    private BaseRestrain m_restrainComponent;
    private int m_lastRestrianId;

    private int m_allObjsNeedVisible = 0;
    private int m_allObjsCurrentVisible = 0;
    private int m_allObjsTempVisible = 0;

    private bool m_addEvent = false;

    #region defalut monobehaviour functions

    protected override void Awake()
    {
        base.Awake();
        EventManager.AddEventListener(Events.SCENE_LOAD_START, OnSceneLoadStart);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventManager.RemoveEventListener(Events.SCENE_LOAD_START, OnSceneLoadStart);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        EventManager.AddEventListener(Module_Guide.EventCheckGuideEnable, OnCheckDisparkItemEnable);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventManager.RemoveEventListener(Module_Guide.EventCheckGuideEnable, OnCheckDisparkItemEnable);
    }

    protected override void Update()
    {
        base.Update();

        UpdateFindedTransActive();
    }
    #endregion

    #region  override

    public override void InitGuideWindow()
    {
        base.InitGuideWindow();
        m_addEvent = false;
    }

    public override void ResetGuideData()
    {
        base.ResetGuideData();
        
        m_lastRestrianId = 0;
        m_allObjsNeedVisible = 0;
        m_allObjsCurrentVisible = 0;
        m_allObjsTempVisible = 0;
        RemoveRestrainCallback();
    }
    protected override void SetPanelMaskEnable(bool active)
    {
        base.SetPanelMaskEnable(active);
        
        //m_hotAreaTrans.gameObject.SetActive(active);

        Color c = m_maskImage.color;
        c.a = active && !maskFade ? 0.75f : 0f;
        m_maskImage.raycastTarget = active;
        m_maskImage.color = c;
    }

    protected override void OnRefreshGuide()
    {
        base.OnRefreshGuide();

        if (m_currentItem.hotAreaData.restrainType == EnumGuideRestrain.CheckID) m_lastRestrianId = m_currentItem.hotAreaData.restrainId;
        else if (m_currentItem.hotAreaData.restrainType == EnumGuideRestrain.CurrentWeapon) m_lastRestrianId = Module_Equip.instance == null ? 0 : (int)Module_Equip.instance.weapon?.itemTypeId;

        OnStartAnimationEnd();
    }

    protected override void OnRefreshGuideEnd()
    {
        base.OnRefreshGuideEnd();

        //set mask alpha
        Color c = m_maskImage.color;
        m_maskImage.color = new Color(c.r, c.g, c.b, 0);
        m_maskImage.raycastTarget = false;
        
        if(m_hotTrans) ResetFindedHotTrans(m_hotTrans);
    }

    #endregion

    private void OnSceneLoadStart(Event_ e)
    {
        m_updateHotAreaSucess = false;
        m_addEvent = false;
    }

    private void OnCheckDisparkItemEnable(Event_ e)
    {
        BaseRestrain c = e.param1 as BaseRestrain;
        if (!c || c.restrainId == 0 || m_lastRestrianId == 0) return;

        //Logger.LogDetail("----OnRecvRestrainItemChange.......item is {1} restrain id is {0}",c.restrainId,c.GetPath());
        if (c.restrainId == m_lastRestrianId) ResetFindedHotTrans(c.rectTransform);
    }

    private void AddRestrainBehaviourValueChange(RectTransform t)
    {
        m_restrainComponent = t.GetComponent<BaseRestrain>();
        if (m_restrainComponent)
        {
            if(m_lastRestrianId == 0) m_lastRestrianId = m_restrainComponent.restrainId;
            //this event only set panel disable
            m_restrainComponent.onCheckGuideDisable = OnCheckDisparkGuideDisable;
            //Logger.LogDetail("set m_lastRestrianId as {0}", m_lastRestrianId);
        }
    }

    private void OnCheckDisparkGuideDisable(BaseRestrain restrain)
    {
        //Logger.LogDetail("----OnCheckDisparkItemEnable.......restrain id is {0} m_lastRestrianId = {1} m_restrainComponent.id = {2}", restrain.restrainId, m_lastRestrianId, m_restrainComponent ? m_restrainComponent.restrainId : -1);
        if (m_lastRestrianId == 0 || !m_restrainComponent) return;
        
        if(restrain.restrainId != m_lastRestrianId) SetGuideUiItemActive(false);
    }

    private void SetGuideUiItemActive(bool active)
    {
        if (m_effectTrans) m_effectTrans.gameObject.SetActive(active);

        //if the m_findedHotTrans change active , to be on the safe side,we need to register the click callback manually
        if (m_hotTrans && !m_currentItem.hasCondition)
        {
            //bool m_addEvent = m_findedHotTrans.GetComponent<EventTriggerListener>() != null;
            //Logger.LogDetail("name is {0} active is {1}  add event is {2}", m_findedHotTrans.name, active, m_addEvent);
            if (active && !m_addEvent)
            {
                m_addEvent = true;
                EventTriggerListener.Get(m_hotTrans.gameObject).onClick += OnHotAreaClick;
            }
        }
    }

    protected override void OnHotAreaClick(GameObject go)
    {
        if (m_restrainComponent && m_restrainComponent.restrainId != m_lastRestrianId) return;

        m_addEvent = false;
        base.OnHotAreaClick(go);
    }

    private void ResetFindedHotTrans(RectTransform rectTrans)
    {
        Logger.LogDetail("----to refind the hot trans.......");
        RemoveRestrainCallback();
        AddRestrainBehaviourValueChange(rectTrans);
        m_updateCheckScrollItem = false;
        m_updateHotAreaSucess = true;
        m_hotTrans = rectTrans;
        PlayHotEffect();
        SetGuideUiItemActive(true);
        UpdateAllObjsActive();
    }

    private void RemoveRestrainCallback()
    {
        if (m_restrainComponent) m_restrainComponent.onCheckGuideDisable = null ;
        m_restrainComponent = null;
    }

    private void UpdateAllObjsActive()
    {
        if (m_effectTrans && m_effectTrans.gameObject.activeSelf)   m_allObjsNeedVisible |= 1;
        if (m_tipObj.activeSelf)                    m_allObjsNeedVisible |= 2;
        if (m_tipIcon.gameObject.activeSelf)        m_allObjsNeedVisible |= 4;

        m_allObjsCurrentVisible = m_allObjsTempVisible = m_allObjsNeedVisible;
    } 

    private void UpdateFindedTransActive()
    {
        if(m_hotTrans)
        {
            m_allObjsTempVisible = m_hotTrans.gameObject.activeInHierarchy ? m_allObjsNeedVisible : 0;
            if (m_allObjsCurrentVisible != m_allObjsTempVisible) SetAllObjsVisible(m_allObjsTempVisible);
        }
    }

    private void SetAllObjsVisible(int visible)
    {
        m_allObjsCurrentVisible = visible;
        var validRestrian = !m_restrainComponent || m_restrainComponent.restrainId == m_lastRestrianId;

        if (m_effectTrans) m_effectTrans.gameObject.SetActive((m_allObjsCurrentVisible & 1) > 0 && validRestrian);
        m_tipObj.SetActive((m_allObjsCurrentVisible & 2) > 0 && validRestrian);
        m_tipIcon.gameObject.SetActive((m_allObjsCurrentVisible & 4) > 0 && validRestrian);

        //if (m_effectTrans) Logger.LogDetail("m_effectTrans active is {0}", m_effectTrans.gameObject.activeSelf);
    }

    protected override void CheckEffect()
    {
        var validRestrian = !m_restrainComponent || m_restrainComponent.restrainId == m_lastRestrianId;
        m_effectLockTrans = m_restrainChildHotTrans ? m_restrainChildHotTrans : m_hotTrans;
        if (m_effectLockTrans && m_effectTrans && m_transPosWhenFinded != m_effectLockTrans.position && validRestrian)
        {
            //Logger.LogInfo("---in update -------- after last frame ,to lock scrollitem position......lock obj is {0}.....", m_effectLockTrans);
            m_transPosWhenFinded = m_effectLockTrans.position;
            PlayHotEffect();
        }
    }

    #region skipGuide
    protected override void OnSkipClick(GameObject go)
    {
        ResetGuideData();
        SetPanelMaskEnable(false);
        RefreshOnGuideEnd();
        moduleGuide.HandleGuideStoryGiveData();
        moduleGuide.HandleGuideInfoEnd();
    }

    #endregion
}
