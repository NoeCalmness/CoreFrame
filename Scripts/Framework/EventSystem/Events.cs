/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Global Events definitions.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-01
 * 
 ***************************************************************************************************/

public static class Events
{
    /// <summary>
    /// sender : LogicObject          argument: Event
    /// trigger: LogicObject 在进入当前帧前
    /// </summary>
    public const string ENTER_FRAME                = "OnEnterFrame";
    /// <summary>
    /// sender : LogicObject          argument: Event
    /// trigger: LogicObject 在退出当前帧前
    /// </summary>
    public const string QUIT_FRAME                 = "OnQuitFrame";
    /// <summary>
    /// sender : ObjectManager        argument: Event
    /// trigger: 在所有逻辑进入当前帧之前
    /// </summary>
    public const string ROOT_ENTER_FRAME           = "OnRootEnterFrame";
    /// <summary>
    /// sender : ObjectManager        argument: Event
    /// trigger: 在所有逻辑退出当前帧之前
    /// </summary>
    public const string ROOT_QUIT_FRAME            = "OnRootQuitFrame";
    /// <summary>
    /// sender : LogicObject          argument: Event
    /// trigger: LogicObject 在 OnDestroy 被调用之前
    /// </summary>
    public const string ON_DESTROY                  = "OnDestroy";
    /// <summary>
    /// sender : SceneObject          argument: Event
    /// trigger: 在 SceneObject.visible 更改时
    /// </summary>
    public const string ON_VISIBILITY_CHANGED       = "OnVisibilityChanged";
    /// <summary>
    /// sender : Root        argument: Event[bool] 当前焦点状态
    /// trigger: 在程序获得/失去焦点时触发
    /// </summary>
    public const string APPLICATION_FOCUS_CHANGED  = "OnApplicationFocusChanged";
    /// <summary>
    /// sender : Root        argument: Event[bool] 当前暂停状态
    /// trigger: 在程序暂停/恢复时触发
    /// </summary>
    public const string APPLICATION_PAUSE_CHANGED  = "OnApplicationPauseChanged";
    /// <summary>
    /// sender : Root        argument: Event
    /// trigger: 在程序退出时触发
    /// </summary>
    public const string APPLICATION_EXIT           = "OnApplicationExit";
    /// <summary>
    /// sender : Root        argument: Event[DeviceOrientation,DeviceOrientation] 更改前状态，当前状态
    /// trigger: 在屏幕发生旋转时触发
    /// </summary>
    public const string APPLICATION_ORIENTATION    = "OnApplicationOrientation";
    /// <summary>
    /// sender : UIManager            argument: Event
    /// trigger: 在 UI 相机更改后
    /// </summary>
    public const string UI_CAMERA_CHANGED          = "OnUICameraChanged";
    /// <summary>
    /// sender : UIManager            argument: Event
    /// trigger: 在 UIManager 根节点 FadeIn 完成后
    /// </summary>
    public const string UI_FADE_IN                 = "OnUIFadeIn";
    /// <summary>
    /// sender : UIManager            argument: Event
    /// trigger: 在 UIManager 根节点 FadeOut 完成后
    /// </summary>
    public const string UI_FADE_OUT                = "OnUIFadeOut";
    /// <summary>
    /// sender : UIManager               argument: Event
    /// trigger: 在 Window 准备打开的时候
    /// </summary>
    public const string UI_WINDOW_WILL_OPEN        = "UI_WINDOW_WILL_OPEN";
    /// <summary>
    /// sender : UIManager               argument: Event
    /// trigger: 在 Window open 出错时
    /// </summary>
    public const string UI_WINDOW_OPEN_ERROR       = "UI_WINDOW_OPEN_ERROR";
    /// <summary>
    /// sender : Window               argument: Event
    /// trigger: 在 Window.OnBecameVisible 被调用时
    /// </summary>
    public const string UI_WINDOW_VISIBLE          = "UI_WINDOW_VISIBLE";
    /// <summary>
    /// sender : Window               argument: Event
    /// trigger: 在 Window._Show 被调用时（UI_WINDOW_VISIBLE 前）
    /// </summary>
    public const string UI_WINDOW_SHOW             = "UI_WINDOW_SHOW";
    /// <summary>
    /// sender : Window               argument: Event[bool] 当前的隐藏行为是否是因为打开一个新窗口导致的
    /// trigger: 在 Window.Hide 被调用 时
    /// <para></para>
    /// <para>注意，窗口的 Hide 行为有两种情况</para>
    /// 第一种情况是因为当某个新的全屏窗口显示时，自动触发，这种情况下，UI_WINDOW_HIDE 会触发两次，第一次在新窗口的 UI_WINDOW_SHOW 之前，第二次在当前窗口的 Window.Hide 中
    /// <para></para>
    /// 第二种情况是主动调用当前窗口的 Hide （比如默认的 Return 行为），只会触发一次 UI_WINDOW_HIDE，在 Window.Hide 中
    /// </summary>
    public const string UI_WINDOW_HIDE             = "UI_WINDOW_HIDE";
    /// <summary>
    /// sender : Window               argument: Event
    /// trigger: 在 Window OnDestroy 调用前
    /// </summary>
    public const string UI_WINDOW_ON_DESTORY       = "UI_WINDOW_ON_DESTORY";
    /// <summary>
    /// sender : any                  argument: Event[bool,string,float,int] Loading 显示状态，要显示的窗口名，延迟显示时间，显示模式 0 = 默认 1 = 黑屏 其它 = 不可见
    /// trigger: 在任何除场景加载以外需要显示加载窗口的地方
    /// </summary>
    public const string SHOW_LOADING_WINDOW        = "SHOW_LOADING_WINDOW";
    /// <summary>
    /// sender : Scene                argument: Event
    /// trigger: 在场景开始加载前
    /// </summary>
    public const string SCENE_LOAD_START           = "OnSceneLoadStart";
    /// <summary>
    /// sender : Scene                argument: Event[float] 当前加载进度
    /// trigger: 在场景加载进度变化时
    /// </summary>
    public const string SCENE_LOAD_PROGRESS        = "OnSceneLoadProgress";
    /// <summary>
    /// sender : Scene                argument: Event
    /// trigger: 场景加载完成（仅指场景本身 不包含其它加载行为）并初始化场景时
    /// </summary>
    public const string SCENE_CREATE_ENVIRONMENT   = "OnSceneCreateEnvironment";
    /// <summary>
    /// sender : Scene                argument: Event
    /// trigger: 在场景加载完成并显示时
    /// </summary>
    public const string SCENE_LOAD_COMPLETE        = "OnSceneLoadComplete";
    /// <summary>
    /// sender : Scene                argument: Event
    /// trigger: 在场景销毁时
    /// </summary>
    public const string SCENE_DESTROY              = "OnSceneDestroy";
    /// <summary>
    /// sender : Scene                argument: Event
    /// trigger: 在场景清理资源时
    /// </summary>
    public const string SCENE_UNLOAD_ASSETS        = "OnSceneUnloadAssets";
    /// <summary>
    /// sender : SceneObject          argument: Event
    /// trigger: 在 SceneObject 添加到场景时触发
    /// </summary>
    public const string SCENE_ADD_OBJECT           = "OnSceneAddObject";
    /// <summary>
    /// sender : Root          argument: Event
    /// trigger: 在配置文件加载完成后
    /// </summary>
    public const string CONFIG_LOADED              = "OnConfigLoaded";
    /// <summary>
    /// sender : Session       argument: Event
    /// trigger: Game Session 成功链接后触发
    /// </summary>
    public const string SESSION_CONNECTED          = "OnSessionConnected";
    /// <summary>
    /// sender : Session       argument: Event
    /// trigger: Game Session 丢失链接后触发
    /// </summary>
    public const string SESSION_LOST_CONNECTION    = "OnSessionLostConnection";
    /// <summary>
    /// sender: SettingsManager argument: Event
    /// trigger: 分辨率更改后
    /// </summary>
    public const string RESOLUTION_CHANGED         = "OnResolutionChanged";
    /// <summary>
    /// sender: UIManager argument: Event
    /// trigger: UI 窗口区域发生更改时（分辨率，比列）
    /// </summary>
    public const string UI_CANVAS_FIT              = "OnUICanvasFit";
    /// <summary>
    /// sender: SettingsManager argument: Event
    /// trigger: 任何渲染设置更改时
    /// </summary>
    public const string VEDIO_SETTINGS_CHANGED     = "OnVideoSettingsUpdate";
    /// <summary>
    /// sender: SettingsManager argument: Event
    /// trigger: 任何声音设置更改时
    /// </summary>
    public const string AUDIO_SETTINGS_CHANGED     = "OnAudioSettingsUpdate";
    /// <summary>
    /// sender: SettingsManager argument: Event
    /// trigger: 任何操作设置更改时
    /// </summary>
    public const string INPUT_SETTINGS_CHANGED     = "OnInputSettingsUpdate";
    /// <summary>
    /// sender : UIManager      argument: Event[bool] 表示战斗相机是否移动
    /// trigger: 在CombatCamera镜头移动的时候触发
    /// </summary>
    public const string COMBAT_CAMERA_MOVE_CHANGE  = "OnCombatCameraMoveChange";
    /// <summary>
    /// sender : Root           argument: Event[boo] 当前 Camera Shot 系统是否开启
    /// trigger: 当 Camera Shot System 状态变化时触发
    /// </summary>
    public const string CAMERA_SHOT_STATE          = "OnCameraShotState";
    /// <summary>
    /// sender : Root           argument: Event[bool,string] 当前战斗窗口内容是否隐藏,遮罩窗口要使用的遮罩资源名,遮罩停留时间
    /// trigger: 当 Camera Shot System 需要控制 UI 状态时触发
    /// </summary>
    public const string CAMERA_SHOT_UI_STATE       = "OnCameraShotUIState";
    /// <summary>
    /// sender : Root           argument: Event[LaunchProcess,int] 当前加载阶段(see <see cref="Launch.LaunchProcess"/>,结果(0 = 未通过 1 = 通过))
    /// trigger: Launcher 的加载阶段变化时
    /// </summary>
    public const string LAUNCH_PROCESS             = "OnLaunchProcess";
    /// <summary>
    /// sender : session           argument: 
    /// trigger: ping回来之后发送事件
    /// </summary>
    public const string EVENT_PING_UPDATE          = "OnPingUpdate";
}
