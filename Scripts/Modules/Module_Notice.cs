// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-02-12      14:00
//  *LastModify：2019-02-12      14:00
//  ***************************************************************************************************/

using System.Collections.Generic;

public interface INoticeKey
{
    NoticeType Type { get; }
}

public enum NoticeType
{
    Pet,
    PetFeed,
    PetEvolve,
    PetFeedGroup,
    PetCompond,
    AwakeHeart,
    AwakeSkill,
    AwakeEnergy,
    AwakeAccompany,
    Soul,
    Sublimation,
}

public enum NoticeState
{
    NotComplete,
    Complete,
}

public struct NoticeDefaultKey : INoticeKey
{
    public NoticeType Type { get; }

    public NoticeDefaultKey(NoticeType rType)
    {
        Type = rType;
    }

    public override bool Equals(object obj)
    {
        if (obj?.GetType() != typeof (NoticeDefaultKey))
            return false;
        return ((NoticeDefaultKey)obj).Type == Type;
    }

    public override int GetHashCode()
    {
        return Type.GetHashCode();
    }
}

public class Module_Notice : Module<Module_Notice>
{
    private readonly Dictionary<INoticeKey, NoticeStateContent> cache = new Dictionary<INoticeKey, NoticeStateContent>();

    public bool GetCacheRead(INoticeKey key)
    {
        if (cache.ContainsKey(key))
        {
            if (!cache[key].complete) return false;
            if (!cache[key].isRead) return false;
        }
        return true;
    }

    public void SetNoticeState(INoticeKey rKey, bool rComplete)
    {
        NoticeStateContent content;
        if (cache.TryGetValue(rKey, out content))
        {
            content.complete = rComplete;
        }
        else
            cache.Add(rKey, new NoticeStateContent {complete = rComplete});
    }

    public void SetNoticeReadState(INoticeKey rKey, string rFlag = null)
    {
        NoticeStateContent content;
        if (cache.TryGetValue(rKey, out content))
        {
            content.isRead = content.complete;
            content.flag = rFlag;
        }
    }

    public bool IsNeedNotice(INoticeKey rKey, string flag = null)
    {
        NoticeStateContent content;
        if (cache.TryGetValue(rKey, out content))
            return content.complete && (!content.isRead || content.flag != flag);
        return false;
    }

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();
        cache.Clear();
    }

    private class NoticeStateContent
    {
        public bool complete;
        public bool isRead;
        public string flag;
    }
}
