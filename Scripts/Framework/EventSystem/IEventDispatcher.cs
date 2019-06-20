/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * The IEventDispatcher interface defines methods for adding or removing event listeners, checks
 * whether specific types of event listeners are registered, and dispatches events. 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-02-28
 * 
 ***************************************************************************************************/

using System;

public delegate void VoidHandler();
public delegate void NormalHandler(Event_ e);
public delegate void ModuleHandler<T>(ModuleEvent<T> e) where T : Module;
public delegate void DynamicHandler<T>(T e) where T : Event_;

public interface IEventDispatcher
{
    bool dispatching { get; }
    int listenersCount { get; }

    EventListener AddEventListener(string name, VoidHandler handler);
    EventListener AddEventListener(string name, NormalHandler handler);
    EventListener AddEventListener<T>(string name, DynamicHandler<T> handler) where T : Event_;
    EventListener AddModuleListener<T>(string name, ModuleHandler<T> handler) where T : Module;
    EventListener AddModuleListener(string name, Delegate handler);
    void RemoveEventListener(string name, VoidHandler handler);
    void RemoveEventListener(string name, NormalHandler handler);
    void RemoveEventListener<T>(string name, DynamicHandler<T> handler) where T : Event_;
    void RemoveModuleListener<T>(ModuleHandler<T> handler) where T : Module;
    void RemoveModuleListener(Delegate handler);
    void RemoveEventListener(object receiver);
    void DispatchEvent(string name, Event_ e = null, bool pool = true);
}