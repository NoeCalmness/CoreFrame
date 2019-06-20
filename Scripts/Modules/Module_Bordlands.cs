/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-08-09
 * 
 ***************************************************************************************************/
using AssetBundles;
using System.Collections.Generic;
using UnityEngine;

#region custom class

public class BorderRankData
{
    public int rank { get; set; }
    public PNmlRankPlayer playerData { get; set; }

    public ulong roleId { get { return validPlayerData ? playerData.roleId : ulong.MinValue; } }
    public string name { get { return validPlayerData ? playerData.name : string.Empty; } }
    public uint score { get { return validRankData ? playerData.rankData.score : 0; } }
    public uint money { get { return validRankData ? playerData.rankData.money : 0; } }

    public bool validPlayerData { get { return playerData != null; } }
    public bool validRankData { get { return validPlayerData && playerData.rankData != null; } }

    public BorderRankData(int rank,PNmlRankPlayer data)
    {
        this.rank = rank;
        playerData = data;
    }

    public void SetRankData(PNmlRankData data)
    {
        if (!validPlayerData) playerData = PacketObject.Create<PNmlRankPlayer>();
        if (!validRankData) playerData.rankData = PacketObject.Create<PNmlRankData>();

        playerData.rankData.score = data.score;
        playerData.rankData.money = data.money;
    }
}

#endregion

public class Module_Bordlands : Module<Module_Bordlands>
{
    #region 事件名称

    public const string EventEnterBordland              = "EventEnterBordland";
    public const string EventLeaveBordland              = "EventLeaveBordland";
    public const string EventFightMonster               = "EventFightMonster";
    public const string EventDetectMonster              = "EventDetectMonster";
    public const string EventBordlandOver               = "EventBordlandOver";
    public const string EventBordlandSettlement         = "EventBordlandSettlement";
    public const string EventRefreshSelfRank            = "EventRefreshSelfRank";
    public const string EventRefreshRankList            = "EventRefreshRankList";
    public const string EventClickScenePlayerSuccess    = "EventClickScenePlayerSuccess";
    public const string EventClickSceneObjSuccess       = "EventClickSceneObjSuccess";
    public const string EventShowRewardOver             = "EventShowRewardOver";
    #endregion

    #region const field
    public const int BORDERLAND_LEVEL_ID = 6;
    public const string BORDLAND_MONSTER_ANIMATOR_NAME = "animator_borderland";
    #endregion

    #region enum

    //怪物和玩家的整体状态管理
    public enum EnumBordlandCreatureState
    {
        Idle = 1,

        Fighting,

        LeaveFromScene,
    }

    //场景上的怪物类型
    public enum EnumBordlandMonsterType
    {
        NormalType = 1,

        /// <summary>
        /// BOSS类型，可能是BOSS假身
        /// </summary>
        BossType,

        /// <summary>
        /// 系统洞悉BOSS类型
        /// </summary>
        SystemBossType,
    }
    #endregion

    #region field

    //重新进入无主之地需要显示的结算奖励
    public PReward bordlandsSettlementReward;

    //当前请求的stageid
    private int m_tempStageId;
    #endregion

    #region 切换场景清理的模型脚本相关

    //移动检测状态
    public int moveMentKey { get; set; }
    //dead monster pool
    private Dictionary<int, List<MonsterCreature>> m_deadMonsterCreatureDic = new Dictionary<int, List<MonsterCreature>>();
    private Dictionary<ulong, BordlandsCreature> m_otherPlayerCreatures = new Dictionary<ulong, BordlandsCreature>();
    private Dictionary<ulong, BordlandsCreature> m_monsterCreature = new Dictionary<ulong, BordlandsCreature>();

    private bool m_isInitMonster = false;
    private bool m_isInitPlayer = false;

    private Coroutine m_loadPlayerCoroutine;
    private Coroutine m_loadMonsterCoroutine;
    #endregion

    #region 切换场景不需要清理的数据区

    //是否主动离开了无主之地
    public bool isLeaveBordland = true;
    //是否隐藏玩家，UI控制该状态
    private bool m_playerVisible = true;
    public bool playerVisible { get { return m_playerVisible; } }

    //玩家侦查消耗数据
    public Dictionary<int, PItem2> detectCostDic { get; private set; }

    //当前已经侦查了的次数
    public int currentDetectTimes = 0;

    private Dictionary<ulong, PNmlPlayer> m_bordlandPlayers = new Dictionary<ulong, PNmlPlayer>();
    private Dictionary<ulong, PNmlMonster> m_bordlandMonster = new Dictionary<ulong, PNmlMonster>();

    private static Vector4 m_edge = Vector4.zero;
    public static Vector4 bordlandsEdge
    {
        get
        {
            if (m_edge == Vector4.zero)
            {
                var e = Level.current.edge;
                var ze = Level.current.zEdge;
                double x, y, z, w = 0f;
                x = e.x > e.y ? e.y : e.x;
                y = e.x > e.y ? e.x : e.y;
                z = ze.x > ze.y ? ze.y : ze.x;
                w = ze.x > ze.y ? ze.x : ze.y;
                m_edge = new Vector4((float)x, (float)y, (float)z, (float)w);
            }
            return m_edge;
        }

        set { m_edge = value; }
    }

    public PNmlMonster lastChallengeMonster { get; set; }
    public BorderRankData selfData { get; private set; }
    public uint selfScore { get { return selfData == null ? 0 : selfData.score; } }
    public List<BorderRankData> rankList { get; private set; } = new List<BorderRankData>();
    #endregion

    #region 活动时间相关
    public bool isValidBordlandTime
    {
        get
        {
            if (bordlandTime == null || recvScNmlOpenMsgTime <= 0f)
                return false;

            uint total = (uint)Mathf.RoundToInt(Time.realtimeSinceStartup - recvScNmlOpenMsgTime);
            total += bordlandTime.serverTime;
            return total >= bordlandTime.startTime && total <= bordlandTime.endTime;
        }
    }

    //活动结束
    public bool isValidBordland
    {
        get
        {
            if (isRecvServerBordlandOver)
                return false;
            return isValidBordlandTime;
        }
    }

    //服务器发送的无主之地结束
    private bool isRecvServerBordlandOver = false;

    //无主之地相关时间
    private ScNmlOpenTime bordlandTime;

    //用来记录收到登录无主之地请求的时间
    public float recvScNmlOpenMsgTime;
    #endregion

    #region properties

    public bool isHasAnyBoss
    {
        get
        {
            Dictionary<ulong, PNmlMonster>.Enumerator e = m_bordlandMonster.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Value.type == (sbyte)EnumBordlandMonsterType.BossType)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// 自己角色初始化的位置
    /// </summary>
    public Vector3 selfInitPosition
    {
        get
        {
            ulong selfId = modulePlayer.roleInfo.roleId;
            if (m_bordlandPlayers.ContainsKey(selfId))
                return m_bordlandPlayers[selfId].pos.ToVector3();
            else
                return Level.current.startPos.position;
        }
    }
    #endregion

    #region init

    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        SendBordlandOpenTime();
    }

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();

        selfData = null;
        m_playerVisible = true;
        InitBordlandsModuleData();
        InitBordlandsCreature();
    }

    /// <summary>
    /// 清空无主之地保存的数据
    /// 该方法只在玩家操作退出无主之地的时候调用
    /// </summary>
    public void InitBordlandsModuleData()
    {
        isLeaveBordland = true;
        if (detectCostDic != null) detectCostDic.Clear();
        m_bordlandPlayers.Clear();
        m_bordlandMonster.Clear();
    }

    /// <summary>
    /// 清空玩家模型脚本相关数据
    /// </summary>
    public void InitBordlandsCreature()
    {
        foreach (var item in m_otherPlayerCreatures)
        {
            item.Value.creature.Destroy();
        }
        foreach (var item in m_monsterCreature)
        {
            item.Value.creature.Destroy();
        }
        m_deadMonsterCreatureDic.Clear();
        m_otherPlayerCreatures.Clear();
        m_monsterCreature.Clear();

        m_isInitMonster = m_isInitPlayer = false;
        moveMentKey = 0;
        m_edge = Vector4.zero;
    }

    public void InitDetectCostDic(PItem2[] detectCosts)
    {
        if (detectCostDic == null) detectCostDic = new Dictionary<int, PItem2>();
        detectCostDic.Clear();

        for (int i = 0; i < detectCosts.Length; i++)
        {
            detectCostDic.Add(i, detectCosts[i]);
        }
    }

    public void Enter()
    {
        if (bordlandTime == null) return;
        if(Level.current && Level.current is Level_Bordlands)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.BorderlandUIText, 33));
            return;
        }

        moduleGuide.needEnterBordland = false;
        if (!isValidBordland && bordlandsSettlementReward == null)
        {
            string content = ConfigText.GetDefalutString(TextForMatType.BorderlandUIText, 17);
            Window_Alert.ShowAlert(content);
            return;
        }

        if (isLeaveBordland) SendEnterBordLand();
        else Game.LoadLevel(BORDERLAND_LEVEL_ID);
    }
    #endregion

    #region create player

    private void StopCreatePlayer()
    {
        if (m_loadPlayerCoroutine != null) Root.instance.StopCoroutine(m_loadPlayerCoroutine);
        m_loadPlayerCoroutine = null;
    }

    /// <summary>
    /// 初始化创建玩家
    /// </summary>
    public void CreatePlayerFromModuleData()
    {
        //已经创建了玩家直接等待推送
        if (m_isInitPlayer) return;

        m_isInitPlayer = true;
        PNmlPlayer[] players = new PNmlPlayer[m_bordlandPlayers.Count];
        m_bordlandPlayers.Values.CopyTo(players, 0);
        CreatePlayerCreature(players);
    }

    private void CreatePlayersFromRecvMsg(PNmlPlayer[] pPlayers)
    {
        PNmlPlayer[] playerList = null;
        pPlayers.CopyTo(ref playerList);

        //管理本地怪物对象池
        for (int i = 0; i < playerList.Length; i++)
        {
            PNmlPlayer player = playerList[i];
            AddPlayerData(player);
        }

        //已经生成了玩家之后，直接创建玩家
        if (m_isInitPlayer)
            CreatePlayerCreature(playerList);
    }

    private void CreatePlayerCreature(PNmlPlayer[] infos)
    {
        if (infos == null || infos.Length == 0) return;

        List<string> allAssets = GetPlayerPreloadAssets(infos);

        StopCreatePlayer();
        m_loadPlayerCoroutine =  Level.PrepareAssets(allAssets, (flag) =>
        {
            if (!flag)
            {
                Logger.LogError("borderland player assets isn't prepare.....");
                return;
            }

            //create all players
            foreach (var item in infos)
            {
                //自己的信息只作为记录缓存，不作为创建的依据
                if (item.roleId == modulePlayer.roleInfo.roleId || m_otherPlayerCreatures.Values.Count >= Level_3DClick.defaultShowPlayerLimit)
                    continue;

                //二次过滤，如果在准备资源途中就有玩家在退出
                if (!m_bordlandPlayers.ContainsKey(item.roleId)) continue;
                
                Creature player = GetPlayerCreature(item.GetProto(), item);
                player.roleId = item.roleId;
                player.roleProto = item.roleProto;
                HandlePlayer(player, item);
            }
        });
    }

    private List<string> GetPlayerPreloadAssets(PNmlPlayer[] infos)
    {
        List<string> list = new List<string>();

        string temp = string.Empty;
        foreach (var p in infos)
        {
            //add creature
            var c = ConfigManager.Get<CreatureInfo>(p.GetProto());
            list.AddRange(c.models);

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

            if (p.pet != null && p.pet.itemTypeId != 0)
            {
                var pet = PetInfo.Create(p.pet);
                Module_Battle.BuildPetSimplePreloadAssets(pet, list, 1);
            } 
        }
        list.Distinct();
        return list;
    }

    public Creature GetPlayerCreature(int creatureId, PNmlPlayer data)
    {
        CreatureInfo info = ConfigManager.Get<CreatureInfo>(creatureId);
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

    public void HandlePlayer(Creature player, PNmlPlayer info, bool isPlayer = false)
    {
        Util.SetLayer(player.gameObject, Layers.MODEL);

        player.isPlayer = isPlayer;
        player.enableUpdate = false;
        player.behaviour.enabled = false;
        player.gameObject.SetActive(m_playerVisible || isPlayer);
        player.gameObject.name = isPlayer ? "self" : info.roleName;
        CharacterEquip.ChangeCloth(player, info.fashion);

        BordlandsCreature bc = player.activeRootNode.GetComponentDefault<BordlandsCreature>();
        PropItemInfo weaponInfo = ConfigManager.Get<PropItemInfo>(info.fashion.weapon);
        if (weaponInfo) bc.LoadPlayerRuntimeAnimator(Creature.GetAnimatorName(weaponInfo.subType, info.gender), weaponInfo.subType, (byte)info.gender);
        bc.playerType = isPlayer ? BordlandsCreature.BordLandCreatureType.Self : BordlandsCreature.BordLandCreatureType.Player;
        bc.roleInfo = info;
        bc.creature = player;
        bc.InitPVECreatureBehaviour();
        bc.creaturePos = info.pos.ToVector3();
        if (player.pet)
        {
            player.pet.gameObject.SetActive(m_playerVisible || isPlayer);
            player.pet.position_ = bc.creaturePos - new Vector3(0.45f, 0, 0);
        }

        if (!isPlayer)
        {
            if (!m_otherPlayerCreatures.ContainsKey(info.roleId)) m_otherPlayerCreatures.Add(info.roleId, null);
            m_otherPlayerCreatures[info.roleId] = bc;
        }
    }
    #endregion

    #region create monsters

    private void StopCreateMonster()
    {
        if (m_loadMonsterCoroutine != null) Root.instance.StopCoroutine(m_loadMonsterCoroutine);
        m_loadMonsterCoroutine = null;
    }

    /// <summary>
    /// 初始化创建怪物
    /// </summary>
    public void CreateMonsterFromModuleData()
    {
        //只用于初始化
        if (m_isInitMonster) return;

        m_isInitMonster = true;
        PNmlMonster[] monsters = new PNmlMonster[m_bordlandMonster.Count];
        m_bordlandMonster.Values.CopyTo(monsters, 0);
        CreateMonsterCreature(monsters);
    }

    private void CreateMonsterFromMsg(PNmlMonster[] pMonster)
    {
        PNmlMonster[] monsterList = null;
        pMonster.CopyTo(ref monsterList);

        //管理本地怪物对象池
        for (int i = 0; i < monsterList.Length; i++)
        {
            PNmlMonster monster = monsterList[i];
            if (m_bordlandMonster.ContainsKey(monster.uid))
            {
                Logger.LogInfo("monster_uid = {0} is has loaded,check out!", monster.uid);
                continue;
            }
            //Logger.LogInfo("monster.uid = {0}", monster.uid);
            m_bordlandMonster.Add(monster.uid, monster);
        }

        //已经生成了怪物之后才能直接通过消息生成怪物
        if (m_isInitMonster) CreateMonsterCreature(monsterList);
    }

    private void CreateMonsterCreature(PNmlMonster[] datas)
    {
        StopCreateMonster();
        m_loadMonsterCoroutine = Level.PrepareAssets(GetMonsterPreloadAssets(datas), (flag) =>
        {
            if (!flag)
            {
                Logger.LogError("borderland player assets isn't prepare.....");
                return;
            }

            for (int i = 0; i < datas.Length; i++)
            {
                PNmlMonster data = datas[i];

                if (!m_bordlandMonster.ContainsKey(data.uid)) continue;

                MonsterCreature monster = GetMonsterCreature(data);
                HandleMonster(monster, data);
            }
        });
    }

    private List<string> GetMonsterPreloadAssets(PNmlMonster[] datas)
    {
        List<string> list = new List<string>();
        foreach (var item in datas)
        {
            if (!list.Contains(item.mesh)) list.Add(item.mesh);
        }
        return list;
    }

    /// <summary>
    /// 延迟处理怪物，等待资源加载完毕
    /// </summary>
    /// <param name="monster"></param>
    public void HandleMonster(MonsterCreature monster, PNmlMonster data)
    {
        if (!monster || !monster.gameObject || !monster.behaviour || !monster.activeRootNode) return;

        monster.behaviour.enabled = false;
        monster.behaviour.collider_.syncPosition = false;
        monster.behaviour.collider_.transform.localPosition = Vector3.zero;
        Util.SetLayer(monster.gameObject, Layers.MODEL);
        monster.gameObject.name = Util.Format("monster_{0}_{1}_{2}", data.uid, (EnumBordlandMonsterType)data.type, data.boss);
        monster.isPlayer = false;
        
        BordlandsCreature bc = monster.activeRootNode.GetComponentDefault<BordlandsCreature>();
        bc.LoadMonsterRuntimeAnimator(BORDLAND_MONSTER_ANIMATOR_NAME, data);
        bc.moveSpeed = GetMonsterMoveSpeed(data.mesh);
        bc.creature = monster;
        bc.ResetToOriginal();
        bc.SetMonsterRandomDir();
        bc.InitPVECreatureBehaviour();
        bc.CreateCollider();

        Vector3 pos = data.pos.ToVector3();
        Vector4 levelEdge = bordlandsEdge;
        if (levelEdge.z == levelEdge.w)
        {
            levelEdge = new Vector4(-22f, 22f, -1.4f, 2f);
            m_edge = levelEdge;
        }
        // edge check
        if (levelEdge.x != 0 || levelEdge.x != levelEdge.y) pos.x = Mathf.Clamp(pos.x, levelEdge.x, levelEdge.y);
        if (levelEdge.z != 0 || levelEdge.z != levelEdge.w) pos.z = Mathf.Clamp(pos.z, levelEdge.z, levelEdge.w);
        bc.creaturePos = pos;

        if (!m_monsterCreature.ContainsKey(data.uid)) m_monsterCreature.Add(data.uid, bc);
        else m_monsterCreature[data.uid] = bc;
    }
    #endregion

    #region other public functions

    public static float GetCreatureScale(float posZ)
    {
        return 0.1f * posZ + 1f;
    }

    private float GetMonsterMoveSpeed(string monsterMesh)
    {
        float delta = 0.3f;
        List<MonsterInfo> monsters = ConfigManager.GetAll<MonsterInfo>();
        for (int i = 0; i < monsters.Count; i++)
        {
            if (monsters[i].GetModel() == monsterMesh)
                return (float)(monsters[i].moveSpeed * 0.1f) * delta;
        }

        return (float)(monsters[0].moveSpeed * 0.1f) * delta;
    }

    public void SetPlayerVisible(bool isVisible)
    {
        m_playerVisible = isVisible;
        foreach (var item in moduleBordlands.m_otherPlayerCreatures)
        {
            item.Value.gameObject.SetActive(isVisible);
            item.Value.creature.gameObject.SetActive(isVisible);
            if (item.Value.creature.pet) item.Value.creature.pet.gameObject.SetActive(isVisible);
        }
    }

    public BordlandsCreature GetBordCreature(GameObject obj)
    {
        foreach (var item in m_monsterCreature)
        {
            if (item.Value.gameObject.Equals(obj))
            {
                return item.Value;
            }
        }

        return null;
    }

    public void HandlePlayerMove(ulong playerId, Vector3 target)
    {
        BordlandsCreature bc = null;
        if (m_otherPlayerCreatures.TryGetValue(playerId, out bc))
        {
            if(bc.transform) bc.SetTargetPos(target);
        }
    }

    /// <summary>
    /// 需要把玩家自己的信息记录到字典中去，保证自己的位置信息能够更新
    /// </summary>
    public PNmlPlayer AddSelfPlayerData()
    {
        PRoleInfo info = modulePlayer.roleInfo;
        if (m_bordlandPlayers.ContainsKey(info.roleId)) return m_bordlandPlayers[info.roleId];

        PNmlPlayer self = PacketObject.Create<PNmlPlayer>();
        self.roleId = info.roleId;
        self.roleProto = (byte)modulePlayer.proto;
        self.roleName = info.roleName;
        self.level = (sbyte)info.level;
        self.gender = (sbyte)modulePlayer.gender;
        self.state = (byte)EnumBordlandCreatureState.Idle;
        self.pos = selfInitPosition.ToPPostion();
        self.fashion = PacketObject.Create<PFashion>();
        self.pet = modulePet.FightingPet != null ? modulePet.FightingPet.Item :null;
        for (int i = 0, count = moduleEquip.currentDressClothes.Count; i < count; i++)
        {
            PItem item = moduleEquip.currentDressClothes[i];
            PropItemInfo itemInfo = item?.GetPropItem();
            if (itemInfo == null)
                continue;

            switch (itemInfo.itemType)
            {
                case PropType.Weapon:
                    if (itemInfo.subType != (byte)WeaponSubType.Gun)
                        self.fashion.weapon = item.itemTypeId;
                    else
                        self.fashion.gun = item.itemTypeId;
                    break;
                case PropType.FashionCloth:
                    switch ((FashionSubType)itemInfo.subType)
                    {
                        case FashionSubType.UpperGarment:
                        case FashionSubType.FourPieceSuit:
                        case FashionSubType.TwoPieceSuit:
                            self.fashion.clothes = item.itemTypeId;
                            break;
                        case FashionSubType.Pants:
                            self.fashion.trousers = item.itemTypeId;
                            break;
                        case FashionSubType.Glove:
                            self.fashion.glove = item.itemTypeId;
                            break;
                        case FashionSubType.Shoes:
                            self.fashion.shoes = item.itemTypeId;
                            break;
                        case FashionSubType.Hair:
                            self.fashion.hair = item.itemTypeId;
                            break;
                        case FashionSubType.ClothGuise:
                            self.fashion.guiseId = item.itemTypeId;
                            break;
                        case FashionSubType.HeadDress :
                            self.fashion.headdress = item.itemTypeId;
                            break;
                        case FashionSubType.HairDress:
                            self.fashion.hairdress = item.itemTypeId;
                            break;
                        case FashionSubType.FaceDress:
                            self.fashion.facedress = item.itemTypeId;
                            break;
                        case FashionSubType.NeckDress:
                            self.fashion.neckdress = item.itemTypeId;
                            break;
                    }
                    break;
            }
        }

        AddPlayerData(self);
        return self;
    }

    public void StopAllCreateSceneObjCoroutine()
    {
        StopCreatePlayer();
        StopCreateMonster();
    }
    #endregion

    #region 消息发送

    public void SendBordlandOpenTime()
    {
        CsNmlOpenTime p = PacketObject.Create<CsNmlOpenTime>();
        session.Send(p);
    }

    public void SendEnterBordLand()
    {
        CsNmlEnterScene p = PacketObject.Create<CsNmlEnterScene>();
        session.Send(p);
    }

    public void SendExitBordland()
    {
        InitBordlandsModuleData();
        Game.GoHome();

        CsNmlSceneCancel p = PacketObject.Create<CsNmlSceneCancel>();
        session.Send(p);
    }

    /// <summary>
    /// 请求怪物信息，一般不用，只用作检查
    /// </summary>
    public void SendQuestMonster()
    {
        CsNmlSceneMonster p = PacketObject.Create<CsNmlSceneMonster>();
        session.Send(p);
    }

    /// <summary>
    /// 请求玩家信息，一般不用，只用作检查
    /// </summary>
    public void SendQuestPlayer()
    {
        CsNmlScenePlayer p = PacketObject.Create<CsNmlScenePlayer>();
        session.Send(p);
    }

    public void SendPlayerPos(Vector3 pos)
    {
        CScNmlScenePlayerMove p = PacketObject.Create<CScNmlScenePlayerMove>();
        p.roleId = modulePlayer.roleInfo.roleId;
        p.pos = pos.ToPPostion();
        session.Send(p);
    }

    public void SendEnterStage(ulong monsterUid, int stageId)
    {
        m_tempStageId = stageId;
        CsNmlSceneFightMonster p = PacketObject.Create<CsNmlSceneFightMonster>();
        p.uid = monsterUid;
        session.Send(p);
    }

    public void SendRefreshBoss()
    {
        CsNmlSceneBoss p = PacketObject.Create<CsNmlSceneBoss>();
        session.Send(p);
    }

    public void SendFightWin()
    {
        CsNmlSceneFightWin p = PacketObject.Create<CsNmlSceneFightWin>();
        p.overData = modulePVE.GetPveDatas();
        session.Send(p);
    }

    public void SendFightLose()
    {
        CsNmlSceneFightFail p = PacketObject.Create<CsNmlSceneFightFail>();
        session.Send(p);
    }

    #endregion

    #region 消息接收
    void _Packet_999(ScNmlOpenTime p)
    {
        p.CopyTo(ref bordlandTime);
        recvScNmlOpenMsgTime = Time.realtimeSinceStartup;
    }

    void _Packet(ScNmlEnterScene p)
    {
        if (p.result == 0)
        {
            //每次成功进入的时候清空
            InitBordlandsCreature();
            isLeaveBordland = false;

            currentDetectTimes = p.detectTime;
            PItem2[] costArray = null;
            p.detectItem.CopyTo(ref costArray);
            InitDetectCostDic(costArray);
            Game.LoadLevel(BORDERLAND_LEVEL_ID);

            HandleSelfInfo(p.rankData);
        }
        DispatchModuleEvent(EventEnterBordland, p);
    }

    void _Packet(ScNmlSceneCancel p)
    {
        InitBordlandsModuleData();
        InitBordlandsCreature();
        DispatchModuleEvent(EventLeaveBordland, p);
    }

    void _Packet(ScNmlSceneMonster p)
    {
        CreateMonsterFromMsg(p.monsterList);
    }

    void _Packet(ScNmlSceneBoss p)
    {
        //增加侦查次数
        if (p.result == 0)
        {
            AudioManager.PlaySound(AudioInLogicInfo.audioConst.clickToSucc);
            currentDetectTimes++;
        }
        DispatchModuleEvent(EventDetectMonster, p);
    }

    void _Packet(ScNmlScenePlayer p)
    {
        CreatePlayersFromRecvMsg(p.playerList);
    }

    private void AddPlayerData(PNmlPlayer player)
    {
        if (m_bordlandPlayers.ContainsKey(player.roleId))
        {
            Logger.LogWarning("player_uid = {0} is has loaded,check out!", player.roleId);
            return;
        }
        m_bordlandPlayers.Add(player.roleId, player);
    }

    void _Packet(ScNmlSceneObjState p)
    {
        EnumBordlandCreatureState state = (EnumBordlandCreatureState)p.state;

        if (m_bordlandMonster.ContainsKey(p.uid))
        {
            Logger.LogInfo("monster.uid = {0},state change to {1}", p.uid, state);
            if (state == EnumBordlandCreatureState.LeaveFromScene)
            {
                m_bordlandMonster.Remove(p.uid);
                BordlandsCreature bc = m_monsterCreature.Get(p.uid);
                if (bc)
                {
                    bc.gameObject.SetActive(false);
                    bc.ResetToOriginal();
                    m_monsterCreature.Remove(p.uid);
                    AddDeadMonsterCreature(bc.creature as MonsterCreature);
                }
            }
            else
            {
                //更新数据层
                PNmlMonster pm = m_bordlandMonster.Get(p.uid);
                if (pm != null) pm.state = p.state;

                //更新UI层
                BordlandsCreature bc = m_monsterCreature.Get(p.uid);
                if (bc)
                {
                    bc.monsterData.state = p.state;
                    bc.isInBattle = state == EnumBordlandCreatureState.Fighting;
                }
            }
        }
        else if (m_bordlandPlayers.ContainsKey(p.uid))
        {
            Logger.LogInfo("player.uid = {0},state change to {1}", p.uid, state);
            if (state == EnumBordlandCreatureState.LeaveFromScene)
            {
                m_bordlandPlayers.Remove(p.uid);
                BordlandsCreature bc = m_otherPlayerCreatures.Get(p.uid);
                if (bc)
                {
                    //离开后需要关掉宠物AI
                    if (bc.creature.pet != null)
                        moduleAI.RemovePetAI(bc.creature.pet);
                    bc.gameObject.SetActive(false);
                    m_otherPlayerCreatures.Remove(p.uid);
                    bc.creature.Destroy();
                }
            }
            else
            {
                PNmlPlayer pm = m_bordlandPlayers.Get(p.uid);
                if (pm != null) pm.state = p.state;

                BordlandsCreature bc = m_otherPlayerCreatures.Get(p.uid);
                if (bc)
                {
                    bc.roleInfo.state = p.state;
                    bc.isInBattle = state == EnumBordlandCreatureState.Fighting;
                }

            }
        }
        else
            Logger.LogError("scene_obj_uid = {0} doesn't load,check out!", p.uid);
    }

    void _Packet(CScNmlScenePlayerMove p)
    {
        HandlePlayerMove(p.roleId, p.pos.ToVector3());

        //更新玩家位置
        if (m_bordlandPlayers.ContainsKey(p.roleId)) m_bordlandPlayers[p.roleId].pos = p.pos;
    }

    void _Packet(ScNmlSceneFightMonster p)
    {
        //成功挑战后，需要设置PVE的关卡信息
        //无主之地不需要复活数据
        if (p.result == 0)
        {
            modulePVE.OnScRoleStartBordland(m_tempStageId,p.overTimes);
        }
        //无论消息是否成功都清理掉缓存
        m_tempStageId = -1;
        DispatchModuleEvent(EventFightMonster, p);
    }
    void _Packet(ScNmlSceneOver p)
    {
        //加载复活需要的数据 
        isRecvServerBordlandOver = true;
        DispatchModuleEvent(EventBordlandOver);
    }

    void _Packet(ScNmlSceneFightWin p)
    {
        if (p.result == 0 && p.rewards != null && p.rewards.Length > 0)
        {
            //普通奖励拷贝给PVE
            modulePVE.SetPVESettlement(p.rewards.GetValue<PReward>(0));
            //无主之地宝箱奖励拷贝给无主之地
            if (p.rewards.Length > 1) p.rewards[1].CopyTo(ref bordlandsSettlementReward);
        }
        else
        {
            modulePVE.settlementReward = PacketObject.Create<PReward>();
        }
        DispatchModuleEvent(EventBordlandSettlement, p.rewards);
    }

    public void OnBordlandStageFail()
    {
        DispatchModuleEvent(EventBordlandSettlement, null);
    }

    #endregion

    #region 场景对象对象池操作

    private MonsterCreature GetMonsterCreature(PNmlMonster monsterData)
    {
        MonsterCreature monster = null;
        List<MonsterCreature> list = m_deadMonsterCreatureDic.Get(monsterData.monster);
        if (list != null && list.Count > 0)
        {
            monster = list[0];
            monster.monsterUID = monsterData.uid;
            list.RemoveAt(0);
        }
        if (monster == null) monster = MonsterCreature.CreateBordlandMonster(monsterData, Vector3.zero, new Vector3(0f, 90f, 0f));

        if (!monster) return null;
        if (!monster.gameObject)
        {
            Logger.LogError("monster is valid ,but monster gameobject is null ,monster mesh is {0}",monsterData.mesh);
            monster.Destroy();
            return null;
        }

        monster.enableUpdate = false;
        monster.gameObject?.SetActive(true);
        return monster;
    }

    private void AddDeadMonsterCreature(MonsterCreature monster)
    {
        if (!m_deadMonsterCreatureDic.ContainsKey(monster.monsterId)) m_deadMonsterCreatureDic.Add(monster.monsterId, new List<MonsterCreature>());
        List<MonsterCreature> list = m_deadMonsterCreatureDic.Get(monster.monsterId);
        list.Add(monster);
    }

    #endregion

    #region borderlands rank list

    void _Packet(ScNmlRankDataChange p)
    {
        HandleSelfInfo(p.nmlRankData);
    }

    private void HandleSelfInfo(PNmlRankData data)
    {
        if (selfData == null)
        {
            selfData = new BorderRankData(0,null);
            selfData.playerData = PacketObject.Create<PNmlRankPlayer>();
            selfData.playerData.roleId = modulePlayer.roleInfo.roleId;
            selfData.playerData.name = modulePlayer.roleInfo.roleName;
            selfData.playerData.rankData = PacketObject.Create<PNmlRankData>();
            selfData.playerData.rankData.score = 0;
            selfData.playerData.rankData.money = 0;
        }
        if (data != null) selfData.SetRankData(data);
        SetSelfRank();
        DispatchModuleEvent(EventRefreshSelfRank);
    }

    public void QuestRankList()
    {
        var p = PacketObject.Create<CsNmlRankInfo>();
        session.Send(p);
    }

    void _Packet(ScNmlRankInfo p)
    {
        PNmlRankPlayer[] infos = null;
        p.rankInfos.CopyTo(ref infos);
        HandleRankList(infos);
    }

    private void HandleRankList(PNmlRankPlayer[] infos)
    {
        if (infos == null) return;

        RefreshRankList(infos);
        SetSelfRank();
        DispatchModuleEvent(EventRefreshSelfRank);
        DispatchModuleEvent(EventRefreshRankList);
    }

    private void RefreshRankList(PNmlRankPlayer[] infos)
    {
        if(rankList.Count < infos.Length)
        {
            for (int i = 0,count = infos.Length - rankList.Count; i < count; i++)
            {
                rankList.Add(new BorderRankData(0, infos[i]));
            }
        }

        for (int i = 0; i < rankList.Count; i++)
        {
            if (i >= infos.Length) continue;

            rankList[i].rank = i + 1;
            rankList[i].playerData = infos[i];
        }
    }

    private void SetSelfRank()
    {
        if (rankList == null || rankList.Count == 0 || selfData == null) return;

        foreach (var item in rankList)
        {
            if (item.roleId == selfData.roleId) selfData.rank = item.rank;
        }
    }

    #endregion

    #region choose scene Obj

    public void DispatchClickPlayerEvent(PVECreatureBehavior c)
    {
        DispatchModuleEvent(EventClickScenePlayerSuccess,c);
    }

    public void DispatchClickSceneObjEvent(Transform trans)
    {
        DispatchModuleEvent(EventClickSceneObjSuccess, trans);
    }
    #endregion
}
