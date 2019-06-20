public class FreeBattleStory : BattleStory
{
    #region init
    protected override void InitComponent()
    {
        base.InitComponent();
        m_maskObj.SetActive(false);
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
        UpdateStoryAndMonster(portraitMode);
        HandleMonsterIcon(showHeadNpcInfo);
        HandleRoleTurn();
        StartCheckWhenItemRefresh();
    }
    #endregion

    #region context functions

    protected override void HandleDelayContext()
    {
        base.HandleDelayContext();
        BaseBattleItem item = portraitMode ? m_portraitItem : m_normalItem;
        item.contentText.text = moduleStory.GetReplaceContent();
    }

    /// <summary>
    /// 强制时间到达后自动跳转到下一阶段
    /// </summary>
    protected override void CheckCanChangeToNext()
    {
        base.CheckCanChangeToNext();
        m_contextStep = EnumContextStep.End;
        ChangeToNextStoryItem();
    }
    #endregion
}
