/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Battle module
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-10-06
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

public class Module_Battle : Module<Module_Battle>
{
    #region Private frame data holder

    public enum FrameAction
    {
        None                = 0,
        StartTransport      = 1,
        TransportOver       = 2,
        PlayerReborn        = 3,
        MemberQuit          = 4,
         
        /// <summary> parameters:[storyId:int   storyIndex:int   step:int]</summary>
        StoryChangeStep     = 5,
    }

    public class FrameData : PoolObject<FrameData>
    {
        public static FrameData Create(int diff, int[] inputs = null, FrameAction action = FrameAction.None,params int[] parameters)
        {
            var frame = Create();
            frame.diff = diff;

            var ii = frame.inputStates;
            for (var i = 0; i < ii.Length; ++i)
                ii[i] = inputs != null && i < inputs.Length ? inputs[i] : 0;

            frame.action = action;
            frame.parameters = parameters;
            frame.strParams = string.Empty;
            return frame;
        }

        public int diff = 0;
        public int[] inputStates = new int[MAX_TEAM_MEMBER_COUNT];
        public FrameAction action;
        public int[] parameters;
        public string strParams;

#if AI_LOG
        public string createTip;
#endif


        private FrameData() { }
    }

    #endregion

    #region Static functions

    public const int KEY_LEFT              = 30;
    public const int KEY_RIGHT             = 31;
    public const int KEY_BITS              = 5;
    public const int MAX_KEY_CACHE_COUNT   = 4;
    public const int MAX_TEAM_MEMBER_COUNT = 40;
    public const int INSTANT_KEY_BITS      = KEY_BITS * MAX_KEY_CACHE_COUNT;

    #region Battle assets

    public static List<string> BuildPlayerPreloadAssets(List<string> assets = null)
    {
        if (assets == null) assets = new List<string>();

        // Player shadow
        assets.Add(CombatConfig.sdefaultSelfShadow);

        // Player model
        var model = ConfigManager.Get<CreatureInfo>(modulePlayer.proto);
        if (model) assets.AddRange(model.models);

        // Player weapons
        BuildWeaponPreloadAssets(modulePlayer.proto, modulePlayer.gender, moduleEquip.weaponID, moduleEquip.weaponItemID, moduleEquip.offWeaponID, moduleEquip.offWeaponItemID, assets, true);
        
        // Buff effects
        BuildBuffPreloadAssets(modulePlayer.buffs, assets);

        // Equipments
        assets.AddRange(CharacterEquip.GetEquipAssets(moduleEquip.currentDressClothes));

        // Pet
        BuildPetPreloadAssets(assets, modulePet.FightingPet);

        return assets;
    }

    public static void BuildPetPreloadAssets(List<string> assets, PetInfo pet)
    {
        if (pet == null) return;

        var creatureInfo = pet.BuildCreatureInfo();

        assets.AddRange(creatureInfo.models);

        //pet weapons
        BuildWeaponPreloadAssets(0, 1, creatureInfo.weaponID, creatureInfo.weaponItemID, creatureInfo.offWeaponID, creatureInfo.offWeaponItemID, assets);

        var skill = pet.GetSkill();
        if (skill != null && skill.buffs != null)
            BuildBuffPreloadAssets(skill.buffs, assets);
    }

    public static List<string> BuildPlayerPreloadAssets(PMatchInfo info, List<string> assets = null)
    {
        if (assets == null) assets = new List<string>();

        // Player shadow
        if (info.roleId == modulePlayer.id_) assets.Add(CombatConfig.sdefaultSelfShadow);

        // Player model
        var model = ConfigManager.Get<CreatureInfo>(info.roleProto);
        if (model) assets.AddRange(model.models);

        var w  = WeaponInfo.GetWeapon(0, info.fashion.weapon);
        var ww = WeaponInfo.GetWeapon(0, info.fashion.gun);

        // Player weapons
        BuildWeaponPreloadAssets(info.roleProto, info.gender, w.weaponID, w.weaponItemId, ww.weaponID, ww.weaponItemId, assets);

        // Buff effects
        BuildBuffPreloadAssets(info.buffs, assets);

        // Equipments
        assets.AddRange(CharacterEquip.GetEquipAssets(info.fashion));

        // Pet
        BuildPetPreloadAssets(assets, info.pet == null || info.pet.itemTypeId == 0 ? null : PetInfo.Create(info.pet));

        return assets;
    }

    public static List<string> BuildPlayerPreloadAssets(PTeamMemberInfo info, List<string> assets = null)
    {
        if (assets == null) assets = new List<string>();
        if (info == null)
            return assets;

        // Player shadow
        if (info.roleId == modulePlayer.id_) assets.Add(CombatConfig.sdefaultSelfShadow);

        // Player model
        var model = ConfigManager.Get<CreatureInfo>(info.roleProto);
        if (model) assets.AddRange(model.models);

        var w = WeaponInfo.GetWeapon(0, info.fashion.weapon);
        var ww = WeaponInfo.GetWeapon(0, info.fashion.gun);

        // Player weapons
        BuildWeaponPreloadAssets(info.roleProto, info.gender, w.weaponID, w.weaponItemId, ww.weaponID, ww.weaponItemId, assets);

        // Buff effects
        BuildBuffPreloadAssets(info.buffs, assets);

        // Equipments
        assets.AddRange(CharacterEquip.GetEquipAssets(info.fashion));

        // Pet
        BuildPetPreloadAssets(assets, info.pet != null && info.pet.itemTypeId != 0 ? PetInfo.Create(info.pet) : null);

        return assets;
    }

    public static List<string> BuildBuffPreloadAssets(IEnumerable<int> bufs, List<string> assets = null)
    {
        if (assets == null) assets = new List<string>();

        foreach (var buf in bufs)
        {
            var bf = ConfigManager.Get<BuffInfo>(buf);
            if (bf) bf.GetAllAssets(assets);
        }

        return assets;
    }

    public static List<string> BuildWeaponPreloadAssets(int proto, int gender, int weaponID, int weaponItemID = -1, int offWeaponID = -1, int offWeaponItemID = -1, List<string> assets = null, bool player = false)
    {
        if (assets == null) assets = new List<string>();

        assets.Add(Creature.GetAnimatorName(weaponID, gender));    // Animator

        var w = WeaponInfo.GetWeapon(weaponID, weaponItemID);      // Main-hand weapon models
        w.GetAllAssets(assets);

        assets.Add(WeaponInfo.GetVictoryAnimation(weaponID, gender));

        if (offWeaponID > -1)
        {
            w = WeaponInfo.GetWeapon(offWeaponID, offWeaponItemID);   // Off-hand weapon models
            w.GetAllAssets(assets);
        }

        // Collect all weapon assets
        StateMachineInfo.GetAllAssets(weaponID, offWeaponID, assets, proto, gender, weaponItemID, offWeaponItemID, player);

        return assets;
    }

    #endregion

    #region Simple assets

    public static List<string> BuildPlayerSimplePreloadAssets(List<string> assets = null)
    {
        if (assets == null) assets = new List<string>();

        // Player model
        var model = ConfigManager.Get<CreatureInfo>(modulePlayer.proto);
        if (model) assets.AddRange(model.models);

        // Player weapons
        BuildWeaponSimplePreloadAssets(modulePlayer.proto, modulePlayer.gender, moduleEquip.weaponID, moduleEquip.weaponItemID, assets);

        // Equipments
        assets.AddRange(CharacterEquip.GetEquipAssets(moduleEquip.currentDressClothes));

        return assets;
    }

    public static List<string> BuildPlayerSimplePreloadAssets(PRoleSummary info, List<string> assets = null)
    {
        if (assets == null) assets = new List<string>();

        // Player model
        var model = ConfigManager.Get<CreatureInfo>(info.proto);
        if (model) assets.Add(model.models[0]);

        var w = WeaponInfo.GetWeapon(0, info.fashion.weapon);

        // Player weapons
        BuildWeaponSimplePreloadAssets(info.proto, info.gender, w.weaponID, w.weaponItemId, assets);

        // Equipments
        assets.AddRange(CharacterEquip.GetEquipAssets(info.fashion));

        if (info.pet == null || info.pet.itemId == 0) return assets;

        var rPet = PetInfo.Create(info.pet);

        BuildPetSimplePreloadAssets(rPet, assets, 2);

        return assets;
    }

    public static List<string> BuildPlayerSimplePreloadAssets(PMatchInfo info, List<string> assets = null)
    {
        if (assets == null) assets = new List<string>();

        // Player model
        var model = ConfigManager.Get<CreatureInfo>(info.roleProto);
        if (model) assets.AddRange(model.models);

        var w  = WeaponInfo.GetWeapon(0, info.fashion.weapon);

        // Player weapons
        BuildWeaponSimplePreloadAssets(info.roleProto, info.gender, w.weaponID, w.weaponItemId, assets);

        // Equipments
        assets.AddRange(CharacterEquip.GetEquipAssets(info.fashion));

        return assets;
    }

    public static List<string> BuildPlayerSimplePreloadAssets(PMatchProcessInfo info, List<string> assets = null)
    {
        if (assets == null) assets = new List<string>();

        // Player model
        var model = ConfigManager.Get<CreatureInfo>(info.roleProto);
        if (model) assets.AddRange(model.models);

        var w = WeaponInfo.GetWeapon(0, info.fashion.weapon);

        // Player weapons
        BuildWeaponSimplePreloadAssets(info.roleProto, info.gender, w.weaponID, w.weaponItemId, assets);

        // Equipments
        assets.AddRange(CharacterEquip.GetEquipAssets(info.fashion));

        return assets;
    }

    public static List<string> BuildWeaponSimplePreloadAssets(int proto, int gender, int weaponID, int weaponItemID, List<string> assets = null)
    {
        if (assets == null) assets = new List<string>();

        assets.Add(Creature.GetAnimatorNameSimple(weaponID, gender)); // Animator

        var w = WeaponInfo.GetWeapon(weaponID, weaponItemID); // Main-hand weapon models
        w.GetAllAssets(assets);

        // Simple statemachine
        StateMachineInfo.GetAllAssets(weaponID, -1, assets, proto, gender, weaponItemID, -1, false, true);

        return assets;
    }

    public static List<string> BuildNpcSimplePreloadAssets(int npcID, List<string> assets = null)
    {
        if (assets == null) assets = new List<string>();

        var info = ConfigManager.Get<NpcInfo>(npcID);
        if (info == null) return assets;

        assets.Add(info.mode);
        // Npc model
        if (info.type == 1)
        {
            var npc = moduleNpc.GetTargetNpc((NpcTypeID)npcID);
            if (npc != null && !assets.Contains(npc.mode)) assets.Add(npc.mode);
        }

        // Animator
        BuildWeaponSimplePreloadAssets(0, 0, info.stateMachine, -1, assets);

        return assets;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rPet"></param>
    /// <param name="assets"></param>
    /// <param name="way">用途 0 ui 1 战斗 2 战斗模型ui</param>
    /// <returns></returns>
    public static List<string> BuildPetSimplePreloadAssets(PetInfo rPet, List<string> assets, int way)
    {
        if (assets == null) assets = new List<string>();

        if (rPet == null) return assets;
        var rGradeInfo = rPet.UpGradeInfo;
        if (rGradeInfo == null) return assets;
        // Npc model
        assets.Add(way > 0 ? rGradeInfo.fightMode : rGradeInfo.mode);

        // Animator
        if (way == 1)
        {
            var creatureInfo = rPet.BuildCreatureInfo();
            //pet weapons
            BuildWeaponPreloadAssets(0, 1, creatureInfo.weaponID, creatureInfo.weaponItemID, creatureInfo.offWeaponID, creatureInfo.offWeaponItemID, assets);
        }
        else if(way == 0)
            BuildWeaponSimplePreloadAssets(0, 0, rGradeInfo.stateMachine, -1, assets);
        else if(way == 2)
            BuildWeaponSimplePreloadAssets(0, 0, rGradeInfo.UIstateMachine, -1, assets);

        return assets;
    }

    #endregion

    #region Random

    /// <summary>
    /// Returns a random double number between 0 [inclusive] and 1.0 [exclusive]
    /// </summary>
    /// <returns></returns>
    public static double Range()
    {
        return instance._Range(0, 1.0);
    }

    /// <summary>
    /// Returns a random double number between min [inclusive] and max [exclusive]
    /// </summary>
    /// <returns></returns>
    public static double Range(double min, double max)
    {
        return instance._Range(min, max);
    }

    /// <summary>
    /// Returns a random int number between min [inclusive] and max [exclusive]
    /// </summary>
    public static int Range(int min, int max)
    {
        return instance._Range(min, max);
    }

    #endregion

    #endregion
    
    #region Random

    private PseudoRandom m_random = new PseudoRandom();

    /// <summary>
    /// Returns a random double number between 0 [inclusive] and 1.0 [exclusive]
    /// </summary>
    /// <returns></returns>
    public double _Range()
    {
        #if AI_LOG
        moduleAI.LogRandomMsg("_Range without parameters");
        #endif
        return m_random.Range(0, 1.0);
    }

    /// <summary>
    /// Returns a random double number between min [inclusive] and max [exclusive]
    /// </summary>
    /// <returns></returns>
    public double _Range(double min, double max)
    {
        #if AI_LOG
        moduleAI.LogRandomMsg("_Range with double parameters min = {0} max = {1}", min, max);
        #endif
        return m_random.Range(min, max);
    }

    /// <summary>
    /// Returns a random int number between min [inclusive] and max [exclusive]
    /// </summary>
    /// <returns></returns>
    public int _Range(int min, int max)
    {
        #if AI_LOG
        moduleAI.LogRandomMsg("_Range with int parameters min = {0} max = {1}", min, max);
        #endif
        return m_random.Range(min, max);
    }

    #endregion

    #region Fields

    public bool teamMode { get { return m_teamMode; } }
    public bool isPvP { get { return m_isPvP; } }
    public bool fightStarted { get { return m_fightStarted; } }
    public bool inDialog { get { return m_inDialog; } }

    private int[] m_inputStates  = new int[MAX_TEAM_MEMBER_COUNT];   // Local input states
    private int[] m_teamInputs   = new int[MAX_TEAM_MEMBER_COUNT];   // Synced team input states

    private int m_inputIndex = 0;

    private bool m_teamMode = false;
    private bool m_isPvP = false;
    private bool m_fightStarted = false;
    private bool m_inDialog = false;

    private Queue<FrameData> m_frames = new Queue<FrameData>();

    /// <summary>
    /// Current controlled Creature in battle scene
    /// </summary>
    public Creature current { get; private set; }
    /// <summary>
    /// Parse input ?
    /// If false, GetInput() always return 0
    /// </summary>
    public bool parseInput { get; set; }

    /// <summary>
    /// max combo, all battle mode(pvp,pve) need record
    /// </summary>
    public int maxCombo;

    private int m_state = 0;
    private int m_teamIndex = -1;
    private int m_disabledKeys = 0;
    
    public FrameData frameActionData { get; private set; }

#if AI_LOG
    private int recvFrameMsgCount;
#endif

    #endregion

    #region Logic

    protected override void OnModuleCreated()
    {
        EventManager.AddEventListener(Events.SCENE_LOAD_START, OnSceneLoadStart);  // Use SCENE_LOAD_START becaus we may create creature or other logic object before SCENE_LOAD_COMPLETE
        EventManager.AddEventListener(Events.ROOT_ENTER_FRAME, OnRootEnterFrame);
        EventManager.AddEventListener(CreatureEvents.PLAYER_ADD_TO_SCENE, OnPlayerAddToScene);
        EventManager.AddEventListener(LevelEvents.START_FIGHT, OnStartFight);
        EventManager.AddEventListener(LevelEvents.DIALOG_STATE, OnDialogState);
    }

    protected override void OnGameDataReset()
    {
        m_state         = 0;
        m_disabledKeys  = 0;
        m_inputIndex    = 0;
        m_teamIndex     = -1;
        current         = null;
        m_teamMode      = false;
        m_isPvP         = false;
        m_fightStarted  = false;
        m_inDialog      = false;

        ResetTeamData();
    }

    public override void OnRootUpdate(float diff)
    {
        if (!m_teamMode || m_frames.Count < 1) return;

        // increase speed rate every 0.5 seconds delay
        var c = Mathf.CeilToInt(m_frames.Count / 30.0f + 1);
        while (c-- > 0 && m_frames.Count > 0)
        {
            var p = m_frames.Dequeue();
            FrameUpdate(p);

            p.Destroy();
        }
    }

    public void SetKeyState(int ID, bool enabled)
    {
        m_disabledKeys = m_disabledKeys.BitMask(ID, !enabled);
    }

    public bool GetKeyState(int ID)
    {
        return !m_disabledKeys.BitMask(ID);
    }

    public void SetKeyState(string key, bool enabled)
    {
        var i = InputManager.GetKey(key);
        if (i == null) return;
        m_disabledKeys = m_disabledKeys.BitMask(i.ID, !enabled);
    }

    public bool GetKeyState(string key)
    {
        var i = InputManager.GetKey(key);
        if (i == null) return false;
        return !m_disabledKeys.BitMask(i.ID);
    }

    public int GetInput(int index)
    {
        if (!parseInput || index < 0 || index >= MAX_TEAM_MEMBER_COUNT) return 0;

        return m_inputStates[index];
    }

    public void ResetInput()
    {
        m_inputStates  = new int[MAX_TEAM_MEMBER_COUNT];   // Local input states
        m_teamInputs   = new int[MAX_TEAM_MEMBER_COUNT];   // Synced team input states

        m_state        = 0;
        m_disabledKeys = 0;
        m_inputIndex   = 0;

        parseInput   = true;
    }

    private void OnInputChanged(InputKey[] changedKeys, int count)
    {
        if (!current) return;

        // Inputs bitmask:The first 20 bits contains 4 input id. every 5 bit stored one input id, 20-29 contains hold input, last 2 bits: left and right movement state
        var ns = m_state;

        //var n = $"({ns.BitMask(24)},{ns >> 20 & 0x0F}|{ns.BitMask(29)},{ns >> 25 & 0x0F}),";
        //for (var pp = 0; pp < MAX_KEY_CACHE_COUNT; ++pp)
        //{
        //    var k = (m_state >> KEY_BITS * pp) & 0x1F;
        //    n += "[" + k + "], ";
        //}
        //Logger.LogDetail("nidx: {0}, keys: {1}", m_inputIndex, n);

        for (var i = 0; i < count; ++i)
        {
            var key = changedKeys[i];

            if (key.value == KEY_LEFT || key.value == KEY_RIGHT)  // Movement key
            {
                ns = ns.BitMask(key.value, key.fired);
                continue;
            }

            if (m_disabledKeys.BitMask(key.ID)) continue; // Key disabled
            if (key.ID < 1 || key.ID > 0x1F) continue; // Valid input id: [1 - 31]
            if (Mathd.Abs(key.value) < 1 || Mathd.Abs(key.value) > 0x1D) continue;   // Valid key value: [1 - 29]

            if (key.isInstant && (m_inputIndex >= MAX_KEY_CACHE_COUNT || !key.fired)) continue;

            if (key.keyFired && !OnValidateKeyboard(key)) continue;

            var val = key.ID;
            if (!key.isInstant)
            {
                val = val.BitMask(4, true);
                var hold = ns >> INSTANT_KEY_BITS & 0x3FF;
                int h1 = hold & 0x1F, h2 = hold >> 5 & 0x1F;
                if (!key.fired)
                {
                    if (val != h1 && val != h2) continue;
                    h1 = val == h1 ? h2 : h1;
                    h2 = 0;
                }
                else
                {
                    h2 = h1;
                    h1 = val;
                }
                hold = h1 == h2 ? h1 : h1 | h2 << 5;
                ns = ns & ~0x3FF00000 | hold << INSTANT_KEY_BITS;
            }
            else
            {
                ns |= val << m_inputIndex * KEY_BITS;
                ++m_inputIndex;
            }
        }

        //n = $"({ns.BitMask(24)},{ns >> 20 & 0x0F}|{ns.BitMask(29)},{ns >> 25 & 0x0F}),";
        //for (var pp = 0; pp < MAX_KEY_CACHE_COUNT; ++pp)
        //{
        //    var k = (m_state >> KEY_BITS * pp) & 0x0F;
        //    n += "[" + k + "], ";
        //}
        //Logger.LogDetail("nidx: {0}, keys: {1}", m_inputIndex, n);

        UpdateState(ns);
    }

    private bool OnValidateKeyboard(InputKey key)
    {
        if (key.name != "O") return true;

        if (current && Level.currentLevel != GeneralConfigInfo.sTrainLevel && current.bulletCount < 1)
        {
            moduleGlobal.ShowMessage(9201);
            return false;
        }

        return true;
    }

    private void OnSceneLoadStart()
    {
        m_teamIndex = -1;

        m_isPvP = Level.next.isPvP;
        m_teamMode = m_isPvP || modulePVE.isTeamMode;
        m_fightStarted = false;
        m_inDialog = false;

        InputManager.onInputChanged -= OnInputChanged;
        if (Level.next.isBattle) InputManager.onInputChanged += OnInputChanged;

        ResetInput();
    }

    private void OnRootEnterFrame(Event_ e)
    {
        if (m_teamIndex < 0) return;

        if (!m_teamMode) m_inputStates[m_teamIndex] = m_state;

        m_state &= ~0xFFFFF;  // Clear instant keys
        m_inputIndex = 0;
    }

    private void UpdateState(int state)
    {
        if (m_state == state) return;
        m_state = state;

        if (m_teamMode)
        {
            var p = PacketObject.Create<CsInputChanged>();
            p.input = m_state;
            if (m_isPvP) modulePVP.Send(p);
            else moduleTeam.Send(p);
        }
    }

    private void OnPlayerAddToScene(Event_ e)
    {
        current = e.sender as Creature;
        if (current)
        {
            SetPlayerTeamIndex(current.teamIndex);

            m_inputStates[m_teamIndex] = 0;
            m_teamInputs[m_teamIndex] = 0;

            current.AddEventListener(Events.ON_DESTROY, OnPlayerDestroyed);
        }
    }

    public void SetPlayerTeamIndex(int index)
    {
        if (index < 0 || index >= MAX_TEAM_MEMBER_COUNT)
        {
            Logger.LogError("Module_Battle::SetPlayerTeamIndex: Invalid team index <b><color=#3D2CFF>[{0}]</color></b>, set to -1", index);
            index = -1;
        }
        m_teamIndex = index;
    }

    private void OnPlayerDestroyed(Event_ e)
    {
        if (e.sender != current) return;

        current = null;
        if (m_teamIndex > -1)
        {
            m_inputStates[m_teamIndex] = 0;
            m_teamInputs[m_teamIndex] = 0;
        }
        m_teamIndex = -1;
    }

    private void OnStartFight()
    {
        m_fightStarted = true;
    }

    private void OnDialogState(Event_ e)
    {
        m_inDialog = (bool)e.param1;
    }

    private void FrameUpdate(FrameData p)
    {
        FightRecordManager.FrameUpdate();
        m_inputStates = p.inputStates;
        if (p.action != FrameAction.None) frameActionData = p;

        ObjectManager.LogicUpdate(p.diff);

#if AI_LOG
        Module_AI.LogBattleMsg(null, "FrameUpdate   diff is {0},create resource is {1}",p.diff,p.createTip);
#endif
    }

    private void ResetTeamData()
    {
        foreach (var frame in m_frames) frame.Destroy();
        m_frames.Clear();

        ResetInput();
        ResetFrameAction();
    }

    public void ResetFrameAction()
    {
        frameActionData = null;
    }
    #endregion

    #region Packet handlers

    void _Packet(ScInputChanged p)
    {
        var idx = p.guid;
        if (idx >= m_teamInputs.Length)
        {
            Logger.LogWarning("Module_Battle::_Packet(ScInputChanged): Receive invalid input packet. [guid: {0}, input: {1}]", p.guid, p.input);
            return;
        }

        m_teamInputs[idx] = p.input;
    }

    void _Packet(ScFrameUpdate p)
    {
        FightRecordManager.UpdateMessageIndex();

        var diff = (int)p.diff;
        do
        {
            var frame = FrameData.Create(diff < 33 ? diff : 33, m_teamInputs);
            m_frames.Enqueue(frame);

#if AI_LOG
            frame.createTip = Util.Format("recv ScFrameUpdate count is {0}", ++recvFrameMsgCount);
#endif
            for (int i = 0, c = m_teamInputs.Length; i < c; ++i) m_teamInputs[i] &= ~0xFFFFF;

            diff -= 33;
        } while (diff > 0);
        
        //if (m_frames.Count > 2) Logger.LogInfo("Receive frame update! current count: {0}", m_frames.Count);
    }

    void _Packet(ScFrameUpdateInput p)
    {
        var inputs = p.inputs;
        foreach (var input in inputs)
        {
            if (input.guid >= m_teamInputs.Length)
            {
                Logger.LogWarning("Module_Battle::_Packet(ScFrameUpdateInput): Receive invalid input packet. [guid: {0}, input: {1}]", input.guid, input.input);
                continue;
            }
            m_teamInputs[input.guid] = input.input;
        }

        var diff = (int)p.diff;
        do
        {
            var frame = FrameData.Create(diff < 33 ? diff : 33, m_teamInputs);
            m_frames.Enqueue(frame);

#if AI_LOG

            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (var input in inputs)
            {
                if (input.guid >= m_teamInputs.Length) continue;
                s.AppendFormat("guid = {0} input = {1}\t", input.guid, input.input);
            }
            frame.createTip = Util.Format("recv ScFrameUpdateInput count is {0} ,msg is {1}", ++recvFrameMsgCount,s.ToString());
#endif

            for (int i = 0, c = m_teamInputs.Length; i < c; ++i) m_teamInputs[i] &= ~0xFFFFF;

            diff -= 33;
        } while (diff > 0);

        //if (m_frames.Count > 2) Logger.LogInfo("Receive frame update! current count: {0}", m_frames.Count);
    }

    void _Packet(ScTeamTransportStart p)
    {
        Logger.LogInfo("create frame as Team room transport scene start loading! ");
        var frame = FrameData.Create(30, m_teamInputs,FrameAction.StartTransport);
        m_frames.Enqueue(frame);

#if AI_LOG
        frame.createTip = Util.Format("recv ScTeamTransportStart frameCount = {0}", ++recvFrameMsgCount);
#endif
    }

    void _Packet(ScTeamTransportOver p)
    {
        Logger.LogInfo("create frame as Team transport completed!");
        var frame = FrameData.Create(30, m_teamInputs, FrameAction.TransportOver);
        m_frames.Enqueue(frame);

#if AI_LOG
        frame.createTip = Util.Format("recv ScTeamTransportOver frameCount = {0}", ++recvFrameMsgCount);
#endif
    }

    void _Packet(ScTeamReborn p)
    {
        Logger.LogDetail("recv msg Team member [{0}] reborn", p.guid);
        var frame = FrameData.Create(30, m_teamInputs, FrameAction.PlayerReborn, p.guid);
        m_frames.Enqueue(frame);

#if AI_LOG
        frame.createTip = Util.Format("Team member [{0}] reborn frameCount = {1}", p.guid, ++recvFrameMsgCount);
#endif
    }

    void _Packet(ScTeamQuit p)
    {
        var frame = FrameData.Create(0, m_teamInputs, FrameAction.MemberQuit,p.reason, p.guid);
        m_frames.Enqueue(frame);

#if AI_LOG
        frame.createTip = Util.Format("Team member [{0}] quit reason {1} frameCount = {2}", p.guid,(EnumTeamQuitState)p.reason, ++recvFrameMsgCount);
#endif
    }

    void _Packet(CScTeamBehaviour p)
    {
        var action = (FrameAction)p.intParams.GetValue<int>(0);
        Logger.LogInfo("recv CScTeamBehaviour..............frame action is {0}",action);

        int[] array = null;
        if(p.intParams != null && p.intParams.Length > 1)
        {
            array = new int[p.intParams.Length - 1];
            Array.Copy(p.intParams, 1, array, 0, p.intParams.Length - 1);
        }

        var frame = FrameData.Create(30, m_teamInputs, action, array);
        if (!string.IsNullOrEmpty(p.strParam)) frame.strParams = p.strParam;

        m_frames.Enqueue(frame);

#if AI_LOG
        frame.createTip = Util.Format("CScTeamBehaviour action is [{0}] intparameers is {1} str param is {2}", action,p.intParams.ToXml(), string.IsNullOrEmpty(p.strParam) ? "empty" : p.strParam);
#endif
    }

    void _Packet(ScRoomOver p)
    {
        // Clear team data if battle ended
        ResetTeamData();

        FightRecordManager.Record(p);

        FightRecordManager.EndRecord(false, false);
        #region NetStat statistic
#if NETSTAT
        var r = modulePVP.useGameSession ? session.receiver : modulePVP.receiver;
        if (r != null) r.pauseNetStatistic = true;
#endif
        #endregion
    }

    void _Packet(ScTeamRequestEnd p)
    {
        // Clear team data if battle ended
        ResetTeamData();

        #region NetStat statistic
        #if NETSTAT
        var r = moduleTeam.useGameSession ? session.receiver : moduleTeam.receiver;
        if (r != null) r.pauseNetStatistic = true;
        #endif
        #endregion
    }

    void _Packet_999(ScRoomStart p)
    {
        FightRecordManager.Set(p);
        FightRecordManager.StartRecord();

        m_random.seed = p.seed;
        Logger.LogDetail("Module_Battle::ScRoomStart: Set random seed to <color=#00FF00FF><b>{0}</b></color> from <color=#00FF00FF><b>PVP</b></color>", m_random.seed);
    }

    void _Packet_999(ScTeamStart p)
    {
        m_random.seed = p.seed;

#if AI_LOG
        recvFrameMsgCount = 0;
#endif
    Logger.LogDetail("Module_Battle::ScTeamStart: Set random seed to <color=#00FF00FF><b>{0}</b></color> from <color=#00FF00FF><b>Team</b></color>", m_random.seed);
    }

    #endregion
}
