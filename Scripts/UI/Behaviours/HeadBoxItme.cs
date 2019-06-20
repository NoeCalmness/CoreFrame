using System;
using UnityEngine;
using UnityEngine.UI;

public class HeadBoxItme : MonoBehaviour
{
    private Image icon;
    private Text headBox_name;
    private Button lockBtn;
    private Image newLockImage;
    private GameObject slecton;

    [HideInInspector]

    public void IniteCompent()
    {
        slecton = transform.Find("click").gameObject;
        icon = transform.Find("headIcon").GetComponent<Image>();
        headBox_name = transform.Find("kuang_name").GetComponent<Text>();
        lockBtn = transform.Find("Lockbtn").GetComponent<Button>();
        newLockImage = transform.Find("newUnLock").GetComponent<Image>();
    }

    public void RefreshHeadBoxItem(PropItemInfo info)
    {
        IniteCompent();
        if (info == null) return;
        AtlasHelper.SetItemIcon(icon, info);
        headBox_name.text = info.itemName;

        var item = Module_Cangku.instance.GetItemByID(info.ID);
        lockBtn.SafeSetActive(item == null);

        if (lockBtn.gameObject.activeInHierarchy)
        {
            lockBtn.onClick.RemoveAllListeners();
            lockBtn.onClick.AddListener(() =>
            {
                Module_Global.instance.UpdateGlobalTip((ushort)info.ID, true, false);
            });
        }

        string GetID = Util.Format("{0},{1}", Module_Player.instance.roleInfo.roleId, info.ID);
        int ID = PlayerPrefs.GetInt(Util.Format("{0}", GetID));
        newLockImage.SafeSetActive(Module_Equip.instance.HasProp(info.ID) && (ID != info.ID && info.ID != 901) && ID != Module_Set.instance.SelectBoxID);
        slecton.SafeSetActive(info.ID == Module_Set.instance.SelectBoxID);
    }

    private int GetTextID(PropItemInfo info)
    {
        int count = Module_Active.instance.Achieveinfo.Count;
        for (int i = 0; i < count; i++)
        {
            if (Module_Active.instance.Achieveinfo[i].reward != null && Module_Active.instance.Achieveinfo[i].reward.rewardList != null)
            {
                if (Module_Active.instance.Achieveinfo[i].reward.rewardList.Length > 0 && Module_Active.instance.Achieveinfo[i].reward.rewardList[0].itemTypeId == info.ID)
                    return Module_Active.instance.Achieveinfo[i].desc;
            }
        }
        return 0;
    }
}
