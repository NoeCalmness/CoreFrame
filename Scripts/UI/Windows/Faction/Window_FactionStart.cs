// /**************************************************************************************************
//  * Copyright (C) 2017-2018 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-06-12      20:27
//  *LastModify：2019-06-12      20:27
//  ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_FactionStart : Window
{
    private struct PersonInfoEntry
    {
        private Text name;
        private Text level;
        private Text kill;
        private Image killIcon;
        private Text score;

        public void BindNode(Transform rNode)
        {
            name        = rNode.GetComponent<Text>("bg/name/name");
            level       = rNode.GetComponent<Text>("bg/name/level");
            kill        = rNode.GetComponent<Text>("bg/name/name/degree/Text");
            score       = rNode.GetComponent<Text>("bg/score/scoreText");
            killIcon    = rNode.GetComponent<Image>("bg/name/name/degree/kill_icon");
        }

        public void BindData(PMatchInfo rMatchInfo, PBattleInfo rBattleInfo)
        {
            Util.SetText(name, rMatchInfo.roleName);
            Util.SetText(level, $"lv:{rMatchInfo.level}");
            Util.SetText(kill, Module_FactionBattle.GetKillString(rBattleInfo.maxCombokill));
            Util.SetText(score, rBattleInfo.score.ToString());
            killIcon.SafeSetActive(!string.IsNullOrEmpty(kill.text));
            var info = ConfigManager.Get<FactionKillRewardInfo>(rBattleInfo.maxCombokill);
            if (!string.IsNullOrEmpty(info?.applique))
                AtlasHelper.SetIcons(killIcon, info.applique);
        }
    }

    private TweenBase       m_tween;
    private PersonInfoEntry m_redEntry;
    private PersonInfoEntry m_blueEntry;
    private Text            m_timer;
    private float           m_timeStamp;
    private Creature        m_enemy;
    private Creature        m_enemyPet;
    private Coroutine       m_LoadCoroutine;
    private Coroutine       m_petLoadCoroutine;
    private bool            m_startLoad;
    private bool            m_tweenComplete;

    protected override void OnOpen()
    {
        ignoreStack = true;
        base.OnOpen();
        MultiLangrage();
        m_redEntry .BindNode(GetComponent<Transform>("left"));
        m_blueEntry.BindNode(GetComponent<Transform>("right"));
        m_timer = GetComponent<Text>("ranking/time");
        m_tween = GetComponent<TweenAlpha>("ranking/vs");
    }

    private void MultiLangrage()
    {
        Util.SetText(GetComponent<Text>("left/bg/score/Text"), ConfigText.GetDefalutString(280, 13));
        Util.SetText(GetComponent<Text>("right/bg/score/Text (1)"), ConfigText.GetDefalutString(280, 13));
        Util.SetText(GetComponent<Text>("left/bg/score/scoreText/Text"), ConfigText.GetDefalutString(280, 14));
        Util.SetText(GetComponent<Text>("right/bg/score/scoreText/Text"), ConfigText.GetDefalutString(280, 14));
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault(-1, false);
        base.OnBecameVisible(oldState, forward);
        m_startLoad = false;
        m_tweenComplete = false;
        m_tween.onComplete.AddListener(b =>
        {
            m_timeStamp = Time.realtimeSinceStartup;
            m_tweenComplete = true;
            m_timer.gameObject.SetActive(true);
        });

        var matchInfo = Window.GetWindowParam<Window_FactionStart>().param1 as ScMatchInfo;
        if (matchInfo == null)
            return;
        ShowModels(matchInfo);
        var index = moduleFactionBattle.factionMatchInfo.infos.FindIndex(item => item.faction == (byte)Module_FactionBattle.Faction.Red);
        m_redEntry.BindData(matchInfo.infoList[index], moduleFactionBattle.factionMatchInfo.infos[index]);
        index = moduleFactionBattle.factionMatchInfo.infos.FindIndex(item => item.faction == (byte)Module_FactionBattle.Faction.Blue);
        m_blueEntry.BindData(matchInfo.infoList[index], moduleFactionBattle.factionMatchInfo.infos[index]);
        
        moduleHome.DispatchEvent(Module_Home.EventSwitchCameraMode, Event_.Pop(CameraShowType.Pvp));
    }

    public override void OnRenderUpdate()
    {
        base.OnRenderUpdate();

        if (m_startLoad || !m_tweenComplete) return;

        if (m_timeStamp + GeneralConfigInfo.defaultConfig.factionReadlyCountDown <= Time.realtimeSinceStartup)
        {
            Game.LoadLevel(13);
            m_startLoad = true;
        }
        else
        {
            Util.SetText(m_timer, Mathf.RoundToInt(m_timeStamp + GeneralConfigInfo.defaultConfig.factionReadlyCountDown - Time.realtimeSinceStartup).ToString());
        }
    }

    private void ShowModels(ScMatchInfo rMatchInfo)
    {
        moduleHome.HideOthersBut(Module_Home.FIGHTING_PET_OBJECT_NAME, Module_Home.PLAYER_OBJECT_NAME, Module_Home.TEAM_OBJECT_NAME, Module_Home.TEAM_PET_OBJECT_NAME);

        moduleHome.DispatchEvent(Module_Home.EventSetMasterPosition, Event_.Pop(GetTransform(moduleFactionBattle.SelfFaction == Module_FactionBattle.Faction.Red ? 0 : 1)));
        var master = (Level.current as Level_Home)?.master;
        if (master)
        {
            master.direction = moduleFactionBattle.SelfFaction == Module_FactionBattle.Faction.Red
                ? CreatureDirection.FORWARD
                : CreatureDirection.BACK;
        }
        for (var i = 0; i < rMatchInfo.infoList.Length; i++)
        {
            if(rMatchInfo.infoList[i].roleId != modulePlayer.id_)
                CreatEnemyCreature(rMatchInfo.infoList[i], moduleFactionBattle.SelfFaction == Module_FactionBattle.Faction.Red ? 1 : 0);
        }
    }

    private void CreatEnemyCreature(PMatchInfo info, int index)
    {
        //创建敌人
        var t = GetTransform(index);
        var pos = t.pos;
        var rot = t.rotation;

        var weaponInfo = ConfigManager.Get<PropItemInfo>(info.fashion.weapon);
        if (weaponInfo == null) return;

        if (m_LoadCoroutine != null)
        {
            Level.current.StopCoroutine(m_LoadCoroutine);
            m_LoadCoroutine = null;
        }
        if (m_petLoadCoroutine != null)
        {
            Level.current.StopCoroutine(m_petLoadCoroutine);
            m_petLoadCoroutine = null;
        }

        m_LoadCoroutine = Level.PrepareAssets(Module_Battle.BuildPlayerSimplePreloadAssets(info), r =>
        {
            m_petLoadCoroutine = null;
            if (!r) return;

            m_enemy = moduleHome.CreatePlayer(info, pos, index == 0 ? CreatureDirection.FORWARD: CreatureDirection.BACK);
            m_enemy.uiName = Module_Home.TEAM_OBJECT_NAME;
            m_enemy.gameObject.name = Module_Home.TEAM_OBJECT_NAME;
            m_enemy.transform.localEulerAngles = rot;
            m_enemy.transform.localScale = Vector3.one * t.size;
        });

        if (info.pet != null && info.pet.itemTypeId != 0)
        {
            var rPet = PetInfo.Create(info.pet);
            var assets = new List<string>();
            m_petLoadCoroutine = Level.PrepareAssets(Module_Battle.BuildPetSimplePreloadAssets(rPet, assets, 2), b =>
            {
                m_petLoadCoroutine = null;
                if (!b)
                    return;
                var rGradeInfo = rPet.UpGradeInfo;
                var show = ConfigManager.Get<ShowCreatureInfo>(rPet.ID);
                if (show == null)
                {
                    Logger.LogError("没有配置config_showCreatureInfo表。宠物ID = {0}, 没有出生位置信息。宠物模型创建失败", rPet.ID);
                    return;
                }
                var showData = show.GetDataByIndex(0);
                var data = showData.data.GetValue<ShowCreatureInfo.SizeAndPos>(0);
                m_enemyPet = moduleHome.CreatePet(rGradeInfo, pos + data.pos, data.rotation, Level.current.startPos, true,
                    Module_Home.TEAM_PET_OBJECT_NAME);
                m_enemyPet.transform.localScale *= data.size;
                m_enemyPet.transform.localEulerAngles = data.rotation;
            });
        }
    }

    private ShowCreatureInfo.SizeAndPos GetTransform(int index)
    {
        var info = ConfigManager.Get<ShowCreatureInfo>(index == 0 ? 11 : 10);
        ShowCreatureInfo.SizeAndPos t = null;
        if (info != null)
        {
            for (int i = 0; i < info.forData.Length; i++)
            {
                if (info.forData[i].index == modulePlayer.proto)
                {
                    t = info.forData[i].data[0];
                    break;
                }
            }
        }
        return t;
    }
}
