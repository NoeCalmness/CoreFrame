using System;
using System.Collections.Generic;
using UnityEngine;

public interface IExp
{
    /// <summary>
    /// 获得的经验值
    /// </summary>
    uint gainExp { get; set; }

    /// <summary>
    /// 当前拥有的数量
    /// </summary>
    uint ownNum { get; set; }

    /// <summary>
    /// 需要使用的数量
    /// </summary>
    int useNum { get; set; }
}

public static class ExpUtil
{
    public static List<IExp> GetValidExps(List<IExp> exps,uint totalExp)
    {
        List<IExp> list = new List<IExp>();

        //use back up with references
        List<IExp> oriList = new List<IExp>(exps);
        int total = (int)totalExp;

        while (total > 0 && oriList.Count > 0)
        {
            //get data
            object[] datas = GetNearestIExp(oriList, total);
            IExp data = datas[0] as IExp;
            int useNum = (int)datas[1];

            if (!list.Contains(data)) list.Add(data);
            data.useNum += useNum;

            total -= (int)data.gainExp * useNum;
            if (data.ownNum == data.useNum) oriList.Remove(data);
        }

        return list;
    }

    /// <summary>
    /// 获取最接近目标经验值的道具
    /// </summary>
    /// <param name="exps"></param>
    /// <param name="totalExp"></param>
    /// <returns></returns>
    private static object[] GetNearestIExp(List<IExp> exps,int totalExp)
    {
        int minExp = int.MaxValue;

        int useNum = 0;
        IExp tar = null;

        foreach (var item in exps)
        {
            int totalNum = totalExp / (int)item.gainExp;
            int restExp = totalExp % (int)item.gainExp;
            //真实的拥有数量
            int realOwnNum = (int)item.ownNum - item.useNum;

            //本身数量不够
            if (totalNum > realOwnNum) restExp += (totalNum - realOwnNum) * (int)item.gainExp;
            else if(totalNum == 0)
            {
                totalNum = 1;
                restExp = totalExp - (int)item.gainExp; 
            }

            if(Mathf.Abs(restExp) < Mathf.Abs(minExp))
            {
                minExp = restExp;
                tar = item;
                useNum = totalNum > realOwnNum ? realOwnNum : totalNum;
            }
        }

        return new object[] {tar,useNum };
    }

    public static List<IExp> GetValidExps(List<PropItemInfo> props, uint totalExp)
    {
        List<IExp> list = new List<IExp>();
        list.AddRange(props);
        return GetValidExps(list, totalExp);
    }

    public static List<IExp> GetValidExps(IExp[] exps, uint totalExp)
    {
        List<IExp> list = new List<IExp>();
        list.AddRange(exps);
        return GetValidExps(list,totalExp);
    }

    public static List<IExp> GetValidExps(List<PItem> exps, uint totalExp)
    {
        return GetValidExps(GetExpsList(exps),totalExp);
    }

    public static Dictionary<PItem,int> GetValidPItems(List<PItem> exps, uint totalExp)
    {
        Dictionary<PItem, int> dic = new Dictionary<PItem, int>();
        List<IExp> iexps = GetValidExps(GetExpsList(exps), totalExp);

        foreach (var e in iexps)
        {
            PItem item = exps.Find(o => o.itemTypeId == ((PropItemInfo)e).ID);
            if(!dic.ContainsKey(item)) dic.Add(item,e.useNum);
        }
        return dic;
    }

    public static List<IExp> GetValidExps(PItem[] exps, uint totalExp)
    {
        List<PItem> list = new List<PItem>();
        list.AddRange(exps);
        return GetValidExps(list, totalExp);
    }

    private static List<IExp> GetExpsList(List<PItem> datas)
    {
        List<IExp> list = new List<IExp>();
        foreach (var item in datas)
        {
            PropItemInfo info = item?.GetPropItem();
            if (!info) continue;

            info.ownNum = item.num;
            info.useNum = 0;
            list.Add(info);
        }
        return list;
    }

    private static List<IExp> GetExpsList(PItem[] datas)
    {
        List<PItem> list = new List<PItem>();
        list.AddRange(datas);
        return GetExpsList(list);
    }
}
