using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#region custom item class

public class BaseBattleItem : CustomSecondPanel
{
    public Text nameText { get; private set; }
    public Text contentText { get; private set; }
    public Transform arrowTrans { get; private set; }

    public CanvasGroup canvasGroup { get; private set; }

    private Vector2 m_size;
    public Vector2 size
    {
        get
        {
            if (m_size == Vector2.zero)
                m_size = new Vector2(rectTransform.rect.width, rectTransform.rect.height);

            return m_size;
        }
    }

    public BaseBattleItem(Transform trans) : base(trans)
    {
    }

    public override void InitComponent()
    {
        base.InitComponent();

        nameText = transform.GetComponent<Text>("background/panel_name/text_name");
        contentText = transform.GetComponent<Text>("background/panel_content/text_content");
        arrowTrans = transform.Find("background/button_next");
        canvasGroup = transform.GetComponentDefault<CanvasGroup>();
        canvasGroup.alpha = 1;
    }

    public override void SetPanelVisible(bool visible = true)
    {
        if (!visible) contentText.text = string.Empty;
        base.SetPanelVisible(visible);
    }

    public virtual void ResetItem()
    {
        nameText.text = string.Empty;
        contentText.text = string.Empty;
        arrowTrans.gameObject.SetActive(false);
    }

    //public void DoDialogBgTween()
    //{
    //    if (canvasGroup)
    //    {
    //        canvasGroup.alpha = 0f;
    //        DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1f, StoryConst.DIALOG_TWEEN_ALPHA_DURACTION).OnComplete(() =>
    //        {
    //            canvasGroup.alpha = 1f;
    //        });

    //        canvasGroup.transform.localScale = new Vector3(StoryConst.DIALOG_TWEEN_SCALE, StoryConst.DIALOG_TWEEN_SCALE, StoryConst.DIALOG_TWEEN_SCALE);
    //        canvasGroup.transform.DOScale(Vector3.one, StoryConst.DIALOG_TWEEN_SCALE_DURACTION).SetEase(Ease.InOutCubic);
    //    }
    //}
}

public class PortraitBattleItem : BaseBattleItem
{
    public Image portrait { get; private set; }

    public PortraitBattleItem(Transform trans) : base(trans)
    {
    }

    public override void InitComponent()
    {
        base.InitComponent();

        portrait = transform.GetComponent<Image>("background/portrait");
    }

    public override void ResetItem()
    {
        base.ResetItem();

        portrait.sprite = null;
    }
}

#endregion

public class BattleStory : BaseStory
{
    protected BaseBattleItem m_normalItem;
    protected PortraitBattleItem m_portraitItem;
    protected Creature m_tempFindedMonster;
    protected Creature m_lastChangeStateMonster;
    protected GameObject m_maskObj;

    //current battle story item is need to show portrait?
    protected bool portraitMode = false;

    #region override functions

    protected override void InitComponent()
    {
        base.InitComponent();
        
        Transform t = transform.Find("panel_scenario/panel_withoutportrait");
        m_normalItem = new BaseBattleItem(t);
        t = transform.Find("panel_scenario/panel_withportrait");
        m_portraitItem = new PortraitBattleItem(t);
        m_maskObj = transform.Find("mask").gameObject;
        m_fastForwardBtn = transform.GetComponent<Button>("fast_forward_btn");
        m_fastForwardBtn.SafeSetActive(false); 
        m_normalBtn = transform.GetComponent<Button>("normalplay_btn");
        m_normalBtn?.gameObject.SetActive(false);

        Text text = m_fastForwardBtn?.transform.GetComponent<Text>("Text");
        if (text) text.text = ConfigText.GetDefalutString(TextForMatType.StoryUIText, 0);
        text = m_normalBtn?.transform.GetComponent<Text>("Text");
        if (text) text.text = ConfigText.GetDefalutString(TextForMatType.StoryUIText, 1);

        ResetAllItems();
        moduleStory.ClearBattleCache();
        moduleStory.SetSwitchPointCamera(Level.current == null ? Camera.main : Level.current.mainCamera,UIManager.worldCamera);
    }
    

    /// <summary>
    /// 处理怪物状态
    /// </summary>
    protected override void HandleDelayPlayState()
    {
        base.HandleDelayPlayState();

        if (!m_tempFindedMonster || storyItem.talkingRoleStates == null || storyItem.talkingRoleStates.Length == 0)
            return;

        m_tempFindedMonster.stateMachine.TranslateTo(storyItem.talkingRoleStates[0].state);
        m_lastChangeStateMonster = m_tempFindedMonster;
    }
    #endregion

    #region reset functions

    protected void ResetAllItems()
    {
        m_normalItem.SetPanelVisible(false);
        m_portraitItem.SetPanelVisible(false);
    }
    

    /// <summary>
    /// 还原上一次播放过动画的怪物
    /// </summary>
    protected void ResetLastTranslatedMonster()
    {
        //if (m_lastChangeStateMonster && m_lastChangeStateMonster.health > 0 && !m_lastChangeStateMonster.stateMachine.currentState.isIdle)
        //    m_lastChangeStateMonster.stateMachine.TranslateTo(Module_AI.STATE_STAND_NAME);

        m_lastChangeStateMonster = null;
        m_tempFindedMonster = null;
    }

    #endregion

    #region refresh

    protected MonsterInfo GetMonsterInfo()
    {
        if (storyItem.talkingRoles == null || storyItem.talkingRoles.Length == 0 || !storyItem.showHeadIcon)
            return null;

        return ConfigManager.Get<MonsterInfo>(storyItem.talkingRoles[0].roleId);
    }

    protected void FindTalkingMonsters()
    {
        if (storyItem.talkingRoles == null || storyItem.talkingRoles.Length == 0)
            return;
        int monId = storyItem.talkingRoles[0].roleId;
        if (monId != 0)
        {   //怪物冒泡
            m_tempFindedMonster = ObjectManager.FindObject<MonsterCreature>(o => o.monsterId == monId && o.gameObject.activeSelf);
            moduleStory.UpdateBattleCache(m_tempFindedMonster, null);
        }
        else
        {   //角色冒泡
            var levelPve = Level.current as Level_PVE;
            m_tempFindedMonster = levelPve.player;
        }
     
        
    }

    protected void UpdateStoryAndMonster(bool portrait)
    {
        if (m_tempFindedMonster == null)
        {
            Logger.LogError("{2} : [battle] storyInfo id = {0} index = {1} connot find the taking roles ", storyId, curStoryItemIndex,Time.time);
            return;
        }

        BaseBattleItem item = portrait ? m_portraitItem : m_normalItem;
        item.SetPanelVisible(true);
        item.ResetItem();
        string monstername = string.Empty;
        if (m_tempFindedMonster is MonsterCreature)
        {
            monstername  = (m_tempFindedMonster as MonsterCreature).monsterInfo.name;
        }
        item.nameText.text = storyItem.needReplaceName ? moduleStory.GetReplaceName() : monstername;
        //item.DoDialogBgTween();
        moduleStory.UpdateBattleCache(m_tempFindedMonster, item);
    }

    protected void HandleMonsterIcon(MonsterInfo info)
    {
        if (info != null) AtlasHelper.SetShared(m_portraitItem.portrait,info.avatar); 
    }

    protected void HandleCameraMove()
    {
        Creature tar = null;
        if (storyItem.cameraLockRoleId == StoryConst.CAMERA_LOCK_ENDED_ID) ResetCameraMove();
        else if (storyItem.cameraLockRoleId != 0) tar = ObjectManager.FindObject<MonsterCreature>(o => o.monsterId == storyItem.cameraLockRoleId);
        else if (storyItem.cameraLockRoleId == 0) tar = m_tempFindedMonster;
        
        Camera_Combat.current?.SetFollowTransform(tar == null ? null : tar.transform);
    }

    protected void ResetCameraMove()
    {
        if (storyItem == null)
            Camera_Combat.current?.SetFollowTransform(null);
    }

    protected void HandleRoleTurn()
    {
        if (storyItem.turnLockRoleId == 0)
            return;

        Creature tar = null;
        if (storyItem.turnLockRoleId < 0)
        {
            if (Level.current && Level.current is Level_PVE) tar = (Level.current as Level_PVE).eventPlayer;
            if(!tar) tar = ObjectManager.FindObject<Creature>(o => o.isPlayer);
        }
        else tar = ObjectManager.FindObject<MonsterCreature>(o => o.monsterId == storyItem.turnLockRoleId && o.gameObject.activeSelf);

        if (tar == null) return;

        foreach (var item in moduleStory.battleStoryDic)
        {
            //怪物在玩家右边并且怪物当前朝向朝前
            if ((Module_AI.GetDirection(item.Key, tar) > 0 && item.Key.direction == CreatureDirection.FORWARD) ||
                (Module_AI.GetDirection(item.Key, tar) < 0 && item.Key.direction == CreatureDirection.BACK))
            {
                item.Key.TurnBack();
            }
        }
    }
    #endregion
    
    #region context functions

    protected void CheckContentTimeValid()
    {
        float contentDur = storyItem.contentDelayTime + storyItem.content.Length * GeneralConfigInfo.scontextInterval;
        if (storyItem.forceTime < contentDur)
            Logger.LogWarning("all content duractio is {0},and force content time is {1},please check out",contentDur,storyItem.forceTime);
    }

    #endregion
}
