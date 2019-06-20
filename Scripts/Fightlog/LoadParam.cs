// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-09-17      15:23
//  * LastModify：2018-09-17      15:23
//  ***************************************************************************************************/
public interface ILoadParam_PVP
{
    int TimeLimit { get; }

    PMatchInfo[] players { get; }

    bool IsMatchRobot { get; }
    ulong MasterId { get; }
}

public class LoadParamPVP : ILoadParam_PVP
{
    private readonly Module_Match moduleMatch;
    public LoadParamPVP(Module_Match rModuleMatch)
    {
        moduleMatch = rModuleMatch;
    }

    public int TimeLimit        { get { return moduleMatch.timeLimit; } }
    public PMatchInfo[] players { get { return moduleMatch.players; } }
    public bool IsMatchRobot    { get { return moduleMatch.isMatchRobot; } }
    public ulong MasterId { get { return Module_Player.instance.id_; } }

}

public class LoadParamRecoverPVP : ILoadParam_PVP
{
    private readonly GameRecordDataPvp data;
    public LoadParamRecoverPVP(GameRecordDataPvp rRecoverData)
    {
        data = rRecoverData;
    }

    public int TimeLimit        { get { return data?.MatchInfomation.timeLimit ?? 0; } }
    public PMatchInfo[] players { get { return data?.MatchInfomation.infoList; } }
    public bool IsMatchRobot    { get { return data?.MatchInfomation.isRobot ?? false; } }
    public ulong MasterId { get { return data?.MasterId ?? 0; } }
}

public interface ILoadParam_Team
{
    int TimeLimit { get; }

    PTeamMemberInfo[] members { get; }

    ulong MasterId { get; }
}


public class LoadParamTeam : ILoadParam_Team
{
    private readonly Module_Team moduleTeam;
    public LoadParamTeam(Module_Team rModuleTeam)
    {
        moduleTeam = rModuleTeam;
    }

    public int TimeLimit { get { return moduleTeam.timeLimit; } }
    public ulong MasterId { get { return Module_Player.instance.id_; } }
    public PTeamMemberInfo[] members { get { return moduleTeam.members; } }
}

public class LoadParamRecoverTeam : ILoadParam_Team
{
    private readonly GameRecordDataTeam data;
    public LoadParamRecoverTeam(IGameRecordData rData)
    {
        data = rData as GameRecordDataTeam;
    }
    public int TimeLimit { get { return data?.Get<ScTeamStartLoading>()?.timeLimit ?? 0; } }
    public PTeamMemberInfo[] members { get { return data?.Get<ScTeamStartLoading>()?.members; } }
    public ulong MasterId { get { return data?.MasterId ?? 0; } }
}