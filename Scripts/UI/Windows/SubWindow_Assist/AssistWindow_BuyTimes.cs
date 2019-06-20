// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-12-13      20:11
//  *LastModify：2018-12-13      20:11
//  ***************************************************************************************************/

using UnityEngine.UI;

public class AssistWindow_BuyTimes : SubWindowBase<Window_Assist>
{

    private Text    contentTip;
    private Text    currentDayCount;
    private Text    costCount;
    private Image   costIcon;
    private Button  sureBtn;
    private Button  cancelBtn;
    private Button  closeBtn;

    private ChaseTask _chaseTask;
    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;
        if(p.Length > 0)
            _chaseTask = p[0] as ChaseTask;
        Refresh();
        sureBtn  ?.onClick.AddListener(OnSure);
        cancelBtn?.onClick.AddListener(()=>UnInitialize());
        closeBtn ?.onClick.AddListener(()=>UnInitialize());
        return true;
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;

        sureBtn.onClick.RemoveAllListeners();
        return true;
    }

    private void OnSure()
    {
        moduleChase.SendRestChallenge((ushort)_chaseTask.taskConfigInfo.ID);
        UnInitialize();
    }

    protected override void InitComponent()
    {
        base.InitComponent();
        
        contentTip        = WindowCache.GetComponent<Text>  ("level_panel/payChallengeCount/kuang/content_tip");
        currentDayCount   = WindowCache.GetComponent<Text>  ("level_panel/payChallengeCount/kuang/currentDayCount");
        costCount         = WindowCache.GetComponent<Text>  ("level_panel/payChallengeCount/kuang/cost/icon/now");
        costIcon          = WindowCache.GetComponent<Image> ("level_panel/payChallengeCount/kuang/cost");
        sureBtn           = WindowCache.GetComponent<Button>("level_panel/payChallengeCount/kuang/sureBtn");
        closeBtn          = WindowCache.GetComponent<Button>("level_panel/payChallengeCount/kuang/closebutton");
        cancelBtn         = WindowCache.GetComponent<Button>("level_panel/payChallengeCount/kuang/unSureBtn");
    }

    private void Refresh()
    {
        if (null == _chaseTask)
            return;

        int index = 0;
        index = _chaseTask.taskData.resetTimes >= _chaseTask.taskConfigInfo.dayRemainCount - 1 ? _chaseTask.taskConfigInfo.dayRemainCount - 1 : _chaseTask.taskData.resetTimes;
        if (index < 0 || index > _chaseTask.taskConfigInfo.cost.Length - 1) return;

        string[] str = _chaseTask.taskConfigInfo.cost[index].Split('-');
        if (str.Length < 2)
            return;
        var prop = ConfigManager.Get<PropItemInfo>(Util.Parse<int>(str[0]));

        contentTip.text = Util.Format(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 30), str[1], prop.itemName);
        int remainCount = _chaseTask.taskConfigInfo.dayRemainCount - _chaseTask.taskData.resetTimes;
        currentDayCount.text = Util.Format(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 31), remainCount >= 0 ? remainCount : 0);
        AtlasHelper.SetItemIcon(costIcon, prop);

        bool canPay = modulePlayer.roleInfo.diamond >= Util.Parse<int>(str[1]);
        string colorStr = GeneralConfigInfo.GetNoEnoughColorString(modulePlayer.roleInfo.diamond.ToString());
        Util.SetText(costCount, (int)TextForMatType.ChaseUIText, 32, str[1], canPay ? modulePlayer.roleInfo.diamond.ToString() : colorStr);

        sureBtn.interactable = canPay && remainCount > 0;
    }
}
