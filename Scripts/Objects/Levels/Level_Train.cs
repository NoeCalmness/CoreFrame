// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-05-07      11:10
//  *LastModify：2019-05-07      11:10
//  ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Launch;

public class Level_Train : Level_PVE
{
    public const int TrainProtoID = 2;

    public override bool canPause => false;

    private float m_loadTime = 0;

    protected override void OnLoadStart()
    {
        base.OnLoadStart();
        Module_Story.instance.workOffline = true;

        m_loadTime = 0;

        DispatchEvent(Events.LAUNCH_PROCESS, Event_.Pop(LaunchProcess.ShowLevelStart, 1));
    }

    protected override List<string> OnBuildPreloadAssets(List<string> assets = null)
    {
        if (assets == null) assets = new List<string>();

        // Player shadow
        assets.Add(CombatConfig.sdefaultSelfShadow);

        // Player model
        var info = ConfigManager.Get<CreatureInfo>(TrainProtoID);
        if (info) assets.AddRange(info.models);

        // Player weapons
        Module_Battle.BuildWeaponPreloadAssets(TrainProtoID, info.gender, info.weaponID, info.weaponItemID, info.offWeaponID, info.offWeaponItemID, assets, true);


        assets.AddRange(OnBuildPVEAssets());
        int loadedCount = 0;
        foreach (var item in m_unitSceneEvents)
        {
            assets.AddRange(item.allAssets);
            loadedCount++;
            if (loadedCount >= MAX_LOADED_UNIT_SCENE_COUNT) break;
        }

        return assets;
    }

    protected override void OnCreateCreatures()
    {
        var info = ConfigManager.Get<CreatureInfo>(TrainProtoID);
        var skillInfos = ConfigManager.GetAll<SkillInfo>();
        info.skills = new PSkill[skillInfos.Count];
        for (var i = 0; i < skillInfos.Count; i++)
        {
            var p = PacketObject.Create<PSkill>();
            p.skillId = skillInfos[i].ID;
            p.level = 1;
            info.skills[i] = p;
        }
        m_player = Creature.Create(info, playerStart, new Vector3(0, 90, 0), true, "player", info.name);
        m_player.roleId = (ulong)info.ID;
        m_player.roleProto = TrainProtoID;

        _SetDOFFocusTarget(m_player.transform);
    }
    protected override void OnStageClear(SStageClearBehaviour behaviour)
    {
        //only send over msg one time
        if (m_isPVEFinish) return;

        EnterState(BattleStates.Winning);
        OnStageOver();
    }

    protected override void OnStageFail(SStageFailBehaviour behaviour)
    {
        //only send over msg one time
        if (m_isPVEFinish) return;

        EnterState(BattleStates.Losing);
        OnStageOver();
    }

    protected override bool WaitEndState()
    {
        return true;
    }

    protected override void OnEnterState(int oldMask, BattleStates state)
    {
        if (state == BattleStates.Ended)
        {
            PlayerPrefs.SetInt("FinishTrain", 1);

            m_loadTime = Time.realtimeSinceStartup - m_loadTime;

            DispatchEvent(Events.LAUNCH_PROCESS, Event_.Pop(LaunchProcess.ShowLevelEnd,  1));
            DispatchEvent(Events.LAUNCH_PROCESS, Event_.Pop(LaunchProcess.ShowLevelTime, m_loadTime));

            moduleStory.workOffline = false;

            DelayEvents.Add(() =>
            {
                moduleGlobal.FadeIn(1.0f, 1, false, () =>
                {
                    Root.instance.StartCoroutine(WaitLoginLoaded());
                });
            }, 1.5f);

            return;
        }
        base.OnEnterState(oldMask, state);
    }

    private IEnumerator WaitLoginLoaded()
    {
        yield return Game.LoadLevel(0, true, 2);
        moduleGlobal.FadeOut(0, 1.0f, true);
    }
}
