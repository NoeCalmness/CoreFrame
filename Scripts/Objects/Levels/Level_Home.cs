// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-07-12      20:46
//  * LastModify：2018-07-13      16:53
//  ***************************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

public enum CameraShowType
{
    Home,
    PetSummon,
    Awake,
    Pvp,
    Rank,
    NpcAwake,
    Dating,
}

public class Level_Home : Level
{
    private const string            AnimatorParam_Summon        = "forword";
    private const string            AnimatorParam_Camera_Summon = "SummonForword";
    private Animator                cameraAnimator;
    private Animator                eggAnimator;
    private Transform               eggRoot;
    private Creature                npc;
    private Creature                pet;
    private Camera                  petCamera;
    private Vector3                 petCameraPos;
    private Quaternion              petCameraRotation;
    private Transform               summonEffect;
    private Transform               petObjects;
    private Coroutine               summonEffectPlayCoroutine;

    #region 觉醒
    public  Camera                  awakeCamera;
    public  Camera                  npcAwakeCamera;
    public  Transform               awakeRoot;
    public  Transform               npcAwakeRoot;
    #endregion

    #region main
    private Transform               _mainScence;
    #endregion

    #region pvp
    public Camera                   pvpCamera;
    private Transform               pvpScence;
    public Camera                   rankCamera;
    private Transform               rankScence;
    #endregion

    #region 约会
    public Camera                   datingCamera;
    public Transform                datingScence;
    private Vector3                 datingCameraPos;
    private Quaternion              datingCameraRotation;
    #endregion

    public Creature                 master;
    public  Creature                fightingPet;
    private Vector3_                originMasterPos;
    private Vector3_                originPetPos;
    private Vector3_                recordMasterPos;

    private Coroutine               sceneLoadCoroutine;
    public GameObject PetGameObject
    {
        get
        {
            if (!pet)
                return null;
            return pet.gameObject;
        }
    }
    private bool m_duringTheatryStory;

    protected override List<string> BuildPreloadAssets()
    {
        var assets = base.BuildPreloadAssets();
        //收到角色信息后才预加载。否则采用动态加载
        if(moduleEquip.weaponID != 0)
            assets.AddRange(Module_Battle.BuildPlayerSimplePreloadAssets());

        assets.AddRange(Module_Battle.BuildPetSimplePreloadAssets(modulePet.FightingPet, null, 2));

        //切换场景回到主城的时候，需要预加载未完成的资源
        if (moduleGuide.currentGuide != null) assets.AddRange(moduleGuide.GetPreLoadAssets(moduleGuide.currentGuide));

        assets.AddRange(FaceName.cachedAssets);
        return assets;
    }

    protected override bool WaitBeforeLoadComplete()
    {
        if (!session.connected || !moduleLogin.loggedIn) return true;  // Lost connection before load complete

        if (moduleLogin.ok)
        {
            CreatePlayer();
            return true;
        }

        return false;
    }

    protected override void OnLoadComplete()
    {
        if (!moduleLogin.ok) return;
        m_duringTheatryStory = false;

        Action applyFriend = () =>
        {
            //如果当前有新手引导，不在显示申请加好友界面
            if (!moduleGuide.isGuiding &&  moduleChase.LastComrade != null)
            {
                if (moduleChase.LastComrade.type == 4 ||
                    (moduleChase.LastComrade.type == 3 &&
                     !moduleFriend.IsFriend(moduleChase.LastComrade.playerInfo.roleId)))
                {
                    Window.SetWindowParam<Window_ApplyFriend>(moduleChase.LastComrade.playerInfo);
                    Window.ShowAsync<Window_ApplyFriend>();
                }
            }
        };

        if (Window.stack.Count > 0)
        {
            var top = Window.stack[0].windowName;
            if (moduleGlobal.targetMatrial.windowName == top)
            {
                Window.SkipBackTo(top, null, w =>
                {
                    moduleGlobal.targetMatrial.InvokeOnShow(w);

                    applyFriend.Invoke();
                });
            }
            else
                Window.SkipBackTo(top, null, w => applyFriend.Invoke());  // 如果窗口堆栈中有窗口记录，即还原了窗口堆栈，则打开堆栈中第一个窗口
        }
        else Window.SkipBackToImmediately<Window_Home>(null, w =>
        {
            moduleGuide.CheckReconnection();
            applyFriend.Invoke();
        });

        EventManager.AddEventListener(Events.VEDIO_SETTINGS_CHANGED, OnVideoSettingsChanged);
        moduleHome.AddEventListener(Module_Home.EventSetMasterPosition, OnSetPosition);
        moduleHome.AddEventListener(Module_Home.EventRevertMasterPosition, OnRevertPosition);
        moduleHome.AddEventListener(Module_Home.EventSetScene, OnSetScene);
        OnVideoSettingsChanged();
    }

    private void OnSetScene(Event_ e)
    {
        var sceneName = (string)e.param1;
        var visible = (bool) e.param2;
        if (string.IsNullOrEmpty(sceneName))
            return;
        var t = m_objects.transform.Find(sceneName);
        if (!t)
        {
            if (!visible)
            {
                if (sceneLoadCoroutine != null)
                {
                    StopCoroutine(sceneLoadCoroutine);
                    sceneLoadCoroutine = null;
                }
                return;
            }
            sceneLoadCoroutine = PrepareAsset<GameObject>(sceneName, go =>
            {
                if (sceneLoadCoroutine == null)
                    return;

                var obj = Object.Instantiate<GameObject>(go);
                obj.name = sceneName;
                obj.transform.SetParent(m_objects.transform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.SafeSetActive(true);
                Util.SetLayer(obj, Layers.SceneObject);
                sceneLoadCoroutine = null;
            });
        }
        else
            t.SafeSetActive(visible);

        if (visible)
        {
            mainCamera.cullingMask = mainCamera.cullingMask | 1 << Layers.SceneObject;
        }
    }

    protected override void OnClearWindowStack(List<Window.WindowHolder> stack)
    {
        if (nextLevel == GeneralConfigInfo.sroleLevel || nextLevel == 0) return;

        var cached = moduleHome.cachedWindowStack;

        cached.Clear();
        cached.AddRange(stack);  // 缓存所有当前堆栈

        stack.Clear();           // 清除被缓存的堆栈列表，避免堆栈记录被销毁
    }

    protected override void OnRestoreWindowStack(List<Window.WindowHolder> stack)
    {
        var cached = moduleHome.cachedWindowStack;

        if (moduleGlobal.targetMatrial.isProcess)
        {
            if (moduleGlobal.targetMatrial.isFinish)
            {
                var mn = moduleGlobal.targetMatrial.windowName;
                var idx = cached.FindIndex(w => w.windowName == mn);
                if (idx > 0) cached.RemoveRange(0, idx, true);
            }
            else
                moduleGlobal.targetMatrial.Clear();
        }

        if (moduleAwakeMatch.matchInfos != null)
            moduleHome.PushWindowStack(Game.GetDefaultName<Window_TeamMatch>());

        if (modulePVE.reopenPanelType == PVEReOpenPanel.RunePanel || modulePVE.reopenPanelType == PVEReOpenPanel.EquipPanel)
            moduleHome.PushWindowStack(modulePVE.GetReopenPanelName(modulePVE.reopenPanelType));

        modulePVE.reopenPanelType = PVEReOpenPanel.None;

        stack.AddRange(cached);  // 恢复被缓存的堆栈记录
        cached.Clear();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (master)
        {
            master.Destroy();
            master = null;
        }
        EventManager.RemoveEventListener(this);
        moduleHome.RemoveEventListener(this);
    }

    private void CreatePlayer()
    {
        if (master)
        {
            master.Destroy();
            master = null;
        }

        var oc = ConfigManager.Get<ShowCreatureInfo>(20000)?.GetDataByIndex(modulePlayer.proto) ?? null;
        if (oc != null && oc.data != null && oc.data.Length > 0)
        {
            startCameraOffset = oc.data[0].pos;
            ResetMainCamera();
        }

        PrepareAssets(Module_Battle.BuildPlayerSimplePreloadAssets(), r =>
        {
            if (!r) return;

            master = moduleHome.CreatePlayer();
            originMasterPos = master.position_;

            if (modulePVP.opType != OpenWhichPvP.None)
                moduleMatch.DispatchModuleEvent(Module_Match.EventComeBackHomeScence);
        });

        CreateFightingPet(modulePet.FightingPet);
    }

    private void OnRevertPosition()
    {
        if (master)
            master.position = master.position_ = originMasterPos;
        if (fightingPet)
            fightingPet.position_ = originPetPos;
        recordMasterPos = Vector3.zero;
    }

    private void OnSetPosition(Event_ e)
    {
        if (master)
        {
            Vector3_ offset = Vector3_.zero;
            if (fightingPet)
                offset = fightingPet.position - master.position;
            var t = (ShowCreatureInfo.SizeAndPos)e.param1;
            master.position_ = t.pos;
            master.position = t.pos;
            master.eulerAngles = t.rotation;
            master.transform.localScale = Vector3.one*t.size;
            if (fightingPet)
                fightingPet.position_ = master.position_ + offset;
        }
        else
        {
            recordMasterPos = ((ShowCreatureInfo.SizeAndPos)e.param1).pos;
        }

        rankCamera.cullingMask |= (Layers.MASK_MODEL | Layers.MASK_WEAPON | Layers.MASK_JEWELRY);
        pvpCamera.cullingMask |= (Layers.MASK_MODEL | Layers.MASK_WEAPON | Layers.MASK_JEWELRY);
    }

    private void OnVideoSettingsChanged()
    {
        SettingsManager.ApplyVideoSettings(petCamera);
        SettingsManager.ApplyVideoSettings(awakeCamera);
        SettingsManager.ApplyVideoSettings(npcAwakeCamera);
        SettingsManager.ApplyVideoSettings(pvpCamera);
        SettingsManager.ApplyVideoSettings(rankCamera);
    }

    protected override void OnInitialized()
    {
        moduleHome.AddEventListener(Module_Home.EventSwitchCameraMode,  OnSwitchCameraToSummon);
        moduleHome.AddEventListener(Module_Home.EventSummonSuccess,     OnSummonSuccess);
        moduleHome.AddEventListener(Module_Home.EventLoadNpc,           OnNpcLoad);
        moduleHome.AddEventListener(Module_Home.EventCloseSubWindow,    OnCloseSubWindow);
        moduleHome.AddEventListener(Module_Home.EventInterruptSummonEffect,    OnInterruptSummonEffect);

        #region Debug
        #if DEVELOPMENT_BUILD || UNITY_EDITOR
        EventManager.AddEventListener("OnPostGmLockClass", OnPostGmLockClass);
        #endif
        #endregion
    }

    private void OnInterruptSummonEffect()
    {
        if (summonEffectPlayCoroutine != null)
        {
            Root.instance.StopCoroutine(summonEffectPlayCoroutine);
            PlayEffect(summonEffect, false);
            eggAnimator.SetBool(AnimatorParam_Summon, false);
            modulePet.DispatchModuleEvent(Module_Pet.EventEggAnimEnd, true);

            moduleGlobal.UnLockUI();
        }
    }

    protected override void CreateEnvironments()
    {
        base.CreateEnvironments();

        petObjects = root.Find("objects/pet");
        petCamera = root.GetComponent<Camera>("cameras/petCamera");
        petCameraPos = petCamera.transform.position;
        petCameraRotation = petCamera.transform.rotation;
        cameraAnimator = petCamera.GetComponent<Animator>();
        eggRoot = petObjects.Find("eggs");
        summonEffect = root.Find("effects/effect");
            
        awakeCamera = root.GetComponent<Camera>("cameras/awakeCamera");
        awakeRoot = root.GetComponent<Transform>("objects/awake");

        npcAwakeCamera = root.GetComponent<Camera>("cameras/npcAwakeCamera");
        npcAwakeRoot = root.GetComponent<Transform>("objects/ControlRoot");

        _mainScence = root.GetComponent<Transform>("objects/screen");

        pvpCamera = root.GetComponent<Camera>("cameras/pvpCamera");
        pvpScence = root.GetComponent<Transform>("objects/pvp");

        rankCamera = root.GetComponent<Camera>("cameras/rankCamera");
        rankScence = root.GetComponent<Transform>("objects/rank");

        datingCamera = root.GetComponent<Camera>("cameras/datingCamera");
        datingScence = root.GetComponent<Transform>("objects/npcDating");
        datingCameraPos = datingCamera.transform.position;
        datingCameraRotation = datingCamera.transform.rotation;

        SwitchCameraByType(CameraShowType.Home);
    }
    
    private IEnumerator WaitForEggAnimator()
    {
        while (!eggAnimator.GetCurrentAnimatorStateInfo(0).IsName("Summon"))
            yield return 0;
        while (cameraAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
            yield return 0;

        PlayEffect(summonEffect);

        while (eggAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
            yield return 0;
        eggAnimator.SetBool(AnimatorParam_Summon, false);
        modulePet.DispatchModuleEvent(Module_Pet.EventEggAnimEnd);

        moduleGlobal.UnLockUI();

        summonEffectPlayCoroutine = null;
    }

    private void PlayEffect(Transform t, bool play = true)
    {
        if (!t) return;

        t.gameObject.SetActive(true);
        var ps = t.GetComponentsInChildren<ParticleSystem>();
        if (ps != null && ps.Length > 0)
        {
            foreach (var p in ps)
            {
                if (play)
                    p.Play();
                else
                    p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
        var animators = t.GetComponentsInChildren<Animator>();
        if (animators != null && animators.Length > 0)
        {
            foreach (var animator in animators)
            {
                if (play)
                    animator.Play(0);
                else
                    animator.enabled = false;
            }
        }
    }

    private void OnSummonSuccess(Event_ e)
    {
        //随机一个蛋
        var d = Random.Range(0, eggRoot.childCount);
        eggAnimator = eggRoot.GetChild(d).GetComponent<Animator>();
        if (eggAnimator)
        {
            eggAnimator.SetBool(AnimatorParam_Summon, true);
            eggAnimator.enabled = true;
            AudioManager.PlaySound(AudioInLogicInfo.audioConst.petSummon);
            summonEffectPlayCoroutine = Root.instance.StartCoroutine(WaitForEggAnimator());
        }
    }

    private void OnNpcLoad(Event_ e)
    {
        npc = (Creature)e.param1;
    }

    private void OnCloseSubWindow(Event_ e)
    {
        cameraAnimator.SetBool(AnimatorParam_Camera_Summon, false);
    }

    private void OnSwitchCameraToSummon(Event_ e)
    {
        var isSummon = (CameraShowType)e.param1;
        if (isSummon == CameraShowType.Rank && !rankCamera)
            isSummon = CameraShowType.Pvp;
        SwitchCameraByType(isSummon);
    }

    private void SwitchCameraByType(CameraShowType showType)
    {
        var isSummon = showType == CameraShowType.PetSummon;
        if (isSummon)
        {
            petCamera.transform.position = petCameraPos;
            petCamera.transform.rotation = petCameraRotation;
            modulePet.DispatchModuleEvent(Module_Pet.FarAwayNpc, false);
            if(npc)
                npc.stateMachine.TranslateTo("StateNpcSit");
        }
        petObjects.SafeSetActive(showType == CameraShowType.PetSummon);
        petCamera.SafeSetActive(showType == CameraShowType.PetSummon);

        //主界面摄像机的开启需要检查是不是触发了对话
        mainCamera.SafeSetActive(showType != CameraShowType.Awake && showType != CameraShowType.NpcAwake);
        mainCamera.enabled = showType == CameraShowType.Home && !m_duringTheatryStory;

        awakeCamera.SafeSetActive(showType == CameraShowType.Awake);
        awakeRoot.SafeSetActive(showType == CameraShowType.Awake);

        npcAwakeCamera.SafeSetActive(showType == CameraShowType.NpcAwake);
        npcAwakeRoot.SafeSetActive(showType == CameraShowType.NpcAwake);

        _mainScence.SafeSetActive(showType == CameraShowType.Home);

        pvpCamera.SafeSetActive(showType == CameraShowType.Pvp);
        pvpScence.SafeSetActive(showType == CameraShowType.Pvp);

        rankCamera.SafeSetActive(showType == CameraShowType.Rank);
        rankScence.SafeSetActive(showType == CameraShowType.Rank);

        var isDating = showType == CameraShowType.Dating;
        if (isDating)
        {
            datingCamera.transform.position = datingCameraPos;
            datingCamera.transform.rotation = datingCameraRotation;
        }
        datingCamera.SafeSetActive(showType == CameraShowType.Dating);
        datingScence.SafeSetActive(showType == CameraShowType.Dating);

        summonEffect.SafeSetActive(false);
        //effect.SafeSetActive(showType != CameraShowType.Awake);
    }

    public bool HasOtherCameraShow()
    {
        foreach (var item in m_cameras)
        {
            if (!item || item == mainCamera) continue;

            if (item.gameObject.activeInHierarchy && item.enabled) return true;
        }

        return false;
    }

    public void CreatePet(PetInfo rPet)
    {
        if (rPet == null) return;
        pet?.Destroy();
        pet = null;

        var rGradeInfo = rPet.UpGradeInfo;
        if (rGradeInfo == null) return;

        PrepareAssets(Module_Battle.BuildPetSimplePreloadAssets(rPet, null, 0), b =>
        {
            var show = ConfigManager.Get<ShowCreatureInfo>(rPet.ID);
            if (show == null)
            {
                Logger.LogError("没有配置config_showCreatureInfo表。宠物ID = {0}, 没有出生位置信息。宠物模型创建失败", rPet.ID);
                return;
            }

            var showData = show.GetDataByIndex(1);
            var data = showData.data.GetValue<ShowCreatureInfo.SizeAndPos>(0);

            pet = moduleHome.CreatePet(rGradeInfo, data.pos, data.rotation, startPos, false, Module_Home.PET_OBJECT_NAME);

            pet.eulerAngles = data.rotation;
            pet.localPosition = data.pos;
            pet.transform.localScale *= data.size;
        });
    }
    
    public void CreateFightingPet(PetInfo rPet)
    {
        var rGradeInfo = rPet?.UpGradeInfo;
        if (rGradeInfo == null) return;

        if (fightingPet && fightingPet.weaponID == rGradeInfo.UIstateMachine)
            return;

        PrepareAssets(Module_Battle.BuildPetSimplePreloadAssets(rPet, null, 2), b =>
        {
            var show = ConfigManager.Get<ShowCreatureInfo>(rPet.ID);
            if (show == null)
            {
                Logger.LogError("没有配置config_showCreatureInfo表。宠物ID = {0}, 没有出生位置信息。宠物模型创建失败", rPet.ID);
                return;
            }

            if (fightingPet)
            {
                fightingPet.Destroy();
                fightingPet = null;
            }

            var showData = show.GetDataByIndex(0);
            var data = showData.data.GetValue<ShowCreatureInfo.SizeAndPos>(0);

            fightingPet = moduleHome.CreatePet(rGradeInfo, data.pos, data.rotation, startPos, true, Module_Home.FIGHTING_PET_OBJECT_NAME);

            fightingPet.eulerAngles = data.rotation;
            fightingPet.position_ = fightingPet.localPosition = data.pos;
            fightingPet.transform.localScale *= data.size;
            originPetPos = fightingPet.position_;

            if (recordMasterPos != Vector3_.zero)
            {
                var offset = fightingPet.position_ - originMasterPos;
                fightingPet.position_ = (recordMasterPos + offset);
            }
            //切换主界面的宠物待机动作
            fightingPet.stateMachine.TranslateTo("StateIdle");
        });
    }

    public void PetTranslateTo(string fightAction)
    {
        pet?.stateMachine?.TranslateTo(fightAction);
    }

    public void DestroyFightPet()
    {
        if (fightingPet)
        {
            fightingPet.Destroy();
            fightingPet = null;
        }
    }

    public void ToggleFightPet(bool show, bool force)
    {
        if(!force)
            show &= master?.gameObject?.activeSelf ?? true;

        if(show)
            CreateFightingPet(modulePet.FightingPet);

        if (fightingPet)
        {
            fightingPet.gameObject.SafeSetActive(true);
            fightingPet.visible = show;
        }
    }

    private void _ME(ModuleEvent<Module_Story> e)
    {
        EnumStoryType type = (EnumStoryType)e.param2;
        switch (e.moduleEvent)
        {
            case Module_Story.EventStoryStart: m_duringTheatryStory = type == EnumStoryType.TheatreStory; break;
            case Module_Story.EventStoryEnd: m_duringTheatryStory = false; break;
        }
    }


    #region Debug helper

    #if DEVELOPMENT_BUILD || UNITY_EDITOR

    private void OnPostGmLockClass(Event_ e)
    {
        var mm = (bool)e.param1;
        if (!mm || !master) CreatePlayer();
        else
        {
            moduleGlobal.LockUI("拼命加载中...", 1.0f, 5.0f);
            PrepareAssets(Module_Battle.BuildPlayerSimplePreloadAssets(), f =>
            {
                moduleGlobal.UnLockUI();
                if (!f || !master) return;

                master.UpdateWeapon(moduleEquip.weaponID, moduleEquip.weaponItemID);
            });
        }
    }

    #endif

    #endregion
}
