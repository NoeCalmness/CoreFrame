// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-12-14      11:11
//  *LastModify：2018-12-14      11:11
//  ***************************************************************************************************/

using UnityEngine.UI;

public class NpcPrecast : AssertOnceBehaviour, IScrollViewData<ISourceItem>
{
    private ISourceItem _source;
    private Toggle toggle;
    private Text npcName;
    private Text endurance;
    private Text goodFeelingValue;
    private Image icon;

    protected override void Init()
    {
        toggle           = transform.GetComponent<Toggle>("toggle");
        npcName          = transform.GetComponent<Text>("name");
        endurance        = transform.GetComponent<Text>("power");
        goodFeelingValue = transform.GetComponent<Text>("goodFeeling_Text/goodFeelingValue");
        icon             = transform.GetComponent<Image>("bg/mask");

        Util.SetText(gameObject?.GetComponent<Text>("isNpc/npc_Txt"), ConfigText.GetDefalutString(9502, 3));
        Util.SetText(gameObject?.GetComponent<Text>("goodFeeling_Text"), ConfigText.GetDefalutString(9502, 5));
    }

    public ISourceItem GetItemData()
    {
        return _source;
    }

    public void SetToggleGroup(ToggleGroup rGroup)
    {
        if (toggle)
            toggle.group = rGroup;
    }

    public void InitData(ISourceItem rSource)
    {
        _source = rSource;
        RefreshInfo();
    }

    private void RefreshInfo()
    {
        AssertInit();

        Util.SetText(npcName         , _source.NpcInfo.name);
        Util.SetText(endurance       , $"{_source.NpcInfo.bodyPower}/{_source.NpcInfo.maxBodyPower}");
        Util.SetText(goodFeelingValue, $"+{_source.addPoint}");
        AtlasHelper.SetAvatar(icon, _source.NpcInfo.icon);
    }
}
