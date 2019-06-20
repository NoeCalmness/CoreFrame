using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

#region custom story class
public class DelayStoryFunc
{
    public EnumStroyCheckFunc funcType;
    public float delayTime;
    public bool isHandle;
    public string paramers;

    public DelayStoryFunc()
    {

    }

    public DelayStoryFunc(EnumStroyCheckFunc type, float delay)
    {
        funcType = type;
        delayTime = delay;
    }

    public void Reset()
    {
        funcType = EnumStroyCheckFunc.None;
        delayTime = 0f;
        isHandle = false;
    }
}
#endregion

public class BaseStory : MonoBehaviour
{
    #region animator filed
    private CanvasGroup m_baseDialogCg;
    private Tween m_baseDialogTween;
    #endregion

    #region protected field
    //内容缓存字典(可能出现需要替换掉{0}{1}的内容，所以将其缓存)
    protected Dictionary<Text, string> m_contentStrDic = new Dictionary<Text, string>();
    //检测延迟功能的列表
    protected List<DelayStoryFunc> m_delayFuncList = new List<DelayStoryFunc>();
    //强制事件是否到达
    protected bool m_isForceTimeEnd;
    //当前是否可以跳转到下一段对话 
    protected EnumContextStep m_contextStep;
    //保存的逻辑层 
    protected Module_Story moduleStory;
    protected Module_PVE modulePVE;
    protected Module_Team moduleTeam;
    //上一次播放的语音
    protected string m_lastPlayVoiceName;
    //特效节点
    protected RectTransform m_effectNode;
    //上一次播放的背景音乐
    protected string m_lastPlayMusic;
    protected bool m_lastMusicLoop;
    protected bool m_playDefalutMusic;
    protected float m_bgmVolume;
    /// <summary>
    /// 是否是处于动画状态
    /// </summary>
    protected bool m_tweenPlaying { get; set; } = false;

    /// <summary>
    /// 快进按钮
    /// </summary>
    protected Button m_fastForwardBtn;
    protected Button m_normalBtn;
    /// <summary>
    /// 跳过剧情按钮
    /// </summary>
    protected Button m_skipStroyBtn;
    /// <summary>
    /// 是否在快进
    /// </summary>
    protected bool fastForward { get; set; } = false;
    /// <summary>
    /// 当前播放速度
    /// </summary>
    protected float speed { get { return fastForward ? GeneralConfigInfo.sstorySpeed : 1f; } } 
    /// <summary>
    /// 是否处于联机模式
    /// </summary>
    protected bool isTeamMode { get { return modulePVE.isTeamMode; } }

    #region server data

    private EnumContextStep m_lastSendStep = EnumContextStep.Wait;
    #endregion

    #endregion

    #region public field
    public StoryInfo storyInfo { get { return moduleStory.currentStory; } }

    public int storyId { get { return storyInfo ? storyInfo.ID : -1; } }

    public EnumStoryType storyType { get { return moduleStory.currentStoryType; } }
    public bool canUseFastForward { get { return storyType == EnumStoryType.TheatreStory || storyType == EnumStoryType.PauseBattleStory || storyType == EnumStoryType.NpcTheatreStory; } }
    /// <summary>
    /// 设置是否可以使用跳过剧情按钮
    /// </summary>
    public bool canUseSkipStoryBtn { get { return storyType== EnumStoryType.TheatreStory|| storyType == EnumStoryType.PauseBattleStory|| storyType == EnumStoryType.FreeBattleStory; } }
    public int curStoryItemIndex
    {
        get { return moduleStory.currentStoryIndex; }
        protected set { moduleStory.currentStoryIndex = value; }
    }

    public StoryInfo.StoryItem storyItem { get { return moduleStory.currentStoryItem; } }

    protected Tweener m_ContentTween = null;
    #endregion

    protected virtual void Awake()
    {
        InitStoryComponent();
    }

    protected virtual void OnEnable()
    {
        ResetPerStoryItem();
    }

    protected virtual void OnDisable()
    {
       // fastForward = false;
        ObjectManager.timeScale = speed;
        SetFastForwardBtnVisible(false);
        SetSkipStoryBtnVisible(false);
    }

    // Use this for initialization
    protected virtual void Start ()
    {

	}
	
	// Update is called once per frame
	protected virtual void Update ()
    {
        UpdateCheckFunction();
    }

    protected virtual void OnDestroy()
    {

    }

    #region public functions

    /// <summary>
    /// 显示对话
    /// </summary>
    /// <param name="type"></param>
    /// <param name="plotId"></param>
    public void ShowDialog(int plotId, EnumStoryType type)
    {
        InitData();
        SetFastForwardBtnVisible(fastForward);
        m_playDefalutMusic = true;
        OnOpenStory();
    }

    #endregion

    #region protected function

    #region init
    protected virtual void InitComponent(){ }

    protected virtual void AddEvent() { }

    protected void InitData()
    {
        curStoryItemIndex = 0;
        m_contentStrDic.Clear();
        m_lastPlayMusic = string.Empty;
        m_tweenPlaying = false;    
        ObjectManager.timeScale = speed;
        ResetPerStoryItem();
        _CameraShake.CancelShake();
        SetSkipStoryBtnVisible(true);
    }

    #endregion

    #region update per story item

    /// <summary>
    /// 每次刷新对话前的还原操作
    /// </summary>
    protected virtual void ResetPerStoryItem()
    {
        //每一次一段对话刷新的时候，需要重置功能的检测时间
        ResetCheckFuncDic();

        m_isForceTimeEnd = false;
        m_contextStep = EnumContextStep.Wait;
        m_lastSendStep = EnumContextStep.Wait; 

        if (!string.IsNullOrEmpty(m_lastPlayVoiceName))
        {
            AudioManager.Stop(m_lastPlayVoiceName);
            m_lastPlayVoiceName = string.Empty;
        }

        //child class need recovery the model state
    }

    /// <summary>
    ///  更新一次对话显示
    /// </summary>
    protected void UpdatePerStoryItem()
    {
        //当前没有对话了
        if (storyItem == null)
        {
            OnUpdatePerStoryFail();
            return;
        }

        ResetPerStoryItem();
        OnUpdatePerStorySuccess();
    }

    protected virtual void OnUpdatePerStoryFail()
    {

        OnCloseStory();
        if (moduleStory != null)
            moduleStory.DispatchStoryEnd(storyId, storyType);

    }

    protected virtual void OnUpdatePerStorySuccess()
    {
        //背景音乐播放检查
        PlayBgMusic();
        //UI特效
        PlayUIEffect();

        //剧场对话（子类）需要单独检查背景显示，黑屏显示
        //战斗剧情（子类）需要单独处理头像显示，镜头移动，转身目标锁定
        //剧场对话（子类）与战斗剧情（子类）需要分别处理说话的对象以及对象需要播放的状态
        //剧场对话（子类）与战斗剧情（子类）的Npc名字显示都不受到delaytime的影响 
    }

    /// <summary>
    /// 子类决定在什么时候调用开始检测
    /// </summary>
    protected void StartCheckWhenItemRefresh()
    {
        CheckCameraShake();
        CheckSoundEffect();
        CheckDelayContext();
        CheckForceContext();
    }
    #endregion

    #region contenxt functions
   
    protected void DoContentTextAnim(Text component,string endText)
    {
        if (!m_contentStrDic.ContainsKey(component))
            m_contentStrDic.Add(component, string.Empty);
        m_contentStrDic[component] = endText;

        if (string.IsNullOrEmpty(endText))
        {
            OnContextAnimEnd();
            return;
        }

        //语音播放
        //if (!string.IsNullOrEmpty(storyItem.voiceName)) AudioManager.PlayVoice(storyItem.voiceName);

        float duration = endText.Length * GeneralConfigInfo.scontextInterval / (float)ObjectManager.timeScale;
        if(duration <= 0.03f)
        {
            Util.SetText(component,endText);
            OnContextAnimEnd();
            return;
        }
    
        if (m_ContentTween != null)
        {
            m_ContentTween.ChangeValues("", endText, duration);
        }
        else
        {
            m_ContentTween = component.DOText(endText, duration).SetEase(Ease.Linear).OnComplete(OnContextAnimEnd).OnRewind(OnContentAnimRewind).SetAutoKill(false);
        }
    }

    protected virtual void OnContentAnimRewind()
    {
        if (m_ContentTween != null) m_ContentTween.Restart();
    }
    protected virtual void OnContextAnimEnd()
    {
        m_contextStep = EnumContextStep.OnlyShowEnded;
        CheckCanChangeToNext();
    }

    /// <summary>
    /// 文字动画真正的结束,若真正结束，则打开箭头显示,此时才可以跳转到下一句
    /// 打开箭头需满足条件1.强制时间到达，2.文字蹦字结束
    /// 特别的，当首次点击文字时，文字会直接全部显示完成
    /// </summary>
    protected virtual void CheckCanChangeToNext()
    {
        if (m_contextStep == EnumContextStep.OnlyShowEnded && m_isForceTimeEnd) m_contextStep = EnumContextStep.End;
    }

    protected void ChangeToNextStoryItem()
    {
        //这里需要准备未加载完的资源
        if(GeneralConfigInfo.sstoryPreLoadNum > 0)
        {
            Level.PrepareAssets(Module_Story.GetPerStoryPreAssets(storyId, storyType, curStoryItemIndex),(t)=>
            {
                if (!t)
                {
                    Logger.LogError("动态加载Story:{0}资源 index{1} 未成功！！",storyId, GeneralConfigInfo.sstoryPreLoadNum+curStoryItemIndex);
                    return;
                }
            });
        }
        curStoryItemIndex++;
        UpdatePerStoryItem();
    }

    #endregion

    #region delay function

    protected void HandleCameraShake()
    {
        if (storyItem.cameraShake.shakeId <= 0)
            return;

        CameraShakeInfo shakeInfo = ConfigManager.Get<CameraShakeInfo>(storyItem.cameraShake.shakeId);
        if(shakeInfo == null)
        {
            Logger.LogError("storyInfo id = {0} ,index = {1},CameraShakeInfoId = {2} connot be finded!!!", storyId,curStoryItemIndex, storyItem.cameraShake.shakeId);
            return;
        }
        _CameraShake.Shake(shakeInfo);
    }

    protected void HandleSoundEffect(string soundEffect)
    {
        if(string.IsNullOrEmpty(soundEffect))
        {
            Logger.LogError("storyInfo id = {0} ,index = {1},sound_effect_name is String.Empty!!!", storyId, curStoryItemIndex);
            return;
        }

        AudioManager.PlayAudio(soundEffect);
    }

    //延迟显示文字
    protected virtual void HandleDelayContext()
    {
        //Logger.LogInfo("handle delay text");
        m_contextStep = EnumContextStep.Show;
    }

    protected virtual void HandleDelayPlayState()
    {

    }

    protected void HandleForceContext()
    {
        m_isForceTimeEnd = true;
        CheckCanChangeToNext();        
    }
    #endregion
    
    #region open and close

    protected virtual void OnOpenStory()
    {
        gameObject.SetActive(true);
        if (m_baseDialogCg)
        {
            m_baseDialogTween?.Kill();
            m_baseDialogCg.alpha = 0f;
            m_tweenPlaying = true;

            m_baseDialogTween = DOTween.To(() => m_baseDialogCg.alpha, x => m_baseDialogCg.alpha = x, 1f, StoryConst.BASE_DIALOG_ALPHA_DURACTION).OnComplete(() =>
            {
                m_tweenPlaying = false;
                m_baseDialogCg.alpha = 1f;
                if (moduleStory && storyInfo) moduleStory.DispatchStoryMaskEnd(storyId, storyType);
                OnUIFadeInComplete();
            });
        }
        
        if (moduleStory && storyInfo) moduleStory.DispatchStoryStart(storyId, storyType);
    }

    /// <summary>
    /// 动画事件(进入)
    /// </summary>
    public virtual void OnUIFadeInComplete()
    {
        UpdatePerStoryItem();
    }

    protected virtual void OnCloseStory()
    {
        //reset time scale
        fastForward = false;
        ObjectManager.timeScale = speed;

        if (m_baseDialogCg)
        {
            m_tweenPlaying = true;
            m_baseDialogTween?.Kill();
            m_baseDialogTween = DOTween.To(() => m_baseDialogCg.alpha, x => m_baseDialogCg.alpha = x, 0f, StoryConst.BASE_DIALOG_ALPHA_DURACTION).OnComplete(() =>
            {
                m_baseDialogCg.alpha = 0f;
                m_tweenPlaying = false;
                OnUIFadeOutComplete();
            });
        }
        if(m_ContentTween != null)
        {
            m_ContentTween.Kill(false);
            m_ContentTween = null;
        }
    }

    /// <summary>
    /// 动画事件(结束)
    /// </summary>
    public void OnUIFadeOutComplete()
    {
        gameObject.SetActive(false);
    }

    #endregion

    #region fast forward

    protected bool DisableFastForward()
    {
        if(fastForward)
        {
            SwitchFastForward(null);
            return true;
        }

        return false;
    }

    protected void DisableFastForwardBtns()
    {
        m_fastForwardBtn.SafeSetActive(false);
        m_normalBtn.SafeSetActive(false);
    }
    
    protected virtual void SwitchFastForward(GameObject sender)
    {
        fastForward = !fastForward;
        ObjectManager.timeScale = speed;
        SetFastForwardBtnVisible(fastForward);
    }

    protected void SetFastForwardBtnVisible(bool fastForward)
    {
        m_fastForwardBtn.SafeSetActive(!fastForward && canUseFastForward && !isTeamMode);
        m_normalBtn.SafeSetActive(fastForward && canUseFastForward && !isTeamMode);
    }

    protected void SetSkipStoryBtnVisible(bool bVisible)
    {
        m_skipStroyBtn.SafeSetActive(bVisible&&canUseSkipStoryBtn);
    }

    #endregion
    #endregion

    #region private functions

    private void InitStoryComponent()
    {
        moduleStory = Module_Story.instance;
        modulePVE = Module_PVE.instance;
        moduleTeam = Module_Team.instance;
        //UIManager.worldCamera.GetComponentDefault<_CameraShake>();
        m_baseDialogCg = GetComponent<CanvasGroup>();
        InitEffectNode();
        InitComponent();
        AddEvent();
    }

    private void InitEffectNode()
    {
        var t = transform.Find("effct_node");
        if (t) m_effectNode = t.rectTransform();
        if (!m_effectNode) m_effectNode = transform.AddUINodeStrech("effect_node");
        m_effectNode.Strech();
    }

    #region audio

    private void PlayBgMusic()
    {
        if(storyItem.musicData.validMusic)
        {
            string musicName = storyItem.musicData.musicName;

            if (musicName == StoryConst.STOP_CURRENT_MUSIC_FLAG)
            {
                if (!string.IsNullOrEmpty(m_lastPlayMusic)) AudioManager.Stop(m_lastPlayMusic);
                else m_bgmVolume = Level.current.audioHelper.globalVolume;

                if (Level.current.audioHelper) Level.current.audioHelper.SetGlobalVolume(0, StoryConst.BGM_CROSS_FADR_DURACTION);
            }
            else if (musicName == StoryConst.RESET_CURRENT_MUSIC_FLAG && Level.current.audioHelper)
            {
                if (!string.IsNullOrEmpty(m_lastPlayMusic)) AudioManager.Stop(m_lastPlayMusic);
                Level.current.audioHelper.SetGlobalVolume(m_bgmVolume, StoryConst.BGM_CROSS_FADR_DURACTION);
                m_playDefalutMusic = true;
            }
            else if (musicName == StoryConst.RESET_LAST_MUSIC_FLAG)
            {
                if (!string.IsNullOrEmpty(m_lastPlayMusic))
                {
                    if (m_playDefalutMusic) m_bgmVolume = Level.current.audioHelper.globalVolume;
                    m_playDefalutMusic = false;

                    AudioManager.PlayMusic(m_lastPlayMusic, m_lastMusicLoop, (holder) =>
                    {
                        if (holder) holder.globalBgmVolume = -1f;
                    }, null, true, StoryConst.BGM_CROSS_FADR_DURACTION);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(m_lastPlayMusic)) AudioManager.Stop(m_lastPlayMusic);
                else m_bgmVolume = Level.current.audioHelper.globalVolume;//默认第一次切换音乐的时候，记录背景音乐的值

                AudioManager.PlayMusic(musicName, storyItem.musicData.loopMusic,(holder)=> {
                    if (holder) holder.globalBgmVolume = -1f;
                },null,true, StoryConst.BGM_CROSS_FADR_DURACTION);
                m_lastPlayMusic = musicName;
                m_lastMusicLoop = storyItem.musicData.loopMusic;
                m_playDefalutMusic = false;
            }
        }
    }
    
    private void HandleDelayVoice()
    {//剧情快进情况下停止播放音频
        if (!string.IsNullOrEmpty(storyItem.voiceName)&& !fastForward)
        {
            m_lastPlayVoiceName = storyItem.voiceName;
            AudioManager.PlayVoice(m_lastPlayVoiceName);
        }
    }
    #endregion

    #region function update check

    private void CheckCameraShake()
    {
        if(storyItem.cameraShake != null) AddCheckFunction(EnumStroyCheckFunc.CameraShake,storyItem.cameraShake.delayTime);
    }

    private void CheckSoundEffect()
    {
        if (storyItem.soundEffect == null || storyItem.soundEffect.Length == 0)
            return;

        StoryInfo.StorySoundEffect se = null;
        for (int i = 0; i < storyItem.soundEffect.Length; i++)
        {
            se = storyItem.soundEffect[i];
            AddCheckFunction(EnumStroyCheckFunc.SoundEffect, se.delayTime, se.soundName);
        }
    }

    private void CheckDelayContext()
    {
        AddCheckFunction(EnumStroyCheckFunc.ContentDelay, storyItem.contentDelayTime);
    }

    private void CheckForceContext()
    {
        AddCheckFunction(EnumStroyCheckFunc.ContentForce, storyItem.forceTime);
    }

    private void ResetCheckFuncDic()
    {
        m_delayFuncList.Clear();
    }

    private void AddCheckFunction(EnumStroyCheckFunc func,float delayTime,string paramers = "")
    {
        DelayStoryFunc delayFunc = new DelayStoryFunc(func,delayTime * 0.001f);
        delayFunc.paramers = paramers;

        if (delayTime <= 0) HandleDelayFunc(delayFunc);
        else m_delayFuncList.Add(delayFunc);
    }

    private void UpdateCheckFunction()
    {
        if (m_delayFuncList == null || m_delayFuncList.Count == 0)
            return;

        DelayStoryFunc df = null;
        for (int i = 0; i < m_delayFuncList.Count; i++)
        {
            df = m_delayFuncList[i];
            if (df.isHandle || df.delayTime <= 0f)
                continue;

            df.delayTime -= Time.deltaTime;
            if(df.delayTime <= 0f)
            {
                df.isHandle = true;
                HandleDelayFunc(df);
            }
        }
    }

    private void HandleDelayFunc(DelayStoryFunc func)
    {
        switch (func.funcType)
        {
            case EnumStroyCheckFunc.CameraShake:
                HandleCameraShake();
                break;
            case EnumStroyCheckFunc.SoundEffect:
                HandleSoundEffect(func.paramers);
                break;
            case EnumStroyCheckFunc.ContentDelay:
                HandleDelayContext();
                HandleDelayPlayState();
                HandleDelayVoice();
                break;
            case EnumStroyCheckFunc.ContentForce:
                HandleForceContext();
                break;
        }
    }

    #endregion
    
    #region ui effect

    private void PlayUIEffect()
    {
        if (storyItem.canPlayEffect)
        {
            //todo how to play particle system?
            GameObject effect = Level.GetPreloadObject<GameObject>(storyItem.effect);

            if (!effect)
            {
                //Logger.LogError("effect name : {0} is null ,cannot be played!",storyItem.effect);
                Level.PrepareAsset<GameObject>(storyItem.effect, (e)=>
                {
                    var o = Object.Instantiate(e) as GameObject;
                    PlayUIEffect(o);
                });
            }
            else
            {
                PlayUIEffect(effect);
            }

        }
    }

    private void PlayUIEffect(GameObject go)
    {
        go.transform.SetParent(m_effectNode);
        go.transform.Strech();
        IUIAnimationComplete t = go.GetComponent<IUIAnimationComplete>();
        if (t != null) t.onComplete = () => { Destroy(go); };
        OnPlayUIEffect(go);
    }
    protected virtual void OnPlayUIEffect(GameObject obj)
    {

    }

    #endregion

    #endregion  

    #region server functions
    
    public void SendStoryStepToServer(EnumContextStep step)
    {
        if (!storyInfo || m_lastSendStep == step) return;

        //只有队长能发送消息
        if (Level.current is Level_PveTeam && (Level.current as Level_PveTeam).selfIsleader)
        {
            m_lastSendStep = step;
            moduleTeam.SendStoryStep(storyInfo.ID,curStoryItemIndex, step);
        }
    }

    public void RecvFrameData(int id, int index, EnumContextStep changeToStep)
    {
        Logger.LogDetail("recv frame story data is [id: {0} index : {1} step :{2}]", id, index, changeToStep);
        if (!storyInfo)
        {
            Logger.LogWarning("current stoty is null,please check out.");
            return;
        }

        //索引对不上的时候，强行刷新当前的对话
        if (curStoryItemIndex != index)
        {
            curStoryItemIndex = index;
            UpdatePerStoryItem();
        }

        OnRecvFrameData(changeToStep);
    }

    protected virtual void OnRecvFrameData(EnumContextStep changeToStep)
    {
        m_contextStep = changeToStep;
    }

    #endregion
}
