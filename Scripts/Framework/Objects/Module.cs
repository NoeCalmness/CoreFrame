/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Base Module class
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-14
 * 
 ***************************************************************************************************/

using System.Reflection;

/// <summary>
/// Module system
/// Base class
/// </summary>
public abstract class Module : LogicObject
{
    #region Static functions

    public static Module Create(string name)
    {
        return _Create<Module>(name, false);
    }

    public static T Create<T>(string name) where T : Module
    {
        return _Create<T>(name, false);
    }

    /// <summary>
    /// Auto collect all method with signature void _ME(ModuleEvent e) to handle module message
    /// <para>Attention: You should call <see cref="EventManager.RemoveEventListener(object)"/> manually when receiver destroy.</para>
    /// </summary>
    /// <param name="receiver">Event receiver</param>
    public static void CollectModuleCallbacks(object receiver)
    {
        if (receiver == null)
        {
            Logger.LogError("Module::CollectModuleCallbacks: Receiver can not be null.");
            return;
        }

        var type = receiver.GetType();
        if (!type.IsClass)
        {
            Logger.LogError("Module::CollectModuleCallbacks: Receiver must be a class type.");
            return;
        }

        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

        foreach (var method in methods)
        {
            if (method.Name != "_ME") continue;

            var ps = method.GetParameters();
            if (ps.Length != 1 || !ps[0].ParameterType.IsSubclassOf(typeof(ModuleEvent)) || !ps[0].ParameterType.IsGenericType)
            {
                Logger.LogWarning("Window::CollectModuleCallbacks: ModuleEventHandler has invalid parameters: [receiver: {0}, type:{1}, method: {2}, paramCount: {3}, paramType: {4}]", receiver, type, method.Name, ps.Length, ps.Length > 0 ? ps[0].ParameterType.Name : "");
                continue;
            }

            var handler = System.Delegate.CreateDelegate(typeof(ModuleHandler<>).MakeGenericType(new System.Type[] { ps[0].ParameterType.GetGenericArguments()[0] }), receiver, method, false);
            EventManager.AddModuleListener(ModuleEvent.GLOBAL, handler);
        }
    }

    #endregion

    public System.Type eventType { get; private set; }

    /// <summary>
    /// Called when module created and added to ObjectManager
    /// </summary>
    protected sealed override void OnInitialized()
    {
        eventType = typeof(ModuleEvent<>).MakeGenericType(GetType());

        Session.CollectNetworkCallbacks(this);

        Game.onGameStarted    += OnGameStarted;
        Game.onGamePauseResum += OnGamePauseResum;
        Game.onGameDataReset  += OnGameDataReset;
        Game.onWillEnterGame  += OnWillEnterGame;
        Game.onEnterGame      += OnEnterGame;

        OnModuleCreated();
    }

    protected override void OnDestroy()
    {
        Session.RemoveNetworkCallbacks(this);

        Game.onGameStarted    -= OnGameStarted;
        Game.onGamePauseResum -= OnGamePauseResum;
        Game.onGameDataReset  -= OnGameDataReset;
        Game.onWillEnterGame  -= OnWillEnterGame;
        Game.onEnterGame      -= OnEnterGame;
    }

    /// <summary>
    /// Called only once when game started (All modules created, all managers initialized)
    /// </summary>
    protected virtual void OnGameStarted() { }

    /// <summary>
    /// Called when game paused or resumed
    /// Use Game.paused to get current state
    /// </summary>
    protected virtual void OnGamePauseResum() { }

    /// <summary>
    /// Called when game data reset.
    /// This is useful for restart game, like "Return to Title", or when a player logged out
    /// </summary>
    protected virtual void OnGameDataReset() { }

    /// <summary>
    /// Called when module created and initialized
    /// </summary>
    protected virtual void OnModuleCreated() { }

    /// <summary>
    /// Called when user will enter main level after login or create role
    /// </summary>
    protected virtual void OnWillEnterGame() { }

    /// <summary>
    /// Called when user first enter main level after login
    /// </summary>
    protected virtual void OnEnterGame() { }

    /// <summary>
    /// Dispatch a module event for UI system, use default event Event
    /// </summary>
    /// <param name="e">The module event name</param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="param3"></param>
    /// <param name="param4"></param>
    public virtual void DispatchModuleEvent(string e, object param1 = null, object param2 = null, object param3 = null, object param4 = null)
    {
        var ee = ModuleEvent.Pop(this, e);
        ee.param1 = param1;
        ee.param2 = param2;
        ee.param3 = param3;
        ee.param4 = param4;
        DispatchEvent(ModuleEvent.GLOBAL, ee);
    }

    /// <summary>
    /// Dispatch a module event for UI system, use ModuleEvent
    /// </summary>
    /// <param name="e">The module event name</param>
    /// <param name="msg">The PacketObject from server or local</param>
    public virtual void DispatchModuleEvent(string e, PacketObject msg)
    {
        DispatchEvent(ModuleEvent.GLOBAL, ModuleEvent.Pop(this, e, msg));
    }
}

public abstract class Module<T> : Module where T : Module<T>, new()
{
    /// <summary>
    /// The instance of module T
    /// </summary>
    public static T instance { get; private set; }

    protected Module()
    {
        if (instance != null)
            throw new System.Exception("Can not create " + GetType().Name + " twice.");

        instance = (T)this;
    }

    protected override void OnDestroy()
    {
        instance = null;
    }
}