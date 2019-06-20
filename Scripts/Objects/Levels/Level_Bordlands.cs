/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-08-09
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class Level_Bordlands : Level_3DClick
{
    public const float SEND_POS_INTERVAL= 3.0f;
    private Vector3 lastPos = Vector3.zero;
    private float m_sendMsgTime = 0f;

    protected override List<string> BuildPreloadAssets()
    {
        var assets = base.BuildPreloadAssets();
        assets.Add(Module_Bordlands.BORDLAND_MONSTER_ANIMATOR_NAME);
        //add player data
        var c = ConfigManager.Get<CreatureInfo>(modulePlayer.proto);
        assets.AddRange(c.models);
        assets.Add(Creature.GetAnimatorName(moduleEquip.weaponID, modulePlayer.gender));
        assets.AddRange(CharacterEquip.GetEquipAssets(moduleEquip.currentDressClothes));
        assets.Add(headPanelName);
        assets.Add(GeneralConfigInfo.ssceneTextObject);
        // Player shadow
        assets.Add(CombatConfig.sdefaultSelfShadow);

        if (moduleBordlands.bordlandsSettlementReward != null) assets.Add(Window_RandomReward.ASSET_NAME);

        if (modulePet.FightingPet != null)
        {
            Module_Battle.BuildPetSimplePreloadAssets(modulePet.FightingPet, assets, 1);
        }

        var emjioList = ConfigManager.GetAll<FaceName>();
        for (int i = 0; i < emjioList.Count; i++)
            assets.Add(emjioList[i]?.head_icon);

        return assets;
    }

    protected override bool WaitBeforeLoadComplete()
    {
        try
        {
            moduleBordlands.CreateMonsterFromModuleData();
            moduleBordlands.CreatePlayerFromModuleData();
        }
        catch (System.Exception e)
        {
            Logger.LogError("on WaitBeforeLoadComplete,create module player and monster failed,error is {0}",e.ToString());
        }
        return true;
    }

    protected override void OnLoadComplete()
    {
        base.OnLoadComplete();

        Window.ShowAsync<Window_Bordlands>();
        PNmlPlayer selfData = moduleBordlands.AddSelfPlayerData();
        m_hero = moduleBordlands.GetPlayerCreature(modulePlayer.proto,selfData);
        moduleBordlands.HandlePlayer(m_hero,selfData,true);
        m_sendMsgTime = Time.realtimeSinceStartup;

        Camera_Combat.current.lockZ = true;
        Camera_Combat.enableShotSystem = false;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        //提前清除所有creature信息(gameobject交由场景释放)
        if (ObjectManager.instance != null) m_hero.Destroy();
        if (moduleBordlands != null)
        {
            moduleBordlands.StopAllCreateSceneObjCoroutine();
            moduleBordlands.InitBordlandsCreature();
        }
    }

    public override void OnUpdate(int diff)
    {
        base.OnUpdate(diff);

        if(!m_hero || m_hero.destroyed)
        {
            enableUpdate = false;
            return;
        }

        if (Time.realtimeSinceStartup - m_sendMsgTime >= SEND_POS_INTERVAL)
        {
            m_sendMsgTime = Time.realtimeSinceStartup;
            if (m_hero.transform.position != lastPos)
            {
                lastPos = m_hero.transform.position;
                moduleBordlands.SendPlayerPos(lastPos);
            }
        }
    }
}
