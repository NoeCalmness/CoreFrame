using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using AssetBundles;
using UnityEngine.PostProcessing;

#region TheatreStoryCreature

public abstract class TheatreStoryCreature
{
    public int currentPosId { get; set; }
    public int creatureId { get; set; }
    public Creature theatreCreature { get; set; }
    public Transform theatreCreatureTrans { get { return theatreCreature.transform; } }
    public EnumTheatreNpcLeaveType leaveDir { get; set; }

    public ShowCreatureInfo m_creaturePointInfo;

    public TheatreStoryCreature(int posId, Creature c)
    {
        currentPosId = posId;
        theatreCreature = c;

    }

    private bool IsContainsState(string state)
    {
        if(theatreCreature&& theatreCreature.stateMachine)
            return theatreCreature.stateMachine.states.Find(s => s.name == state) == null ? true : false;
        return false;
    }

    public void PlayAnimaton(string state)
    {
        if (!theatreCreature || !theatreCreature.stateMachine || theatreCreature.stateMachine.currentState.name.Equals(state)) return;
        if(IsContainsState(state))
        {
            Logger.LogError("Creature: {0} don`t exist Config  StateMachineState  Story:{1} by Index:{2} State: {3} ", GetCreatureName(), Module_Story.instance.currentStory.ID, Module_Story.instance.currentStoryIndex,state);
            return;
        }
        Logger.LogDetail("npc:{0} player state:{1}", GetCreatureName(), state);
        theatreCreature.stateMachine.TranslateTo(state);
    }

    public void PlayIdleAnim()
    {
        PlayAnimaton(NpcMono.NPC_IDLE_STATE);
    }

    public void DoEnterAnimation()
    {
        if (theatreCreature.gameObject.activeSelf) return;

        theatreCreature.gameObject.SetActive(true);
        if (Module_Story.instance.currentStoryType != EnumStoryType.NpcTheatreStory)
            PlayIdleAnim();
    }

    public void DoLeaveAnimation()
    {
        switch (leaveDir)
        {
            case EnumTheatreNpcLeaveType.LeaveToRight:
            case EnumTheatreNpcLeaveType.LeaveToLeft:
                theatreCreature.behaviour.enabled = false;
                float targetX = leaveDir == EnumTheatreNpcLeaveType.LeaveToRight ?
                    theatreCreatureTrans.localPosition.x - StoryConst.THEATRE_NPC_LEAVE_DIS : theatreCreatureTrans.localPosition.x + StoryConst.THEATRE_NPC_LEAVE_DIS;
                theatreCreatureTrans.DOLocalMoveX(targetX, GeneralConfigInfo.snpcLeaveTime).SetEase(Ease.Linear).OnComplete(() =>
                {
                    theatreCreature.Destroy();
                });
                break;
            case EnumTheatreNpcLeaveType.DispearImme:
                theatreCreature.Destroy();
                break;
        }

    }

    public void PlayAnimation(StoryInfo.StoryItem data,int roleid)
    {
        if(data.talkingRoleStates!=null)
        {
            for(int i = 0; i<data.talkingRoleStates.Length;i++)
            {
                if (data.talkingRoleStates[i].roleId == roleid)
                    PlayAnimaton(data.talkingRoleStates[i].state);
            }
        }
    }

    public void ChangePosition(int posId)
    {
        currentPosId = posId;
        SetCreaturePosition();
    }

    public void SetCreaturePosition()
    {
        m_creaturePointInfo = ConfigManager.Get<ShowCreatureInfo>(StoryConst.THEATRE_PLAYER_POS_POINT_ID);
        if (!m_creaturePointInfo) return;

        ShowCreatureInfo.CreatureOrNpcData posData = m_creaturePointInfo.GetDataByIndex(currentPosId);
        if (posData != null && posData.data.Length > 0 && theatreCreature.activeRootNode)
        {
            theatreCreature.activeRootNode.localPosition = posData.data[0].pos;
            theatreCreature.activeRootNode.localEulerAngles = posData.data[0].rotation;
        }
    }

    public virtual void SetRenderQueue(int renderQueue = -1) { }

    public virtual string GetCreatureName() { return string.Empty; }

    public virtual string GetCreatureIcon() { return string.Empty; }

    public virtual void ResetMaterial() { }
}

#endregion



#region custom story npc data

public class TheatreStoryNpc: TheatreStoryCreature
{
    public Creature npcCreature { get; private set; }

    public NpcInfo npcInfo { get; private set; }
    private Dictionary<Material, int> m_npcMats = new Dictionary<Material, int>();

    public TheatreStoryNpc(int posId, Creature c, NpcInfo npc) : base(posId, c)
    {
        npcInfo = npc;
        creatureId = npc.ID;
        InitAllMaterial();
    }

   

    public override string GetCreatureName()
    {
        if(npcInfo!=null)
        {
            return ConfigText.GetDefalutString(npcInfo.name);
        }

        return string.Empty;
    }

    #region handle material

    private void InitAllMaterial()
    {
        m_npcMats.Clear();
        if (theatreCreature && theatreCreature.model)
        {
            for (int i = 0; i< theatreCreature.model.childCount; i++)
            {
                SkinnedMeshRenderer smr = theatreCreature.model.GetChild(i).GetComponent<SkinnedMeshRenderer>();
                if (!smr) continue;

                foreach (var item in smr.sharedMaterials)
                {
                    if(!item)
                    {
                        var assetName = string.Empty;
                        if (npcInfo.type == 1)
                        {
                            var _npc = Module_Npc.instance.GetTargetNpc((NpcTypeID)npcInfo.ID);
                            if (_npc != null) assetName = _npc.mode;
                        }
                        else
                            assetName = npcInfo.mode;

                        Logger.LogError("model {0} transform {1} material is null", assetName, smr.name);
                        continue;
                    }
                    if (m_npcMats.ContainsKey(item)) continue;

                    m_npcMats.Add(item, item.renderQueue);
                }
            }
        }
    }

    /// <summary>
    /// renderqueue < 0  : recovery defalut render queue
    /// else set material renderqueue as value
    /// </summary>
    /// <param name="renderQueue"></param>
    public override void SetRenderQueue(int renderQueue = -1)
    {
        bool recovery = renderQueue < 0;
        foreach (var item in m_npcMats)
        {
            if (item.Key) item.Key.renderQueue = recovery ? item.Value : renderQueue;
        }
    }
    #endregion
}

#endregion

#region Theatre Role Struct
public class TheatreStoryRole: TheatreStoryCreature
{

    private Dictionary<Material, int> m_roleMats = new Dictionary<Material, int>();

    private Dictionary<string,float> m_ShadowContrast = new Dictionary<string, float>();
    public TheatreStoryRole(int posId, Creature c):base(posId,c)
    {
        creatureId = 0;//约定是0的就是角色本身
        SetCreaturePosition();
        InitAllMaterial();
    }

    private void InitAllMaterial()
    {
        m_roleMats.Clear();
        if (theatreCreature && theatreCreature.model)
        {
            for (int i = 0; i < theatreCreature.model.childCount; i++)
            {
                SkinnedMeshRenderer smr = theatreCreature.model.GetChild(i).GetComponent<SkinnedMeshRenderer>();
                if (!smr) continue;

                foreach (var item in smr.sharedMaterials)
                {
                    if (!item)
                    {
                        continue;
                    }
                    if (m_roleMats.ContainsKey(item))
                        continue;
                    if (item.HasProperty("_ShadowContrast"))
                    {
                        m_ShadowContrast.Add(item.name, item.GetFloat("_ShadowContrast"));
                        item.SetFloat("_ShadowContrast", 8);
                    }
                        
                    m_roleMats.Add(item, item.renderQueue);
                }
            }
        }
    }

    public override void SetRenderQueue(int renderQueue = -1)
    {
        bool recovery = renderQueue < 0;
        foreach (var item in m_roleMats)
        {
            if (item.Key) item.Key.renderQueue = recovery ? item.Value : renderQueue;
        }
    }

    public override string GetCreatureName()
    {
        if (theatreCreature != null)
           return theatreCreature.uiName;
        return string.Empty;
    }

    public override string GetCreatureIcon()
    {
        return Util.Format("ui_character_sword_emoji_{0}", Module_Player.instance.proto);
    }

    public override void ResetMaterial()
    {
        foreach (var item in m_roleMats)
        {
            if (item.Key)
            {
                float value = m_ShadowContrast[item.Key.name];
                if (value != 0)
                    item.Key.SetFloat("_ShadowContrast", value);
            }
        }
    }

}
#endregion
public class TheatreStory : BaseStory
{
    #region custom class

    public class StoryPropItem : CustomSecondPanel
    {
        private static readonly string[] qualityBgNames = new string[] { "quality/1", "quality/2", "quality/3", "quality/4", "quality/5", "quality/6" };
        private static readonly string[] itemStar = new string[] { "stars/count/1", "stars/count/2", "stars/count/3", "stars/count/4", "stars/count/5", "stars/count/6" };

        public StoryPropItem(Transform trans) : base(trans)
        {
        }

        private Image m_icon;
        private List<GameObject> m_qualityBg = new List<GameObject>();
        private List<GameObject> m_star = new List<GameObject>();
        private GameObject m_runeParent;
        private Text m_runBearing;
        private Text m_level;
        private Text m_count;

        public override void InitComponent()
        {
            base.InitComponent();

            m_icon = transform.GetComponent<Image>("icon");
            m_count = transform.GetComponent<Text>("numberdi/count");
            m_qualityBg.Clear();
            foreach (var item in qualityBgNames)
            {
                m_qualityBg.Add(transform.Find(item).gameObject);
            }
            m_star.Clear();
            for (int i =0;i< itemStar.Length;i++)
            {
                m_star.Add(transform.Find(itemStar[i]).gameObject);
            }
            m_level = transform.GetComponent<Text>("intentify/mark");
            m_runeParent = transform.Find("qualityRune").gameObject;
            //m_levelParent = transform.Find("intentify").gameObject;
            m_runBearing = transform.GetComponent<Text>("qualityRune/txt");

        }

        public void RefreshItem(StoryInfo.GivePropData data)
        {
            PropItemInfo info = ConfigManager.Get<PropItemInfo>(data.itemTypeId);
            if (!info) return;
            //等级 星级 数量 图标
            AtlasHelper.SetItemIcon(m_icon, info);
            if (data.level > 0)
            {
                Util.SetText(m_level, $"Lv.{data.level}");
                m_level.transform.parent.SafeSetActive(true);
            }
            else
                m_level.transform.parent.SafeSetActive(false);
            Util.SetText(m_count, data.num > 0 ? data.num.ToString() : "");
            for (int i = 0; i < m_star.Count; i++) m_star[i].SetActive(i <= data.star - 1);
            for (int i = 0; i < m_qualityBg.Count; i++) m_qualityBg[i].SetActive(i == info.quality - 1);
            if (info.itemType == PropType.Rune) RefreshRune(data, info);
            else RefreshNormalProp(data,info);
        }
        
        private void RefreshRune(StoryInfo.GivePropData data,PropItemInfo info)
        {
            var index = info.subType - 1 < 0 ? 0 : info.subType - 1;
            Util.SetText(m_runBearing, Util.GetString(10103, index));
            m_runeParent.gameObject.SetActive(true);
        }

        private void RefreshNormalProp(StoryInfo.GivePropData data, PropItemInfo info)
        {
            m_runeParent.gameObject.SetActive(false);
        }
    }

    #endregion

    private const int MAX_DISPLAY_PROP_COUNT = 3;
    public const float ROOT_POSITION = -1000f;

    private static readonly int m_mainTexNameID = Shader.PropertyToID("_MainTex");
    private static readonly List<Vector2> m_tmpUVs = new List<Vector2>() { new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 1) };
    private static readonly int[] m_tmpIndices = new int[] { 0, 2, 1, 1, 3, 0 };

    protected GameObject m_rolePanel;
    protected Image m_blurMask;

    protected MeshRenderer m_backRender;
    protected Mesh m_backgroundQuad;
    protected Material m_backgroundMaterial = null;
    protected string m_currentBack = null;
    protected Vector3[] m_tmpVertices = new Vector3[4];

    protected GameObject m_maskPanel;
    protected CanvasGroup m_maskGroup;

    protected GameObject m_maskBg;
    protected Text m_nameText;
    protected Text m_contentText;
    private RectTransform m_nameTextRect;
    private RectTransform m_contentTextRect;
    protected Transform m_arrowTrans;
    protected CanvasGroup m_storyDialogCg;

    protected GameObject m_propParent;
    protected GameObject m_propPreafab;
    protected List<StoryPropItem> m_propItems = new List<StoryPropItem>();

    protected Dictionary<GameObject, int> m_modelTrans = new Dictionary<GameObject, int>();
    protected ShowCreatureInfo m_npcPointInfo;
    protected ShowCreatureInfo m_cameraPointInfo;
    protected ShowCreatureInfo m_modelPointInfo;

    protected Image m_contentBg;
    protected RawImage m_playerImg;

    /// <summary>
    /// whether current story is dating story
    /// </summary>
    protected bool m_bIsDatingStory = false;

    #region creature field
    protected Transform m_npcRoot;
    protected Transform m_npcCameraTrans;
    protected Camera m_renderCamera;
    protected List<TheatreStoryCreature> m_theatreStoryCreatureList = new List<TheatreStoryCreature>();

    protected GameObject m_theatreMask;
    protected Material m_theatryMaskMat;
    #endregion

    #region Tipes
    private Text m_tipes;
    #endregion

    #region Camera fields
    private float m_cameraFarClip = -1;
    private float m_cameraFoV = -1;
    private float m_cameraSize = -1;
    #endregion

    #region content width&hight
    private Vector2 contentWidthWithoutAvatar = new Vector2(855,130);
    private Vector2 contentWidthWithAvatar = new Vector2(680,130);
    #endregion

    #region init
    protected override void InitComponent()
    {
        base.InitComponent();
        m_bIsDatingStory = false;
        m_rolePanel = transform.Find("panel_scenario").gameObject;
        m_blurMask  = transform.GetComponent<Image>("panel_scenario/blurmask");
        m_blurMask.SafeSetActive(false);
        if (m_blurMask) m_blurMask.raycastTarget = false;
        else Logger.LogError("m_blurMask cannot be finded....");

        m_fastForwardBtn = transform.GetComponent<Button>("fast_forward_btn");
        m_normalBtn = transform.GetComponent<Button>("normalplay_btn");
        m_skipStroyBtn = transform.GetComponent<Button>("jump_forward_btn");
        m_tipes = transform.GetComponent<Text>("tip");
        m_contentBg = transform.GetComponent<Image>("panel_dialog/background");
        m_playerImg = transform.GetComponent<RawImage>("panel_dialog/background/player");

        m_maskPanel = transform.Find("panel_mask").gameObject;
        m_maskGroup = m_maskPanel.GetComponent<CanvasGroup>();
        m_maskPanel.SetActive(false);

        m_maskBg = transform.Find("mask").gameObject;
        m_nameText = transform.GetComponent<Text>("panel_dialog/background/panel_name/text_name");
        m_contentText = transform.GetComponent<Text>("panel_dialog/background/panel_content/content");
        m_nameTextRect = transform.GetComponent<RectTransform>("panel_dialog/background/panel_name/text_name");
        m_contentTextRect = transform.GetComponent<RectTransform>("panel_dialog/background/panel_content/content");
        m_arrowTrans = transform.Find("panel_dialog/background/button_next");

        m_storyDialogCg = transform.Find("panel_dialog").GetComponentDefault<CanvasGroup>();
        m_storyDialogCg.alpha = 0;

        LoadAllShowCreatureInfoData();

        CreateNpcRoot();
        CreateMask();
        InitRenderCamera();
        CreateBackground();
        InitPropDisplay();

        Text t = m_fastForwardBtn?.transform.GetComponent<Text>("Text");
        if (t) t.text = ConfigText.GetDefalutString(TextForMatType.StoryUIText, 0);
        t = m_normalBtn?.transform.GetComponent<Text>("Text");
        if (t) t.text = ConfigText.GetDefalutString(TextForMatType.StoryUIText, 1);
        Text tt = m_skipStroyBtn?.transform.GetComponent<Text>("Text");
        if (tt) tt.text = ConfigText.GetDefalutString(TextForMatType.StoryUIText, 2);
        
        EventManager.AddEventListener(Events.VEDIO_SETTINGS_CHANGED, OnVideoSettingsChanged);
#if UNITY_EDITOR
        EventManager.AddEventListener("EditorReloadConfig", OnEditorReloadConfig);
#endif
    }

    protected override void AddEvent()
    {
        base.AddEvent();

        EventTriggerListener.Get(m_maskBg).onClick = OnContextBgClick;
        EventTriggerListener.Get(m_fastForwardBtn.gameObject).onClick = SwitchFastForward;
        EventTriggerListener.Get(m_normalBtn.gameObject).onClick = SwitchFastForward;
        EventTriggerListener.Get(m_skipStroyBtn.gameObject).onClick = SkipTheatreStory;
    }

    private void CreateNpcRoot()
    {
        if (m_npcRoot) return;

        m_npcRoot = Level.current.root.Find("story_npc_root");
        if (!m_npcRoot)
        {
            m_npcRoot = Level.current.root.AddNewChild();
            m_npcRoot.name = "story_npc_root";
        }

        m_npcRoot.localPosition = new Vector3(ROOT_POSITION, 0f, 0f);
        m_npcRoot.localEulerAngles = new Vector3(0, 180, 0);
    }

    private void CreateMask()
    {
        var t = m_npcRoot.Find(StoryConst.THEATRE_MASK_ASSET_NAME);
        if (t) m_theatreMask = t.gameObject;
        else
        {
            m_theatreMask = Level.GetPreloadObject<GameObject>(StoryConst.THEATRE_MASK_ASSET_NAME, true);
            m_theatreMask.transform.SetParent(m_npcRoot);
        }

        ShowCreatureInfo.CreatureOrNpcData posData = m_npcPointInfo?.GetDataByIndex(100);
        if (posData != null && posData.data.Length > 0)
        {
            m_theatreMask.transform.localPosition = posData.data[0].pos;
            m_theatreMask.transform.localEulerAngles = posData.data[0].rotation;
            m_theatreMask.transform.localScale = new Vector3(posData.data[0].size, posData.data[0].size,1);
        }

        var mr = m_theatreMask.GetComponent<MeshRenderer>();
        if (mr)
        {
            m_theatryMaskMat = mr.material;
            m_theatryMaskMat.renderQueue = GeneralConfigInfo.sstoryMaskRenderQueue;
        }
    }

    private void InitRenderCamera()
    {
        if (moduleStory.npcCameraTrans == null) return;

        m_npcCameraTrans = moduleStory.npcCameraTrans;
        m_npcCameraTrans.SetParent(m_npcRoot);

        m_renderCamera = m_npcCameraTrans.GetComponent<Camera>();
        m_renderCamera.GetComponentDefault<_CameraShake>();
        if (Level.current is Level_NpcDating)
        {
            PostProcessingProfile profile = Level.current.mainCamera.GetComponent<PostProcessingBehaviour>().profile;
            profile.depthOfField.enabled = false;
            m_npcCameraTrans.GetComponent<PostProcessingBehaviour>().profile = profile;
        }
        else
        {
            m_npcCameraTrans.GetComponent<PostProcessingBehaviour>().profile = moduleStory.postProcessingDefalutProfile;
        }
        m_renderCamera.targetTexture = null;
        m_renderCamera.depth = 1;
        //m_renderCamera.backgroundColor = new Color(1,1,1,0);
        m_renderCamera.cullingMask |= 1 << Layers.GROUND;

        ShowCreatureInfo.CreatureOrNpcData data = m_cameraPointInfo?.GetDataByIndex(StoryConst.THEATRE_CAMREA_POS_POINT_INDEX);
        if (data != null && data.data.Length > 0)
        {
            if (m_npcCameraTrans)
            {
                m_npcCameraTrans.localPosition = data.data[0].pos;
                m_npcCameraTrans.localEulerAngles = data.data[0].rotation;
                if (data.data[0].size > 0)
                    m_renderCamera.farClipPlane = data.data[0].size;
                if (data.data[0].fov > 0)
                    m_renderCamera.fieldOfView = data.data[0].fov;
            }
        }
        m_cameraFarClip = m_renderCamera.farClipPlane;
        m_cameraFoV = m_renderCamera.fieldOfView;
        m_cameraSize = m_renderCamera.orthographicSize;
    }

    private void CreateBackground()
    {
        if (!m_renderCamera) return;

        var node = m_npcCameraTrans.Find("background");
        if (!node)
        {
            node = new GameObject("background").transform;
            node.gameObject.layer = Layers.MODEL;

            Util.AddChild(m_npcCameraTrans, node);
        }

        if (!m_backgroundQuad)
        {
            m_backgroundQuad = new Mesh();
            m_backgroundQuad.hideFlags = HideFlags.HideAndDontSave;
        }

        if (!m_backgroundMaterial)
        {
            m_backgroundMaterial = new Material(Shader.Find("HYLR/Effect/Story Background"));
            m_backgroundMaterial.hideFlags = HideFlags.DontSave;
            m_backgroundMaterial.renderQueue = GeneralConfigInfo.sstoryMaskRenderQueue + 10;
            m_backgroundMaterial.SetTexture("_MainTex", null);
        }

        var filter   = node.gameObject.GetComponentDefault<MeshFilter>();
        var renderer = node.gameObject.GetComponentDefault<MeshRenderer>();

        filter.mesh = m_backgroundQuad;

        renderer.lightProbeUsage            = UnityEngine.Rendering.LightProbeUsage.Off;
        renderer.reflectionProbeUsage       = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        renderer.receiveShadows             = false;
        renderer.shadowCastingMode          = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        renderer.material                   = m_backgroundMaterial;

        m_backRender = renderer;

        UpdateBackgroundQuad(true);

        m_backRender.gameObject.SetActive(false);

        m_currentBack = null;
    }

    private void UpdateBackgroundQuad(bool forceUpdate = false)
    {
        if (!m_renderCamera || !m_backgroundQuad) return;

        if (!forceUpdate && m_cameraFarClip == m_renderCamera.farClipPlane && m_cameraFoV == m_renderCamera.fieldOfView && m_cameraSize == m_renderCamera.orthographicSize) return; // Nothing changed...

        m_cameraSize    = m_renderCamera.orthographicSize;
        m_cameraFarClip = m_renderCamera.farClipPlane;
        m_cameraFoV     = m_renderCamera.fieldOfView;

        var fix = (GeneralConfigInfo.smaxAspectRatio / UIManager.aspect - 1.0f) * 0.5f;
        var far = m_renderCamera.farClipPlane * 0.99f; // Make far distance a little smaller than clip distance, to avoid clipping

        // To avoid matrix translation precesion issue, we first move camera node to default state
        var t = m_renderCamera.transform;
        var p = t.position;
        var r = t.rotation;

        t.position = Vector3.zero;
        t.rotation = Quaternion.identity;
        m_renderCamera.ResetWorldToCameraMatrix();

        if (m_tmpVertices == null) m_tmpVertices = new Vector3[4];

        // Now calculate far clip plane quad
        var a = m_renderCamera.ViewportToWorldPoint(new Vector3(1 + fix, 0, far));
        var b = m_renderCamera.ViewportToWorldPoint(new Vector3(0 - fix, 1, far));
        var c = m_renderCamera.ViewportToWorldPoint(new Vector3(0 - fix, 0, far));
        var d = m_renderCamera.ViewportToWorldPoint(new Vector3(1 + fix, 1, far));

        // Reset camera matrix
        t.position = p;
        t.rotation = r;

        a.z = b.z = c.z = d.z = 0; // Vertices do not need Z position, we set it to tansform node's local position

        m_tmpVertices[0] = a;
        m_tmpVertices[1] = b;
        m_tmpVertices[2] = c;
        m_tmpVertices[3] = d;

        m_backgroundQuad.vertices = m_tmpVertices;
        m_backgroundQuad.SetIndices(m_tmpIndices, MeshTopology.Triangles, 0);
        m_backgroundQuad.SetUVs(0, m_tmpUVs);

        if (m_backRender)
        {
            m_backRender.transform.localPosition = new Vector3(0, 0, far);

            if (storyType == EnumStoryType.NpcTheatreStory)
                m_renderCamera.cullingMask &= ~(1 << Layers.PvpNode);
        }
    }

    private void DestroyBackground()
    {
        if (m_backgroundQuad) Destroy(m_backgroundQuad);
        if (m_backgroundMaterial) Destroy(m_backgroundMaterial);

        if(m_backRender) Destroy(m_backRender.gameObject);

        m_currentBack = null;
        m_tmpVertices = null;
    }

    private void OnVideoSettingsChanged()
    {
        SettingsManager.ApplyVideoSettings(m_renderCamera);
    }

    private void InitPropDisplay()
    {
        m_propParent = transform.Find("getwupin").gameObject;
        m_propPreafab = transform.Find("getwupin/0").gameObject;
        m_propPreafab.SetActive(false);
        m_propItems.Clear();
    }

    private void RestAllTheatreData()
    {
        if (m_maskPanel)        m_maskPanel.SetActive(false);
        if (m_npcCameraTrans)   m_npcCameraTrans.gameObject.SetActive(false);
        if (m_propParent)       m_propParent.SetActive(false);
        DestoryNpc();
        SetMainCameraEnable(true);
    }

    private void DestoryNpc()
    {
        foreach (var item in m_theatreStoryCreatureList)
        {
            item.ResetMaterial();
            item.SetRenderQueue();
            item.theatreCreature.Destroy();
        }
        m_theatreStoryCreatureList.Clear();
    }

    protected override void OnEnable()
    {
        SetMainCameraEnable(false);
        if (m_npcCameraTrans)
        {
            var node = m_npcCameraTrans.Find("background");
            if (m_backgroundQuad != null)
            {
                var filter = node.gameObject.GetComponent<MeshFilter>();
                filter.mesh = m_backgroundQuad;
            }
            if (m_backgroundMaterial != null)
            {
                var renderer = node.gameObject.GetComponent<MeshRenderer>();
                renderer.material = m_backgroundMaterial;
            }
            m_npcCameraTrans.gameObject.SetActive(true);
            UpdateBackgroundQuad();
        }
        EventManager.AddEventListener(Events.UI_WINDOW_VISIBLE, OnWindowOpen);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        SetMainCameraEnable(true);
        DestoryNpc();
        DestoryModels();
        EventManager.RemoveEventListener(Events.UI_WINDOW_VISIBLE, OnWindowOpen);
    }

    protected override void Update()
    {
        base.Update();

        UpdateBackgroundQuad();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventManager.RemoveEventListener(Events.VEDIO_SETTINGS_CHANGED, OnVideoSettingsChanged);
#if UNITY_EDITOR
        EventManager.RemoveEventListener("EditorReloadConfig", OnEditorReloadConfig);
#endif
        DestoryModels();
        DestroyBackground();
    }

    private void SetMainCameraEnable(bool enable)
    {
        if (Level.current && Level.current.mainCamera && !HasAnyHomeCameraEnable())
        {
            Level.current.mainCamera.enabled = enable;
        }
    }

    /// <summary>
    /// 如果主场景其他分场景的摄像机打开的时候，不强制恢复mainCamera
    /// </summary>
    /// <returns></returns>
    private bool HasAnyHomeCameraEnable()
    {
        if (!Level.current || !(Level.current is Level_Home)) return false;

        return (Level.current as Level_Home).HasOtherCameraShow();
    }
    #endregion

    #region click event
    private void OnContextBgClick(GameObject sender)
    {
        if (fastForward && (m_propParent != null && !m_propParent.activeSelf))
        {
           return;
        }
       
        if (!isTeamMode)
            OnChangeToStoryStep();
        else
            ChangeTeamStoryStep();
    }

    private void ChangeTeamStoryStep()
    {
        if (m_contextStep == EnumContextStep.Show || m_contextStep == EnumContextStep.End) SendStoryStepToServer(m_contextStep);
    }

    private void OnChangeToStoryStep()
    {
        switch (m_contextStep)
        {
            //show all context
            case EnumContextStep.Show:
                {
                    ShowAllContextImme();
                }
                break;
            //change to next story item
            case EnumContextStep.End:
                {
                    OnDialogEnd();
                }
                break;
            default:
                break;
        }
    }

    protected override void OnRecvFrameData(EnumContextStep changeToStep)
    {
        base.OnRecvFrameData(changeToStep);
        OnChangeToStoryStep();
    }
    //该段对话结束
    private void OnDialogEnd()
    {
        if (m_bIsDatingStory && moduleStory != null && moduleStory.hasNotRenderItem == false)
        {
            if (moduleStory.DispatchStoryWillEnd(storyId, storyType))
            {
                Debug.Log("Dispatch story will end");
                return;
            }
        }
        if (storyItem == null)
        {
            OnUpdatePerStoryFail();
            return;
        }
        if (storyItem.canChangeNextDialog)
            ChangeToNextStoryItem();
        else
            OpenWindow();
    }

    private void OpenWindow()
    {
        ObjectManager.timeScale = 1;
        var type = Game.GetType(storyItem.openWindow);
        if (type == null)
        {
            Logger.LogError("window_name = {0} cannot be loaded,please check out", storyItem.openWindow);
            ChangeToNextStoryItem();
        }
        else Window.ShowAsync(storyItem.openWindow);
    }

    private void OnWindowOpen(Event_ e)
    {
        Window w = e.sender as Window;
        //Logger.LogInfo("{1} : display window = {0}", w.name, Time.realtimeSinceStartup);
        if (w is Window_DefaultLoading || w is Window_Gm) return;

        if (w.GetType().GetInterface("IBackToStory") != null)
        {
            IBackToStory callback = (IBackToStory)w;
            callback.onBackToStory = OnConfirmClick;
            UIManager.SetCameraLayer(Layers.UI, Layers.Dialog);
        }
    }

    private void OnConfirmClick()
    {
        // if (fastForward) return;
        ObjectManager.timeScale = speed;
        UIManager.SetCameraLayer(Layers.Dialog);
        ChangeToNextStoryItem();
    }
    
    protected override void SwitchFastForward(GameObject sender)
    {
        base.SwitchFastForward(sender);
        
        CheckAutoNextDialog();
    }

    void SkipTheatreStory(GameObject sender)
    {
        if (storyInfo == null)
            return;
        if(!storyInfo.IsSkipStory())
        {
            if (m_tipes!=null)
            {
                m_tipes.text = ConfigText.GetDefalutString(TextForMatType.StoryUIText, 3);
                m_tipes.gameObject.SetActive(true);
            }
            //提示不能跳过该剧情
            return;
        }
        //停止音频音效
        if (!string.IsNullOrEmpty(m_lastPlayMusic)) AudioManager.Stop(m_lastPlayMusic);
        if (!string.IsNullOrEmpty(m_lastPlayVoiceName)) AudioManager.Stop(m_lastPlayVoiceName);
        //有奖励的发放相关道具
        List<List<StoryInfo.GivePropData>> givePropDatas = storyInfo.GetGivePropDatas();
        if(givePropDatas!= null)
        {
           for(int i = 0; i<givePropDatas.Count;i++)
            {
                moduleStory.RequestProp(storyInfo.ID, givePropDatas[i].ToArray());
            }
        }
        OnUpdatePerStoryFail();
    }
    #endregion

    #region refresh per story item
    protected override void ResetPerStoryItem()
    {
        base.ResetPerStoryItem();

        m_nameText.text = string.Empty;
        m_nameTextRect.sizeDelta = contentWidthWithoutAvatar;
        m_contentText.text = string.Empty;
        m_contentTextRect.sizeDelta = contentWidthWithoutAvatar;
        m_arrowTrans.gameObject.SetActive(false);
        for (int i = 0; i < m_propItems.Count; i++)
        {
            m_propItems[i].gameObject.SetActive(false);
        }
        if (m_propParent) m_propParent.SetActive(false);
    }

    protected override void OnUpdatePerStoryFail()
    {
        base.OnUpdatePerStoryFail();
        RestAllTheatreData();
       
    }

    protected override void OnUpdatePerStorySuccess()
    {
        base.OnUpdatePerStorySuccess();

        SetMaskColor();
        SetBackground();
        SetStoryContextTexture();
        ShowBlackMask();
        ////must call this between npc leave and create function
        //HandleNpcResetToIdle();
        HandleCreateNpc();
        HandleHideNpc();
        //HandleDialogBg();
        CreateModel();
        SetModelPos();
        SetCameraPos();
       
        //call this function at last
        StartCheckWhenItemRefresh();
    }
    
    protected override void OnPlayUIEffect(GameObject effect)
    {
        base.OnPlayUIEffect(effect);
        if (!effect) return;
        OnBGAndRoleGauss(effect);
    }

    private void OnBGAndRoleGauss(GameObject effect)
    {
        if(storyItem.effect.Equals(StoryConst.EYE_CLIP_EFFECT))
        {
            m_blurMask.SafeSetActive(true);

            var t = effect.GetComponent<UIEyeTween>();
            if (t != null)
            {
                t.PlayMatTween(m_blurMask.material);
                t.onComplete = ()=> { AfterGaussFinish(effect); };
            }
        }
    }

    private void AfterGaussFinish(GameObject obj)
    {
        if (!obj) return;

        DestroyImmediate(obj);
        m_blurMask.SafeSetActive(false);
    }

    protected virtual void SetBackground()
    {
        if (!m_backRender) return;

        if (!string.IsNullOrEmpty(storyItem.background) && m_currentBack != storyItem.background)
        {
            m_currentBack = storyItem.background;
            m_npcCameraTrans.SafeSetActive(true);
            var tex = Level.GetPreloadObject<Texture>(storyItem.background, false);
            if (tex)
            {
                m_backRender.sharedMaterial.SetTexture(m_mainTexNameID, tex);
                m_backRender.gameObject.SafeSetActive(true);
            }
            else
            {
                Level.PrepareAsset<Texture>(storyItem.background, t =>
                {
                    if (!t || t.name != m_currentBack) return;
                    m_backRender.sharedMaterial.SetTexture(m_mainTexNameID, t);
                    m_backRender.gameObject.SetActive(true);
                });
            }
        }
        
    }

    private void ShowBlackMask()
    {
        if (storyItem.blackData.blackScreen)
        {
            m_maskPanel.SetActive(true);

            if (storyItem.blackData.immediately) m_maskGroup.alpha = 1f;
            else
            {
                m_maskGroup.alpha = 0f;
                DOTween.To(() => m_maskGroup.alpha, x => m_maskGroup.alpha = x, 1f, StoryConst.MASK_PANEL_ALPHA_DURACTION).SetUpdate(true).OnComplete(() =>
                {
                    m_maskGroup.alpha = 1f;
                });
            }
        }

        if (storyItem.blackData.cancelBlack)
        {
            if (storyItem.blackData.immediately) OnMaskTweenComplete();
            else
            {
                m_maskGroup.alpha = 1f;
                DOTween.To(() => m_maskGroup.alpha, (x) => m_maskGroup.alpha = x, 0f, StoryConst.MASK_PANEL_ALPHA_DURACTION).SetUpdate(true).OnComplete(OnMaskTweenComplete);
            }
        }
    }

    private void OnMaskTweenComplete()
    {
        m_maskGroup.alpha = 0f;
        m_maskPanel.SetActive(false);
    }

    private void LoadAllShowCreatureInfoData()
    {
        m_npcPointInfo = ConfigManager.Get<ShowCreatureInfo>(StoryConst.THEATRE_PLAYER_POS_POINT_ID);
        m_cameraPointInfo = ConfigManager.Get<ShowCreatureInfo>(StoryConst.THEATRE_CAMERA_POS_POINT_ID);
        m_modelPointInfo = ConfigManager.Get<ShowCreatureInfo>(StoryConst.THEATRE_MODEL_POS_POINT_ID);
    }

    #region handle npc
    
    private void HandleNpcLeave()
    {
        List<TheatreStoryNpc> leaveNpcs = GetLeaveCreatureList();
        if (leaveNpcs.Count == 0)
            return; 

        for (int i = 0; i < leaveNpcs.Count; i++) leaveNpcs[i].DoLeaveAnimation();
    }

    private List<TheatreStoryNpc> GetLeaveCreatureList()
    {
        List<TheatreStoryNpc> leaveNpcs = new List<TheatreStoryNpc>();

        StoryInfo.TalkingRoleData data = null;
        if (storyItem == null)
            return leaveNpcs;
        for (int i = 0; i < storyItem.talkingRoles.Length; i++)
        {
            data = storyItem.talkingRoles[i];
            if (IsNeedLeave(data.rolePos))
            {
                TheatreStoryCreature curNpc = GetTargetCreature(data.roleId);
                if (curNpc != null)
                {
                    curNpc.leaveDir = (EnumTheatreNpcLeaveType)data.rolePos;
                    leaveNpcs.Add(curNpc as TheatreStoryNpc);
                    m_theatreStoryCreatureList.Remove(curNpc);
                }
            }
        }

        return leaveNpcs;
    }

    private bool IsNeedLeave(int pos)
    {
        return pos == StoryConst.THEATRE_NPC_LEAVE_TO_LEFT || pos == StoryConst.THEATRE_NPC_LEAVE_TO_RIGHT;
    }

    private bool IsNeedHide(int pos)
    {
        return pos == StoryConst.THEATRE_NPC_HIDE;
    }

    private TheatreStoryCreature GetTargetCreature(int id)
    {
        return m_theatreStoryCreatureList.Find(n=>n.creatureId == id);
    }

    private void HandleNpcResetToIdle()
    {
        //遍历，如果该角色当前对话还要做动画的话，就不处理，否则还原到IDLE
        foreach (var item in m_theatreStoryCreatureList)
        {
            if (storyItem.IsNeedChangeState(item.creatureId))
                continue;

            item.PlayIdleAnim();
        }
    }

    private void HandleCreateNpc()
    {
        StoryInfo.TalkingRoleData data = null;
        for (int i = 0; i < storyItem.talkingRoles.Length; i++)
        {
            data = storyItem.talkingRoles[i];
            if (IsNeedLeave(data.rolePos) || IsNeedHide(data.rolePos))
                continue;
            //角色
            if(data.roleId == 0)
            {
                TheatreStoryRole role = GetTargetCreature(data.roleId) as TheatreStoryRole;
                //存在角色需要更新位置
                if (role != null && role.currentPosId != data.rolePos)
                {
                    role.theatreCreature.direction = CreatureDirection.FORWARD;
                    role.ChangePosition(data.rolePos);
                }
                if(role == null)
                {   //创建剧情角色
                    Creature c = null;
                    c = Module_Home.instance.CreatePlayer();
                    if (c == null || !c.gameObject) continue;
                    c.checkEdge = false;
                    c.localEulerAngles = Vector3.zero;
                    c.position_ = m_npcRoot.position;
                    Util.AddChild(m_npcRoot, c.transform);
                    Util.SetLayer(c.gameObject, Layers.MODEL);
                    role = new TheatreStoryRole(data.rolePos, c);
                    role.PlayAnimation(storyItem, data.roleId);
                    m_theatreStoryCreatureList.Add(role);
                }
            }
            else
            {
                //NPC
                TheatreStoryNpc npc = GetTargetCreature(data.roleId) as TheatreStoryNpc;
                //we need move the npc to new pos
                if (npc != null && npc.currentPosId != data.rolePos)
                {
                    npc.ChangePosition(data.rolePos);
                    continue;
                }

                //we need create a new npc
                if (npc == null)
                {
                    NpcInfo info = ConfigManager.Get<NpcInfo>(data.roleId);
                    if (info == null)
                    {
                        Logger.LogError("storyInfo id = {0} index = {1} ,npcId = {2} connot be finded ", storyId, curStoryItemIndex, data.roleId);
                        continue;
                    }
                    List<string> npcasset = Module_Battle.BuildNpcSimplePreloadAssets(info.ID);
                    npcasset.Add(info.mode);
                    Level.PrepareAssets(npcasset, (e)=>
                      {
                            Creature c = Module_Home.instance.CreateNpc(data.roleId, Vector3.zero, Vector3.zero, m_npcRoot);
                            c.checkEdge = false;
                            c.localEulerAngles = Vector3.zero;
                            c.position_ = m_npcRoot.position;
                            npc = new TheatreStoryNpc(data.rolePos, c, info);
                            npc.SetCreaturePosition();
                            npc.PlayAnimation(storyItem, data.roleId);
                          var modenpc = Module_Npc.instance.GetTargetNpc((NpcTypeID)data.roleId);
                          CharacterEquip.ChangeNpcFashion(c, modenpc.mode);
                          m_theatreStoryCreatureList.Add(npc);
                       });
                }
            }
        }
        StoryInfo.TalkingRoleState rolestate = null;
        for (int m =0;m<storyItem.talkingRoleStates.Length;m++)
        {
            rolestate = storyItem.talkingRoleStates[m];
            TheatreStoryCreature creature = GetTargetCreature(rolestate.roleId);
            if (creature != null)
                creature.PlayAnimation(storyItem, rolestate.roleId);
            else
            {
                Logger.LogError("未找到剧情角色ID{0}所以不进行动作播放！！！", rolestate.roleId);
            }
        }
    }
    private void HandleShowNpc()
    {
        StoryInfo.TalkingRoleData data = null;
        for (int i = 0; i < storyItem.talkingRoles.Length; i++)
        {
            data = storyItem.talkingRoles[i];
            if (data.rolePos == StoryConst.THEATRE_NPC_HIDE) continue;

            TheatreStoryCreature npc = GetTargetCreature(data.roleId);
            if (npc != null) npc.DoEnterAnimation();
        }
    }

    private void HandleHighLightNpc()
    {
        StoryInfo.TalkingRoleData data = null;
        foreach (var item in m_theatreStoryCreatureList)
        {
            if (item == null) continue;

            //if (!item.npcInfo)
            //{
            //    item.SetRenderQueue();
            //    continue;
            //}

            if(storyItem.talkingRoles != null)
              data = storyItem.talkingRoles.Find(d => d.roleId == item.creatureId);
            item.SetRenderQueue(data!= null && data.isHighlight ? GeneralConfigInfo.sstoryMaskRenderQueue + 1 : GeneralConfigInfo.sstoryMaskRenderQueue - 1);
        }
    }

    private void HandleNpcName()
    {
        //优先使用替换名
        if (storyItem.needReplaceName)
        {
            m_nameText.text = moduleStory.GetReplaceName();
            return;
        }

        if (storyItem.talkingRoles.Length == 0)
            return;
        
        TheatreStoryCreature storycreature = GetTargetCreature(storyItem.talkingRoles[0].roleId);
        if (storycreature == null) return;

        m_nameText.text = storycreature.GetCreatureName();
    }

    private void HandleHideNpc()
    {
        StoryInfo.TalkingRoleData data = null;
        for (int i = 0; i < storyItem.talkingRoles.Length; i++)
        {
            data = storyItem.talkingRoles[i];
            //隐藏玩家
            if (data.rolePos == StoryConst.THEATRE_NPC_HIDE)
            {
                TheatreStoryCreature npc = GetTargetCreature(data.roleId);
                if (npc != null) npc.theatreCreature.gameObject.SetActive(false);
            }
        }
    }

    private void HandlePlayNpcState()
    {
        StoryInfo.TalkingRoleState data = null;
        for (int i = 0; i < storyItem.talkingRoleStates.Length; i++)
        {
            data = storyItem.talkingRoleStates[i];
            TheatreStoryCreature npc = GetTargetCreature(data.roleId);
            if (npc != null && !string.IsNullOrEmpty(data.state)) npc.PlayAnimaton(data.state);
        }
    }

    #endregion

    #region context functions
    private void HandleDialogBg()
    {
        m_storyDialogCg.alpha = 1;
        m_storyDialogCg.gameObject.SetActive(storyItem == null ? false : !string.IsNullOrEmpty(storyItem.content));
        //Logger.LogDetail("m_storyDialogCg.visible is  {0} content is {1}", m_storyDialogCg.gameObject.activeSelf, storyItem.content);
    }

    protected override void HandleDelayContext()
    {
        base.HandleDelayContext();
        HandleDialogBg();
        NeedChangeContentWidth();
        DoContentTextAnim(m_contentText, moduleStory.GetReplaceContent());
        HandleShowNpc();
        HandleHighLightNpc();
        HandlePlayNpcState();
        HandleNpcName();
        ShowProp();
    }

    private void NeedChangeContentWidth()
    {
        if (string.IsNullOrEmpty(storyItem.playerIconDetail))
        {
            m_contentTextRect.sizeDelta = contentWidthWithoutAvatar;
            m_nameTextRect.sizeDelta = contentWidthWithoutAvatar;
        }
        else
        {
            m_contentTextRect.sizeDelta = contentWidthWithAvatar;
            m_nameTextRect.sizeDelta = contentWidthWithAvatar;
        }
    }

    private void ShowAllContextImme()
    {
        m_ContentTween = null;
        DOTween.Kill(m_contentText, true);
        m_contentText.text = m_contentStrDic.Get(m_contentText);
       // OnContextAnimEnd();
    }

    protected override void OnContextAnimEnd()
    {
        base.OnContextAnimEnd();
       
    }

    protected override void CheckCanChangeToNext()
    {
        base.CheckCanChangeToNext();
        bool isEnded = m_contextStep == EnumContextStep.End;
        m_arrowTrans.gameObject.SetActive(isEnded);
        //强制时间到了之后再处理npc离场
        if(isEnded) HandleNpcLeave();
        CheckAutoNextDialog();
    }

    /// <summary>
    /// 是否忽略当前的answerId
    /// </summary>
    /// <param name="answerId"></param>
    /// <returns></returns>
    protected virtual bool IsIgnoreAnswerId(int answerId)
    {
        bool bIgnore = answerId <= 0;
        return bIgnore;
    }

    private void CheckAutoNextDialog()
    {
       
        if (storyItem == null)
        {
            return;
        }
        
        bool isEnded = m_contextStep == EnumContextStep.End;
        
        //没有配置对话的时候 或者快进完成没有显示道具的时候，自动跳转
        if ((string.IsNullOrEmpty(storyItem.content) || (fastForward && m_propParent != null && !m_propParent.activeSelf)) && isEnded) OnDialogEnd();
    }
    #endregion

    #region display prop
    
    private void ShowProp()
    {
        //Time.timeScale = speed;
        if (storyItem.giveDatas == null || storyItem.giveDatas.Length == 0) return;

        m_propParent.SetActive(true);
        //Time.timeScale = 1;
        moduleStory.RequestProp(storyId,storyItem.giveDatas);
        int showCount = storyItem.giveDatas.Length > MAX_DISPLAY_PROP_COUNT ? MAX_DISPLAY_PROP_COUNT : storyItem.giveDatas.Length;
        CreateProp(showCount);
 
        for (int i = 0; i < m_propItems.Count; i++)
        {
            if(i < showCount) m_propItems[i].RefreshItem(storyItem.giveDatas[i]);
            m_propItems[i].gameObject.SetActive(i < showCount);
        }
    }

    private void CreateProp(int count)
    {
        if (m_propItems.Count > MAX_DISPLAY_PROP_COUNT) return;
        int c = count - m_propItems.Count;
        if (c > 0)
        {
            for (int i = 0; i < c; i++)
            {
                Transform t = m_propParent.transform.AddNewChild(m_propPreafab);
                StoryPropItem item = new StoryPropItem(t);
                m_propItems.Add(item);

            }
        }
    }

    #endregion

    #region create model
    private void CreateModel()
    {
        if(storyItem.models != null && storyItem.models.Length > 0)
        {
            foreach (var item in storyItem.models)
            {
                if (HadLoadedSameNameModel(item.model)) continue;

                var obj = Level.GetPreloadObject<GameObject>(item.model);
                if (!obj)
                {
                    Logger.LogWarning("cannot load story 3d object with name {0} in story [id:{1} index:{2}]",item.model,storyInfo.ID,curStoryItemIndex);
                    continue;
                }

                Util.AddChild(m_npcRoot,obj.transform);
                Util.SetLayer(obj, Layers.GROUND);
                obj.SafeSetActive(false);
                m_modelTrans.Add(obj,item.positionIndex);
            }
        }
    }

    private bool HadLoadedSameNameModel(string name)
    {
        foreach (var item in m_modelTrans)
        {
            if(item.Key && item.Key.name.Equals(name))
            {
                Logger.LogWarning(" load a model with same name :{0}  in story [id:{1} index:{2}]", name, storyInfo.ID, curStoryItemIndex);
                return true;
            }
        }

        return false;
    }

    private void SetModelPos()
    {
        if (!m_modelPointInfo) return;

        foreach (var item in m_modelTrans)
        {
            if (!item.Key) continue;

            var data = m_modelPointInfo.GetDataByIndex(item.Value);
            if (data == null)
            {
                Logger.LogWarning(" the model position with showcreature info [id:{0} index:{1}] is null, in story [id:{2} index:{3}]",
                    StoryConst.THEATRE_CAMERA_POS_POINT_ID,item.Value, storyInfo.ID, curStoryItemIndex);
                continue;
            }

            item.Key.SafeSetActive(true);
            var trans = item.Key.transform;
            trans.localPosition = data.data[0].pos;
            trans.localEulerAngles = data.data[0].rotation;
        }
    }

    private void DestoryModels()
    {
        foreach (var item in m_modelTrans)
        {
            if (!item.Key) continue;

            Destroy(item.Key);
        }
        m_modelTrans.Clear();
    }
    #endregion

    #region refresh ui

    private void SetDefalutBg()
    {
        if (m_contentBg)
        {
            //UIDynamicImage.LoadImage(m_contentBg.transform, StoryConst.DEFALUT_CONTENT_BG);
            SetStoryBg(StoryConst.DEFALUT_CONTENT_BG);
        }
    }
    private void SetStoryBg(string bg)
    {
        UIDynamicImage.LoadImage(m_contentBg.transform, bg);
    }

    private void SetStoryContextTexture()
    {
        string creatureIcon = storyItem.playerIconDetail;
        if (storyItem.playerIcon.Equals("PlayerIcon"))
        {
            creatureIcon = Util.Format("ui_character_sword_emoji_{0}", Module_Player.instance.proto);
        }

        m_playerImg.SafeSetActive(!string.IsNullOrEmpty(creatureIcon));

        //todo set default content bg
        if (!string.IsNullOrEmpty(storyItem.contentBg) && m_contentBg)
        {
            SetStoryBg(storyItem.contentBg);
        }

        if (!string.IsNullOrEmpty(creatureIcon) && m_playerImg)
        {
            UIDynamicImage.LoadImage(m_playerImg.transform, creatureIcon);
            //Texture2D tex = Level.GetPreloadObject<Texture2D>(storyItem.playerIconDetail, false);
            //if (tex) m_playerImg.texture = tex;
        }
    }

    protected void SetCameraPos()
    {
        if (storyItem == null || storyItem.cameraPosIndex < 0 || !m_cameraPointInfo) return;

        var data = m_cameraPointInfo.GetDataByIndex(storyItem.cameraPosIndex);
        if (data != null && data.data.Length > 0)
        {
            if (m_npcCameraTrans)
            {
                m_npcCameraTrans.localPosition    = data.data[0].pos;
                m_npcCameraTrans.localEulerAngles = data.data[0].rotation;
            }

            if (m_renderCamera)
            {
                var far = data.data[0].size;
                if (!string.IsNullOrEmpty(storyItem.background) && far > 150.0f)
                {
                    Logger.LogError($"Story camera farclip size should not greater than 150.0f when use 2D background, force to 150.0f. Camera config [<b>ID:{m_cameraPointInfo.ID}, index:{storyItem.cameraPosIndex}</b>]");
                    far = 150.0f;
                }

                m_renderCamera.farClipPlane = far;

                if (data.data[0].fov > 0) m_renderCamera.fieldOfView = data.data[0].fov;
            }
        }
    }

    private void SetMaskColor()
    {
        if (m_theatryMaskMat && storyItem != null) m_theatryMaskMat.color = storyItem.maskColor;
    }
    #endregion

    #endregion

    #region close and open

    protected override void OnOpenStory()
    {
        base.OnOpenStory();
        ResetPerStoryItem();
        SetDefalutBg();
        m_npcCameraTrans.SafeSetActive(true);
        m_theatreMask.SafeSetActive(true);

        m_storyDialogCg.SafeSetActive(false);
        m_rolePanel.SafeSetActive(false);

        if (m_npcRoot) m_npcRoot.localPosition = new Vector3(ROOT_POSITION, 0,0);
        SetBackground();
    }

    public override void OnUIFadeInComplete()
    {
        base.OnUIFadeInComplete();
        
        m_rolePanel.SetActive(true);
        SetMaskColor();
    }

    protected override void OnCloseStory()
    {
        base.OnCloseStory();
        if(m_npcCameraTrans) m_npcCameraTrans.gameObject.SetActive(false);
        _CameraShake.CancelShake();
    }
    #endregion

    #region reload config

#if UNITY_EDITOR

    private void OnEditorReloadConfig(Event_ e)
    {
        var config = (string)e.param1;

        if (config == "config_generalconfiginfos")
        {
            if (m_theatryMaskMat) m_theatryMaskMat.renderQueue = GeneralConfigInfo.sstoryMaskRenderQueue;
            if (m_backgroundMaterial) m_backgroundMaterial.renderQueue = GeneralConfigInfo.sstoryMaskRenderQueue + 10;

            UpdateBackgroundQuad();
        }

        if (string.IsNullOrEmpty(tag)) return;

        if (config != "config_showcreatureinfos") return;

        LoadAllShowCreatureInfoData();
        foreach (var item in m_theatreStoryCreatureList)
        {
            if (item == null || item.theatreCreature == null) continue;
            item.SetCreaturePosition();
        }

        SetModelPos();
        SetCameraPos();
    }

#endif
    #endregion
}
