using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatTipItem
{
    public GameObject handObj { get; private set; }
    public GameObject arrowObj { get; private set; }
    public Vector3 position { get; private set; }
    public Vector3 rotation { get; private set; }
    private bool m_needResetPos = false;

    public CombatTipItem(GameObject hand,GameObject arrow,Vector3 pos,Vector3 rot,bool resetPos = false)
    {
        handObj             = hand;
        arrowObj            = arrow;
        position            = pos;
        rotation            = rot;
        m_needResetPos      = resetPos;
    }

    public void SetItemVisible(bool active)
    {
        if (handObj)    handObj.SetActive(active);
        if (arrowObj)   arrowObj.SetActive(active);
        if (active)
        {
            if (handObj && m_needResetPos) handObj.transform.localPosition = position;
            if (arrowObj) arrowObj.transform.localEulerAngles = rotation;
        }
    }
}

public class ForceGuide : GuideBase
{
    public const string MOVE_PAD_NAME = "movementPad";
    private const int CLICK = 1, RIGHT = 2, UP = 3, DOWN = 4,POWER = 7;

    protected override int m_guideSorting { get { return 4; } }
    private Module_Guide m_moduleGuide;
    private Dictionary<int, CombatTipItem> m_combatTipDic = new Dictionary<int, CombatTipItem>();

    #region default monobehaviour funcs

    protected override void Awake()
    {
        base.Awake();
        InitCombatItem();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        EventManager.AddEventListener(Module_Guide.EventUnlockFunctionComplete, UnlockComplete);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventManager.RemoveEventListener(Module_Guide.EventUnlockFunctionComplete, UnlockComplete);
    }

    #endregion

    #region refresh
    protected override void OnRefreshGuide()
    {
        base.OnRefreshGuide();

        SetPanelMaskEnable(false);
        UnlockFunctionToWindowHome();
    }

    protected override void OnRefreshGuideEnd()
    {
        base.OnRefreshGuideEnd();
        if (m_currentItem != null && Window.GetOpenedWindow(m_currentItem.hotWindow) == null)
        {
            Logger.LogWarning("{0} has not been opened, fresh guide will be executed when window be opened", m_currentItem.hotWindow);
            return;
        }
        m_countDownTime = 0f;
        bUseGuideProtection = false;
        if (m_currentItem.hasHotArea && !hotTransValid)
        {
            bUseGuideProtection = true;
            RefreshCountDownTime();
        }

        if (!m_hotTrans) return;

        UpdateCanvas();
        UpdateContent();
        UpdateTipIcon();
        RefreshCombatTip();
    }

    public override void ResetGuideData()
    {
        base.ResetGuideData();

        foreach (var item in m_combatTipDic)
        {
            if (item.Value == null) continue;
            item.Value.SetItemVisible(false);
        }
    }

    protected override void SetPanelMaskEnable(bool active)
    {
        base.SetPanelMaskEnable(active);

        m_tipObj.SetActive(active);
        m_tipIcon.gameObject.SetActive(active);
        m_combatObj.gameObject.SetActive(active && m_currentItem!= null && m_currentItem.hasCondition);

        SetMaskColor(active);
        m_maskImage.raycastTarget = active;
    }

    private void SetMaskColor(bool active)
    {
        Color c = m_maskImage.color;
        c.a = active && !maskFade ? 0.75f : 0f;
        m_maskImage.color = c;
    }

    private void UpdateCanvas()
    {
        m_lastHotCanvas = m_hotTrans.GetComponentDefault<Canvas>();
        m_lastHotCanvas.overrideSorting = true;
        m_lastHotCanvas.sortingLayerName = GUIDE_SORTING_LAYER;
        m_lastHotCanvas.sortingOrder = m_guideSorting + 1;

        if(!m_currentItem.hotAreaData.isTipHotArea) m_lastHotGR = m_lastHotCanvas.GetComponentDefault<GraphicRaycaster>();
        if (!m_currentItem.hasCondition && !m_currentItem.hotAreaData.isTipHotArea)
        {
            GameObject o = m_restrainChildHotTrans ? m_restrainChildHotTrans.gameObject : m_hotTrans.gameObject;
            EventTriggerListener.Get(o).onClick -= OnHotAreaClick;
            EventTriggerListener.Get(o).onClick += OnHotAreaClick;

            //we need add EventTriggerListener component to m_findedHotTrans obj to avoid scroll drag
            if (m_restrainChildHotTrans) m_hotTrans.gameObject.AddComponent<EventTriggerListener>();
        }
    }

    private const float scaler_x = 1280 / 1476f;
    protected void UpdateContent()
    {
        //Logger.LogInfo("---on UpdateContent");
        m_tipObj.SetActive(!string.IsNullOrEmpty(m_currentItem.content));
        m_tipText.text = m_currentItem.content;
        //convert 1476 * 720f => 1280 * 720
        Vector2 tar_pos = m_currentItem.contentPos;
        tar_pos.x *= scaler_x;
        Vector2 realRes = UIManager.realViewResolution;
        CanvasScaler scaler = UIManager.canvasScaler;
        float scale = 1f;
        float diff = 0;
        if (UIManager.aspect != UIManager.refAspect && scaler != null )
        {
            if(UIManager.aspect < UIManager.refAspect)
            {
                //scale y 
                scale = realRes.x / scaler.referenceResolution.x;
                diff = realRes.y - scaler.referenceResolution.x * scale;
                tar_pos.y *= diff / scaler.referenceResolution.y + 1;
            }
            else
            {
                scale = realRes.y / scaler.referenceResolution.y;
                diff = realRes.x - scaler.referenceResolution.x * scale;
                tar_pos.x *= diff / scaler.referenceResolution.x + 1;
            }
        }

        if (m_tipObj.activeSelf) m_tipObj.rectTransform().anchoredPosition = tar_pos;
    }

    protected void UpdateTipIcon()
    {
        //Logger.LogInfo("---on UpdateTipIcon");
        //bool isShowIcon = m_currentItem.iconInfo != null && !string.IsNullOrEmpty(m_currentItem.iconInfo.icon);
        m_tipIcon.gameObject.SetActive(false);
    }
    #endregion

    #region combat tip

    private void InitCombatItem()
    {
        m_combatTipDic.Clear();
        m_combatTipDic.Add(CLICK,   new CombatTipItem(m_clickObj, null, Vector3.zero,Vector3.zero,true));
        m_combatTipDic.Add(RIGHT,   new CombatTipItem(m_rightHand,m_arrowTrans.gameObject, Vector3.zero, Vector3.zero));
        m_combatTipDic.Add(UP,      new CombatTipItem(m_upHand, m_arrowTrans.gameObject, Vector3.zero, new Vector3(0f,0f,90f)));
        m_combatTipDic.Add(DOWN,    new CombatTipItem(m_downHand, m_arrowTrans.gameObject, Vector3.zero, new Vector3(0f, 0f, -90f)));
        m_combatTipDic.Add(POWER,   new CombatTipItem(m_pressObj, null, Vector3.zero, Vector3.zero, true));
    }

    private void RefreshCombatTip()
    {
        bool hasCondition = m_currentItem.hasCondition;
        m_combatObj.SetActive(hasCondition);
        if (hasCondition)
        {
            m_combatObj.transform.localPosition = m_currentItem.successCondition.tipPos;
            SetClickOrSlide(m_currentItem.defalutHotArea.Equals(MOVE_PAD_NAME));

            string c = NeedLockMovepadClickPos();
            if (!string.IsNullOrEmpty(c)) SetMovePadPos(c);
        }
    }

    private void SetClickOrSlide(bool isClick = false)
    {
        m_clickObj.SetActive(isClick);
        m_slideObj.SetActive(!isClick);
        m_handSlide.SetActive(!isClick);

        if (!isClick) RefreshSlideTween();
    }

    private string NeedLockMovepadClickPos()
    {
        if (!m_currentItem.hasCondition || !m_currentItem.defalutHotArea.Equals(MOVE_PAD_NAME)) return string.Empty;

        var type = m_currentItem.successCondition.type;
        var parames = m_currentItem.successCondition.intParams;
        int key = parames != null && parames.Length > 0 ? parames[0] : 0;

        if (type == EnumGuideCondition.Position) return "right";
        else if (type == EnumGuideCondition.InputKey) return key == 7 ? "left" : key == 6 ? "right" : string.Empty;
        return string.Empty;
    }

    private void SetMovePadPos(string childName)
    {
        Logger.LogDetail("battle guide lock the movepad child is {0}",childName);
        Window_Combat w = Window.GetOpenedWindow<Window_Combat>();
        if (!w) return;

        RectTransform t = w.transform.Find(Util.Format("{0}/{1}", MOVE_PAD_NAME,childName)) as RectTransform;
        if (!t) return;

        m_combatObj.transform.position = GetCenterPoint(t);
    }

    private void RefreshSlideTween()
    {
        foreach (var item in m_combatTipDic)
        {
            item.Value.SetItemVisible(false);
        }
        int key = m_currentItem.successCondition.intParams[0];
        if (m_combatTipDic.ContainsKey(key)) m_combatTipDic[key].SetItemVisible(true);
    }

    #endregion

    #region unlock functions
    private void UnlockFunctionToWindowHome()
    {
        //Logger.LogDetail("force panel UnlockFunctionToWindowHome...");
        if (!m_moduleGuide) m_moduleGuide = Module_Guide.instance;

        if (m_moduleGuide && m_currentItem.hasUnlockFuncs)
        {
            m_maskImage.raycastTarget = true;
            SetMaskColor(false);
            if (m_currentItem.playUnlockAudio) AudioManager.PlaySound(m_currentItem.unlockAudio);
            m_moduleGuide.PlayUnlockNewFuncAnimation(m_currentItem.unlockFunctionId);
        }
        else UnlockComplete();
    }

    private void UnlockComplete()
    {
        //Logger.LogDetail("force panel UnlockComplete...");
        SetPanelMaskEnable(true);
        OnStartAnimationEnd();
    }

    #endregion
    
    #region check valid
    
    private float m_countDownTime = 0;
    
    private void UpdateHotAreaTimeout()
    {
        if (m_countDownTime <= 0)
        {
            //如果界面开启之后有任何一次其他逻辑导致热点区域被关闭，那么该强制引导需要被关闭
            RefreshCountDownTime();
            return;
        }

        if (m_countDownTime > 0 && (m_countDownTime -= Time.deltaTime) <= 0)
        {
            Logger.LogError("引导超时保护，自动结束 ID：{0} Index: {1} HotValid: {2} RestrainValid:{3}", moduleGuide.currentGuide.ID, moduleGuide.currentGuideIndex, hotTransValid, restrianTransValid);

            //超时处理
            ResetGuideData();
            RefreshOnGuideEnd();
            moduleGuide.HandleGuideStoryGiveData();
            moduleGuide.HandleGuideInfoEnd();
        }
    }

    private void RefreshCountDownTime()
    {
        var protectTime = m_currentItem == null || m_currentItem.protectTime <= 0 ? GeneralConfigInfo.sguideInvalidTime : m_currentItem.protectTime;
        m_countDownTime = hotTransValid ? 0 : protectTime;
        if (m_countDownTime == 0 && m_currentItem != null && m_currentItem.hotAreaData != null && m_currentItem.hotAreaData.hasRestrainChild)
            m_countDownTime = restrianTransValid ? 0 : protectTime;

        if (hotTransValid)
        {
            var pos = UIManager.worldCamera.WorldToViewportPoint(m_hotTrans.position);
            var valid = pos.x >= 0.0f && pos.x <= 1.0f && pos.y >= 0.0f &pos.y <= 1.0f;
            m_countDownTime = valid ? 0 : protectTime;
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

    protected override void Update()
    {
        base.Update();
        if(bUseGuideProtection)
            UpdateHotAreaTimeout();
        else
        {
            if(m_hotTrans != null && !hotTransValid)
            {
                bUseGuideProtection = true;
                RefreshCountDownTime();
            }
        }
    }

    #endregion
}
