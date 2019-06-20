using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AwardGetSucced : MonoBehaviour
{
    private int AwardNum = -1;
    private List<GameObject> objlist = new List<GameObject>();


    public void SetAward(PReward Info, List<GameObject> list, bool pos = true)
    {
        if (Info == null) return;

        SetDetailInfo(Info.expr, Info.activePoint, Info.fatigue, Info.coin, Info.diamond, list, pos, Info.rewardList);
    }

    public void SetAward(TaskInfo.TaskStarReward Info, List<GameObject> list, bool pos = true)
    {
        if (Info == null) return;

        SetDetailInfo(Info.expr, Info.activePoint, Info.fatigue, Info.coin, Info.diamond, list, pos, null,Info.props);
    }
    private void SetDetailInfo(int expr, int activePoint, int fatigue, int coin, int diamond, List<GameObject> list, bool pos, PItem2[] rewardList = null, TaskInfo.TaskStarProp[] coop = null)
    {
        AwardNum = -1;
        objlist = list;
        for (int i = 0; i < objlist.Count; i++)
        {
            objlist[i].gameObject.SetActive(false);
        }

        //0 日常 1 宝箱提示 2 成就  3 领取成功
        if (expr != 0)//经验
        {
            SetInfo(expr, PropItemInfo.exp);
        }
        if (activePoint != 0)//活跃点
        {
            SetInfo(activePoint, PropItemInfo.activepoint);
        }
        if (fatigue != 0)//tili
        {
            var prop = ConfigManager.Get<PropItemInfo>(15);
            if (prop) SetInfo(fatigue, prop);
        }

        if (coin != 0)
        {
            PropItemInfo propinfo = ConfigManager.Get<PropItemInfo>(1);
            SetInfo(coin, propinfo);

        }
        if (diamond != 0)
        {
            PropItemInfo propinfo = ConfigManager.Get<PropItemInfo>(2);
            SetInfo(diamond, propinfo);

        }
        if (rewardList != null)
        {
            for (int i = 0; i < rewardList.Length; i++)
            {
                if (rewardList[i] != null)
                {
                    var ward = rewardList[i];
                    PropItemInfo propinfo = ConfigManager.Get<PropItemInfo>(ward.itemTypeId);

                    SetInfo((int)ward.num, propinfo, ward.level, ward.star);
                }
            }
        }
        else if (coop != null)
        {
            for (int i = 0; i < coop.Length; i++)
            {
                if (coop[i] != null)
                {
                    var ward = coop[i];
                    PropItemInfo propinfo = ConfigManager.Get<PropItemInfo>(ward.propId);

                    SetInfo(ward.num, propinfo, ward.level, ward.star);
                }
            }
        }

        if (pos) SetPostion(AwardNum, objlist);
    }

    private void SetInfo(int num, PropItemInfo Info, int level = 0, int star = 1)
    {
        if (Info != null)
        {
            if (AwardNum < objlist.Count - 1)
            {
                AwardNum++;
                GameObject obj = objlist[AwardNum];
                obj.gameObject.SetActive(true);
                Util.SetItemInfo(obj, Info, level, num, false, star);

                SetBtnClick(obj, (ushort)Info.ID);
            }
        }
    }

    public void SetUnionAward(List<GameObject> objUnionList, int[] itemList)
    {
        AwardNum = -1;
        for (int i = 0; i < objUnionList.Count; i++)
        {
            objUnionList[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < itemList.Length; i++)
        {
            if (i >= objUnionList.Count) continue;
            AwardNum++;
            PropItemInfo propinfo = ConfigManager.Get<PropItemInfo>(itemList[i]);
            Util.SetItemInfoSimple(objUnionList[i], propinfo);
            objUnionList[i].gameObject.SetActive(true);

            var obj = objUnionList[i];
            SetBtnClick(obj, (ushort)propinfo.ID);
        }
        SetPostion(AwardNum, objUnionList);
    }

    private void SetBtnClick(GameObject obj,ushort itemTypeId)
    {
        var btn = obj.transform.GetComponentDefault<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(delegate
        {
            Module_Global.instance.UpdateGlobalTip(itemTypeId, true, false);
        });
    }
    
    private void SetPostion(int num, List<GameObject> list)
    {
        //设置坐标
        if (num == 0 && list.Count > 0)
        {
            list[0].GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        }
        else if (num == 1 && list.Count > 1)
        {
            list[0].GetComponent<RectTransform>().anchoredPosition = new Vector3(-55, 0, 0);
            list[1].GetComponent<RectTransform>().anchoredPosition = new Vector3(55, 0, 0);
        }
        else if (num == 2 && list.Count > 2)
        {
            list[0].GetComponent<RectTransform>().anchoredPosition = new Vector3(-110, 0, 0);
            list[1].GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            list[2].GetComponent<RectTransform>().anchoredPosition = new Vector3(110, 0, 0);
        }
        else if (num == 3 && list.Count > 3)
        {
            list[0].GetComponent<RectTransform>().anchoredPosition = new Vector3(-165, 0, 0);
            list[1].GetComponent<RectTransform>().anchoredPosition = new Vector3(-55, 0, 0);
            list[2].GetComponent<RectTransform>().anchoredPosition = new Vector3(55, 0, 0);
            list[3].GetComponent<RectTransform>().anchoredPosition = new Vector3(165, 0, 0);
        }
    }
}