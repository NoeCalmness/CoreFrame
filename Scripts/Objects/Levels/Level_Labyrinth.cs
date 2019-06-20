/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-07-17
 * 
 ***************************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level_Labyrinth : Level_3DClick
{
    private bool m_levelLoadComplete = false;
    private bool m_duringCreatePlayer = false;
    private List<LabyrinthCreature> m_players = new List<LabyrinthCreature>();
    private GameObject m_activeEffect;
    private GameObject m_inactiveEffect;

    public const float SEND_POS_INTERVAL = 3f;
    private Vector3 lastPos = Vector3.zero;
    private float m_sendMsgTime = 0f;
    private Coroutine m_createPlayerCoroutine;
    private EnumLabyrinthTimeStep m_lastStep = EnumLabyrinthTimeStep.None;

    protected override List<string> BuildPreloadAssets()
    {
        var assets = base.BuildPreloadAssets();
       
        assets.Add(Module_Bordlands.BORDLAND_MONSTER_ANIMATOR_NAME);
        assets.Add(headPanelName);
        assets.Add(GeneralConfigInfo.ssceneTextObject);
        assets.AddRange(Module_Battle.BuildPlayerPreloadAssets());
        if(moduleLabyrinth.labyrinthInfo)
        {
            assets.Add(moduleLabyrinth.labyrinthInfo.activeEffect);
            assets.Add(moduleLabyrinth.labyrinthInfo.inactiveEffect);
        }

        var emjioList = ConfigManager.GetAll<FaceName>();
        for (int i = 0; i < emjioList.Count; i++)
            assets.Add(emjioList[i]?.head_icon);

        return assets;
    }

    protected override void OnLoadStart()
    {
        base.OnLoadStart();
        m_levelLoadComplete = false;
        m_duringCreatePlayer = false;
    }

    protected override bool WaitBeforeLoadComplete()
    {
        CreateSceneTrigger();
        return base.WaitBeforeLoadComplete();
    }

    protected override void OnLoadComplete()
    {
        base.OnLoadComplete();

        m_levelLoadComplete = true;
        Window.ShowAsync<Window_Labyrinth>();
        if(Camera_Combat.current) Camera_Combat.current.lockZ = true;
        Camera_Combat.enableShotSystem = false;
        CreatePlayersFromModuleData();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        m_levelLoadComplete = false;
        m_lastStep = EnumLabyrinthTimeStep.None;
        foreach (var item in m_players)
        {
            item.creature?.Destroy();
        }
        m_players.Clear();
        Module_Bordlands.bordlandsEdge = Vector4.zero;
        if(m_createPlayerCoroutine != null) StopCoroutine(m_createPlayerCoroutine);
    }

    public override void OnUpdate(int diff)
    {
        base.OnUpdate(diff);

        if(m_hero && !m_hero.destroyed && Time.realtimeSinceStartup - m_sendMsgTime >= SEND_POS_INTERVAL)
        {
            m_sendMsgTime = Time.realtimeSinceStartup;
            if (m_hero.transform.position != lastPos)
            {
                lastPos = m_hero.transform.position;
                moduleLabyrinth.SendPlayerPos(lastPos);
            }
        }
    }

    private void _ME(ModuleEvent<Module_Labyrinth> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Labyrinth.EventSetPlayerVisible:
                bool visible = (bool)e.param1;
                SetOtherPlayerVisible(visible);
                break;

            case Module_Labyrinth.EventCreatePlayers:
                CreatePlayersFromModuleData();
                break;

            case Module_Labyrinth.EventPlayerStateChange:
                OnPlayerStateChange(e.msg as ScMazePlayerState);
                break;

            case Module_Labyrinth.EventPlayerPosChange:
                OnPlayerPosChange(e.msg as CScMazeScenePlayerMove);
                break;

            case Module_Labyrinth.EventLabyrinthTimeRefresh:
                SetTriggerEffectVisible();
                break;
        }
    }

    private void CreateSceneTrigger()
    {
        if(!moduleLabyrinth.labyrinthInfo)
        {
            Logger.LogError("labyrinth info is null,cannot find the valid config info from config_labyrinthInfo");
        }
        else if (string.IsNullOrEmpty(moduleLabyrinth.labyrinthInfo.trigger))
        {
            Logger.LogError("labyrinth trigger is null with id : {0},please check out config_labyrinthInfo",moduleLabyrinth.labyrinthInfo.ID);
        }

        if (moduleLabyrinth.labyrinthInfo && !string.IsNullOrEmpty(moduleLabyrinth.labyrinthInfo.trigger))
        {
            Transform t = Util.FindChild(root, moduleLabyrinth.labyrinthInfo.trigger);
            if (t)
            {
                BoxCollider c = t.GetComponentDefault<BoxCollider>();
                c.size = moduleLabyrinth.labyrinthInfo.triggerSize;
                c.center = moduleLabyrinth.labyrinthInfo.triggerOffset;

                Rigidbody r = t.GetComponentDefault<Rigidbody>();
                r.isKinematic = true;
                r.useGravity = false;

                string eff = moduleLabyrinth.labyrinthInfo.activeEffect;
                if (!string.IsNullOrEmpty(eff))
                {
                    m_activeEffect = GetPreloadObject<GameObject>(eff);
                    if(m_activeEffect)
                    {
                        m_activeEffect.transform.SetParent(t);
                        m_activeEffect.transform.localScale = Vector3.one;
                        m_activeEffect.transform.localEulerAngles = Vector3.zero;
                        m_activeEffect.transform.localPosition = Vector3.zero;
                    }
                }

                eff = moduleLabyrinth.labyrinthInfo.inactiveEffect;
                if (!string.IsNullOrEmpty(eff))
                {
                    m_inactiveEffect = GetPreloadObject<GameObject>(eff);
                    if (m_inactiveEffect)
                    {
                        m_inactiveEffect.transform.SetParent(t);
                        m_inactiveEffect.transform.localScale = Vector3.one;
                        m_inactiveEffect.transform.localEulerAngles = Vector3.zero;
                        m_inactiveEffect.transform.localPosition = Vector3.zero;
                    }
                }
            }

            SetTriggerEffectVisible();
        }
    }

    private void SetTriggerEffectVisible()
    {
        if (m_lastStep == moduleLabyrinth.currentLabyrinthStep || !m_activeEffect || !m_inactiveEffect) return;

        m_activeEffect.SafeSetActive(moduleLabyrinth.currentLabyrinthStep == EnumLabyrinthTimeStep.Chanllenge);
        m_inactiveEffect.SafeSetActive(moduleLabyrinth.currentLabyrinthStep == EnumLabyrinthTimeStep.Rest);
        m_lastStep = moduleLabyrinth.currentLabyrinthStep;
    }

    private void SetOtherPlayerVisible(bool visible)
    {
        foreach (var item in m_players)
        {
            if (!item || !item.creature || item.playerType == LabyrinthCreature.LabyrinthCreatureType.Self) continue;

            item.gameObject.SetActive(visible);
            item.creature.visible = visible;
            if (item.creature.pet != null) item.creature.pet.visible = visible;
        }
    }

    private void OnPlayerStateChange(ScMazePlayerState p)
    {
        if (p == null) return;

        foreach (var item in p.state)
        {
            var lc = m_players.Find(o => o.roleInfo != null && o.roleInfo.roleId == item.roleId);
            if (lc)
            {
                lc.roleInfo.mazeState = (byte)item.state;
                lc.playerType = moduleLabyrinth.GetLabyrinthCreatureType(item.roleId, (byte)item.state);
            }
        }
    }

    private void OnPlayerPosChange(CScMazeScenePlayerMove p)
    {
        if (p == null) return;

        foreach (var item in m_players)
        {
            if(item.roleInfo.roleId == p.roleId && item.playerType != LabyrinthCreature.LabyrinthCreatureType.Self)
            {
                item.SetTargetPos(p.pos.ToVector3());
                break;
            }
        }
    }
    
    #region create player
    /// <summary>
    /// 初始化创建玩家
    /// </summary>
    public void CreatePlayersFromModuleData()
    {
        //已经创建了玩家直接等待推送
        if (!m_levelLoadComplete || m_duringCreatePlayer || (m_players != null && m_players.Count >= defaultShowPlayerLimit)) return;

        m_duringCreatePlayer = true;
        List<PMazePlayer> allPlayers = new List<PMazePlayer>();
        foreach (var item in moduleLabyrinth.areaPlayerDic)
        {
            allPlayers.AddRange(item.Value);
        }
        int index = 0;
        while (index < allPlayers.Count)
        {
            var c = m_players.Find(o => o.roleInfo.roleId == allPlayers[index].roleId);
            if (c) allPlayers.RemoveAt(index);
            else index++;
        }
        CreateSelfPlayer(allPlayers);
        CreatePlayerCreature(allPlayers);
        Logger.LogInfo("{0} create players from module data ,players.count  = {1}", Time.time, allPlayers.Count);
    }

    private void CreatePlayerCreature(List<PMazePlayer> infos)
    {
        if (infos == null || infos.Count== 0) return;

        List<string> allAssets = GetPlayerPreloadAssets(infos);
        PrepareAssets(allAssets, (flag) =>
        {
            if (!flag)
            {
                Logger.LogError("labyrinth player assets isn't prepare.....");
                return;
            }

            //create all players
            m_createPlayerCoroutine = StartCoroutine(CreateAsynPlayers(infos));
        });
    }

    private void CreateSelfPlayer(List<PMazePlayer> infos)
    {
        if (m_hero) return;

        foreach (var item in infos)
        {
            if (item.roleId == modulePlayer.roleInfo.roleId)
            {
                var c = GetPlayerCreature(item.GetProto(), item);
                if (!c) continue;

                c.roleId = item.roleId;
                c.roleProto = item.roleProto;
                HandlePlayer(c, item, true);
                m_hero = c;
                break;
            }
        }
    }

    private IEnumerator CreateAsynPlayers(List<PMazePlayer> infos)
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        foreach (var item in infos)
        {
            if (item.roleId == modulePlayer.roleInfo.roleId) continue;
            
            var c = GetPlayerCreature(item.GetProto(), item);
            if (!c) continue;

            c.roleId = item.roleId;
            c.roleProto = item.roleProto;
            HandlePlayer(c, item, false);

            if (m_players.Count >= defaultShowPlayerLimit) yield break;
            else yield return wait;
        }

        m_duringCreatePlayer = false;
    }

    private List<string> GetPlayerPreloadAssets(List<PMazePlayer> infos)
    {
        List<string> list = new List<string>();

        string temp = string.Empty;
        foreach (var p in infos)
        {
            //add creature
            var c = ConfigManager.Get<CreatureInfo>(p.GetProto());
            if (!c)
            {
                Logger.LogError("wrong player proto with pmazePlayer : id:{0},name:{1},gender:{2} proto:{3}",
                    p.roleId,p.roleName,p.gender,p.GetProto());
                continue;
            }
            list.AddRange(c.models);

            if(p.fashion != null)
            {
                //add animator asset
                var prop = ConfigManager.Get<PropItemInfo>(p.fashion.weapon);
                if (prop)
                {
                    temp = Creature.GetAnimatorName(prop.subType, p.gender);
                    if (!list.Contains(temp)) list.Add(temp);
                }

                //equip assets
                List<string> equips = CharacterEquip.GetEquipAssets(p.fashion);
                foreach (var e in equips)
                {
                    if (!list.Contains(e)) list.Add(e);
                }
            }
            
            //pet assets
            if (p.pet != null && p.pet.itemTypeId != 0)
            {
                var pet = PetInfo.Create(p.pet);
                Module_Battle.BuildPetSimplePreloadAssets(pet, list, 1);
            }
        }
        list.Distinct();
        return list;
    }

    public Creature GetPlayerCreature(int creatureId, PMazePlayer data)
    {
        CreatureInfo info = ConfigManager.Get<CreatureInfo>(creatureId);
        if(!info)
        {
            Logger.LogError("Get CreatureInfo failed, could not find template config: {0}", creatureId);
            return null;
        }
        var prop = ConfigManager.Get<PropItemInfo>(data.fashion.weapon);
        if (prop)
        {
            info.weaponID = prop.subType;
            info.weaponItemID = data.fashion.weapon;
        }
        Creature player = Creature.Create(info, Vector3.zero, new Vector3(0f, 90f, 0f), data.roleId.Equals(modulePlayer.roleInfo.roleId), false);

        if (data.pet != null && data.pet.itemTypeId != 0)
        {
            player.pet = PetCreature.Create(player, PetInfo.Create(data.pet), player.position_, player.eulerAngles, false, Module_Home.PET_OBJECT_NAME);
            if (player.pet != null)
            {
                if (!moduleAI.IsStartAI)
                    moduleAI.StartAI();
                player.pet.DisableSkills(true);
                moduleAI.AddPetAI(player.pet);
            }
        }
        return player;
    }

    public void HandlePlayer(Creature player, PMazePlayer info, bool isPlayer = false)
    {
        Util.SetLayer(player.gameObject, Layers.MODEL);

        player.isPlayer = isPlayer;
        player.enableUpdate = false;
        player.behaviour.enabled = false;
        player.visible = moduleLabyrinth.playerVisible || isPlayer;
        if (player.pet) player.pet.visible = player.visible;
        player.gameObject.name = isPlayer ? "self" : info.roleName;
        CharacterEquip.ChangeCloth(player, info.fashion);

        LabyrinthCreature lc = player.activeRootNode.GetComponentDefault<LabyrinthCreature>();
        lc.InitCreatureData(player,info);
        if(!isPlayer) m_players.Add(lc);
    }
    #endregion
}
