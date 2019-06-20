/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * ModuleEvent used for communicating between module/controller and view(Window) system.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-14
 * 
 ***************************************************************************************************/

public abstract class ModuleEvent : Event_
{
    #region Static functions

    public const string GLOBAL = "GLOBAL_MODULE_EVENT";

    public static readonly System.Type type = typeof(ModuleEvent);

    public static ModuleEvent<T> Pop<T>(T module, string name, PacketObject msg = null) where T : Module
    {
        var e = Pop<ModuleEvent<T>>();
        e.moduleEvent = name;
        e.baseModule = module;
        e.msg = msg;
        return e;
    }

    public static ModuleEvent Pop(Module module, string name, PacketObject msg = null)
    {
        var e = Pop(module.eventType) as ModuleEvent;
        e.moduleEvent = name;
        e.baseModule = module;
        e.msg = msg;
        return e;
    }

    #endregion

    public Module baseModule = null;
    public PacketObject msg = null;
    public string moduleEvent = string.Empty;

    public override void Reset()
    {
        baseModule = null;
        moduleEvent = null;
        msg = null;
        
        base.Reset();
    }
}

public class ModuleEvent<T> : ModuleEvent where T : Module
{
    public T module { get { return baseModule as T; } }

    protected ModuleEvent() { }
}
