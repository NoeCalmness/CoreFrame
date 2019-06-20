using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class GuideBase : MonoBehaviour
{
    #region const definition
    protected readonly static string[] INCLUDE_INACTIVE_HOTAREA = new string[] { "1frame", "frame_01", "frame_02" };
    //protected readonly static string[] DONT_FIND_CHILD_RESTRAIN = new string[] { "suceessEvolvePanel" };

    //override sorting layer
    protected const string GUIDE_SORTING_LAYER = "TopMost";
    #endregion

    protected GameObject m_tipObj;
    protected Text m_tipText;
    protected Image m_tipIcon;
    //protected RectTransform m_hotAreaTrans;
    protected Image m_maskImage;
    protected Image m_skipImage;

    protected GameObject m_combatObj;
    protected GameObject m_clickObj;
    protected GameObject m_slideObj;
    protected Transform m_arrowTrans;
    protected GameObject m_handSlide;
    protected GameObject m_upHand;
    protected GameObject m_rightHand;
    protected GameObject m_downHand;
    protected GameObject m_pressObj;
    
    protected GuideInfo.GuideItem m_currentItem;

    protected Canvas m_lastHotCanvas;
    protected GraphicRaycaster m_lastHotGR;
    protected bool m_updateHotAreaSucess = false;
    protected RectTransform m_hotTrans;
    protected bool m_transActiveWhenFinded;
    protected Vector3 m_transPosWhenFinded;
    //仅被用于配置的滑动面板内部item上的按钮
    protected RectTransform m_restrainChildHotTrans;
    protected RectTransform m_effectLockTrans;
    protected EventTriggerListener m_originalEventTrigger;
    protected EventTriggerListener m_originalRestrainHasTrigger;

    protected EnhanceScrollView m_enhanceScroll;
    protected ScrollRect m_lastHotScroll;
    protected ScrollView m_lastScrollView;
    protected DynamicallyCreateObject m_createObject;
    //是否需要在update中去重新检测item
    protected bool m_updateCheckScrollItem = false;
    protected bool m_updateCheckCreateItem = false;

    protected bool m_init = false;
    protected Module_Guide moduleGuide;
    protected CanvasGroup m_cavansGroup;
    protected Tween m_canvasTween;
    
    protected Dictionary<string, GameObject> m_effcetDic = new Dictionary<string, GameObject>();
    protected Transform m_effectTrans;

    public virtual EnumGuideType panelType { get { return EnumGuideType.NormalGuide; } }
    protected virtual int m_guideSorting { get { return 3; } }
    protected bool maskFade { get { return m_currentItem != null && m_currentItem.isMaskFade; } }
    protected bool hotTransValid { get { return m_hotTrans != null && m_hotTrans.gameObject.activeInHierarchy; } }
    protected bool restrianTransValid { get { return m_restrainChildHotTrans != null && m_restrainChildHotTrans.gameObject.activeInHierarchy; } }

    /// <summary>
    ///
    /// </summary>
    protected bool bUseGuideProtection = false;

    #region defalut funcs of monobehaviour
    protected virtual void Awake()
    {
        InitComponent();
    }

    protected virtual void OnEnable()
    {

    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        CheckScrollItem();
        CheckEffect();
        CheckHotAreaVisibleChange();
    }

    protected virtual void OnDisable()
    {

    }

    protected virtual void OnDestroy()
    {

    }
    #endregion

    #region init

    /// <summary>
    /// Called when window added to level
    /// </summary>
    protected void InitComponent()
    {
        if (m_init) return;

        m_init = true;
        moduleGuide = Module_Guide.instance;

        m_cavansGroup   = gameObject.GetComponent<CanvasGroup>();
        m_tipObj        = transform.Find("aside").gameObject;
        m_tipText       = m_tipObj.transform.GetComponent<Text>("text_bg/text");
        m_tipIcon       = transform.GetComponent<Image>("icon");
        m_maskImage     = transform.GetComponent<Image>("mask");
        m_skipImage     = transform.GetComponent<Image>("skip");
        m_combatObj     = transform.Find("combatguide").gameObject;
        m_clickObj      = m_combatObj.transform.Find("click").gameObject;
        m_slideObj      = m_combatObj.transform.Find("slide").gameObject;
        m_pressObj      = m_combatObj.transform.Find("press").gameObject;
        m_arrowTrans    = m_slideObj.transform.Find("arrowslide");
        m_handSlide     = m_slideObj.transform.Find("handslide").gameObject;
        m_upHand        = m_handSlide.transform.Find("handuptowards").gameObject;
        m_rightHand     = m_handSlide.transform.Find("handrighttowards").gameObject;
        m_downHand      = m_handSlide.transform.Find("handdowntowards").gameObject;

        m_combatObj.SetActive(false);
        RectTransform m_hotAreaTrans = transform.GetComponent<RectTransform>("hot_area");
        if (m_hotAreaTrans) m_hotAreaTrans.gameObject.SetActive(false);
        m_maskImage.gameObject.SetActive(true);
        EventTriggerListener.Get(m_maskImage.gameObject).onClick = OnMaskClick;
       // EventTriggerListener.Get(m_skipImage?.gameObject).onClick = OnSkipClick;

        m_effcetDic.Clear();
        ResetGuideCanvas();
        SetPanelMaskEnable(true);
    }

    protected void ResetGuideCanvas()
    {
        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas)
        {
            canvas.overrideSorting = true;
            canvas.sortingLayerName = GUIDE_SORTING_LAYER;
            canvas.sortingOrder = m_guideSorting;
        }
    }

    public virtual void InitGuideWindow()
    {
        SetPanelMaskEnable(true);
        SetPanelVisible(true);
    }

    public void SetPanelVisible(bool visible)
    {
        if (visible) gameObject.SetActive(visible);

        float target = visible ? 1 : 0;
        m_cavansGroup.alpha = 1 - target;

        if (m_canvasTween != null) m_canvasTween.Kill();
        m_canvasTween = DOTween.To(() => m_cavansGroup.alpha, x => m_cavansGroup.alpha = x, target, 0.5f).SetUpdate(true).OnComplete(() =>
        {
            if (!visible) gameObject.SetActive(visible);
        });
    }
    #endregion

    #region click event
    protected void OnMaskClick(GameObject go)
    {
        if (m_currentItem == null || m_currentItem.hasCondition || m_currentItem.hasHotArea && !m_currentItem.hotAreaData.isTipHotArea) return;
        moduleGuide.UpdateStep();
    }

    protected virtual void OnSkipClick(GameObject go) { }

    protected virtual void OnHotAreaClick(GameObject go)
    {
        if (!moduleGuide.afterPause) return;
        bUseGuideProtection = false;
        EventTriggerListener.Get(go).onClick -= OnHotAreaClick;
        ClearScrollCache();
        moduleGuide.UpdateStep();
    }
    #endregion

    #region refresh window

    public virtual void ResetGuideData()
    {
        ClearScrollCache();
        m_tipObj.SetActive(false);
        m_tipIcon.gameObject.SetActive(false);
        foreach (var item in m_effcetDic)
        {
            if (item.Value) item.Value.SetActive(false);
        }
        m_effectTrans = null;
        m_combatObj.SetActive(false);

        //Logger.LogDetail("guide name is {0}   last find name is {1}   remove EventTriggerListener", gameObject.name, m_findedHotTrans ? m_findedHotTrans.name : "null");
        if (!m_originalEventTrigger && m_hotTrans)
        {
            EventTriggerListener elistener = m_hotTrans.GetComponent<EventTriggerListener>();
            if(elistener!=null)
            {
                elistener.triggers.Clear();
                DestroyImmediate(elistener);
            }
            
        }
        if (!m_originalRestrainHasTrigger && m_restrainChildHotTrans)
        {
            EventTriggerListener elistener = m_restrainChildHotTrans.GetComponent<EventTriggerListener>();
            if (elistener != null)
            {
                elistener.triggers.Clear();
                DestroyImmediate(elistener);
            }
        }

        m_originalEventTrigger = null;
        m_originalRestrainHasTrigger = null;
        m_hotTrans = null;
        m_updateHotAreaSucess = false;
        m_restrainChildHotTrans = null;
        m_effectLockTrans = null;

        RemoveLastRectCanvas();
    }

    public void RefreshGuideWindow(GuideInfo.GuideItem guideItem)
    {
        //Logger.LogDetail("base panel with name :{0} RefreshGuideWindow...",gameObject.name);
        m_currentItem = guideItem;

        if (m_currentItem == null)
        {
            RefreshOnGuideEnd();
            return;
        }
        gameObject.SetActive(guideItem.type == panelType);
        if (guideItem.type == EnumGuideType.Dialog || guideItem.type != panelType) return;
        
        OnRefreshGuide();
    }

    protected virtual void RefreshOnGuideEnd()
    {
        SetPanelVisible(false);
    }

    protected virtual void OnRefreshGuide() { }

    protected void OnStartAnimationEnd()
    {
        //Logger.LogInfo("---on OnStartAnimationEnd");
        UpdateHotArea();
        OnRefreshGuideEnd();
       
    }

    protected virtual void OnRefreshGuideEnd()
    {

    }

    protected virtual void SetPanelMaskEnable(bool active)
    {

    }

    #region refresh hot area
    protected void ClearScrollCache()
    {
        m_lastHotScroll = null;
        m_lastScrollView = null;
        m_enhanceScroll = null;
    }

    public virtual void UpdateHotArea()
    {
        if (m_updateHotAreaSucess || m_currentItem == null) return;

        Logger.LogInfo("---ready to find hot area,hot area names is {0}", m_currentItem.hasHotArea ? m_currentItem?.hotAreaData?.hotArea.ToXml() : "empty");
        //m_hotAreaTrans.gameObject.SetActive(false);
        m_hotTrans = GetHotTrans();

        //to avoid screen is loced
        if (!m_currentItem.hasHotArea) SetPanelMaskEnable(true);
        else SetPanelMaskEnable(m_hotTrans != null);
        
        if (!m_hotTrans) return;

        m_originalEventTrigger = m_hotTrans.GetComponent<EventTriggerListener>();
        //找到约束物
        if (m_currentItem.hotAreaData.hasRestrainChild)
        {
            m_restrainChildHotTrans = Util.FindChild(m_hotTrans, m_currentItem.hotAreaData.restrainChild) as RectTransform;
            //Logger.LogInfo("---hot area hasRestrainChild,,restrainChild is {0} and finded trans is {1}", m_currentItem.hotAreaData.restrainChild, m_restrainChildHotTrans ? m_restrainChildHotTrans.GetPath() : "null");
        }

        m_transActiveWhenFinded = m_hotTrans.gameObject.activeInHierarchy;
        //set effect pos
        if (m_restrainChildHotTrans)
        {
            m_originalRestrainHasTrigger = m_restrainChildHotTrans.GetComponent<EventTriggerListener>();
            m_transPosWhenFinded = m_restrainChildHotTrans.position;
        }
        else m_transPosWhenFinded = m_hotTrans.position;

        Logger.LogInfo("---hot area update success,,hot area path is {0} transPos when finded is {1}", m_hotTrans.transform.GetPath(), m_transPosWhenFinded);
        m_updateHotAreaSucess = true;
        //UpdateHotImage(m_findedHotTrans);
        PlayHotEffect();
    }

    protected void RemoveLastRectCanvas()
    {
       // Logger.LogInfo("base panel with name {0} remove last hot canvas......", gameObject.name);
        //remove last hot area
        m_updateHotAreaSucess = false;
        //must remove graphicraycaster before canvas
        if (m_lastHotGR) DestroyImmediate(m_lastHotGR);
        if (m_lastHotCanvas) DestroyImmediate(m_lastHotCanvas);
        m_lastHotCanvas = null;
        m_lastHotGR = null;
    }

    #region find hot area

    protected RectTransform GetHotTrans()
    {
        if (!m_currentItem.hasHotArea) return null;
        //refresh hot area

        Window window = Window.GetOpenedWindow(m_currentItem.hotWindow);
        if (window == null) return null;
        
        Transform t = window.transform;
        if(m_currentItem.hasProtoArea)
        {
            var protoId = Module_Player.instance?.proto;
            string propArea = m_currentItem.GetProtoArea(protoId ?? 0);
            t = Util.FindChild(t,propArea);
        }

        foreach (var item in m_currentItem.hotAreaData.hotArea)
        {
            t = Util.FindChild(t,item);
            if (t == null) break;
        }
        if (!t || t == window.transform)
        {
            Logger.LogError("window {0} haven't a child with path: {1},please check out guide_config with id {2}", window.name, m_currentItem.hotAreaData.hotArea.ToXml(), moduleGuide.currentGuide.ID);
            return null;
        }

        m_lastHotScroll = t.GetComponent<ScrollRect>();
        m_lastScrollView = t.GetComponent<ScrollView>();
        m_enhanceScroll = t.GetComponent<EnhanceScrollView>();
        m_createObject = t.GetComponent<DynamicallyCreateObject>();
        //m_loopScrollRect = t.GetComponent<LoopScrollRect>();

        if (m_lastHotScroll)            return GetItemFromScroll(m_lastHotScroll.content);
        else if (m_lastScrollView)      return GetItemFromScroll(m_lastScrollView.content);
        else if (m_enhanceScroll)       return GetEnhanceScrollItem();
        else if(m_createObject)         return GetItemFromDyObject(m_createObject.transform as RectTransform);
        else if (INCLUDE_INACTIVE_HOTAREA.Contains(t.name))
        {
            BaseRestrain b = t.GetComponentInChildren<BaseRestrain>(true);
            m_updateCheckScrollItem = b == null;
            return b?.rectTransform;
        }
        else return t as RectTransform;
    }
    protected RectTransform GetItemFromDyObject(RectTransform itemParent)
    {
        var d = m_currentItem.hotAreaData;
        var type = d.restrainType;

        int checkId = 0;
        switch (type)
        {
            case EnumGuideRestrain.CheckID:
            case EnumGuideRestrain.Rune: checkId = d.restrainId; break;
            case EnumGuideRestrain.CurrentWeapon: checkId = (Module_Equip.instance == null || Module_Equip.instance.weapon == null) ? 0 : Module_Equip.instance.weapon.itemTypeId; break;
            case EnumGuideRestrain.ProtoID: checkId = d.GetProtoCheckId((byte)Module_Player.instance?.proto); break;
        }
        RectTransform t = null;
        if (d.canFindByCheckID && checkId != 0) t = GetScrollItemByCheckId(itemParent, checkId);
        m_updateCheckCreateItem = t == null;
        return t;
    }
    protected RectTransform GetItemFromScroll(RectTransform itemParent)
    {
        var d = m_currentItem.hotAreaData;
        var type = d.restrainType;

        int checkId = 0;
        switch (type)
        {
            case EnumGuideRestrain.CheckID:
            case EnumGuideRestrain.Rune:            checkId = d.restrainId;                                                                                                             break;
            case EnumGuideRestrain.CurrentWeapon:   checkId = (Module_Equip.instance == null || Module_Equip.instance.weapon == null) ? 0 : Module_Equip.instance.weapon.itemTypeId;    break;
            case EnumGuideRestrain.ProtoID:         checkId = d.GetProtoCheckId((byte)Module_Player.instance?.proto);break;
        }
        Logger.LogInfo("GetItemFromScroll---find hot area is a scroll item,will finde target item of scroll restrain type is {0},id is {1}", type, checkId);
        RectTransform t = null;
        if (d.canFindByCheckID && checkId != 0) t = GetScrollItemByCheckId(itemParent, checkId);
        else if (type == EnumGuideRestrain.Rune && m_currentItem.hotAreaData.validRuneCondition) t = GetScrollItemByParames(itemParent, d.restrainParames);
        else t = GetDefalutItem(itemParent);
        
        //if we don't find a valid hot area,then ,update hot area (scroll item)
        m_updateCheckScrollItem = t == null;
        return t;
    }

    protected RectTransform GetScrollItemByCheckId(RectTransform itemParent,int checkId)
    {
        if (!itemParent) return null;

        List<BaseRestrain> behaviours = GetComponentsInChildrenSubclassFirst(itemParent);
        foreach (var item in behaviours)
        {
            if (item.SameData(checkId) && item.gameObject.activeSelf) return item.rectTransform;
        }
        return null;
    }

    protected RectTransform GetScrollItemByParames(RectTransform itemParent,int[] parames)
    {
        Logger.LogInfo("to find rune item,id = {0},level = {1},star = {2}", parames[0], parames[1], parames[2]);
        List<BaseRestrain> behaviours = GetComponentsInChildrenSubclassFirst(itemParent);
        foreach (var item in behaviours)
        {
            if (item.SameData(parames) && item.gameObject.activeSelf) return item.rectTransform;
        }

        return null;
    }

    private List<BaseRestrain> GetComponentsInChildrenSubclassFirst(Transform parent)
    {
        List<BaseRestrain> subclassL = new List<BaseRestrain>();
        if (!parent) return subclassL;

        BaseRestrain[] behaviours = parent.GetComponentsInChildren<BaseRestrain>();
        if (behaviours == null || behaviours.Length == 0) return subclassL;

        Type baseType = typeof(BaseRestrain);
        List<BaseRestrain> baseL = new List<BaseRestrain>();
        foreach (var item in behaviours)
        {
            if (item.GetType().IsSubclassOf(baseType)) subclassL.Add(item);
            else baseL.Add(item);
        }
        if(baseL.Count > 0) subclassL.AddRange(baseL);
        return subclassL;
    }

    protected RectTransform GetDefalutItem(RectTransform itemParent)
    {
        if (itemParent == null) return null;
        
        Transform t = null;
        for (int i = 0, childCount = itemParent.childCount; i < childCount; i++)
        {
            t = itemParent.GetChild(i);
            if (t.gameObject.activeSelf)
            {
                BaseRestrain b = t.GetComponentInChildren<BaseRestrain>();
                if (b) return b.rectTransform;
                else return t as RectTransform;
            }
        }
        return null;
    }

    protected RectTransform GetEnhanceScrollItem()
    {
        var d = m_currentItem.hotAreaData;
        EnumGuideRestrain type = m_currentItem.hotAreaData.restrainType;
        int checkId = 0;
        switch (type)
        {
            case EnumGuideRestrain.CheckID:
            case EnumGuideRestrain.Rune:            checkId = d.restrainId; break;
            case EnumGuideRestrain.CurrentWeapon:   checkId = (Module_Equip.instance == null || Module_Equip.instance.weapon == null) ? 0 : Module_Equip.instance.weapon.itemTypeId; break;
            case EnumGuideRestrain.ProtoID:         checkId = d.GetProtoCheckId((byte)Module_Player.instance?.proto); break;
        }
        
        Logger.LogInfo("GetEnhanceScrollItem---find hot area is a scroll item,will finde target item of scroll restrain type is {0},id is {1}", type, checkId);
        RectTransform t = null;
        if ((type == EnumGuideRestrain.CheckID || type == EnumGuideRestrain.CurrentWeapon) && checkId != 0)
        {
            ShowEnhanceItem[] components = m_enhanceScroll.GetComponentsInChildren<ShowEnhanceItem>();
            foreach (var item in components)
            {
                if (item.ThisID.Equals(checkId))
                {
                    t = item.transform as RectTransform;
                    break;
                }
            }
        }
        else if (m_enhanceScroll.transform.childCount > 0)
        {
            for (int i = 0,count = m_enhanceScroll.transform.childCount ; i < count; i++)
            {
                Transform trans = m_enhanceScroll.transform.GetChild(i);
                if(trans.gameObject.activeSelf)
                {
                    t = trans as RectTransform;
                    break;
                }
            }
        }

        //if we don't find a valid hot area,then ,update hot area (scroll item)
        m_updateCheckScrollItem = t == null;
        return t;
    }

    #endregion

    #region lock scroll item
    
    protected void CheckScrollItem()
    {
        if(IsNeedRefindScrollItem())
        {
            Logger.LogInfo("---in update -------- after last frame ,continue to find hot area ...........");
            m_updateCheckScrollItem = false;
            OnStartAnimationEnd();
        }
        if(IsNeedRefindDyItem())
        {
            Logger.LogInfo("---in update -------- after last frame ,continue to find hot area ...........");
            m_updateCheckCreateItem = false;
            OnStartAnimationEnd();
        }
    }

    protected virtual void CheckEffect()
    {
        m_effectLockTrans = m_restrainChildHotTrans ? m_restrainChildHotTrans : m_hotTrans;
        if (m_effectLockTrans && m_effectTrans && m_transPosWhenFinded != m_effectLockTrans.position)
        {
            //Logger.LogInfo("---in update -------- after last frame ,to lock scrollitem position......lock obj is {0}.....", m_effectLockTrans);
            m_transPosWhenFinded = m_effectLockTrans.position;
            PlayHotEffect();
        }
    }
    private bool IsNeedRefindDyItem()
    {
        if (!m_updateCheckCreateItem) return false;
        bool validDy = m_createObject && m_createObject.transform.childCount == m_createObject.haveChildCount;
        return m_updateCheckCreateItem && validDy;
    }

    private bool IsNeedRefindScrollItem()
    {
        if (!m_updateCheckScrollItem) return false;

        bool validSRect = m_lastHotScroll && m_lastHotScroll.content.childCount > 0;
        bool validSView = m_lastScrollView && m_lastScrollView.content.childCount > 0;
        bool validEnhance = m_enhanceScroll && m_enhanceScroll.transform.childCount > 0;
        bool validFrameItem = m_currentItem != null && INCLUDE_INACTIVE_HOTAREA.Contains(m_currentItem.defalutHotArea);

        return m_updateCheckScrollItem && (validSRect || validSView || validEnhance || validFrameItem);
    }

    #endregion

    #region lock finded item

    protected void CheckHotAreaVisibleChange()
    {
        if(m_currentItem != null && m_hotTrans && m_hotTrans.gameObject.activeInHierarchy != m_transActiveWhenFinded)
        {
            m_transActiveWhenFinded = m_hotTrans.gameObject.activeInHierarchy;
            if(m_transActiveWhenFinded && m_lastHotCanvas) m_lastHotCanvas.overrideSorting = true;
        }
    }

    #endregion

    #region playEffect
    
    protected void PlayHotEffect()
    {
        //Logger.LogInfo("---on PlayHotEffect");
        if (m_currentItem.hotAreaData.hasEffect) PlayParticleEffect();
        else PlayUIEffect();
    }

    protected void PlayParticleEffect()
    {
        if (!m_hotTrans || m_currentItem == null|| !m_currentItem.hotAreaData.hasEffect) return;

        GameObject effect = GetEffectObj(m_currentItem.hotAreaData.effect);
        if (effect == null)
        {
            Logger.LogError("effect:{0} is null,please check out..", m_currentItem.hotAreaData.effect);
            return;
        }
        UIEffectManager.PlayEffect(transform as RectTransform, effect);
        effect.SetRealtimeParticle();
        m_effectTrans = effect.transform;
        m_effectTrans.position = GetCenterPoint(m_restrainChildHotTrans == null ? m_hotTrans : m_restrainChildHotTrans);
    }

    protected Vector3 GetCenterPoint(RectTransform rectTrans)
    {
        if (!rectTrans) return Vector3.zero;

        Vector3[] vs = new Vector3[4];
        rectTrans.GetWorldCorners(vs);
        Vector3 v = Vector3.zero;
        foreach (var item in vs)
        {
            v += item;
        }
        return v / 4;
    }

    protected GameObject GetEffectObj(string name)
    {
        if (!m_effcetDic.ContainsKey(name)) m_effcetDic.Add(name,Level.GetPreloadObject(name));
        return m_effcetDic[name];
    }

    protected void PlayUIEffect()
    {

    }
    #endregion

    public void OnGameDataReset()
    {
        gameObject.SetActive(false);
        ResetGuideData();
    }
    #endregion

    #endregion

}
