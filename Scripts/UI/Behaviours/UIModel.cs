// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-10-11      18:13
//  * LastModify：2018-10-12      10:25
//  ***************************************************************************************************/

#region

using System.Collections.Generic;
using UnityEngine;

#endregion

[RequireComponent(typeof(UICharacter))]
public class UIModel : MonoBehaviour
{
    private static int id;

    private readonly HashSet<GameObject> objectModels = new HashSet<GameObject>();
    private UICharacter character;

    private Camera itemRenderCamera;
    private Camera mainCamera;
    private RenderTexture renderTex;
    private int cullingMask;
    public int ID;

    private UICharacter CharacterAssert
    {
        get { return character ?? (character = GetComponent<UICharacter>()); }
    }

    private Vector3 RootPosition { get { return Vector3.up*ID*10; } }

    private void Start()
    {
        ID = ++id;
        InitCamera();
    }

    private void InitCamera()
    {
        if (itemRenderCamera) return;

        mainCamera = Camera.main;
        if (!mainCamera) return;
        var t = mainCamera.transform.parent.AddNewChild(mainCamera.gameObject);
        var cs = t.GetComponentsInChildren<Component>();
        foreach (var item in cs)
        {
            if (item is Camera || item is Transform) continue;
            DestroyImmediate(item);
        }
        t.name = "item_camera" + ID;
        t.position = mainCamera.transform.position + RootPosition;
        t.rotation = mainCamera.transform.rotation;
        t.localScale = mainCamera.transform.localScale;

        itemRenderCamera = t.GetComponent<Camera>();
        itemRenderCamera.cullingMask = cullingMask;

        itemRenderCamera.gameObject.SetActive(mainCamera.targetTexture != null);
        if (mainCamera.targetTexture != null)
        {
            var mrt = mainCamera.targetTexture;
            renderTex = RenderTexture.GetTemporary(mrt.width, mrt.height, (int)mrt.depthBuffer.GetNativeRenderBufferPtr(), mrt.format);
            itemRenderCamera.targetTexture = renderTex;
            CharacterAssert.Initialize();
            CharacterAssert.cameraName = itemRenderCamera.name;
        }
    }

    private void OnEnable()
    {
        itemRenderCamera?.SafeSetActive(true);
    }

    private void OnDisable()
    {
        itemRenderCamera.SafeSetActive(false);
        foreach (var go in objectModels)
        {
            if(go) Destroy(go);
        }
        objectModels.Clear();
    }

    private void OnDestroy()
    {
        RenderTexture.ReleaseTemporary(renderTex);
        Destroy(itemRenderCamera.gameObject);
    }

    private void HideOtherModels()
    {
        foreach (var go in objectModels)
            if (go) go.SafeSetActive(false);
    }

    #region load weapon 

    public void LoadItemModel(PItem item, int layer = Layers.WEAPON)
    {
        cullingMask = 1 << layer;
        if (null != itemRenderCamera)
            itemRenderCamera.cullingMask = cullingMask;
        HideOtherModels();

        if (item == null || !(Level.current is Level_Home)) return;

        var level = Level.current;
        var models = GetItemModels(item);

        Module_Global.instance?.LockUI("", 0.5f);
        Level.PrepareAssets(models, r =>
        {
            if (!r)
            {
                Module_Global.instance?.UnLockUI();
                return;
            }

            //when loaded complete ,must reget models
            models = GetItemModels(item);
            var data = GetShowInfoData(item);
            var equipType = Module_Equip.GetEquipTypeByItem(item);

            for (var i = 0; i < models.Count; i++)
            {
                var go = level.startPos.Find(models[i])?.gameObject;
                if (!go)
                {
                    go = Level.GetPreloadObject<GameObject>(models[i]);
                    if (go == null)
                        Logger.LogError("equip with propId {1} and asset name {0} cannot be loaded,please check config", models[i],
                            item.itemTypeId);
                    else
                    {
                        go.transform.SetParent(level.startPos.transform);
                        go.transform.localPosition = RootPosition;
                    }
                    objectModels.Add(go);
                }
                SetWeaponInfo(go, data.GetValue<ShowCreatureInfo.SizeAndPos>(i), layer, equipType);
            }

            Module_Global.instance?.UnLockUI();
        });
    }

    private List<string> GetItemModels(PItem item)
    {
        var l = new List<string>();
        if (item == null) return l;

        var type = Module_Equip.GetEquipTypeByItem(item);
        var info = item.GetPropItem();
        if (type == EquipType.Cloth)
        {
            var model = info?.previewModel;
            if (!string.IsNullOrEmpty(model)) l.Add(model);
        }
        else if (type == EquipType.Gun || type == EquipType.Weapon)
        {
            var weapon = WeaponInfo.GetWeapon(info.subType, info.ID);
            weapon.GetAllAssets(l);
        }
        return l;
    }

    private ShowCreatureInfo.SizeAndPos[] GetShowInfoData(PItem item)
    {
        var info = item.GetPropItem();
        if (!info) return null;

        var type = Module_Equip.GetEquipTypeByItem(item);
        var showId = type == EquipType.Cloth ? 300 : info.subType + 100;

        var showInfo = ConfigManager.Get<ShowCreatureInfo>(showId);
        if (showInfo == null)
        {
            Logger.LogError("can not find config showCreatureInfo, please check config [{0}]", showId);
            return null;
        }

        var showData = showInfo.GetDataByIndex(info.ID);
        if (showData == null)
        {
            showData = showInfo.forData.Length > 0 ? showInfo.forData[0] : null;
            Logger.LogWarning("cause ShowCreatureInfo.Id = [{0}] with itemTypeId = [{1}] cannot be finded,we use itemTypeId = [{2}] to instead!", showId, info.ID, showData == null ? "null" : showData.index.ToString());
        }
        return showData?.data;
    }

    private void SetWeaponInfo(GameObject weaponInstance, ShowCreatureInfo.SizeAndPos data, int layer, EquipType type = EquipType.Weapon)
    {
        if (weaponInstance == null || data == null) return;

        weaponInstance.transform.SetParent(Level.current.startPos);
        weaponInstance.SetActive(true);
        //为了避免跟人物用相同的
        Util.SetLayer(weaponInstance, layer);
        weaponInstance.transform.localPosition = data.pos;
        weaponInstance.transform.localEulerAngles = data.rotation;
        var c = weaponInstance.GetComponentDefault<WeaponRotation>();
        c.type = type;
    }

    #endregion
}
