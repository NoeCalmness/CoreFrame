using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class CharacterEquip : MonoBehaviour
{
    public class BindNodeInfo
    {
        public static readonly string[][] nodeNames =
        {
            new string[]{ "" },
            new string[]{ "body" },
            new string[]{ "leg" },
            new string[]{ "hand" },
            new string[]{ "foot" },
            new string[]{ "hair_f_point", "hair_m_point" },
            new string[]{ "shipin_guadian" },
            new string[]{ "" },
            new string[]{ "" },
            new string[]{ "" },
            new string[]{ "" },
            new string[]{ "" },
            new string[]{ "shipin_guadian" },
            new string[]{ "shipin_guadian" },
            new string[]{ "shipin_guadian" }
        };
        //仍旧使用之前的节点 判断是否存在同类型的物品 如果存在隐藏同类型

        /// <summary>
        /// 根据性别获取对应的绑定点
        /// </summary>
        /// <param name="type">对应 FashionSubType</param>
        /// <param name="gender">0 = 女性 其它= 男性</param>
        /// <returns></returns>
        public static string GetNodeName(int type, int gender)
        {
            if (type < 0 || type >= nodeNames.Length) return string.Empty;
            var n = nodeNames[type];
            if (gender != 0) gender = 1;
            return n.Length == 1 ? n[0] : n[gender];
        }

        public FashionSubType type { get; private set; }
        public int i { get; private set; }
        public SkinnedMeshRenderer renderer { get; private set; }
        public Transform node { get; private set; }
        public PropItemInfo current { get { return m_current; } }
        private PropItemInfo m_current;
        public int currentItem { get { return m_current ? m_current.ID : 0; } }
        public bool isSet { get; private set; }
        public bool isMesh { get; private set; }
        public bool isSetSlot { get; private set; }
        public bool valid { get { return isMesh ? renderer : (bool)node; } }

        private Transform m_root;
        private CharacterEquip m_e;

        public BindNodeInfo(int _type, Transform root, CharacterEquip e)
        {
            type = (FashionSubType)_type;
            i = (int)type;

            isMesh = type != FashionSubType.Hair && type != FashionSubType.HeadDress && type != FashionSubType.HairDress && type != FashionSubType.FaceDress && type != FashionSubType.NeckDress;
            isSetSlot = type == FashionSubType.TwoPieceSuit || type == FashionSubType.FourPieceSuit;
            m_root = root;
            m_e = e;
        }
        
        public void UpdateRenderer(int gender)
        {
            renderer = null;
            node = null;

            if (!m_root) return;

            node = Util.FindChild(m_root, GetNodeName(i, gender));
            if (!node) node = Util.FindChild(m_root, GetNodeName(i, gender == 0 ? 1 : 0));
            renderer = node?.GetComponent<SkinnedMeshRenderer>();

        }

        public void UpdateCurrent(PropItemInfo value)
        {
            if (_UpdateCurrent(value) && value && m_e)
                m_e.OnChanged(this);
        }

        private bool _UpdateCurrent(PropItemInfo value)
        {
            if (m_current == value) return true;

            var rr = !m_current && value || m_current && value && m_current.sex != value.sex;
            m_current = value;
            isSet = m_current && (m_current.subType == (int)FashionSubType.TwoPieceSuit || m_current.subType == (int)FashionSubType.FourPieceSuit);

            if (rr) UpdateRenderer(value.sex);

            if (!node) return true;

            //隐藏所有子节点
            if (!m_current || m_current.mesh.Length < 1)
            {
                if (type == FashionSubType.Hair) Util.DisableAllChildren(node);
                else if (type == FashionSubType.HeadDress || type == FashionSubType.HairDress || type == FashionSubType.FaceDress || type == FashionSubType.NeckDress)
                    SetDisableChildren(type);
                return true;
            }

            var iid = m_current.ID;
            var asset = isSet ? i > current.mesh.Length ? "" : m_current.mesh[i - 1] : m_current.mesh[0];
            if (string.IsNullOrEmpty(asset)) return true;

            Level.PrepareAsset<GameObject>(asset, t =>
            {
                if (t && iid == currentItem && valid) _UpdateCurrent(t, Module_Cangku.instance?.FashionShow((FashionSubType)m_current?.subType));
                if (m_e) m_e.OnChanged(this);
            });

            return false;
        }

        private void SetDisableChildren(FashionSubType type)
        {
            var list = Module_Cangku.instance.FashionShow(type);
            if (list == null) return;
            for (int i = 0; i < node.childCount; i++)
            {
                var c = node.GetChild(i);
                if (list.Contains(c.name)) c.SafeSetActive(false);
            }
        }

        private void _UpdateCurrent(GameObject t,List<string> name)
        {
            if (type == FashionSubType.Hair || type == FashionSubType.HeadDress || type == FashionSubType.HairDress || type == FashionSubType.FaceDress || type == FashionSubType.NeckDress)
            {
                GameObject o = null;
                for (int j = 0, count = node.childCount; j < count; j++)
                {
                    var c = node.GetChild(j);
                    if (type == FashionSubType.Hair) c.SafeSetActive(false);
                    else if (name != null)
                    {
                        if(name .Contains (c.name)) c.SafeSetActive(false);
                    }
                    if (c.name == t.name) o = c.gameObject;
                }

                if (!o) o = node.AddNewChild(t).gameObject;
                o.SetActive(true);
                o.name = t.name;

                Util.SetLayer(o, type == FashionSubType.Hair ? Layers.MODEL : Layers.JEWELRY);
            }
            else
            {
                var mr = t?.GetComponentInChildren<SkinnedMeshRenderer>();
                if (!mr) return;

                var obs = mr.bones ?? new Transform[] { };
                var nbs = new List<Transform>();

                foreach (var ob in obs)
                {
                    var nb = ob ? Util.FindChild(m_root, ob.name) : null;
                    nbs.Add(nb);
                }

                renderer.sharedMaterial = mr.sharedMaterial;
                renderer.sharedMesh = mr.sharedMesh;

                if (nbs.Count > 0) renderer.bones = nbs.ToArray();
            }

            var b = m_root.GetComponent<CreatureBehaviour>();
            if (b && b.creature) b.creature.UpdateInverter(true);
            else
            {
                var iv = m_root.GetComponentInChildren<InvertHelper>();
                iv?.Refresh(true);
            }
        }
    }

    public BindNodeInfo[] bindNodes { get; private set; }
    private bool m_isInitSkinMesh = false;

    private void Awake()
    {
        InitDressSkinMesh();
        InitializedNpc();
    }

    private void OnDestroy()
    {
        bindNodes = null;
    }

    #region 对外静态接口

    /// <summary>
    /// 仅处理更换时装
    /// </summary>
    /// <param name="c"></param>
    /// <param name="fashion"></param>
    /// <param name="isCombine">如果合并网格，则需要重新加载动画控制器</param>
    public static void ChangeCloth(Creature c, PFashion fashion, bool isCombine = false)
    {
        if (fashion == null) return;

        var e = c.activeRootNode.GetComponentDefault<CharacterEquip>();
        c.AddEventListener(CreatureEvents.RESET_LAYERS, e.OnCreatureResetLayer);
        e.RefreshCloth(fashion);
        if (isCombine) e.CombineMesh();
    }

    /// <summary>
    /// 仅处理更换时装
    /// </summary>
    /// <param name="c"></param>
    /// <param name="pitems"></param>
    /// <param name="isCombine">如果合并网格，则需要重新加载动画控制器</param>
    public static void ChangeCloth(Creature c, List<PItem> pitems, bool isCombine = false)
    {
        var e = c.activeRootNode.GetComponentDefault<CharacterEquip>();
        c.AddEventListener(CreatureEvents.RESET_LAYERS, e.OnCreatureResetLayer);
        e.RefreshCloth(pitems);
        if (isCombine) e.CombineMesh();
    }
    
    public static void ChangeCloth(Creature c, int[] items, bool isCombine = false)
    {
        var e = c.activeRootNode.GetComponentDefault<CharacterEquip>();
        c.AddEventListener(CreatureEvents.RESET_LAYERS, e.OnCreatureResetLayer);
        e.RefreshCloth(items);
        if (isCombine) e.CombineMesh();
    }

    public static List<string> GetEquipAssets(List<PItem> items)
    {
        List<string> list = new List<string>();
        for (int i = 0; i < items.Count; i++)
        {
            PropItemInfo info = items[i]?.GetPropItem();
            if (info == null)
                continue;

            switch (info.itemType)
            {
                //武器和枪械的都去weapon表里面获取
                case PropType.Weapon:
                    var ws = WeaponInfo.GetSingleWeapons(info.subType, info.ID);
                    foreach (var w in ws)
                    {
                        if (!string.IsNullOrEmpty(w.model)) list.Add(w.model);
                        if (!string.IsNullOrEmpty(w.effects))
                        {
                            var es = Util.ParseString<string>(w.effects);
                            list.AddRange(es);
                        }
                    }
                    break;
                case PropType.FashionCloth:
                    for (int j = 0; j < info.mesh.Length; j++)
                    {
                        list.Add(info.mesh[j].ToLower());
                    }
                    break;
            }
        }
        return list;
    }

    public static List<string> GetEquipAssets(PFashion fashionData)
    {
        if (fashionData == null) return new List<string>();

        var fashions = GetValidFashionList(fashionData);
        var list = new List<string>();
        foreach (var i in fashions)
        {
            if (!i) continue;

            switch (i.itemType)
            {
                //武器和枪械的都去weapon表里面获取
                case PropType.Weapon:
                    var ws = WeaponInfo.GetSingleWeapons(i.subType, i.ID);
                    foreach (var w in ws)
                    {
                        if (!string.IsNullOrEmpty(w.model)) list.Add(w.model);
                        if (!string.IsNullOrEmpty(w.effects))
                        {
                            var es = Util.ParseString<string>(w.effects);
                            list.AddRange(es);
                        }
                    }
                    break;
                case PropType.FashionCloth:
                    for (int j = 0; j < i.mesh.Length; j++)
                        list.Add(i.mesh[j]);
                    break;
            }
        }
        return list;
    }

    public static List<string> GetAnimatorAssets(PFashion fashion)
    {
        List<string> list = new List<string>();
        PropItemInfo info = ConfigManager.Get<PropItemInfo>(fashion.weapon);
        if (info == null)
        {
            Logger.LogError("propid = {0} cannot be found,please check out", fashion.weapon);
            return list;
        }

        for (int j = 0; j < 2; j++)
        {
            list.Add(Util.Format("animator_weapon_{0}_{1}", info.subType, j));
        }
        return list;
    }

    public static List<PropItemInfo> GetValidFashionList(PFashion fashion)
    {
        var items = GetValidFashionClothList(fashion);

        if (fashion.weapon > 0) items.Add(ConfigManager.Get<PropItemInfo>(fashion.weapon));
        if (fashion.gun > 0) items.Add(ConfigManager.Get<PropItemInfo>(fashion.gun));

        return items;
    }

    public static List<PropItemInfo> GetValidFashionClothList(PFashion fashion)
    {
        var items = new List<PropItemInfo>();

        if (fashion.hair > 0) items.Add(ConfigManager.Get<PropItemInfo>(fashion.hair));
        if (fashion.clothes > 0) items.Add(ConfigManager.Get<PropItemInfo>(fashion.clothes));
        if (fashion.trousers > 0) items.Add(ConfigManager.Get<PropItemInfo>(fashion.trousers));
        if (fashion.glove > 0) items.Add(ConfigManager.Get<PropItemInfo>(fashion.glove));
        if (fashion.shoes > 0) items.Add(ConfigManager.Get<PropItemInfo>(fashion.shoes));
        if (fashion.headdress > 0) items.Add(ConfigManager.Get<PropItemInfo>(fashion.headdress));
        if (fashion.guiseId > 0) items.Add(ConfigManager.Get<PropItemInfo>(fashion.guiseId));
        if (fashion.hairdress > 0) items.Add(ConfigManager.Get<PropItemInfo>(fashion.hairdress));
        if (fashion.facedress > 0) items.Add(ConfigManager.Get<PropItemInfo>(fashion.facedress));
        if (fashion.neckdress > 0) items.Add(ConfigManager.Get<PropItemInfo>(fashion.neckdress));

        return items;
    }
    #endregion

    #region 更换服装

    /// <summary>
    /// 当某件装备更换完成时调用，int 参数表明还有几件剩余装备再更换中，为 0 时表示全部更换完成
    /// </summary>
    public System.Action<CharacterEquip, BindNodeInfo, int> onEquipmentChanged;

    private int m_changing = 0;

    private void OnChanged(BindNodeInfo node)
    {
        if (--m_changing < 0) m_changing = 0;
        onEquipmentChanged?.Invoke(this, node, m_changing);
    }

    /// <summary>
    /// 初始化服装挂载点
    /// </summary>
    private void InitDressSkinMesh()
    {
        if (m_isInitSkinMesh) return;
        m_isInitSkinMesh = true;

        bindNodes = new BindNodeInfo[(int)FashionSubType.Count];
        for (var i = 0; i < bindNodes.Length; ++i)
        {
            bindNodes[i] = new BindNodeInfo(i, transform, this);
        }
    }

    public BindNodeInfo GetNode(FashionSubType type)
    {
        var idx = (int)type;
        return bindNodes == null || idx < 0 || idx >= bindNodes.Length ? null : bindNodes[idx];
    }

    public void RefreshCloth(int[] items, bool forceUpdate = false)
    {
        #region Editor helper, support equipment preview in editor mode
        #if UNITY_EDITOR
        if (!Game.started)  // make sure all equipment base config data loaded
        {
            ConfigManager.EnsureLoad("config_configtexts");
            ConfigManager.EnsureLoad("config_propiteminfos");
        }
        #endif
        #endregion

        var iis = new List<PropItemInfo>();
        foreach (var i in items) iis.Add(ConfigManager.Get<PropItemInfo>(i));
        _RefreshCloth(iis, forceUpdate);
    }

    public void RefreshCloth(PFashion fashion)
    {
        _RefreshCloth(GetValidFashionClothList(fashion));
    }

    public List<int> GetCurrent()
    {
        var iis = new List<int>();
        if (bindNodes == null) return iis;
        foreach (var bn in bindNodes)
        {
            if (!bn.isSetSlot && bn.isSet || !bn.current) continue; // 忽略套装的单项位置
            iis.Add(bn.currentItem);
        }
        return iis;
    }

    /// <summary>
    /// 对外接口，刷新外观模型(时装部分)
    /// 服装只需要刷新即可，不需要脱下
    /// </summary>
    /// <param name="items">当前穿戴信息</param>
    public void RefreshCloth(List<PItem> items, bool forceUpdate = false)
    {
        var iis = new List<PropItemInfo>();  
        foreach (var i in items)
        {
            var item = i?.GetPropItem();
            if (item) iis.Add(item);
            else
                Logger.LogError("CharacterEquip::_RefreshCloth: Player have invalid equipment item info, could not find item [{0}, {1}, {2}] from config file.", i.GetPropItem().itemName, i.GetPropItem().itemType, i.itemTypeId);
        }

        _RefreshCloth(iis, forceUpdate);
    }
    
    private void _RefreshCloth(List<PropItemInfo> items, bool forceUpdate = false)
    {
        #if DEVELOPMENT_BUILD || UNITY_EDITOR
        var logmsg = "Current dress: ["; foreach (var i in items) if (i && i.itemType == PropType.FashionCloth) logmsg += (i ? i.ID + ":" + i.itemName : "null") + ", ";
        Logger.LogInfo(logmsg.TrimEnd(' ', ',') + "]");
        #endif

        InitDressSkinMesh();

        if (!forceUpdate && IsSameCloth(items))
        {
            Logger.LogDetail("Current equipment list and new list is the same, ignore.");
            return;
        }

        m_changing = 0;
        for (var i = 0; i < items.Count;)
        {
            var e = items[i];
            if (!e || e.itemType != PropType.FashionCloth || e.subType < 0 || e.subType >= bindNodes.Length) items.RemoveAt(i);   // Remove invalid items
            else
            {
                var et = (FashionSubType)e.subType;
                m_changing += et == FashionSubType.TwoPieceSuit ? 3 : (et == FashionSubType.FourPieceSuit || et == FashionSubType.ClothGuise) ? 5 : 1;
                ++i;
            }
        }

        foreach (var bn in bindNodes)
        {
            var t = bn.i;
            var i = items.Find(e => e.subType == t);
            if (i) _SetCloth(i);
            else bn.UpdateCurrent(null);
        }
    }

    private void _SetCloth(PropItemInfo item)
    {
        var t = item.subType;
        bindNodes[t].UpdateCurrent(item);

        if (t == (int)FashionSubType.TwoPieceSuit || t == (int)FashionSubType.FourPieceSuit || t == (int)FashionSubType.ClothGuise) // 对于套装，将套装直接应用到套装对应的栏位上，由 node 自身选择对应的 mesh 资源
        {
            bindNodes[1].UpdateCurrent(item);
            bindNodes[2].UpdateCurrent(item);
            if (t == (int)FashionSubType.FourPieceSuit || t == (int)FashionSubType.ClothGuise)
            {
                bindNodes[3].UpdateCurrent(item);
                bindNodes[4].UpdateCurrent(item);
            }
        }
    }

    /// <summary>
    /// 当前穿戴的装备是否和指定列表相同
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    private bool IsSameCloth(List<PropItemInfo> items)
    {
        if (bindNodes == null)
        {
            m_isInitSkinMesh = false;
            InitDressSkinMesh();
        }
        foreach (var bn in bindNodes)
        {
            if (!bn.isSetSlot && bn.isSet) continue; // 忽略套装的单项位置，套装将在套装栏位检测     
            var t = bn.i;
            var n = items.Find(i => i && i.subType == t);    // 寻找当前位置对应的装备
            if (bn.current ^ n || n && n.ID != bn.currentItem) return false; // 其中一个有但另一个没有 || 都有但 ID 不同
        }
        
        return true;
    }

    public void OnCreatureResetLayer()
    {
        if (bindNodes == null) return;

        foreach (var item in bindNodes)
        {
            if (item == null) continue;

            if ((item.type == FashionSubType.HeadDress || item.type == FashionSubType.HairDress || item.type == FashionSubType.FaceDress || item.type == FashionSubType.NeckDress) && item.node)
            {
                Util.SetLayer(item.node, Layers.JEWELRY);
                break;
            }
        }
    }

    #endregion

    #region  网格和材质球合并

    public const string COMBINE_DIFFUSE_TEXTURE = "_Texture";
    public const int COMBINE_TEXTURE_MAX = 2048;

    public readonly static string[] COMBINE_SELFSHADER_TEXTURES =new string[] { "_MainTex", "_SSSTex", "_ILMTex" };

    #region 普通网格合并(条件：一个网格只对应一个材质球)

    [ContextMenu("CombineMesh")]
    /// <summary>
    /// 网格合并方法，合并之后必须重新加载runtime动画控制器！
    /// </summary>
    public void CombineMesh()
    {
        SkinnedMeshRenderer[] smrs = GetComponentsInChildren<SkinnedMeshRenderer>();
        if (smrs == null || smrs.Length == 0)
            return;
        
        Stopwatch watch = Stopwatch.StartNew();

        List<CombineInstance> combine = new List<CombineInstance>();

        List<Material> materials = new List<Material>();
        List<Texture2D> textures = new List<Texture2D>();

        Transform combineTrans = transform.Find("combine");
        if(combineTrans == null)
        {
            GameObject obj = new GameObject("combine");
            obj.layer = smrs[0].gameObject.layer;
            combineTrans = obj.transform;
            combineTrans.parent = transform;
            combineTrans.localRotation = Quaternion.identity;
            combineTrans.localScale = Vector3.one;
            combineTrans.localPosition = Vector3.zero;
        }

        SkinnedMeshRenderer smrCombine = combineTrans.gameObject.GetComponentDefault<SkinnedMeshRenderer>();
        smrCombine.shadowCastingMode = smrs[0].shadowCastingMode;
        smrCombine.receiveShadows = smrs[0].receiveShadows;
        Logger.LogDetail("准备合并网格脚本耗时:{0}", watch.ElapsedMilliseconds);
        watch.Reset();
        watch.Start();

        //combine the texture
        for (int i = 0; i < smrs.Length; i++)
        {
            Material[] shares = smrs[i].sharedMaterials;
            for (int j = 0; j < shares.Length; j++)
            {
                materials.Add(shares[j]);

                Texture2D tx = shares[j].GetTexture(COMBINE_DIFFUSE_TEXTURE) as Texture2D;
                Texture2D tx2D = new Texture2D(tx.width, tx.height, TextureFormat.ARGB32, false);
                tx2D.SetPixels(tx.GetPixels(0, 0, tx.width, tx.height));
                tx2D.Apply();
                textures.Add(tx2D);
            }
        }
        Logger.LogDetail("合并贴图耗时:{0}", watch.ElapsedMilliseconds);
        watch.Reset();
        watch.Start();

        Material materialNew = new Material(materials[0].shader);
        materialNew.CopyPropertiesFromMaterial(materials[0]);

        Texture2D texture = new Texture2D(COMBINE_TEXTURE_MAX, COMBINE_TEXTURE_MAX);
        Rect[] rects = texture.PackTextures(textures.ToArray(), 0, COMBINE_TEXTURE_MAX);
        materialNew.SetTexture(COMBINE_DIFFUSE_TEXTURE, texture);
        Logger.LogDetail("合并材质球耗时:{0}", watch.ElapsedMilliseconds);
        watch.Reset();
        watch.Start();

        Transform rootBone = null;
        List<Transform> boneTmp = new List<Transform>();

        int rectIndex = 0;

        //collect the mesh
        for (int i = 0; i < smrs.Length; i++)
        {
            SkinnedMeshRenderer smr = smrs[i];
            if (smr.transform == transform)
            {
                continue;
            }

            if(rootBone == null)
            {
                rootBone = smr.rootBone;
            }

            for (int k = 0; k < smr.sharedMesh.subMeshCount; k++)
            {
                Rect rect = rects[rectIndex];

                Mesh meshCombine = CreatMeshWithMesh(smr.sharedMesh);
                Vector2[] uvs = new Vector2[meshCombine.uv.Length];
                for (int j = 0; j < uvs.Length; j++)
                {
                    uvs[j].x = rect.x + meshCombine.uv[j].x * rect.width;
                    uvs[j].y = rect.y + meshCombine.uv[j].y * rect.height;
                }
                meshCombine.uv = uvs;

                CombineInstance ci = new CombineInstance();
                ci.mesh = meshCombine;
                ci.subMeshIndex = k;
                ci.transform = smrs[i].transform.localToWorldMatrix;
                combine.Add(ci);
                rectIndex ++;
            }

            //bone data just add one times ignore how many subMesh exist
            boneTmp.AddRange(smr.bones);
            //GameObject.Destroy(smrs[i].gameObject);
            smrs[i].gameObject.SetActive(false);
        }
        Logger.LogDetail("收集网格信息耗时:{0}", watch.ElapsedMilliseconds);
        watch.Reset();
        watch.Start();

        Mesh newMesh = new Mesh();
        newMesh.CombineMeshes(combine.ToArray(), true, false);
        Logger.LogDetail("合并网格耗时:{0}", watch.ElapsedMilliseconds);
        watch.Reset();
        watch.Start();

        smrCombine.bones = boneTmp.ToArray();
        smrCombine.rootBone = rootBone;
        smrCombine.sharedMesh = newMesh;
        smrCombine.sharedMaterial = materialNew;

        Logger.LogDetail("刷新网格耗时:{0}", watch.ElapsedMilliseconds);
        watch.Stop();
    }

    private Mesh CreatMeshWithMesh(Mesh mesh)
    {
        Mesh mTmp = new Mesh();
        mTmp.vertices = mesh.vertices;
        mTmp.name = mesh.name;
        mTmp.uv = mesh.uv;
        mTmp.uv2 = mesh.uv2;
        mTmp.uv2 = mesh.uv2;
        mTmp.bindposes = mesh.bindposes;
        mTmp.boneWeights = mesh.boneWeights;
        mTmp.bounds = mesh.bounds;
        mTmp.colors = mesh.colors;
        mTmp.colors32 = mesh.colors32;
        mTmp.normals = mesh.normals;
        mTmp.subMeshCount = mesh.subMeshCount;
        mTmp.tangents = mesh.tangents;
        mTmp.triangles = mesh.triangles;

        return mTmp;
    }
    #endregion

    #region 测试合并材质球

    [ContextMenu("CombineMeshRenderer")]
    void CombineMeshRenderer()
    {
        MeshRenderer[] smrs = GetComponentsInChildren<MeshRenderer>();
        if (smrs == null || smrs.Length == 0)
            return;

        CombineInstance[] combine = new CombineInstance[smrs.Length];

        List<Material> materials = new List<Material>();
        Dictionary<string, List<Texture2D>> textures = new Dictionary<string, List<Texture2D>>();

        Transform combineTrans = transform.Find("combine");
        if (combineTrans == null)
        {
            GameObject obj = new GameObject("combine");
            obj.layer = smrs[0].gameObject.layer;
            combineTrans = obj.transform;
            combineTrans.parent = transform;
            combineTrans.localRotation = Quaternion.identity;
            combineTrans.localScale = Vector3.one;
            combineTrans.localPosition = Vector3.zero;
        }

        MeshFilter filter = combineTrans.gameObject.GetComponentDefault<MeshFilter>();
        MeshRenderer smrCombine = combineTrans.gameObject.GetComponentDefault<MeshRenderer>();
        smrCombine.shadowCastingMode = smrs[0].shadowCastingMode;
        smrCombine.receiveShadows = smrs[0].receiveShadows;

        for (int i = 0; i < smrs.Length; i++)
        {
            Material[] shares = smrs[i].sharedMaterials;
            for (int j = 0; j < shares.Length; j++)
            {
                materials.Add(shares[j]);

                for (int k = 0; k < COMBINE_SELFSHADER_TEXTURES.Length; k++)
                {
                    Texture2D tx = shares[j].GetTexture(COMBINE_SELFSHADER_TEXTURES[k]) as Texture2D;
                    Texture2D tx2D = new Texture2D(tx.width, tx.height, TextureFormat.ARGB32, false);
                    tx2D.SetPixels(tx.GetPixels(0, 0, tx.width, tx.height));
                    tx2D.Apply();
                    if (!textures.ContainsKey(COMBINE_SELFSHADER_TEXTURES[k]))
                        textures.Add(COMBINE_SELFSHADER_TEXTURES[k],new List<Texture2D>());

                    textures[COMBINE_SELFSHADER_TEXTURES[k]].Add(tx2D);
                }
            }
        }

        Material materialNew = new Material(materials[0].shader);
        materialNew.CopyPropertiesFromMaterial(materials[0]);

        Rect[] rects = null;
        foreach (var item in textures)
        {
            Texture2D texture = new Texture2D(COMBINE_TEXTURE_MAX, COMBINE_TEXTURE_MAX);

            Rect[] tempRects = texture.PackTextures(item.Value.ToArray(), 0, COMBINE_TEXTURE_MAX);
            materialNew.SetTexture(item.Key, texture);
            if (rects == null) rects = tempRects;
        }

        for (int i = 0; i < smrs.Length; i++)
        {
            MeshRenderer smr = smrs[i];
            if (smr.transform == transform)
            {
                continue;
            }

            Rect rect = rects[i];

            Mesh meshCombine = CreatMeshWithMesh(smr.GetComponent<MeshFilter>().sharedMesh);
            Vector2[] uvs = new Vector2[meshCombine.uv.Length];

            for (int j = 0; j < uvs.Length; j++)
            {
                uvs[j].x = rect.x + meshCombine.uv[j].x * rect.width;
                uvs[j].y = rect.y + meshCombine.uv[j].y * rect.height;
            }

            meshCombine.uv = uvs;
            combine[i].mesh = meshCombine;
            combine[i].transform = smrs[i].transform.localToWorldMatrix;
            smrs[i].gameObject.SetActive(false);
        }

        Mesh newMesh = new Mesh();
        newMesh.CombineMeshes(combine, true, false);
        smrCombine.sharedMaterial = materialNew;
        filter.sharedMesh = newMesh;
    }

    #endregion

    #endregion

    private NPCFashion npcFashion;
    private bool isInitializedNpc;

    private void InitializedNpc()
    {
        if (isInitializedNpc) return;
        isInitializedNpc = true;
        npcFashion = new NPCFashion(transform, this);
    }

    public static void ChangeNpcFashion(Creature c, string mesh)
    {
        if (c == null || string.IsNullOrEmpty(mesh)) return;
        if (c.activeRootNode == null) return;
        var e = c.activeRootNode.GetComponentDefault<CharacterEquip>();
        if (e == null) return;
        e.RefreshNpcCloth(mesh);
    }

    private void RefreshNpcCloth(string mesh)
    {
        InitializedNpc();
        if (npcFashion != null) npcFashion.UpdateCurrent(mesh);
    }

    private void CombineNpcMesh()
    {
        SkinnedMeshRenderer smr = npcFashion.skin;
        if (smr == null)
            return;

        var watch = TimeWatcher.Watch("combine npc mesh");

        List<CombineInstance> combine = new List<CombineInstance>();

        List<Material> materials = new List<Material>();
        List<Texture2D> textures = new List<Texture2D>();

        Transform combineTrans = transform.Find("combine");
        if (combineTrans == null)
        {
            GameObject obj = new GameObject("combine");
            obj.layer = smr.gameObject.layer;
            combineTrans = obj.transform;
            combineTrans.SetParent(transform);
            combineTrans.localRotation = Quaternion.identity;
            combineTrans.localScale = Vector3.one;
            combineTrans.localPosition = Vector3.zero;
        }

        SkinnedMeshRenderer smrCombine = combineTrans.gameObject.GetComponentDefault<SkinnedMeshRenderer>();
        smrCombine.shadowCastingMode = smr.shadowCastingMode;
        smrCombine.receiveShadows = smr.receiveShadows;        
        watch.See("准备合并网格脚本耗时");
        watch.UnWatch();

        //combine the texture
        Material[] shares = smr.sharedMaterials;
        if (shares != null)
        {
            for (int j = 0; j < shares.Length; j++)
            {
                materials.Add(shares[j]);

                Texture2D tx = shares[j].GetTexture(COMBINE_DIFFUSE_TEXTURE) as Texture2D;
                Texture2D tx2D = new Texture2D(tx.width, tx.height, TextureFormat.ARGB32, false);
                tx2D.SetPixels(tx.GetPixels(0, 0, tx.width, tx.height));
                tx2D.Apply();
                textures.Add(tx2D);
            }
        }

        watch.See("合并贴图耗时");
        watch.UnWatch();

        Material materialNew = new Material(materials[0].shader);
        materialNew.CopyPropertiesFromMaterial(materials[0]);

        Texture2D texture = new Texture2D(COMBINE_TEXTURE_MAX, COMBINE_TEXTURE_MAX);
        Rect[] rects = texture.PackTextures(textures.ToArray(), 0, COMBINE_TEXTURE_MAX);
        materialNew.SetTexture(COMBINE_DIFFUSE_TEXTURE, texture);
        watch.See("合并材质球耗时");
        watch.UnWatch();

        Transform rootBone = smr.rootBone;
        List<Transform> boneTmp = new List<Transform>();

        int rectIndex = 0;

        //collect the mesh
        for (int k = 0; k < smr.sharedMesh.subMeshCount; k++)
        {
            Rect rect = rects[rectIndex];

            Mesh meshCombine = CreatMeshWithMesh(smr.sharedMesh);
            Vector2[] uvs = new Vector2[meshCombine.uv.Length];
            for (int j = 0; j < uvs.Length; j++)
            {
                uvs[j].x = rect.x + meshCombine.uv[j].x * rect.width;
                uvs[j].y = rect.y + meshCombine.uv[j].y * rect.height;
            }
            meshCombine.uv = uvs;

            CombineInstance ci = new CombineInstance();
            ci.mesh = meshCombine;
            ci.subMeshIndex = k;
            ci.transform = smr.transform.localToWorldMatrix;
            combine.Add(ci);
            rectIndex++;
        }

        boneTmp.AddRange(smr.bones);
        smr.gameObject.SetActive(false);
        watch.See("收集网格信息耗时");
        watch.UnWatch();

        Mesh newMesh = new Mesh();
        newMesh.CombineMeshes(combine.ToArray(), true, false);
        watch.See("合并网格耗时");
        watch.UnWatch();

        smrCombine.bones = boneTmp.ToArray();
        smrCombine.rootBone = rootBone;
        smrCombine.sharedMesh = newMesh;
        smrCombine.sharedMaterial = materialNew;

        watch.See("刷新网格耗时");
        watch.UnWatch();
    }

    public class NPCFashion
    {
        public static readonly string nodeName = "body";

        public SkinnedMeshRenderer skin { get; private set; }

        public string curFashion { get; private set; }

        public Transform node { get; private set; }

        public Transform root { get; private set; }

        public CharacterEquip equip;

        public NPCFashion(Transform _root, CharacterEquip _equip)
        {
            root = _root;
            equip = _equip;
        }

        private void UpdateNodeAndRenderer()
        {
            skin = null;

            node = Util.FindChild(root, nodeName);
            skin = node?.GetComponent<SkinnedMeshRenderer>();
        }

        public void UpdateCurrent(string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            if (curFashion == value) return;

            curFashion = value;

            UpdateNodeAndRenderer();

            if (!node) return;

            Level.PrepareAsset<GameObject>(curFashion, gameObj =>
            {
                if (!gameObj) return;
                _UpdateCurrent(gameObj);
            });
        }

        private void _UpdateCurrent(GameObject t)
        {
            var mr = t?.GetComponentInChildren<SkinnedMeshRenderer>();
            if (!mr) return;

            var obs = mr.bones ?? new Transform[] { };
            var nbs = new List<Transform>();

            if (root == null) return;
            foreach (var ob in obs)
            {
                var nb = ob ? Util.FindChild(root, ob.name) : null;
                nbs.Add(nb);
            }

            if (skin == null) return;
            skin.sharedMaterial = mr.sharedMaterial;
            skin.sharedMesh = mr.sharedMesh;

            if (nbs.Count > 0) skin.bones = nbs.ToArray();
            var b = root.GetComponent<SceneObjectBehaviour>();
            if (b)
            {
                var creature = b.sceneObject as Creature;
                creature?.UpdateInverter(true);
            }
            else
            {
                var iv = root.GetComponentInChildren<InvertHelper>();
                iv?.Refresh(true);
            }
        }
    }
}