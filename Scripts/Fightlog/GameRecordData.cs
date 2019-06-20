// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-11-05      14:58
//  * LastModify：2018-11-05      14:58
//  ***************************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;

public struct RecordDataEntry
{
    public Type         type;
    public PacketObject msg;
}

public interface IGameRecordData
{
    List<PacketObject> SimulateUpdate(float delta);
    void StartRecover();
    void Load(Stream rStream);
    void Save(Stream rStream);
    void SaveToJson(StreamWriter rStream);

    bool OnLoadComplete();

    void OnTransportComplete();
    void FightEnd();

    int CommandsCount { get; }
    Type DataType { get; set; }
    ScMatchInfo MatchInfomation { get; }
    ulong MasterId { get; set; }

    #region 存数据
    void EnqueueCommand(Command rCommand);

    void Set<T>(T msg) where T : PacketObject<T>;
    void Set(Type type, PacketObject msg);
    T Get<T>() where T : PacketObject<T>;

    #endregion

}

public abstract class GameRecordDataBase : IGameRecordData
{
    public Queue<Command> Commands = new Queue<Command>();
    public int LevelId;
    private readonly Dictionary<Type, RecordDataEntry> cache = new Dictionary<Type, RecordDataEntry>();

    public ulong MasterId { get; set; }

    private float timer;


    public int CommandsCount { get { return Commands?.Count ?? 0; } }

    public Type DataType {get;set;}

    public ScMatchInfo MatchInfomation
    {
        get { return Get<ScMatchInfo>(); }
    }

    public void Set<T>(T rMsg) where T : PacketObject<T>
    {
        var t = typeof(T);
        if (cache.ContainsKey(t))
            cache.Remove(t);
        cache.Add(t, new RecordDataEntry { type = t, msg = rMsg });
    }

    public void Set(Type rType, PacketObject rMsg)
    {
        if (cache.ContainsKey(rType))
            cache.Remove(rType);
        cache.Add(rType, new RecordDataEntry { type = rType, msg = rMsg });
    }

    public T Get<T>() where T : PacketObject<T>
    {
        var t = typeof (T);
        if (cache.ContainsKey(t))
            return cache[t].msg as T;
        return default(T);
    }

    public virtual void OnTransportComplete() { }

    public List<PacketObject> SimulateUpdate(float delta)
    {
        delta *= FightRecordManager.PlaySpeed;
        if (timer > 0)
            timer -= (int)(delta * 1000);
        if (timer > 0) return null;

        List<PacketObject> msg = null;
        while (Commands.Count > 0)
        {
            var command = Commands.Peek();
            if (command.Frame <= FightRecordManager.MessageIndex)
            {
                var p = Commands.Dequeue().cache;
                if (msg == null) msg = new List<PacketObject>();
                msg.Add(p);
                var update = p as ScFrameUpdate;
                if (update != null)
                {
                    timer += update.diff;
                }
            }
            else
                break;
        }
        return msg;
    }

    public virtual void StartRecover()
    {
        timer = 0;
    }

    public virtual void FightEnd()
    {
    }
    public void SaveToJson(StreamWriter rStream)
    {
        rStream.WriteLine("level = {0}", LevelId);
        rStream.WriteLine("materId = {0}", MasterId);
        foreach (var kv in cache)
        {
            if(kv.Value.msg != null)
                rStream.WriteLine("{0}:{1}", kv.Value.msg._name, LitJson.JsonMapper.ToJson(kv.Value.msg, true, 5, true));
            kv.Value.msg?.Destroy();
        }
        while (Commands.Count > 0)
        {
            var command = Commands.Dequeue();
            if(command?.cache != null)
                rStream.WriteLine("{2} {0}:{1}", command.cache._name, LitJson.JsonMapper.ToJson(command.cache, true, 5, true), command.Frame);
            command?.Destroy();
        }
        cache.Clear();
    }

    public virtual bool OnLoadComplete()
    {
        return true;
    }

    public virtual void Load(Stream rStream)
    {
        rStream.Read(out LevelId);
        ulong masterId;
        rStream.Read(out masterId);
        MasterId = masterId;

        int count = 0;
        Command c = Command.Create();
        rStream.Read(out count);
        for (int i = 0; i < count; i++)
        {
            string typeName;
            rStream.Read(out typeName);
            Type t = Type.GetType(typeName);
            if(null != t)
                cache.Add(t, new RecordDataEntry {type = t, msg = c.UnSerialize(rStream).cache});
        }
        c.cache = null;
        c.Destroy();
        Commands.Clear();
        while (rStream.CanRead && rStream.Position < rStream.Length)
        {
            Command command = Command.Create();
            command.UnSerialize(rStream);
            Commands.Enqueue(command);
        }
    }

    public virtual void Save(Stream rStream)
    {
        rStream.Write(Level.current.levelID);
        rStream.Write(Module_Player.instance.id_);

        rStream.Write(cache.Count);
        foreach (var kv in cache)
        {
            Command command = Command.Create();
            rStream.Write(kv.Key.FullName);
            command.Reset(kv.Value.msg).Serialize(rStream);
            command.Destroy();
        }
        while (Commands.Count > 0)
        {
            var command = Commands.Dequeue();
            command.Serialize(rStream);
            command.Destroy();
        }
    }

    public void EnqueueCommand(Command rCommand)
    {
        Commands.Enqueue(rCommand);
    }
}

public class GameRecordDataPvp : GameRecordDataBase
{
    public override void StartRecover()
    {
        base.StartRecover();

        FightRecordManager.InstanceHandle<GameRecordDataPvp>();

        Game.LoadLevel(LevelId);

        Module_PVP.instance.HandlePacket(Get<ScRoomEnter>());
        Module_PVP.instance.HandlePacket(Get<ScRoomStartLoading>());
    }

    public override bool OnLoadComplete()
    {
        base.OnLoadComplete();
        Module_PVP.instance.HandlePacket(Get<ScRoomStart>());
        return true;
    }

    public override void FightEnd()
    {
        base.FightEnd();
        Module_PVP.instance.HandlePacket(Get<ScRoomReward>());
    }
}

public class GameRecordDataTeam : GameRecordDataBase
{
    bool isLoaded = false;
    public override void StartRecover()
    {
        base.StartRecover();

        FightRecordManager.InstanceHandle<GameRecordDataTeam>();

        Module_Team.instance.HandlePacket(Get<ScTeamStartLoading>());
    }
    public override bool OnLoadComplete()
    {
        if (isLoaded)
            return false;
        isLoaded = true;
        Module_Team.instance.HandlePacket(Get<ScTeamStart>());
        return true;
    }

    public override void OnTransportComplete()
    {
        Module_Team.instance.HandlePacket(Get<ScTeamTransportOver>());
    }

    public override void FightEnd()
    {
        base.FightEnd();
        Module_Team.instance.HandlePacket(Get<ScChaseTaskOver>());
    }
}