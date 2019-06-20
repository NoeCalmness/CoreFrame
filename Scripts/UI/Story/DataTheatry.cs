using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class DataTheatry : TheatreStory
{
    private bool m_bNeedPostEvents = false;

    private bool m_bNeedPostClosedEvent = false;
    protected override void InitComponent()
    {
        base.InitComponent();
        m_bIsDatingStory = true;
        if (!moduleStory)
            moduleStory = Module_Story.instance;
       
        //register listener 
        Module_NPCDating.instance.AddEventListener(Module_NPCDating.EventAllStoryEnd, OnAllStoryEnd);
        Module_NPCDating.instance.AddEventListener(Module_NPCDating.EventWillDoBehaviour, OnWillDoBehaviour);
        Module_NPCDating.instance.AddEventListener(Module_NPCDating.EventNotifyGMCommond, OnGMCommond);
        CreateGM();
        m_bNeedPostEvents = false;
    }

   
    /// <summary>
    /// called when all story has been done;
    /// </summary>
    /// <param name="e"></param>
    private void OnAllStoryEnd(Event_ e)
    {
        DisableFastForward();
        m_bNeedPostEvents = true;
        DisableFastForwardBtns();

#if UNITY_EDITOR
        OnUpdatePerStoryFail();
#endif
    }

    /// <summary>
    /// on will do other behaviour
    /// </summary>
    /// <param name="e"></param>
    private void OnWillDoBehaviour(Event_ e)
    {
        switch ((DatingEventBehaviourType)e.param1)
        {
            case DatingEventBehaviourType.StartSettlement:
                {
                    if (moduleStory != null && moduleStory.hasNotRenderItem == false)
                    {
                        if (DisableFastForward())
                            return;
                        OnUpdatePerStoryFail();
                        DisableFastForwardBtns();
                    }
                }
                break;
        }

    }

    /// <summary>
    /// on close story
    /// </summary>
    protected override void OnCloseStory()
    {
        base.OnCloseStory();
       
        if (m_bNeedPostEvents && moduleStory != null )
        {
            m_bNeedPostEvents = false;
            moduleStory.DispatchStoryClosed(storyId, storyType);
        }
    }

    protected override void OnOpenStory()
    {
        base.OnOpenStory();
        if (m_npcRoot) m_npcRoot.localPosition = Vector3.zero;
        if (string.IsNullOrEmpty(storyItem.background))
            m_backRender.SafeSetActive(false);
        m_theatreMask.SafeSetActive(false);
        SetCameraPos();
    }

    protected override void SetBackground()
    {
        if (!string.IsNullOrEmpty(storyItem.background))
        {
            var tex = Level.GetPreloadObject<Texture>(storyItem.background, false);
            if (tex)
            {
                m_backRender.sharedMaterial.SetTexture(Shader.PropertyToID("_MainTex"), tex);
                m_backRender.gameObject.SafeSetActive(true);
            }
            else
            {
                Level.PrepareAsset<Texture>(storyItem.background, t =>
                {
                    if (!t || t.name != m_currentBack) return;
                    m_backRender.sharedMaterial.SetTexture(Shader.PropertyToID("_MainTex"), t);
                    m_backRender.gameObject.SetActive(true);
                });
            }
        }
        else
        {
            //@todo current direct use  the last one. do not disable this object
            //m_backRender.SafeSetActive(false);
        }
    }
 

    protected override void OnDestroy()
    {
        base.OnDestroy();
        DisableFastForward();
        //remove listeners
        Module_NPCDating.instance.RemoveEventListener(Module_NPCDating.EventWillDoBehaviour, OnWillDoBehaviour);
        Module_NPCDating.instance.RemoveEventListener(Module_NPCDating.EventAllStoryEnd, OnAllStoryEnd);
        Module_NPCDating.instance.RemoveEventListener(Module_NPCDating.EventNotifyGMCommond, OnGMCommond);
    }

    #region GM
    /// <summary>
    /// 剧情面板的GM工具面板
    /// </summary>
    private GameObject m_theatreGmPanel;
    private Transform m_tfGmInputPanel;
    private InputField m_inputGmInput;
    private GMTheaterInputType m_inputType = GMTheaterInputType.DatingEvent;
    private Transform m_gmTfGmPanel;
    private Text m_gmTextInputTip;
    private int m_gmRanGroupId = 0;
    private int[] m_gmRandomRange;
    private void CreateGM()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        var t = transform.Find(StoryConst.THEATRE_GM_ASSET_NAME);
        if (t) m_theatreGmPanel = t.gameObject;
        else
        {
            m_theatreGmPanel = Level.GetPreloadObject<GameObject>(StoryConst.THEATRE_GM_ASSET_NAME, true);
            Util.AddChild(transform, m_theatreGmPanel.transform);
            m_theatreGmPanel.rectTransform().anchoredPosition = new Vector2(0, 0);
        }

        m_gmTfGmPanel = m_theatreGmPanel.GetComponent<RectTransform>("panel");
        Button btnOpenGm = m_theatreGmPanel.GetComponent<Button>("btnOpenGmPanel");
        Text btnOpenGmText = m_theatreGmPanel.GetComponent<Text>("btnOpenGmPanel/Text");
        btnOpenGm.onClick.AddListener(() =>
        {
            m_gmTfGmPanel.SafeSetActive(!m_gmTfGmPanel.gameObject.activeSelf);
            if (m_gmTfGmPanel.gameObject.activeSelf) btnOpenGmText.text = "隐藏Gm面板";
            else btnOpenGmText.text = "显示Gm面板";
        });

        Button quitDatingScene = m_theatreGmPanel.GetComponent<Button>("panel/quitDatingScene");
        quitDatingScene.onClick.AddListener(()=>Module_NPCDating.instance.GM_DatingCommand(DatingGmType.QuitScene));

        //踢出图书馆
        Button kickoutLib = m_theatreGmPanel.GetComponent<Button>("panel/btnLibWrongAnswer");
        kickoutLib.onClick.AddListener(()=>Module_NPCDating.instance.GM_DatingCommand(DatingGmType.KickOutLib));

        //结束并退出当前约会场景
        Button overDatingScene = m_theatreGmPanel.GetComponent<Button>("panel/overDatingScene");
        overDatingScene.onClick.AddListener(() => Module_NPCDating.instance.GM_DatingCommand(DatingGmType.QuitScene));

        //执行约会事件
        m_tfGmInputPanel = m_theatreGmPanel.GetComponent<RectTransform>("panel/oneInputParamPanel");
        m_inputGmInput = m_theatreGmPanel.GetComponent<InputField>("panel/oneInputParamPanel/input");
        m_gmTextInputTip = m_theatreGmPanel.GetComponent<Text>("panel/oneInputParamPanel/text/Text");
        Button btnForceQuitScene = m_theatreGmPanel.GetComponent<Button>("panel/quitDatingScene");
        btnForceQuitScene.onClick.AddListener(() => Module_NPCDating.instance.GM_DatingCommand(DatingGmType.ForceQuitScene));

        Button btnSure = m_theatreGmPanel.GetComponent<Button>("panel/oneInputParamPanel/sure");
        btnSure.onClick.AddListener(() =>
        {
            switch (m_inputType)
            {
                case GMTheaterInputType.SetDiviResult:
                    int[] dResultId = GMGetInputText(m_inputGmInput);
                    if(dResultId.Length > 0 && dResultId[0] > 0) Module_NPCDating.instance.GM_DatingCommand(DatingGmType.SetDivinationResult,dResultId[0]);
                    else Module_NPCDating.instance.GM_DatingCommand(DatingGmType.SetDivinationResult, -1);
                    m_gmTfGmPanel.SafeSetActive(false);
                    break;
                case GMTheaterInputType.SetRandom:
                    int[] dRandomId = GMGetInputText(m_inputGmInput);
                    Module_NPCDating.instance.GM_DatingCommand(DatingGmType.SetRandomValue, m_gmRanGroupId,dRandomId[0]);
                    m_gmTfGmPanel.SafeSetActive(false);
                    break;
                default:
                    break;
            }
            m_tfGmInputPanel.SafeSetActive(false);
        });
        Button btnUnSure = m_theatreGmPanel.GetComponent<Button>("panel/oneInputParamPanel/unSure");
        btnUnSure.onClick.AddListener(() =>
        {
            switch (m_inputType)
            {
                case GMTheaterInputType.SetDiviResult:
                    Module_NPCDating.instance.GM_DatingCommand(DatingGmType.SetDivinationResult, -1);
                    break;
                case GMTheaterInputType.SetRandom:
                    Module_NPCDating.instance.GM_DatingCommand(DatingGmType.SetRandomValue, -1, -1);
                    break;
                default:
                    break;
            }
            m_gmTfGmPanel.SafeSetActive(false);
            m_tfGmInputPanel.SafeSetActive(false);
        });
        m_theatreGmPanel.SafeSetActive(!Root.simulateReleaseMode && (storyType == EnumStoryType.NpcTheatreStory || storyType == EnumStoryType.None));

        EventManager.AddEventListener("EditorSimulateReleaseMode", () => m_theatreGmPanel.SafeSetActive(!Root.simulateReleaseMode));
#endif
    }

    private int[] GMGetInputText(InputField input)
    {
        string[] strp = new string[] { input.text };
        if (input.text.Contains(",")) strp = input.text.Split(',');
        else if (input.text.Contains(";")) strp = input.text.Split(';');

        int[] dEventIds = new int[strp.Length];
        for (int i = 0; i < strp.Length; i++)
        {
            var eventId = Util.Parse<int>(strp.GetValue<string>(i));
            dEventIds[i] = eventId;
        }
        return dEventIds;
    }

    public enum GMTheaterInputType
    {
        DatingEvent,
        SetDiviResult,
        SetRandom,
    }

    private void OnGMCommond(Event_ e)
    {
        DatingGmType t = (DatingGmType)e.param1;
        m_gmTfGmPanel.SafeSetActive(true);
        m_tfGmInputPanel.SafeSetActive(true);
        if (t == DatingGmType.SetRandomValue)
        {
            m_inputType = GMTheaterInputType.SetRandom;

            m_gmRanGroupId = (int)e.param2;
            m_gmRandomRange = (int[])e.param3;
            Util.SetText(m_gmTextInputTip, "输入随机数({0}--{1})", m_gmRandomRange[0], m_gmRandomRange[1] - 1);
        }
        else if (t == DatingGmType.SetDivinationResult)
        {
            m_inputType = GMTheaterInputType.SetDiviResult;
            Util.SetText(m_gmTextInputTip, "占卜结果（球:1-4;签:5-9）");
        }

    }
    #endregion

}