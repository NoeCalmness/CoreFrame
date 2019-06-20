// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-02-23      10:13
//  *LastModify：2019-02-23      10:40
//  ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class Level_role : Level
{
    const int MaskRenderQueue = 3010;

    protected GameObject m_theatreMask;
    protected Material m_theatryMaskMat;

    private RoleEntry[] _slots;

    protected override List<string> BuildPreloadAssets()
    {
        var assets = base.BuildPreloadAssets();

        for (var i = 0; i < moduleSelectRole.roleList?.Length; i++)
            Module_Battle.BuildPlayerSimplePreloadAssets(moduleSelectRole.roleList[i], assets);
        assets.Add(StoryConst.THEATRE_MASK_ASSET_NAME);
        assets.Add(GeneralConfigInfo.defaultConfig.roleSelectEffect.effect);
        assets.Add("ui_selectcharacter");
        return assets;
    }

    protected override bool WaitBeforeLoadComplete()
    {
        if (!session.connected) return true;  // Lost connection before load complete

        for (var i = 0; i < moduleSelectRole.roleList?.Length; i++)
        {
            CreateRole(moduleSelectRole.roleList[i], _slots.GetValue<RoleEntry>(i));
        }

        for (int i = 0, iMax = Mathf.Min(moduleSelectRole.roleList?.Length ?? 0, _slots.Length); i < iMax; i++)
        {
            var entry = _slots.GetValue<RoleEntry>(i);
            if (entry == null || entry.creature == null) continue;
            entry.effect = entry.creature.behaviour.effects.PlayEffect(GeneralConfigInfo.defaultConfig.roleSelectEffect);
        }

        return true;
    }

    private void CreateRole(PRoleSummary rRole, RoleEntry rEntry)
    {
        if (rEntry?.root == null || rRole == null)
            return;
        rEntry.materialList.Clear();

        //没有时装数据的角色不创建模型。避免报错
        if (rRole.fashion.weapon == 0)
            return;
        Vector3 pos = Vector3.zero, rot = Vector3.zero;
        var info =ConfigManager.Get<ShowCreatureInfo>(20001+rEntry.index);
        var d = info?.GetDataByIndex(rRole.proto);
        if (d != null && d.data?.Length > 0)
        {
            pos = d.data[0].pos;
            rot = d.data[0].rotation;
        }
        var assets = new List<string>();
        Level.PrepareAssets(Module_Battle.BuildPlayerSimplePreloadAssets(rRole, assets), b =>
        {
            rEntry.creature = Creature.Create(modulePlayer.BuildPlayerInfo(rRole), rEntry.root.position + pos, rot, true, rRole.name, rRole.name, false);
            rEntry.creature.transform.SetParent(rEntry.root);
            rEntry.creature.localRotation = Quaternion.Euler(rot);
            rEntry.creature.roleId = rRole.roleId;

            CharacterEquip.ChangeCloth(rEntry.creature, rRole.fashion);

            var renderers = rEntry.creature.gameObject.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
                rEntry.materialList.AddRange(r.materials);

            //暂时不显示宠物了
            /*if (rRole.pet == null || rRole.pet.itemId == 0)
                return;

            var rPet = PetInfo.Create(rRole.pet);
            var rGradeInfo = rPet.UpGradeInfo;
            var show = ConfigManager.Get<ShowCreatureInfo>(rPet.ID);
            if (show == null)
            {
                Logger.LogError("没有配置config_showCreatureInfo表。宠物ID = {0}, 没有出生位置信息。宠物模型创建失败", rPet.ID);
                return;
            }
            var showData = show.GetDataByIndex(0);
            var data = showData.data.GetValue<ShowCreatureInfo.SizeAndPos>(0);
            rEntry.pet = moduleHome.CreatePet(rGradeInfo, data.pos + rEntry.creature.position, data.rotation, rEntry.root, true,
                Module_Home.TEAM_PET_OBJECT_NAME);
            rEntry.pet.transform.localScale *= data.size;
            rEntry.pet.transform.localEulerAngles = data.rotation;

            renderers = rEntry.pet.gameObject.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
                rEntry.materialList.AddRange(r.materials);*/
        });
    }

    protected override void OnLoadComplete()
    {
        CreateMask();

        moduleSelectRole.AddEventListener(Module_SelectRole.ChangeSelectRoleEvent, OnSelectRoleChange);
        moduleSelectRole.AddEventListener(Module_SelectRole.RoleListChangeEvent, OnRoleListChange);

        Window.ShowImmediatelyAsync<Window_SelectRole>();
    }

    private void OnRoleListChange()
    {
        for (var i = 0; i < _slots.Length; i++)
        {
            if (i >= moduleSelectRole.RoleCount)
            {
                _slots[i].Destory();
            }
            else
            {
                if (_slots[i].creature?.roleId != moduleSelectRole.roleList[i].roleId)
                {
                    _slots[i].Destory();
                    var entry = _slots.GetValue<RoleEntry>(i);
                    CreateRole(moduleSelectRole.roleList[i], entry);
                }
            }
        }
    }

    private void OnSelectRoleChange(Event_ e)
    {
        var roleId = 0UL;
        if (e.param1 != null)
            roleId = (ulong) e.param1;
        for (var i = 0; i < _slots.Length; i++)
        {
            var c = _slots[i].creature;
            if (c == null) continue;
            var ms = _slots[i].materialList;
            if (ms == null) continue;
            for (var m = 0; m < ms.Count; m++)
            {
                if(ms[m] != null)
                    ms[m].renderQueue = MaskRenderQueue + (roleId == c.roleId ? +1 : - 1);
            }

            if (roleId == c.roleId)
            {
                if (!_slots[i].effect)
                {
                    _slots[i].effect = _slots[i].creature.behaviour.effects.PlayEffect(GeneralConfigInfo.defaultConfig.roleSelectEffect);
                    _slots[i].effect.visible = true;
                }
                else
                    _slots[i].effect.visible = true;
            }
            else if (_slots[i].effect != null)
                _slots[i].effect.visible = false;
        }
    }


    private void CreateMask()
    {
        var t = mainCamera.transform.Find(StoryConst.THEATRE_MASK_ASSET_NAME);
        if (t) m_theatreMask = t.gameObject;
        else
        {
            m_theatreMask = Level.GetPreloadObject<GameObject>(StoryConst.THEATRE_MASK_ASSET_NAME, true);
            m_theatreMask.transform.SetParent(mainCamera.transform);
        }

        m_theatreMask.transform.localPosition = new Vector3(0, 0, 2);
        m_theatreMask.transform.localRotation = Quaternion.identity;
        m_theatreMask.transform.localScale = new Vector3(30, 30, 30);

        var mr = m_theatreMask.GetComponent<MeshRenderer>();
        if (mr)
        {
            m_theatryMaskMat = mr.material;
            m_theatryMaskMat.renderQueue = MaskRenderQueue;
        }
    }

    protected override void CreateEnvironments()
    {
        base.CreateEnvironments();

        var arr = startPos.GetChildList();
        _slots = new RoleEntry[arr?.Count ?? 0];
        for (var i = 0; i < arr?.Count; i++)
            _slots[i] = new RoleEntry {root = arr[i], index = i};

        var go = mainCamera.transform.Find("bg");
        if (go)
        {
            var r = go.GetComponent<MeshRenderer>();
            if (r) r.material.renderQueue = MaskRenderQueue + 1;
        }
    }

    private class RoleEntry
    {
        public int index;
        public Transform root;
        public Creature creature;
        public Creature pet;
        public Effect effect;
        public List<Material> materialList = new List<Material>();

        public void Destory()
        {
            creature?.Destroy();
            pet?.Destroy();
            effect?.Destroy();
            creature = null;
            pet = null;
            effect = null;
        }

        public void DestroyEffect()
        {
            effect?.Destroy();
            effect = null;
        }
    }
}