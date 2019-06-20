using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PauseBattleStory : BattleStory
{
    private Text m_currentText;
    private bool m_handleCameraEvent = false;

    #region init
    protected override void InitComponent()
    {
        base.InitComponent();
        m_maskObj.SetActive(true);
        m_fastForwardBtn.SafeSetActive(true);
        EventTriggerListener.Get(m_maskObj).onClick = OnMaskClick;
    }

    protected override void AddEvent()
    {
        base.AddEvent();
        EventTriggerListener.Get(m_fastForwardBtn.gameObject).onClick = SwitchFastForward;
        EventTriggerListener.Get(m_normalBtn.gameObject).onClick = SwitchFastForward;
    }

    protected override void SwitchFastForward(GameObject sender)
    {
        base.SwitchFastForward(sender);
        
        CheckAutoNextDialog();
    }

    private void CheckAutoNextDialog()
    {
        bool isEnded = m_contextStep == EnumContextStep.End;
        //快进完成的时候，自动跳转
        if (fastForward && isEnded) ChangeToNextStoryItem();
    }

    protected override void Awake()
    {
        base.Awake();
        Root.instance.AddEventListener(Events.COMBAT_CAMERA_MOVE_CHANGE, OnCombatCameraMove);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if(Root.instance) Root.instance.RemoveEventListener(Events.COMBAT_CAMERA_MOVE_CHANGE, OnCombatCameraMove);
    }
    #endregion

    #region refresh per item
    protected override void ResetPerStoryItem()
    {
        base.ResetPerStoryItem();

        ResetAllItems();
        moduleStory.ClearBattleCache();
        ResetLastTranslatedMonster();
        portraitMode = false;
        m_currentText = null;
        m_handleCameraEvent = false;
    }

    protected override void OnUpdatePerStoryFail()
    {
        base.OnUpdatePerStoryFail();

        ResetCameraMove();
    }

    protected override void OnUpdatePerStorySuccess()
    {
        base.OnUpdatePerStorySuccess();

        CheckContentTimeValid();
        MonsterInfo showHeadNpcInfo = GetMonsterInfo();
        portraitMode = showHeadNpcInfo != null;
        FindTalkingMonsters();
        HandleMonsterIcon(showHeadNpcInfo);
        HandleCameraMove();
        HandleRoleTurn();
        //StartCheckWhenItemRefresh();
    }
    #endregion

    #region context functions
    protected override void HandleDelayContext()
    {
        base.HandleDelayContext();
        BaseBattleItem item = portraitMode ? m_portraitItem : m_normalItem;
        m_currentText = item.contentText;
        m_currentText.text = string.Empty;
        DoContentTextAnim(m_currentText, moduleStory.GetReplaceContent());
    }
    
    #endregion

    #region click event

    private void OnMaskClick(GameObject sender)
    {
        if (fastForward) return;
        if (moduleStory.combatCameraMoving) return;

        if (!isTeamMode) OnChangeToStoryStep();
        else ChangeTeamStoryStep();
    }

    private void OnChangeToStoryStep()
    {
        switch (m_contextStep)
        {
            //show all context
            case EnumContextStep.Show:
                ShowAllContextImme();
                break;

            //change to next story item
            case EnumContextStep.End:
                ChangeToNextStoryItem();
                break;
        }
    }

    private void ChangeTeamStoryStep()
    {
        if(m_contextStep == EnumContextStep.Show || m_contextStep == EnumContextStep.End) SendStoryStepToServer(m_contextStep);
    }

    protected override void OnRecvFrameData(EnumContextStep changeToStep)
    {
        base.OnRecvFrameData(changeToStep);
        OnChangeToStoryStep();
    }

    private void ShowAllContextImme()
    {
        m_ContentTween = null;
        DOTween.Kill(m_currentText);
        m_ContentTween = null;
        m_currentText.text = m_contentStrDic.Get(m_currentText);
        OnContextAnimEnd();
    }

    protected override void CheckCanChangeToNext()
    {
        base.CheckCanChangeToNext();
        bool isEnded = m_contextStep == EnumContextStep.End;
        BaseBattleItem item = portraitMode ? m_portraitItem : m_normalItem;
        item.arrowTrans.gameObject.SetActive(isEnded);
        CheckAutoNextDialog();
    }

    #endregion

    #region camera move

    private void OnCombatCameraMove(Event_ e)
    {
        if (Level.current is Level_PVE && storyItem != null && !m_tweenPlaying)
        {
            bool moving = (bool)e.param1;
            if (!moving && !m_handleCameraEvent)
            {
                m_handleCameraEvent = true;
                UpdateStoryAndMonster(portraitMode);
                StartCheckWhenItemRefresh();
            }
        }
    }

    #endregion
}
