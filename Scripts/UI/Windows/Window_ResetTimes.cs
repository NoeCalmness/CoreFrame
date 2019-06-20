// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-04-03      14:52
//  *LastModify£º2019-04-03      15:01
//  ***************************************************************************************************/

#region

using UnityEngine.UI;

#endregion

public class Window_ResetTimes : Window
{
    private ChaseTask _chaseTask;
    private Text    contentTip;
    private Text    costCount;
    private Image   costIcon;
    private Text    currentDayCount;
    private Button  sureBtn;
    private Button  closeBtn;
    private Button  cancelBtn;

    /// <summary>
    /// Called when window added to level
    /// </summary>
    protected override void OnOpen()
    {
        isFullScreen = false;
        InitComponent();
        // Add initialize code here
        sureBtn.onClick.AddListener(OnSure);
        closeBtn.onClick.AddListener(() => Hide());
        cancelBtn.onClick.AddListener(() => Hide());
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        base.OnBecameVisible(oldState, forward);
        var data = GetWindowParam<Window_ResetTimes>();
        _chaseTask = data.param1 as ChaseTask;
        Refresh();
    }

    private void OnSure()
    {
        moduleChase.SendRestChallenge((ushort)_chaseTask.taskConfigInfo.ID);
        Hide();
    }

    protected void InitComponent()
    {
        contentTip        = GetComponent<Text>  ("payChallengeCount/kuang/content_tip");
        currentDayCount   = GetComponent<Text>  ("payChallengeCount/kuang/currentDayCount");
        costCount         = GetComponent<Text>  ("payChallengeCount/kuang/cost/icon/now");
        costIcon          = GetComponent<Image> ("payChallengeCount/kuang/cost");
        sureBtn           = GetComponent<Button>("payChallengeCount/kuang/sureBtn");
        closeBtn          = GetComponent<Button>("payChallengeCount/kuang/closebutton");
        cancelBtn         = GetComponent<Button>("payChallengeCount/kuang/unSureBtn");
    }

    private void Refresh()
    {
        if (null == _chaseTask)
            return;

        var index = 0;
        index = _chaseTask.taskData.resetTimes >= _chaseTask.taskConfigInfo.dayRemainCount - 1 ? _chaseTask.taskConfigInfo.dayRemainCount - 1 : _chaseTask.taskData.resetTimes;
        if (index < 0 || index > _chaseTask.taskConfigInfo.cost.Length - 1) return;

        var str = _chaseTask.taskConfigInfo.cost[index].Split('-');
        if (str.Length < 2)
            return;
        var prop = ConfigManager.Get<PropItemInfo>(Util.Parse<int>(str[0]));

        contentTip.text = Util.Format(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 30), str[1], prop.itemName);
        var remainCount = _chaseTask.taskConfigInfo.dayRemainCount - _chaseTask.taskData.resetTimes;
        currentDayCount.text = Util.Format(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 31), remainCount >= 0 ? remainCount : 0);
        AtlasHelper.SetItemIcon(costIcon, prop);

        var canPay = modulePlayer.roleInfo.diamond >= Util.Parse<int>(str[1]);
        var colorStr = GeneralConfigInfo.GetNoEnoughColorString(modulePlayer.roleInfo.diamond.ToString());
        Util.SetText(costCount, (int)TextForMatType.ChaseUIText, 32, str[1], canPay ? modulePlayer.roleInfo.diamond.ToString() : colorStr);

        sureBtn.interactable = canPay && remainCount > 0;
    }
}
