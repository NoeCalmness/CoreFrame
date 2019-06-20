/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Setting Helper Class
 * Used to manage Audio/Graphic settings
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-12-26
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.PostProcessing;
using System.Collections;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

public enum PresetTypes
{
    Low      = 0,
    Medium   = 1,
    High     = 2,
    Fantasic = 3,
    Custom   = 4,
}

public struct AudioSettings
{
    private const string CACHE_NAME = "cache_settings_audio";

    /// <summary>
    /// Get cached audio settings from cache settings
    /// </summary>
    /// <returns></returns>
    public static AudioSettings ReadFromCache()
    {
        var cache = PlayerPrefs.GetInt(CACHE_NAME, -1);

        var s = new AudioSettings();
        s.FromInt(cache);

        return s;
    }

    /// <summary>
    /// Global volume
    /// </summary>
    public float volume;
    /// <summary>
    /// Bgm volume
    /// </summary>
    public float bgmVolume;
    /// <summary>
    /// Voice volume
    /// </summary>
    public float voiceVolume;
    /// <summary>
    /// Sound effect volume
    /// </summary>
    public float soundVolume;

    public int ToInt()
    {
        return ((int)(volume * 100) & 0xFF) | ((int)(bgmVolume * 100) & 0xFF) << 8 | ((int)(voiceVolume * 100) & 0xFF) << 16 | ((int)(soundVolume * 100) & 0xFF) << 24;
    }

    public void FromInt(int config)
    {
        if (config >= 0)
        {
            volume      = (config & 0xFF) * 0.01f;
            bgmVolume   = (config >> 8 & 0xFF) * 0.01f;
            voiceVolume = (config >> 16 & 0xFF) * 0.01f;
            soundVolume = (config >> 24 & 0xFF) * 0.01f;
        }
        else
        {
            volume      = GeneralConfigInfo.svolume;
            bgmVolume   = GeneralConfigInfo.sbgmVolume;
            voiceVolume = GeneralConfigInfo.svoiceVolume;
            soundVolume = GeneralConfigInfo.ssoundVolume;
        }
    }

    public void WriteToCache()
    {
        PlayerPrefs.SetInt(CACHE_NAME, ToInt());
    }

    public bool EqualsTo(AudioSettings other)
    {
        return ToInt() == other.ToInt();
    }

    public override string ToString()
    {
        return Util.Format("volume: {0:F2}, bgmVolume: {1:F2}, voiceVolume: {2:F2}, soundVolume: {3:F2}", volume, bgmVolume, voiceVolume, soundVolume);
    }

    public string ToString(bool color)
    {
        if (!color) return ToString();
        return Util.Format("<color=#00DDFF>volume: <color=#00FF00><b>{0}</b></color></color>, <color=#00DDFF>bgmVolume: <color=#00FF00><b>{1}</b></color></color>," +
            " <color=#00DDFF>voiceVolume: <color=#00FF00><b>{2}</b></color></color>, <color=#00DDFF>soundVolume: <color=#00FF00><b>{3}</b></color></color>",
            volume, bgmVolume, voiceVolume, soundVolume);
    }
}

public struct VideoSettings
{
    private const string CACHE_NAME = "cache_settings_video";

    public static readonly VideoSettings low      = new VideoSettings() { type = PresetTypes.Low,      FPS = 30, MSAA = 0, effectLevel = 0, postEffect = false, HDR = false, HD = false, DOF = false };
    public static readonly VideoSettings medium   = new VideoSettings() { type = PresetTypes.Medium,   FPS = 30, MSAA = 2, effectLevel = 1, postEffect = true,  HDR = true,  HD = false, DOF = false };
    public static readonly VideoSettings high     = new VideoSettings() { type = PresetTypes.High,     FPS = 30, MSAA = 4, effectLevel = 2, postEffect = true,  HDR = true,  HD = false, DOF = true  };
    public static readonly VideoSettings fantasic = new VideoSettings() { type = PresetTypes.Fantasic, FPS = 60, MSAA = 4, effectLevel = 2, postEffect = true,  HDR = true,  HD = true,  DOF = true  };

    /// <summary>
    /// Get cached video settings from cache settings
    /// </summary>
    /// <returns></returns>
    public static VideoSettings ReadFromCache()
    {
        var cache = PlayerPrefs.GetInt(CACHE_NAME, SettingsManager.recommend.ToInt());

        var s = new VideoSettings();
        s.FromInt(cache);

        return s;
    }

    public static VideoSettings GetFromLevel(string level, VideoSettings current)
    {
        var s = level == "0" ? low : level == "1" ? medium : level == "2" ? high : level == "3" ? fantasic : current;
        s.notch = current.notch;
        return s;
    }

    public PresetTypes type;
    public int FPS;
    public int MSAA;
    public int effectLevel;
    public bool postEffect;
    public bool HDR;
    public bool HD;
    public bool DOF;
    public bool notch;

    public int ToInt()
    {
        return FPS & 0xFF | (MSAA & 0xF) << 8 | (effectLevel & 0x03) << 12 | (postEffect ? 1 : 0) << 14 | (HDR ? 1 : 0) << 15 | (HD ? 1 : 0) << 16 | (DOF ? 1 : 0) << 17 | (notch ? 1 : 0) << 18;
    }

    public void FromInt(int config)
    {
        FPS         = config & 0xFF;
        MSAA        = config >> 8 & 0xF;
        effectLevel = config >> 12 & 0x03;
        postEffect  = (config >> 14 & 0x01) == 1;
        HDR         = (config >> 15 & 0x01) == 1;
        HD          = (config >> 16 & 0x01) == 1;
        DOF         = (config >> 17 & 0x01) == 1;
        notch       = (config >> 18 & 0x01) == 1;

        Check();
    }

    public void WriteToCache()
    {
        PlayerPrefs.SetInt(CACHE_NAME, ToInt());
    }

    public bool EqualsTo(VideoSettings other, bool ignoreNotch = false)
    {
        return !ignoreNotch ? ToInt() == other.ToInt() : ToInt().BitMask(18, false) == other.ToInt().BitMask(18, false);
    }

    public void Check()
    {
        if (FPS % 10 != 0) FPS = FPS / 10 * 10;

        FPS         = Mathf.Clamp(FPS, 30, 60); 
        MSAA        = Mathf.Clamp(MSAA, 0, 8);
        effectLevel = Mathf.Clamp(effectLevel, 0, 3);

        type = EqualsTo(low, true) ? PresetTypes.Low : EqualsTo(medium, true) ? PresetTypes.Medium : EqualsTo(high, true) ? PresetTypes.High : EqualsTo(fantasic, true) ? PresetTypes.Fantasic : PresetTypes.Custom;
    }

    public override string ToString()
    {
        return Util.Format("type: {0}, FPS: {1}, MSAA: {2}, effectLevel: {3}, postEffect: {4}, HDR: {5}, HD: {6}, DOF: {7}, notch: {8}", type, FPS, MSAA, effectLevel, postEffect, HDR, HD, DOF, notch);
    }

    public string ToString(bool color)
    {
        if (!color) return ToString();
        return Util.Format("<color=#00DDFF>type: <color=#00FF00><b>{0}</b></color></color>, <color=#00DDFF>FPS: <color=#00FF00><b>{1}</b></color></color>," +
            " <color=#00DDFF>MSAA: <color=#00FF00><b>{2}</b></color></color>, <color=#00DDFF>effecLevel: <color=#00FF00><b>{3}</b></color></color>, <color=#00DDFF>postEffect: <color=#00FF00><b>{4}</b></color></color>, <color=#00DDFF>HDR: <color=#00FF00><b>{5}</b></color></color>, <color=#00DDFF>HD: <color=#00FF00><b>{6}</b></color></color>, <color=#00DDFF>DOF: <color=#00FF00><b>{7}</b></color></color>, <color=#00DDFF>notch: <color=#00FF00><b>{8}</b></color></color>",
            type, FPS, MSAA, effectLevel, postEffect, HDR, HD, DOF, notch);
    }
}

public struct InputSettings
{
    private const string CACHE_NAME = "cache_settings_input";

    /// <summary>
    /// Get cached input settings from cache settings
    /// </summary>
    /// <returns></returns>
    public static InputSettings ReadFromCache()
    {
        var cache = PlayerPrefs.GetInt(CACHE_NAME, -1);

        var s = new InputSettings();
        s.FromInt(cache);

        return s;
    }

    /// <summary>
    /// Movement input type
    /// </summary>
    public int movementType;
    /// <summary>
    /// Touch sensitivity
    /// </summary>
    public float touchSensitivity;

    public int ToInt()
    {
        return (movementType & 0x0F) | ((int)touchSensitivity & 0xFF) << 4;
    }

    public void FromInt(int config)
    {
        if (config >= 0)
        {
            movementType     = config & 0x0F;
            touchSensitivity = config >> 4 & 0xFF;
        }
        else
        {
            movementType     = CombatConfig.sdefaultMovementType;
            touchSensitivity = CombatConfig.sdefaultTouchSensitivity;
        }
    }

    public void WriteToCache()
    {
        PlayerPrefs.SetInt(CACHE_NAME, ToInt());
    }

    public bool EqualsTo(InputSettings other)
    {
        return ToInt() == other.ToInt();
    }

    public override string ToString()
    {
        return Util.Format("movementType: {0}, touchSensitivity: {1:F2}", movementType, touchSensitivity);
    }

    public string ToString(bool color)
    {
        if (!color) return ToString();
        return Util.Format("<color=#00DDFF>movementType: <color=#00FF00><b>{0}</b></color></color>, <color=#00DDFF>touchSensitivity: <color=#00FF00><b>{1}</b></color></color>", movementType, touchSensitivity);
    }
}

/// <summary>
/// Manage all in game Audio and Graphics settings
/// </summary>
[AddComponentMenu("HYLR/Utilities/Settings Manager")]
public class SettingsManager : SingletonBehaviour<SettingsManager>
{
    #region Static functions

    #region Presets

    #region Video

    public static VideoSettings current
    {
        get { return m_current; }
        set
        {
            value.Check();

            if (m_current.EqualsTo(value)) return;
            m_current = value;
            m_current.WriteToCache();

            UpdateCurrent();
        }
    }
    public static VideoSettings recommend { get; private set; }

    private static VideoSettings m_current = VideoSettings.medium;

    public static void UpdateCurrent(bool force = false)
    {
        instance._UpdateCurrent(force);
    }

    #endregion

    #region Audio

    public static AudioSettings currentAudio
    {
        get { return m_currentAudio; }
        set
        {
            if (m_currentAudio.EqualsTo(value)) return;
            m_currentAudio = value;
            m_currentAudio.WriteToCache();

            UpdateCurrentAudio();
        }
    }
    public static AudioSettings recommendAudio { get; private set; }

    private static AudioSettings m_currentAudio = new AudioSettings() { bgmVolume = 1, soundVolume = 1, voiceVolume = 1, volume = 1 };

    public static void UpdateCurrentAudio()
    {
        instance.m_volume      = m_currentAudio.volume;
        instance.m_bgmVolume   = m_currentAudio.bgmVolume;
        instance.m_voiceVolume = m_currentAudio.voiceVolume;
        instance.m_soundVolume = m_currentAudio.soundVolume;

        instance._UpdateAudioSettings();
    }

    #endregion

    #region Input

    public static InputSettings currentInput
    {
        get { return m_currentInput; }
        set
        {
            if (m_currentInput.EqualsTo(value)) return;
            m_currentInput = value;
            m_currentInput.WriteToCache();

            UpdateCurrentInput();
        }
    }
    public static InputSettings recommendInput { get; private set; }

    private static InputSettings m_currentInput = new InputSettings() { movementType = 0, touchSensitivity = 1.0f };

    public static void UpdateCurrentInput()
    {
        instance.m_movementType     = m_currentInput.movementType;
        instance.m_touchSensitivity = m_currentInput.touchSensitivity;

        instance._UpdateInputSettings();
    }

    #endregion

    #endregion

    private static bool m_initialized = false;

    /// <summary>
    /// Initialize Settings Manager
    /// </summary>
    public static void Initialize()
    {
        if (m_initialized) return;
        m_initialized = true;
    }

    /// <summary>
    /// Initialize audio settings (after config manager initialized)
    /// </summary>
    public static void LoadAudioSettings()
    {
        if (!SystemInfo.supportsAudio)
            Logger.LogWarning("Current device does not support audio...");

        var r = new AudioSettings();
        r.FromInt(-1);
        recommendAudio = r;

        m_currentAudio = AudioSettings.ReadFromCache();

        Logger.LogInfo("Def audio settings: [{0}]", recommendAudio.ToString(true));
        Logger.LogInfo("Cur audio settings: [{0}]", m_currentAudio.ToString(true));

        UpdateCurrentAudio();
    }

    /// <summary>
    /// Initialize input settings (after config manager initialized)
    /// </summary>
    public static void LoadInputSettings()
    {
        var r = new InputSettings();
        r.FromInt(-1);
        recommendInput = r;

        m_currentInput = InputSettings.ReadFromCache();

        Logger.LogInfo("Def input settings: [{0}]", recommendInput.ToString(true));
        Logger.LogInfo("Cur input settings: [{0}]", m_currentInput.ToString(true));

        UpdateCurrentInput();
    }

    /// <summary>
    /// Check current device hardware environment and requirements before we start game (Game.started)
    /// </summary>
    /// <returns>if passed return true</returns>
    public static void CheckHardwareEnvironment()
    {
        instance._CheckHardwareEnvironment();
    }

    #region Video settings

    /// <summary>
    /// Limit max FPS ?
    /// </summary>
    public static bool limitFPS { get { return instance._limitFPS; } set { instance._limitFPS = value; } }
    /// <summary>
    /// Enable or disable HDR
    /// </summary>
    public static bool enableHDR { get { return instance._enableHDR; } set { instance._enableHDR = value; } }
    /// <summary>
    /// Enable or disable post effect
    /// </summary>
    public static bool enablePosEffect { get { return instance._enablePosEffect; } set { instance._enablePosEffect = value; } }
    /// <summary>
    /// Enable or disable bloom effect
    /// </summary>
    public static bool enableBloom { get { return instance._enableBloom; } set { instance._enableBloom = value; } }
    /// <summary>
    /// Enable or disable color grading effect
    /// </summary>
    public static bool enableColorGrading { get { return instance._enableColorGrading; } set { instance._enableColorGrading = value; } }
    /// <summary>
    /// Enable or disable user lut effect
    /// </summary>
    public static bool enableLut { get { return instance._enableLut; } set { instance._enableLut = value; } }
    /// <summary>
    /// Enable or disable depth of field effect
    /// </summary>
    public static bool enableDOF { get { return instance._enableDOF; } set { instance._enableDOF = value; } }
    /// <summary>
    /// Enable or disable HD mode
    /// HD mode use the highest supported resolution of current device, otherwise we choose a lower supported resolution
    /// </summary>
    public static bool HD { get { return instance._HD; } set { instance._HD = value; } }
    /// <summary>
    /// Enable or disable notch screen mode
    /// </summary>
    public static bool notch { get { return instance._notch; } set { instance._notch = value; } }
    /// <summary>
    /// Set msaa level
    /// </summary>
    public static int msaaLevel { get { return instance._msaaLevel; } set { instance._msaaLevel = value; } }
    /// <summary>
    /// Set effect level
    /// </summary>
    public static int effectLevel { get { return instance._effectLevel; } set { instance._effectLevel = value; } }
    /// <summary>
    /// Set FPS limitation
    /// </summary>
    public static int FPS { get { return instance._FPS; } set { instance._FPS = value; } }

    #endregion

    #region Audio settings

    /// <summary>
    /// Current global volume
    /// Note the real audio volume is calculated by each audio setting
    /// e.g: current real bgm music volume = bgmVolume * volume
    /// </summary>
    public static float volume { get { return instance._volume; } set { instance._volume = value; } }
    /// <summary>
    /// Current bgm volume
    /// </summary>
    public static float bgmVolume { get { return instance._bgmVolume; } set { instance._bgmVolume = value; } }
    /// <summary>
    /// Current voice volume
    /// </summary>
    public static float voiceVolume { get { return instance._voiceVolume; } set { instance._voiceVolume = value; } }
    /// <summary>
    /// Current sound effect volume
    /// </summary>
    public static float soundVolume { get { return instance._soundVolume; } set { instance._soundVolume = value; } }

    /// <summary>
    /// Current calculated global volume
    /// </summary>
    public static float realVolume { get { return instance._realVolume; } }
    /// <summary>
    /// Current calculated bgm volume
    /// </summary>
    public static float realBgmVolume { get { return instance._realBgmVolume; } }
    /// <summary>
    /// Current calculated voice volume
    /// </summary>
    public static float realVoiceVolume { get { return instance._realVoiceVolume; } }
    /// <summary>
    /// Current calculated sound effect volume
    /// </summary>
    public static float realSoundVolume { get { return instance._realSoundVolume; } }

    #endregion

    #region Input settings

    /// <summary>
    /// Current movement input type
    /// </summary>
    public static int movementType { get { return instance._movementType; } set { instance._movementType = value; } }
    /// <summary>
    /// Current touch input sensitivity
    /// </summary>
    public static float touchSensitivity { get { return instance._touchSensitivity; } set { instance._touchSensitivity = value; } }

    #endregion

    #region Public interface

    /// <summary>
    /// Update all setting to current main camera
    /// </summary>
    public static void UpdateSettings()
    {
        instance._UpdateVideoSettings();
    }

    /// <summary>
    /// Update all setting to audio system
    /// </summary>
    public static void UpdateAudioSettings()
    {
        instance._UpdateAudioSettings();
    }

    /// <summary>
    /// Update all setting to input system
    /// </summary>
    public static void UpdateInputSettings()
    {
        instance._UpdateInputSettings();
    }

    /// <summary>
    /// Set resolution
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="fullscreen"></param>
    public static void SetResolution(int width, int height, bool fullscreen = true)
    {
        instance._SetResolution(width, height, fullscreen);
    }

    /// <summary>
    /// Set ressolution with custom refresh rate
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="refreshrate"></param>
    /// <param name="fullscreen"></param>
    public static void SetResolution(int width, int height, int refreshrate, bool fullscreen = true)
    {
        instance._SetResolution(width, height, refreshrate, fullscreen);
    }

    /// <summary>
    /// Apply current videosettings to target camera
    /// </summary>
    /// <param name="camera"></param>
    public static void ApplyVideoSettings(Camera camera)
    {
        if (!camera || !instance) return;

        camera.allowHDR  = instance.m_supportHDR && instance.m_enableHDR;
        camera.allowMSAA = instance.m_msaaLevel > 0;

        var post = camera.GetComponent<PostProcessingBehaviour>();
        var postLow = camera.GetComponent<FastMobileBloomDOF>();
        var lutLow = camera.GetComponent<ColorCorrectionLookup>();

        if (post)    post.enabled    = false;
        if (postLow) postLow.enabled = false;
        if (lutLow)  lutLow.enabled  = false;

        if (instance.m_supportPostEffect && instance.m_enablePosEffect)
        {
            if (camera.allowHDR || recommend.type != PresetTypes.Low)
            {
                if (!post) return;

                post.enabled = true;
                var p = post.profile;

                p.bloom.enabled        = camera.allowHDR;
                p.bloomMedium.enabled  = !p.bloom.enabled;

                p.colorGrading.enabled = instance.m_enableColorGrading && p.bloom.enabled;
                p.userLut.enabled      = instance.m_enableLut && p.bloomMedium.enabled;
            }
            else
            {
                if (postLow) postLow.enabled = true;
                if (lutLow)  lutLow.enabled  = true;
            }
        }
    }

    #endregion

    #endregion

    #region Hardware level support

    /// <summary>
    /// Current device actually support HDR (pass hdr test) ?
    /// </summary>
    public bool supportHDR { get { return m_supportHDR; } }
    /// <summary>
    /// Current device support post effect ?
    /// </summary>
    public bool supportPostEffect { get { return m_supportPostEffect; } }
    /// <summary>
    /// Current device supported hdr render texture format
    /// </summary>
    public RenderTextureFormat HDRFormat { get { return m_HDRFormat; } }
    /// <summary>
    /// Current device aspect ratio
    /// </summary>
    public float deviceAspectRatio { get { return m_deviceAspectRatio; } }
    /// <summary>
    /// The original hardware resolution
    /// </summary>
    public Vector3Int OriginResolution { get { return m_originResolution; } }
    /// <summary>
    /// The highest hardware support resolution
    /// </summary>
    public Vector3Int HDResolution { get { return m_HDResolution; } }
    /// <summary>
    /// The lower hardware support resolution
    /// </summary>
    public Vector3Int LDResolution { get { return m_LDResolution; } }

    [Header("Hardware Support")]
    [SerializeField]
    private bool m_supportHDR;
    [SerializeField]
    private bool m_supportPostEffect;
    [SerializeField]
    private RenderTextureFormat m_HDRFormat;
    [SerializeField]
    private float m_deviceAspectRatio;
    [SerializeField]
    private Vector3Int m_HDResolution;
    [SerializeField]
    private Vector3Int m_originResolution;
    [SerializeField]
    private Vector3Int m_LDResolution;

    private void _CheckHardwareEnvironment()
    {
        if (Game.started) return;

        Logger.LogInfo("Checking Hardware Environment...");

        var cpu = SystemInfo.processorType;
        if (cpu.IndexOf('@') > -1) cpu = cpu.Substring(0, cpu.IndexOf('@'));

        Logger.LogDetail("Device Info: <color=#00DDFF>Device: [{0}-{1}], Type: {2}, OS: {3}, OS Family: {4}</color>", SystemInfo.deviceModel, SystemInfo.deviceName, SystemInfo.deviceType, SystemInfo.operatingSystem, SystemInfo.operatingSystemFamily);
        Logger.LogDetail("CPU Info: <color=#00DDFF>{0} @ {1:F2}Ghz x {2} processors</color>", cpu, SystemInfo.processorFrequency * 0.001f, SystemInfo.processorCount);
        Logger.LogDetail("Graphics Device Info: <color=#00DDFF>Vendor: {0}, Device: {1}, Actived tier: {2}, ColorGamut: {3}</color>", SystemInfo.graphicsDeviceVendor, SystemInfo.graphicsDeviceName, Graphics.activeTier, Graphics.activeColorGamut);
        Logger.LogDetail("Graphics API: <color=#00DDFF>{0}, Version: {1}, Multi Thread Rendering: {2}, Shader Level: {3}</color>", SystemInfo.graphicsDeviceType, SystemInfo.graphicsDeviceVersion, SystemInfo.graphicsMultiThreaded, SystemInfo.graphicsShaderLevel);
        Logger.LogDetail("Memory Info: <color=#00DDFF>RAM: {0} mb, SGRAM: {1} mb</color>", SystemInfo.systemMemorySize, SystemInfo.graphicsMemorySize);

        m_supportPostEffect = SystemInfo.supportsImageEffects;

        _CheckRenderTextureSupport();
        _CheckHDRSupport();
        _CheckRecommendSettings();
        _CheckResolutionSupport();

        var s = recommend;
        s.notch = Root.hasNotch;
        recommend = s;

        Logger.LogInfo("Checking Hardware Environment Complete, <color=#00DDFF>HDR: <color={0}><b>{1}</b></color>, Post Effect: <color={2}><b>{3}</b></color>, HDR Format: <color={4}><b>{5}</b></color>, Final Tier: <color=#00FF00><b>{6}</b></color></color>",
            m_supportHDR ? "#00FF00" : "#FF0000", m_supportHDR ? "passed" : "failed",
            m_supportPostEffect ? "#00FF00" : "#FF0000", m_supportPostEffect ? "passed" : "failed",
            m_HDRFormat < 0 ? "#FF0000" : "#00FF00", m_HDRFormat < 0 ? "None" : m_HDRFormat.ToString(), Graphics.activeTier);

        m_current = VideoSettings.ReadFromCache();
        m_current.Check();

        Logger.LogInfo("Rec video settings: [{0}]", recommend.ToString(true));
        Logger.LogInfo("Cur video settings: [{0}]", m_current.ToString(true));

        UpdateCurrent(true);
    }

    private void _CheckResolutionSupport()
    {
        var rs = Screen.resolutions;
        if (rs.Length < 1) rs = new Resolution[] { Screen.currentResolution };  // If failed, use current resolution

        int mw = 0, mh = 0, mr = 0;
        foreach (var r in rs)
        {
            if (r.width >= mw && r.height >= mh)
            {
                mw = r.width;
                mh = r.height;
                mr = r.refreshRate;
            }
        }

        m_originResolution.Set(mw, mh, mr);
        m_HDResolution.Set(mw, mh, mr);
        m_deviceAspectRatio = (float)mw / mh;

        int lw = mw * 2 / 3, dd = mw;
        mw = mh = mr = 0;
        float ad = float.MaxValue;
        foreach (var r in rs)
        {
            var d = Mathf.Abs(r.width - lw);
            var a = Mathf.Abs((float)r.width / r.height - m_deviceAspectRatio);
            if (d <= dd && a <= ad)
            {
                dd = d;
                ad = a;

                mw = r.width;
                mh = r.height;
                mr = r.refreshRate;
            }
        }
        m_LDResolution.Set(mw, mh, mr);

        if (mw <= m_HDResolution.x * 0.3f) m_LDResolution = m_HDResolution;

        var fix = m_HDResolution.y > 720 ? 2 / 3.0 : 4 / 5.0;

        if (m_LDResolution.x == m_HDResolution.x)
        {
            mw = (int)(m_HDResolution.x * fix);
            mh = (int)(mw / m_deviceAspectRatio);

            if (mw % 2 != 0) mw += 1;
            if (mh % 2 != 0) mh += 1;

            m_LDResolution.Set(mw, mh, m_LDResolution.z);
        }

        #if !UNITY_EDITOR && !HIGH_RESOLUTION_MODE
        if (m_HDResolution.y > 810)
        {
            m_HDResolution.x = (int)(m_HDResolution.x * 0.75);
            m_HDResolution.y = (int)(m_HDResolution.y * 0.75);

            if (recommend.type < PresetTypes.High)
            {
                m_LDResolution.x = (int)(m_LDResolution.x * 0.75);
                m_LDResolution.y = (int)(m_LDResolution.y * 0.75);
            }
        }
        #endif

        Logger.LogDetail("Resolution Info: <color=#00DDFF>AspectRatio: <color=#00FF00><b>[{0:F2}]</b></color>, Original: <color=#00FF00><b>[{7:F0}x{8:F0}@{9:F0}Hz]</b></color>, HD mode: <color=#00FF00><b>[{1:F0}x{2:F0}@{3:F0}Hz]</b></color>, LD mode: <color=#00FF00><b>[{4:F0}x{5:F0}@{6:F0}Hz]</b></color></color>",
            m_deviceAspectRatio.ToString("F2"), m_HDResolution.x, m_HDResolution.y, m_HDResolution.z, m_LDResolution.x, m_LDResolution.y, m_LDResolution.z, m_originResolution.x, m_originResolution.y, m_originResolution.z);
    }

    private void _CheckRenderTextureSupport()
    {
        var hdr = -1;
        var ss = "";
        for (var i = 0; i < (int)RenderTextureFormat.BGR101010_XR; ++i)
        {
            if (i == 21) continue;  // 21 is unused
            var fmt = (RenderTextureFormat)i;
            var passed = SystemInfo.SupportsRenderTextureFormat(fmt);
            if (!passed) ss += fmt + ", ";

            if (/*fmt == RenderTextureFormat.RGB111110Float || */fmt == RenderTextureFormat.ARGBHalf)  // We always use ARGBHalf now
            {
                Logger.LogInfo("HDR Render Format [<color=#00DDFF><b>{0}</b></color>] is [<color={1}><b>{2}</b></color>]", fmt, passed ? "#00FF00" : "#FF0000", passed ? "Supported" : "Unsupported");
                if (passed) hdr = i;
            }
        }

        if (!string.IsNullOrEmpty(ss))
            Logger.LogDetail("Unsupported RenderTexture Format: [<color=#FF0000><b>{0}</b></color>]", ss.Substring(0, ss.Length - 2));

        if (Graphics.activeTier == GraphicsTier.Tier1)   // Always disable hdr and post effect on old devices
        {
            Logger.LogInfo("Disable hdr and post effect support on old devices...");

            hdr = -1;
            m_supportPostEffect = false;
        }

        m_HDRFormat = (RenderTextureFormat)hdr;
        m_supportHDR = hdr > -1;
    }

    private void _CheckHDRSupport()
    {
        if (!m_supportHDR) return;

        var o = GameObject.Find("hdrTest");
        var c = o ? o.GetComponent<Camera>() : null;
        if (!c || !c.targetTexture)
        {
            Logger.LogError("SettingsManager::CheckHDRSupport: Missing or invalid test camera, HDR support is disabled...");
            m_supportHDR = false;
            return;
        }

        c.enabled = true;

        c.clearFlags = CameraClearFlags.Color;
        c.backgroundColor = Color.red;
        c.cullingMask = 0;

        m_supportHDR = _CheckHDRSupportWithTier(c, GraphicsTier.Tier2);

        c.enabled = false;

        if (!m_supportHDR) Graphics.activeTier = GraphicsTier.Tier1;
    }

    private bool _CheckHDRSupportWithTier(Camera c, GraphicsTier tier)
    {
        Graphics.activeTier = tier;

        c.Render();

        var t = c.targetTexture;
        var tt = new Texture2D(t.width, t.height);

        var ot = RenderTexture.active;
        RenderTexture.active = t;

        tt.ReadPixels(new Rect(0, 0, t.width, t.height), 0, 0);
        tt.Apply();

        RenderTexture.active = ot;

        m_supportHDR = tt.GetPixel(1, 1) == Color.red;

        if (m_supportHDR)
            m_HDRFormat = tier == GraphicsTier.Tier3 ? RenderTextureFormat.RGB111110Float : RenderTextureFormat.ARGBHalf;

        DestroyImmediate(tt);

        return m_supportHDR;
    }

    #region CPU info detection

    #if UNITY_EDITOR
    private void _CheckRecommendSettings()
    {
        recommend = VideoSettings.fantasic;
    }
    #elif UNITY_ANDROID
    private void _CheckRecommendSettings()
    {
        var cpuInfo = SDKManager.CallAliya<string>("GetCPUInfo");
        if (string.IsNullOrEmpty(cpuInfo)) cpuInfo = "unknow";

        cpuInfo = cpuInfo.Replace("\t", "");

        Logger.LogDetail("CPU Hardware: <color=#00DDFF>{0}</color>", cpuInfo);

        if (!m_supportPostEffect || !m_supportHDR) // 不支持后处理/HDR 无视其他条件以低端设备处理
        {
            recommend = SystemInfo.systemMemorySize <= 4000 ? VideoSettings.medium : VideoSettings.medium;
            return;
        }

        // 优先以 CPU 型号作为指标
        var reg = new System.Text.RegularExpressions.Regex(@"((?:SDM)|(?:MSM))(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);   // 高通
        var m = reg.Match(cpuInfo);
        if (m.Success)
        {
            var s = m.Groups[1].Value.ToLower();
            var v = Util.Parse<int>(m.Groups[2].Value);
            recommend = s == "sdm" ? v >= 660 ? VideoSettings.high : v >= 630 ? VideoSettings.medium : VideoSettings.medium : // SDM series
                v >= 8996 ? VideoSettings.high : v >= 8953 ? VideoSettings.medium : VideoSettings.medium;                     // MSM series

            return;
        }

        reg = new System.Text.RegularExpressions.Regex(@"((?:kirin)|(?:hi))\s*(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);   // 麒麟
        m = reg.Match(cpuInfo);
        if (m.Success)
        {
            var s = m.Groups[1].Value.ToLower();
            var v = Util.Parse<int>(m.Groups[2].Value);
            recommend = s == "kirin" && v >= 970 ? VideoSettings.high : VideoSettings.medium;

            return;
        }

        reg = new System.Text.RegularExpressions.Regex(@"universal\s*(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);   // 三星
        m = reg.Match(cpuInfo);
        if (m.Success)
        {
            var v = Util.Parse<int>(m.Groups[1].Value);
            recommend = v >= 8895 ? VideoSettings.high : v >= 7420 ? VideoSettings.medium : VideoSettings.medium;

            return;
        }

        reg = new System.Text.RegularExpressions.Regex(@"MT(?:\s|-)*(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);   // 联发科
        m = reg.Match(cpuInfo);
        if (m.Success)
        {
            var v = Util.Parse<int>(m.Groups[1].Value);
            recommend = v == 6799 || v == 6771 ? VideoSettings.medium : VideoSettings.medium;

            return;
        }

        #region GPU check

        //var gpu = SystemInfo.graphicsDeviceName;
        //reg = new Regex(@"Adreno\s*(?:\(\s*TM\s*\))?\s*(\d+)", RegexOptions.IgnoreCase);
        //m = reg.Match(gpu);
        //if (m.Success) // Qualcomm Adreno Gpus
        //{
        //    var v = Util.Parse<int>(m.Groups[1].Value);
        //    recommend = v >= 530 ? VideoSettings.high : v >= 508 ? VideoSettings.high : v >= 505 ? VideoSettings.medium : VideoSettings.low;

        //    return;
        //}

        //reg = new Regex(@"Mali-(T|G)(\d+)", RegexOptions.IgnoreCase);
        //m = reg.Match(gpu);
        //if (m.Success) // Arm Mali GPUs
        //{
        //    var v = Util.Parse<int>(m.Groups[1].Value);
        //    recommend = m.Groups[1].Value == "T" ? v >= 820 ? VideoSettings.high : v >= 760 ? VideoSettings.high : v >= 720 ? VideoSettings.medium : VideoSettings.low : // Mali-T series
        //        v >= 71 ? VideoSettings.fantasic : VideoSettings.high; // Mali-G series

        //    return;
        //}

        //// 除 Adreno 和 Mali-G/T 系列以外的所有 GPU 根据内存，显存，CPU 核心和频率适配
        //if (SystemInfo.systemMemorySize <= 2048)   // 内存低于 2G
        //{
        //    recommend = VideoSettings.low;
        //    return;
        //}

        //if (SystemInfo.processorCount < 2)         // 单处理器
        //{
        //    recommend = VideoSettings.low;
        //    return;
        //}

        //if (SystemInfo.graphicsMemorySize <= 1024)  // 显存低于 1G
        //{
        //    recommend = VideoSettings.medium;
        //    return;
        //}

        //if (SystemInfo.processorFrequency <= 2500) // 处理器频率低于 2.5 Ghz
        //{
        //    recommend = VideoSettings.medium;
        //    return;
        //}

        //if (SystemInfo.processorCount < 4)         // 处理器数量低于 4 个
        //{
        //    recommend = VideoSettings.medium;
        //    return;
        //}

        #endregion

        recommend = VideoSettings.high;
    }
    #elif UNITY_IOS
    private void _CheckRecommendSettings()
    {
        var device = Device.generation;
        Logger.LogDetail("IOS Device Generation: <color=#00DDFF>{0}</color>", device);

        if (device < DeviceGeneration.iPadMini2Gen)
        {
            recommend = VideoSettings.medium;
            return;
        }

        switch (device)
        {
            case DeviceGeneration.iPadMini2Gen:
            case DeviceGeneration.iPhone5S:
            case DeviceGeneration.iPhone6:
            case DeviceGeneration.iPhone6Plus: recommend = VideoSettings.medium; return;
            default: break;
        }

        recommend = VideoSettings.high;
    }
    #endif

    #endregion

    #endregion

    #region Runtime settings

    #region Video settings

    /// <summary>
    /// Limit max FPS ?
    /// </summary>
    public bool _limitFPS
    {
        get { return m_limitFPS; }
        set
        {
            if (m_limitFPS == value) return;
            m_limitFPS = value;

            Application.targetFrameRate = m_FPS <= 30 ? 30 : recommend.type < PresetTypes.High ? 30 : m_limitFPS ? 40 : m_FPS;  // Limit fps on mobile devices
        }
    }
    /// <summary>
    /// Enable or disable HDR
    /// </summary>
    public bool _enableHDR
    {
        get { return m_enableHDR; }
        set
        {
            if (m_enableHDR == value) return;
            m_enableHDR = value;

            m_current.HDR = m_enableHDR;
            m_current.WriteToCache();

            _UpdateVideoSettings();
        }
    }
    /// <summary>
    /// Enable or disable post effect
    /// </summary>
    public bool _enablePosEffect
    {
        get { return m_enablePosEffect; }
        set
        {
            if (m_enablePosEffect == value) return;
            m_enablePosEffect = value;

            m_current.postEffect = m_enablePosEffect;
            m_current.WriteToCache();

            _UpdateVideoSettings();
        }
    }
    /// <summary>
    /// Enable or disable bloom effect
    /// </summary>
    public bool _enableBloom
    {
        get { return m_enableBloom; }
        set
        {
            if (m_enableBloom == value) return;
            m_enableBloom = value;

            _UpdateVideoSettings();
        }
    }
    /// <summary>
    /// Enable or disable color grading effect
    /// </summary>
    public bool _enableColorGrading
    {
        get { return m_enableColorGrading; }
        set
        {
            if (m_enableColorGrading == value) return;
            m_enableColorGrading = value;

            _UpdateVideoSettings();
        }
    }
    /// <summary>
    /// Enable or disable user lut effect
    /// </summary>
    public bool _enableLut
    {
        get { return m_enableLut; }
        set
        {
            if (m_enableLut == value) return;
            m_enableLut = value;

            _UpdateVideoSettings();
        }
    }
    /// <summary>
    /// Enable or disable depth of field effect
    /// </summary>
    public bool _enableDOF
    {
        get { return m_enableDOF; }
        set
        {
            if (m_enableDOF == value) return;
            m_enableDOF = value;

            m_current.DOF = m_enableDOF;
            m_current.WriteToCache();

            _UpdateVideoSettings();
        }
    }
    /// <summary>
    /// Enable or disable HD mode
    /// HD mode use the highest supported resolution of current device, otherwise we choose a lower supported resolution
    /// </summary>
    public bool _HD
    {
        get { return m_HD; }
        set
        {
            if (m_HD == value) return;
            m_HD = value;

            m_current.HD = m_HD;
            m_current.WriteToCache();

            _UpdateCurrent(true);
        }
    }
    /// <summary>
    /// Enable or disable notch screen mode
    /// </summary>
    public bool _notch
    {
        get { return m_notch; }
        set
        {
            if (m_notch == value) return;
            m_notch = value;

            m_current.notch = m_notch;
            m_current.WriteToCache();

            _UpdateCurrent(true);
        }
    }
    /// <summary>
    /// Set msaa level
    /// </summary>
    public int _msaaLevel
    {
        get { return m_msaaLevel; }
        set
        {
            if (m_msaaLevel == value) return;
            m_msaaLevel = value;

            if      (m_msaaLevel <= 0) m_msaaLevel = 0;
            else if (m_msaaLevel <  4) m_msaaLevel = 2;
            else if (m_msaaLevel <  8) m_msaaLevel = 4;
            else                       m_msaaLevel = 8;

            m_current.MSAA = m_msaaLevel;
            m_current.WriteToCache();

            _UpdateVideoSettings();
        }
    }
    /// <summary>
    /// Set effect level
    /// </summary>
    public int _effectLevel
    {
        get { return m_effectLevel; }
        set
        {
            if (m_effectLevel == value) return;
            m_effectLevel = value;
            
            m_current.effectLevel = m_effectLevel;
            m_current.WriteToCache();

            _UpdateVideoSettings();
        }
    }
    /// <summary>
    /// Set FPS limitation
    /// </summary>
    public int _FPS
    {
        get { return m_FPS; }
        set
        {
            value = Mathf.Clamp(value, 30, 80);
            if (m_FPS == value) return;
            m_FPS = value;

            m_current.FPS = m_FPS;
            m_current.WriteToCache();

            _UpdateVideoSettings();
        }
    }

    [Header("Video Settings"), Space(10)]
    [SerializeField, Set("_limitFPS")]
    private bool m_limitFPS = true;
    [SerializeField, Set("_enableHDR")]
    private bool m_enableHDR = true;
    [SerializeField, Set("_enablePosEffect")]
    private bool m_enablePosEffect = true;
    [SerializeField, Set("_enableBloom")]
    private bool m_enableBloom = true;
    [SerializeField, Set("_enableColorGrading")]
    private bool m_enableColorGrading = true;
    [SerializeField, Set("_enableLut")]
    private bool m_enableLut = true;
    [SerializeField, Set("_enableDoF")]
    private bool m_enableDOF = true;
    [SerializeField, Set("_HD")]
    private bool m_HD = true;
    [SerializeField, Set("_notch")]
    private bool m_notch = true;
    [SerializeField, Set("_msaaLevel")]
    private int m_msaaLevel = 2;
    [SerializeField, Set("_effectLevel")]
    private int m_effectLevel = 1;
    [SerializeField, Set("_FPS")]
    private int m_FPS = 60;

    #endregion

    #region Audio settings

    /// <summary>
    /// Current global volume
    /// Note the real audio volume is calculated by each audio setting
    /// e.g: current real bgm music volume = _bgmVolume * _volume
    /// </summary>
    public float _volume
    {
        get { return m_volume; }
        set
        {
            if (m_volume == value) return;
            m_volume = value;

            _UpdateAudioSettings();
        }
    }
    /// <summary>
    /// Current bgm volume
    /// </summary>
    public float _bgmVolume
    {
        get { return m_bgmVolume; }
        set
        {
            if (m_bgmVolume == value) return;
            m_bgmVolume = value;

            _UpdateAudioSettings();
        }
    }
    /// <summary>
    /// Current voice volume
    /// </summary>
    public float _voiceVolume
    {
        get { return m_voiceVolume; }
        set
        {
            if (m_voiceVolume == value) return;
            m_voiceVolume = value;

            _UpdateAudioSettings();
        }
    }
    /// <summary>
    /// Current sound effect volume
    /// </summary>
    public float _soundVolume
    {
        get { return m_soundVolume; }
        set
        {
            if (m_soundVolume == value) return;
            m_soundVolume = value;

            _UpdateAudioSettings();
        }
    }

    /// <summary>
    /// Current calculated global volume
    /// </summary>
    public float _realVolume
    {
        get { return m_realVolume; }
    }
    /// <summary>
    /// Current calculated bgm volume
    /// </summary>
    public float _realBgmVolume
    {
        get { return m_realBgmVolume; }
    }
    /// <summary>
    /// Current calculated voice volume
    /// </summary>
    public float _realVoiceVolume
    {
        get { return m_realVoiceVolume; }
    }
    /// <summary>
    /// Current calculated sound effect volume
    /// </summary>
    public float _realSoundVolume
    {
        get { return m_realSoundVolume; }
    }

    [Header("Audio Settings"), Space(10)]
    [SerializeField, _Range(0, 1.0f, "_volume")]
    private float m_volume = 1.0f;
    [SerializeField, _Range(0, 1.0f, "_bgmVolume")]
    private float m_bgmVolume = 1.0f;
    [SerializeField, _Range(0, 1.0f, "_voiceVolume")]
    private float m_voiceVolume = 1.0f;
    [SerializeField, _Range(0, 1.0f, "_soundVolume")]
    private float m_soundVolume = 1.0f;

    private float m_realVolume      = 1.0f;
    private float m_realBgmVolume   = 1.0f;
    private float m_realVoiceVolume = 1.0f;
    private float m_realSoundVolume = 1.0f;

    #endregion

    #region Input settings

    /// <summary>
    /// Current touch input sensitivity
    /// </summary>
    public int _movementType
    {
        get { return m_movementType; }
        set
        {
            if (m_movementType == value) return;
            m_movementType = value;

            _UpdateInputSettings();
        }
    }
    /// <summary>
    /// Current touch input sensitivity
    /// </summary>
    public float _touchSensitivity
    {
        get { return m_touchSensitivity; }
        set
        {
            if (m_touchSensitivity == value) return;
            m_touchSensitivity = value;

            _UpdateInputSettings();
        }
    }

    [Header("Input Settings"), Space(10)]
    [SerializeField, Set("_movementType")]
    private int m_movementType = 0;
    [SerializeField, Set("_touchSensitivity")]
    private float m_touchSensitivity = 75.0f;

    #endregion

    private PostProcessingBehaviour m_post;
    private FastMobileBloomDOF m_postLow;
    private ColorCorrectionLookup m_lutLow;
    private Camera m_mainCamera;
    private Coroutine m_changeResolution;

    /// <summary>
    /// Update all setting to current main camera
    /// </summary>
    private void _UpdateVideoSettings()
    {
        _UpdateVideoSettings(!Level.current || !Level.current.isBattle);
    }

    /// <summary>
    /// Update all setting to current main camera
    /// </summary>
    /// <param name="canUseDOF">Can use depth of field effect ?</param>
    private void _UpdateVideoSettings(bool canUseDOF = true)
    {
        QualitySettings.antiAliasing = m_msaaLevel;
        Application.targetFrameRate  = m_FPS <= 30 ? 30 : recommend.type < PresetTypes.High ? 30 : m_limitFPS ? 40 : m_FPS;  // Limit fps on mobile devices

        if (!CheckCamera()) return;
        
        m_mainCamera.allowHDR  = m_supportHDR && m_enableHDR;
        m_mainCamera.allowMSAA = m_msaaLevel > 0;

        m_post.enabled    = false;
        m_postLow.enabled = false;
        m_lutLow.enabled  = false;

        if (m_supportPostEffect && m_enablePosEffect)
        {
            m_post.enabled = m_mainCamera.allowHDR || recommend.type != PresetTypes.Low;

            if (m_post.enabled)
            {
                var p = m_post.profile;

                p.bloom.enabled        = m_enableBloom && m_mainCamera.allowHDR;
                p.bloomMedium.enabled  = m_enableBloom && !p.bloom.enabled;

                p.colorGrading.enabled = m_enableColorGrading && p.bloom.enabled;
                p.userLut.enabled      = m_enableLut && p.bloomMedium.enabled;

                p.depthOfField.enabled = canUseDOF && m_enableDOF;

                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    bloomSettings = p.bloom.enabled ? p.bloom.settings.bloom : p.bloomMedium.settings.bloom;
                    dofSettings = p.depthOfField.settings;
                #endif
            }
            else
            {
                m_postLow.enabled = true;
                m_lutLow.enabled  = true;
            }
        }

        DispatchEvent(Events.VEDIO_SETTINGS_CHANGED);

        Logger.LogDetail("Video settings update");
    }

    /// <summary>
    /// Update all setting to audio system
    /// </summary>
    public void _UpdateAudioSettings()
    {
        m_realVolume      = m_volume      * GeneralConfigInfo.smaxVolume;
        m_realBgmVolume   = m_bgmVolume   * GeneralConfigInfo.smaxBgmVolume;
        m_realVoiceVolume = m_voiceVolume * GeneralConfigInfo.smaxVoiceVolume;
        m_realSoundVolume = m_soundVolume * GeneralConfigInfo.smaxSoundVolume;

        DispatchEvent(Events.AUDIO_SETTINGS_CHANGED);
    }

    /// <summary>
    /// Update all setting to input system
    /// </summary>
    public void _UpdateInputSettings()
    {
        DispatchEvent(Events.INPUT_SETTINGS_CHANGED);
    }

    /// <summary>
    /// Set resolution
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="fullscreen"></param>
    public void _SetResolution(int width, int height, bool fullscreen = true)
    {
        _SetResolution(width, height, -1, fullscreen, true);
    }

    /// <summary>
    /// Set ressolution with custom refresh rate
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="refreshrate"></param>
    /// <param name="fullscreen"></param>
    public void _SetResolution(int width, int height, int refreshrate, bool fullscreen = true)
    {
        _SetResolution(width, height, refreshrate, fullscreen, true);
    }

    /// <summary>
    /// Set ressolution with custom refresh rate
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="refreshrate"></param>
    /// <param name="fullscreen"></param>
    /// <param name="dispatchEvent"></param>
    public void _SetResolution(int width, int height, int refreshrate, bool fullscreen, bool dispatchEvent)
    {
        #if !UNITY_EDITOR
        m_HD = width >= m_HDResolution.x && height >= m_HDResolution.y;
        if (refreshrate < 1) Screen.SetResolution(width, height, Application.isMobilePlatform || fullscreen);
        else Screen.SetResolution(width, height, Application.isMobilePlatform || fullscreen, refreshrate);
        #endif

        if (dispatchEvent) DispatchEvent(Events.RESOLUTION_CHANGED);
    }

    private void _UpdateCurrent(bool force = false)
    {
        if (force || _HD != m_current.HD)  // wait resolution changed
        {
            if (m_changeResolution != null) StopCoroutine(m_changeResolution);
            m_changeResolution = StartCoroutine(_UpdateCurrentDelay());
        }
        else
        {
            m_FPS             = m_current.FPS;
            m_enablePosEffect = instance.supportPostEffect && m_current.postEffect;
            m_enableHDR       = instance.supportHDR && m_current.HDR;
            m_enableDOF       = m_current.DOF;
            m_msaaLevel       = m_current.MSAA;
            m_effectLevel     = m_current.effectLevel;
            m_notch           = m_current.notch;

            _UpdateVideoSettings();
        }
    }

    private float m_resChangeStartTime = -1;

    private IEnumerator _UpdateCurrentDelay()
    {
        m_HD = m_current.HD;

        if (m_HD) _SetResolution(m_HDResolution.x, m_HDResolution.y, m_HDResolution.z, true, false);
        else _SetResolution(m_LDResolution.x, m_LDResolution.y, m_HDResolution.z, true, false);

        m_resChangeStartTime = Time.time;
        var wait = new WaitUntil(ValidateResolution);

        yield return wait;

        DispatchEvent(Events.RESOLUTION_CHANGED);
        m_changeResolution = null;

        m_FPS             = m_current.FPS;
        m_enablePosEffect = instance.supportPostEffect && m_current.postEffect;
        m_enableHDR       = instance.supportHDR && m_current.HDR;
        m_enableDOF       = m_current.DOF;
        m_msaaLevel       = m_current.MSAA;
        m_effectLevel     = m_current.effectLevel;
        m_notch           = m_current.notch;

        _UpdateVideoSettings();
    }

    private bool ValidateResolution()
    {
        var n = m_HD ? m_HDResolution : m_LDResolution;
        var res = Screen.currentResolution;
        return res.width == n.x && res.height == n.y && res.refreshRate == n.z || Time.time - m_resChangeStartTime > 2.0f; // 2.0 seconds passed, and resolution still not changed, maybe failed ?
    }

    private bool CheckCamera()
    {
        if (!m_mainCamera || m_mainCamera != Level.currentMainCamera)
        {
            m_post    = null;
            m_postLow = null;
            m_lutLow  = null;

            m_mainCamera = Level.currentMainCamera;

            if (m_mainCamera)
            {
                m_post    = m_mainCamera.GetComponent<PostProcessingBehaviour>();
                m_postLow = m_mainCamera.GetComponent<FastMobileBloomDOF>();
                m_lutLow  = m_mainCamera.GetComponent<ColorCorrectionLookup>();
            }
            else if (Game.started) Logger.LogException("SettingsManager::CheckCamera: Current level <b>[{0}]</b> missing main camera.", Level.currentOrNextLevel);
        }

        if (!m_post || !m_post.profile || !m_postLow || !m_lutLow) return false;

        return true;
    }

    private void OnSceneCreateEnvironment(Event_ e)
    {
        var level = e.sender as Level;
        var canUseDOF = !level || !level.isBattle; 
        _UpdateVideoSettings(canUseDOF);
    }

    protected override void Awake()
    {
        base.Awake();

        EventManager.AddEventListener(Events.SCENE_CREATE_ENVIRONMENT, OnSceneCreateEnvironment);
    }

    #endregion

    #region Editor helper

    [Header("Editor Helper"), Space(10)]
    public bool e_RuntimeDebug = true;

#if UNITY_EDITOR || DEVELOPMENT_BUILD

    #region Real audio volume

#if UNITY_EDITOR
    [Space(10)]
    [Range(0, 1.0f)] public float m_E_RealVolume      = 1.0f;
    [Range(0, 1.0f)] public float m_E_RealBgmVolume   = 1.0f;
    [Range(0, 1.0f)] public float m_E_RealVoiceVolume = 1.0f;
    [Range(0, 1.0f)] public float m_E_RealSoundVolume = 1.0f;

    private void Start()
    {
        EventManager.AddEventListener("EditorReloadConfig", OnEditorReloadConfig);
    }

    private void OnEditorReloadConfig(Event_ e)
    {
        var config = (string)e.param1;
        if (config == "config_generalconfiginfos")
            _UpdateAudioSettings();
    }

    private void LateUpdate()
    {
        m_E_RealVolume      = m_realVolume;
        m_E_RealBgmVolume   = m_realBgmVolume;
        m_E_RealVoiceVolume = m_realVoiceVolume;
        m_E_RealSoundVolume = m_realSoundVolume;

        // We need to handle resolution change event (GameView window resize) in editor mode
        var c = UIManager.worldCamera;
        var v = UIManager.viewResolution;

        if (!c || v.x == c.pixelWidth && v.y == c.pixelHeight) return;

        DispatchEvent(Events.RESOLUTION_CHANGED);
    }
#endif

    #endregion

    private bool m_showPanel = false;
    private bool m_pauseGame = false;
    private bool m_ui = false;
    private bool m_disableInput = true;
    private float m_timeScale = 1.0f;
    private float m_lastTimeScale = -1;
    private bool m_freeCamera = false;
    private Vector3 m_cameraDistance = Vector3.zero;

    BloomModel.BloomSettings bloomSettings;
    DepthOfFieldModel.Settings dofSettings;

    private void OnGUI()
    {
        if (Root.simulateReleaseMode || !e_RuntimeDebug || !m_post || !m_post.profile) return;
        
        var vv = UIManager.viewResolution;
        var xx = vv.x * 0.5f;
        var ww = xx - 10;
        var bw = ww / 6.0f;

        if (!m_showPanel)
        {
            m_showPanel = GUI.Button(new Rect(xx - 340, 0, 200, 60), "Show Post Effect Panel", GUI.skin.button);
            if (!m_showPanel) return;
            else
            {
                m_ui = false;
                UIManager.Hide();
            }
        }

        if (m_lastTimeScale < 0) m_lastTimeScale = Time.timeScale;

        var y = 0;
        var x = xx;

        GUI.Label(new Rect(x - 10, 0, x + 10, vv.y), "", GUI.skin.box);

        if (GUI.Toggle(new Rect(x, y, bw, 60), _enablePosEffect, "Post Effect", GUI.skin.button) != _enablePosEffect) _enablePosEffect = !_enablePosEffect;  x += bw;
        if (GUI.Toggle(new Rect(x, y, bw, 60), _enableHDR, "HDR", GUI.skin.button) != _enableHDR) _enableHDR = !_enableHDR; x += bw;
        if (GUI.Toggle(new Rect(x, y, bw, 60), Root.hideAllColliders, "Hide Colliders", GUI.skin.button) != Root.hideAllColliders) Root.hideAllColliders = !Root.hideAllColliders; x += bw;

        m_pauseGame = GUI.Toggle(new Rect(x, y, bw, 60), m_pauseGame, "Pause Game", GUI.skin.button); x += bw;
        m_disableInput = GUI.Toggle(new Rect(x, y, bw, 60), m_disableInput, "Disable Input", GUI.skin.button); x += bw;

        if (GUI.Button(new Rect(x, y, bw, 60), "Hide", GUI.skin.button))
        {
            m_showPanel = false;
            m_pauseGame = false;
            m_ui = true;

            UIManager.inputState = true;
            UIManager.Show();

            if (m_lastTimeScale >= 0) Time.timeScale = m_lastTimeScale;
            m_lastTimeScale = -1;
            
            return;
        }
        x += bw;

        {
            var xxx = xx;
            var yyy = y + 60 + 65;
            var www = (ww - 30.0f) / 3.0f;

            GUI.Label(new Rect(xxx, yyy, www, 25), "<size=20><color=#00FF00>TimeScale (" + m_timeScale.ToString("F3") + ")</color></size>");
            m_timeScale = GUI.Slider(new Rect(xxx, yyy + 25, www, 20), m_timeScale, 0.1f, 0, 1.1f, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb, true, 9); xxx += www + 15.0f;

            GUI.Label(new Rect(xxx, yyy, www, 25), "<size=20><color=#00FF00>MSAA level (" + _msaaLevel.ToString() + ")</color></size>");
            _msaaLevel = (int)GUI.Slider(new Rect(xxx, yyy + 25, www, 20), _msaaLevel, 2, 0, 10, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb, true, 10); xxx += www + 15.0f;

            GUI.Label(new Rect(xxx, yyy, www, 25), "<size=20><color=#00FF00>Effect level (" + _effectLevel.ToString() + ")</color></size>");
            _effectLevel = (int)GUI.Slider(new Rect(xxx, yyy + 25, www, 20), _effectLevel, 1, 0, 4, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb, true, 11); xxx += www + 15.0f;
        }

        UIManager.inputState = !m_disableInput;
        ObjectManager.timeScale = m_pauseGame ? 0 : m_timeScale;
    
        x = xx;
        y += 60;

        if (m_enablePosEffect)
        {
            if (GUI.Toggle(new Rect(x, y, bw, 60), _enableBloom, "Bloom", GUI.skin.button) != _enableBloom) _enableBloom = !_enableBloom; x += bw; GUI.Toggle(new Rect(x, y, 1, 60), true, "", GUI.skin.label);
            if (GUI.Toggle(new Rect(x, y, bw, 60), _enableColorGrading, "Color Grading", GUI.skin.button) != _enableColorGrading) _enableColorGrading = !_enableColorGrading; x += bw; GUI.Toggle(new Rect(x, y, 1, 60), true, "", GUI.skin.label);
            if (GUI.Toggle(new Rect(x, y, bw, 60), _enableLut, "User Lut", GUI.skin.button) != _enableLut) _enableLut = !_enableLut; x += bw; GUI.Toggle(new Rect(x, y, 1, 60), true, "", GUI.skin.label);
            if (GUI.Toggle(new Rect(x, y, bw, 60), _enableDOF, "DOF", GUI.skin.button) != _enableDOF) _enableDOF = !_enableDOF; x += bw; GUI.Toggle(new Rect(x, y, 1, 60), true, "", GUI.skin.label);
            if (!Application.isEditor) { if (GUI.Toggle(new Rect(x, y, bw, 60), _HD, "HD Mode", GUI.skin.button) != _HD) _HD = !_HD; x += bw; }
            if (GUI.Toggle(new Rect(x, y, bw, 60), m_ui, "UI", GUI.skin.button) != m_ui) { m_ui = !m_ui; if (m_ui) UIManager.Show(); else UIManager.Hide(); } x += bw;

            x = xx;
            y += 65 + 55;

            if (_enableBloom)
            {
                GUI.Label(new Rect(x, y, 250, 25), "<size=20><color=#00FF00>Intensity (" + bloomSettings.intensity.ToString("F3") + ")</color></size>"); y += 25;
                bloomSettings.intensity = GUI.Slider(new Rect(x, y, ww, 20), bloomSettings.intensity * 10, 1f, 0, 101f, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb, true, 90) * 0.1f; y += 25;

                GUI.Label(new Rect(x, y, 250, 25), "<size=20><color=#00FF00>Threshold (" + bloomSettings.threshold.ToString("F3") + ")</color></size>"); y += 25;
                bloomSettings.threshold = GUI.Slider(new Rect(x, y, ww, 20), bloomSettings.threshold * 10, 1f, 0, 21f, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb, true, 91) * 0.1f; y += 25;

                GUI.Label(new Rect(x, y, 250, 25), "<size=20><color=#00FF00>Soft (" + bloomSettings.softKnee.ToString("F3") + ")</color></size>"); y += 25;
                bloomSettings.softKnee = GUI.Slider(new Rect(x, y, ww, 20), bloomSettings.softKnee * 10, 1f, 0, 11f, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb, true, 92) * 0.1f; y += 25;

                GUI.Label(new Rect(x, y, 250, 25), "<size=20><color=#00FF00>Radius (" + bloomSettings.radius.ToString("F3") + ")</color></size>"); y += 25;
                bloomSettings.radius = GUI.Slider(new Rect(x, y, ww, 20), bloomSettings.radius * 10, 1f, 10, 71f, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb, true, 93) * 0.1f; y += 25;

                var s = m_post.profile.bloom.enabled ? m_post.profile.bloom.settings : m_post.profile.bloomMedium.settings;
                s.bloom = bloomSettings;
                if (m_post.profile.bloom.enabled) m_post.profile.bloom.settings = s;
                else m_post.profile.bloomMedium.settings = s;
            }

            if (_enableDOF)
            {
                x = xx;

                if (GUI.Toggle(new Rect(x, y, bw, 60), dofSettings.useCameraFov, "CameraFoV", GUI.skin.button) != dofSettings.useCameraFov) dofSettings.useCameraFov = !dofSettings.useCameraFov; x += bw;
                if (GUI.Toggle(new Rect(x, y, bw, 60), m_freeCamera, "FreeCamera", GUI.skin.button) != m_freeCamera) m_freeCamera = !m_freeCamera; x += bw;

                x = xx;
                y += 65;

                GUI.Label(new Rect(x, y, 400, 25), "<size=20><color=#00FF00>Focus Distance (" + dofSettings.focusDistance.ToString("F3") + "), Extend (" + m_post.profile.depthOfField.settings.extendedFocusDistance.ToString("F3") + ")</color></size>"); y += 25;
                dofSettings.focusDistance = GUI.Slider(new Rect(x, y, ww, 20), dofSettings.focusDistance * 10, 1f, -20, 20f, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb, true, 94) * 0.1f; y += 25;

                GUI.Label(new Rect(x, y, 250, 25), "<size=20><color=#00FF00>Aperture (" + dofSettings.aperture.ToString("F3") + ")</color></size>"); y += 25;
                dofSettings.aperture = GUI.Slider(new Rect(x, y, ww, 20), dofSettings.aperture, 0.5f, 0, 30.0f, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb, true, 95); y += 25;

                if (!dofSettings.useCameraFov)
                {
                    GUI.Label(new Rect(x, y, 250, 25), "<size=20><color=#00FF00>Focal Length (" + dofSettings.focalLength.ToString("F3") + ")</color></size>"); y += 25;
                    dofSettings.focalLength = GUI.Slider(new Rect(x, y, ww, 20), dofSettings.focalLength, 5, 0, 200f, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb, true, 96); y += 25;
                }

                if (m_mainCamera && m_freeCamera)
                {
                    var off = Vector3.zero;

                    x = xx;

                    var fix = Mathf.Clamp((1 - (m_cameraDistance.z + 1.5f) / 3.0f) * 5.0f, 0.5f, 2.0f);

                    GUI.Label(new Rect(x, y, 250, 25), "<size=20><color=#00FF00>Camera X (" + m_cameraDistance.x.ToString("F3") + ")</color></size>"); y += 25;
                    m_cameraDistance.x = GUI.Slider(new Rect(x, y, ww, 20), m_cameraDistance.x * 50, fix, -40f, 40f, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb, true, 97) * 0.02f; y += 25;

                    GUI.Label(new Rect(x, y, 250, 25), "<size=20><color=#00FF00>Camera Y (" + m_cameraDistance.y.ToString("F3") + ")</color></size>"); y += 25;
                    m_cameraDistance.y = GUI.Slider(new Rect(x, y, ww, 20), m_cameraDistance.y * 50, fix, -50f, 50f, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb, true, 98) * 0.02f; y += 25;

                    GUI.Label(new Rect(x, y, 250, 25), "<size=20><color=#00FF00>Camera Z (" + m_cameraDistance.z.ToString("F3") + ")</color></size>"); y += 25;
                    m_cameraDistance.z = GUI.Slider(new Rect(x, y, ww, 20), m_cameraDistance.z * 50, fix, -150f, 150f, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb, true, 99) * 0.02f; y += 25;

                    GUI.Label(new Rect(x, y, 250, 25), "<size=20><color=#00FF00>Camera FoV (" + m_mainCamera.fieldOfView.ToString("F3") + ")</color></size>"); y += 25;
                    m_mainCamera.fieldOfView = GUI.Slider(new Rect(x, y, ww, 20), m_mainCamera.fieldOfView, 1, 0, 179.0f, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb, true, 100); y += 25;

                    var anim = m_mainCamera.GetComponent<Animator>();
                    if (anim) anim.enabled = false;

                    var combat = m_mainCamera.GetComponent<Camera_Combat>();
                    if (combat) combat.offset += m_cameraDistance.x * Vector3.right + m_cameraDistance.y * Vector3.up + m_cameraDistance.z * Vector3.forward;
                    else
                    {
                        var tt = m_mainCamera.transform;
                        m_mainCamera.transform.position = Level.current.startCameraPosition + Level.current.startCameraOffset + (m_cameraDistance.x * tt.right + m_cameraDistance.y * tt.up + m_cameraDistance.z * tt.forward);
                    }
                }

                dofSettings.extendedFocusDistance = m_post.profile.depthOfField.settings.extendedFocusDistance;
                m_post.profile.depthOfField.settings = dofSettings;
            }
        }
        else if (GUI.Toggle(new Rect(x, y, 120, 60), m_ui, "UI", GUI.skin.button) != m_ui)
        {
            m_ui = !m_ui;
            if (m_ui) UIManager.Show();
            else UIManager.Hide();
        }
        x += 120;
    }
#endif

    #endregion
}
