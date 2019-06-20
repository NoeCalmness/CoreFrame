/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Global game class.
 * Contains global game infomation.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.4
 * Created:  2018-03-28
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;

public delegate void OnGameStarted();
public delegate void OnGamePauseResum();
public delegate void OnGameDataReset();
public delegate void OnWillEnterGame();
public delegate void OnEnterGame();

public static partial class Game
{
    /// <summary>
    /// Game initialized ?
    /// All Internal class types (Modules, Windows...) are registered after initialize.
    /// </summary>
    public static bool initialized { get; private set; }
    /// <summary>
    /// All data initialized (Modules, Configs...)
    /// </summary>
    public static bool started { get; private set; }
    /// <summary>
    /// Pause/Resum game
    /// </summary>
    public static bool paused
    {
        get { return m_paused; }
        set
        {
            if (m_paused == value) return;
            m_paused = value;

            onGamePauseResum?.Invoke();
        }
    }
    private static bool m_paused = false;

    /// <summary>
    /// The first game level load in <see cref="Start"/>
    /// <para>Set it when <see cref="onGameStarted"/></para>
    /// </summary>
    public static int startLevel = 0;

    /// <summary>
    /// Callback for GameStarted(All modules created) event
    /// </summary>
    public static OnGameStarted onGameStarted;
    /// <summary>
    /// Callback for GamePauseResum event
    /// </summary>
    public static OnGamePauseResum onGamePauseResum;
    /// <summary>
    /// Callback for GameDataReset event
    /// </summary>
    public static OnGameDataReset onGameDataReset;
    /// <summary>
    /// Callback for WillEnterGame event
    /// </summary>
    public static OnWillEnterGame onWillEnterGame;
    /// <summary>
    /// Callback for EnterGame event
    /// </summary>
    public static OnEnterGame onEnterGame;

    private static Dictionary<string, Type> m_types = new Dictionary<string, Type>();
    private static Dictionary<int, string> m_typeNames = new Dictionary<int, string>();

    /// <summary>
    /// Get a registerd Type by module name
    /// </summary>
    /// <param name="name">The class name</param>
    /// <returns>The resgisterd runtime Type</returns>
    /// <remarks>See RegisterTypes</remarks>
    public static Type GetType(string name) { return m_types.Get(name, true); }
    /// <summary>
    /// Get a registerd type name by type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetDefaultName(Type type) { return m_typeNames.Get(type.GetHashCode()); }
    /// <summary>
    /// Get a registerd type name by tyoe
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static string GetDefaultName<T>() { return m_typeNames.Get(typeof(T).GetHashCode()); }

    /// <summary>
    /// Initialize Game
    /// </summary>
    public static void Initialize()
    {
        if (initialized) return;
        initialized = true;

        RegisterTypes();
        PacketObject.CollectAllRegisteredPackets();
    }

    /// <summary>
    /// Start Game, must after Initialize()
    /// </summary>
    public static void Start()
    {
        LogicObject.CreateModules();
        InputManager.Initialize();
        AudioManager.Initialize();

        onGameStarted?.Invoke();

        started = true; // Game started
        Root.instance.AddEventListener(Events.APPLICATION_EXIT, () => started = false);

        LoadLevel(startLevel); // Enter first game scene        
    }

    /// <summary>
    /// Quit Game, stop application
    /// </summary>
    public static void Quit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        UnityEngine.Application.Quit();
        #endif
    }

    /// <summary>
    /// Go to home level, same as Game.LoadLevel(GeneralConfigInfo.shomeLevel)
    /// </summary>
    public static UnityEngine.Coroutine GoHome()
    {
        return LoadLevel(GeneralConfigInfo.shomeLevel);
    }

    /// <summary>
    /// Load a level by level id, if we already in level [id], do nothing
    /// </summary>
    /// <param name="levelID">The level id from config: config_levelInfo</param>
    /// <param name="async">Load asynchronous ?</param>
    /// <param name="loadingWindowMode">0 = default  1 = black mask  other = diable</param>
    public static void LoadLevelGuard(int levelID, bool async = true, int loadingWindowMode = 0)
    {
        if (Level.currentLevel == levelID) return;

        var level = ConfigManager.Get<LevelInfo>(levelID);
        if (!level)
        {
            Logger.LogInfo("Could not load level from id [{0}], level info config not found.", levelID);
            return;
        }

        LoadLevel(level, async, loadingWindowMode);
    }

    /// <summary>
    /// Load a level by level id
    /// </summary>
    /// <param name="levelID">The level id from config: config_levelInfo</param>
    /// <param name="async">Load asynchronous ?</param>
    /// <param name="loadingWindowMode">0 = default  1 = black mask  other = diable</param>
    public static UnityEngine.Coroutine LoadLevel(int levelID, bool async = true, int loadingWindowMode = 0)
    {
        var level = ConfigManager.Get<LevelInfo>(levelID);
        if (!level)
        {
            Logger.LogInfo("Could not load level from id [{0}], level info config not found.", levelID);
            return null;
        }

        return LoadLevel(level, async, loadingWindowMode);
    }

    /// <summary>
    /// Load a level by level name
    /// </summary>
    /// <param name="name">The level name from config: config_levelInfo</param>
    /// <param name="async">Load asynchronous ?</param>
    /// <param name="loadingWindowMode">0 = default  1 = black mask  other = diable</param>
    public static void LoadLevel(string name, bool async = true, int loadingWindowMode = 0)
    {
        var level = ConfigManager.Find<LevelInfo>(l => l.name == name);
        if (!level)
        {
            Logger.LogInfo("Could not load level from name [{0}], level info config not found.", name);
            return;
        }

        LoadLevel(level, async, loadingWindowMode);
    }

    /// <summary>
    /// Load a level by level info
    /// </summary>
    /// <param name="level">The config info</param>
    /// <param name="async">Load asynchronous ?</param>
    /// <param name="loadingWindowMode">0 = default  1 = black mask  other = diable</param>
    public static UnityEngine.Coroutine LoadLevel(LevelInfo level, bool async = true, int loadingWindowMode = 0)
    {
        var map = level.SelectMap();
        if (string.IsNullOrEmpty(map))
        {
            Logger.LogInfo("Could not load level [ID:{0}, name:{1}], level must have a map assigned.", level.ID, level.name);
            return null;
        }

        var script = GetType(level.script);
        if (script == null)
        {
            Logger.LogInfo("Could not load level [ID:{0}, name:{1}, script:{2}], could not find level script [{2}].", level.ID, level.name, level.script);
            return null;
        }

        if (async) return Level.LoadAsync(level.name, map, script, level.loadingWindow, loadingWindowMode, level);
        else Level.Load(level.name, map, script, level.loadingWindow, loadingWindowMode, level);
        return null;
    }

    /// <summary>
    /// Reset game data, call this function will trigger all
    /// Module.OnGameDataReset() event
    /// </summary>
    public static void ResetGameData()
    {
        onGameDataReset.Invoke();

        Logger.LogDetail("Game::ResetGameData: Game data reset.");
    }

    /// <summary>
    /// Do not call this...
    /// </summary>
    public static void WillEnterGame()
    {
        onWillEnterGame.Invoke();

        Logger.LogDetail("Game::WillEnterGame: Prepare main level.");
    }

    /// <summary>
    /// Do not call this....
    /// </summary>
    public static void EnterGame()
    {
        onEnterGame.Invoke();

        Logger.LogDetail("Game::EnterGame: Enter game.");
    }
}