// /**************************************************************************************************
//  * Copyright (C) 2017-2018 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-05-23      17:41
//  *LastModify：2019-05-23      17:41
//  ***************************************************************************************************/

using System.Diagnostics;
using System.Text.RegularExpressions;

public static class FightStatistic
{
    public struct Entry
    {
        public int pvpTimes;
        public int pveTimes;
        public int pvpAsyncTimes;
        public int pveAsyncTimes;
    }

    #region 统计
    /// <summary>
    /// 总次数+1
    /// </summary>
    [Conditional("FIGHT_TIMES_STAT")]
    public static void AddPvpFightTimes()
    {
        var values = GetStatictValues();
        values.pvpTimes += 1;
        SaveStaticData(values);
    }

    public static void AddPveFightTimes()
    {
        var values = GetStatictValues();
        values.pveTimes += 1;
        SaveStaticData(values);
    }

    /// <summary>
    /// 不同步次数+1
    /// </summary>
    [Conditional("FIGHT_TIMES_STAT")]
    public static void AddPvpAsyncFightTimes()
    {
        var values = GetStatictValues();
        values.pvpAsyncTimes += 1;
        SaveStaticData(values);
    }
    [Conditional("FIGHT_TIMES_STAT")]
    public static void AddPveAsyncFightTimes()
    {
        var values = GetStatictValues();
        values.pveAsyncTimes += 1;
        SaveStaticData(values);
    }

    private static Entry GetStatictValues()
    {
        var e = new Entry();

        var bytes = Util.LoadFile("GameRecorder/战斗统计.txt");
        if (bytes == null || bytes.Length == 0)
        {
            Logger.LogWarning("没有统计文件，从0开始统计");

            return e;
        }
        var s = System.Text.Encoding.UTF8.GetString(bytes);
        Logger.LogWarning($"开始匹配:{s}");

        var m = new Regex(@"pvp对局数\s*:\s*([0-9]+)");
        var r = m.Match(s);
        if(r.Success)
            e.pvpTimes = Util.Parse<int>(r.Groups[1].Value);

        m = new Regex(@"pvp不同步局数\s*:\s*([0-9]+)");
        r = m.Match(s);
        if(r.Success)
            e.pvpAsyncTimes = Util.Parse<int>(r.Groups[1].Value);

        m = new Regex(@"pve对局数\s*:\s*([0-9]+)");
        r = m.Match(s);
        if(r.Success)
            e.pveTimes = Util.Parse<int>(r.Groups[1].Value);

        m = new Regex(@"pve不同步局数\s*:\s*([0-9]+)");
        r = m.Match(s);
        if(r.Success)
            e.pveAsyncTimes = Util.Parse<int>(r.Groups[1].Value);

        return e;
    }

    private static void SaveStaticData(Entry e)
    {
        string s = @"总对局数：{0} 不同步局数：{1} 不同步几率:{2:P}
pvp对局数:{3} pvp不同步局数:{4} pvp不同步几率:{5:P}
pve对局数:{6} pve不同步局数:{7} pve不同步几率:{8:P}";

        var str = Util.Format(s, e.pvpTimes + e.pveTimes, e.pvpAsyncTimes + e.pveAsyncTimes, e.pveTimes + e.pvpTimes == 0 ? 0 : (float)(e.pvpAsyncTimes + e.pveAsyncTimes)/(e.pveTimes + e.pvpTimes),
                                 e.pvpTimes, e.pvpAsyncTimes, e.pvpTimes == 0 ? 0 : ((float)e.pvpAsyncTimes)/e.pvpTimes,
                                 e.pveTimes, e.pveAsyncTimes, e.pveTimes == 0 ? 0 :((float)e.pveAsyncTimes)/e.pveTimes);
        Logger.LogWarning(str);
        Util.SaveFile("GameRecorder/战斗统计.txt", System.Text.Encoding.UTF8.GetBytes(str));
    }

    #endregion
}
