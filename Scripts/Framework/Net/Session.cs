/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Socket Session
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-27
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;

public class Session : Module
{
    #region Static functions

    public static Session instance { get { return m_instance; } }

    private static Session m_instance = null;

    private static Dictionary<ushort, List<PacketCallBack>> m_callBacks = new Dictionary<ushort, List<PacketCallBack>>();

    private static System.Text.RegularExpressions.Regex m_reg = new System.Text.RegularExpressions.Regex(@"^_Packet(?:_?(\d+))?");

    /// <summary>
    /// Auto collect all method with signature void _Packet(Packet p) to handle server message
    /// <para>Attention: You should call <see cref="RemoveNetworkCallbacks"/> manually when receiver destroy.</para>
    /// </summary>
    /// <param name="receiver">Message receiver</param>
    public static void CollectNetworkCallbacks(object receiver)
    {
        if (receiver == null)
        {
            Logger.LogError("Session::CollectNetworkCallbacks: Receiver can not be null.");
            return;
        }

        var type = receiver.GetType();
        if (!type.IsClass)
        {
            Logger.LogError("Session::CollectNetworkCallbacks: Receiver must be a class type.");
            return;
        }


        if (!instance)
        {
            Logger.LogError("Session::CollectNetworkCallbacks: Session not created.");
            return;
        }

        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var method in methods)
        {
            var match = m_reg.Match(method.Name);
            if (!match.Success) continue;

            var p = string.IsNullOrEmpty(match.Groups[1].Value) ? 100 : Util.Parse<int>(match.Groups[1].Value);
            instance.RegisterPacketCallBack(receiver, method, p - 100);
        }
    }

    /// <summary>
    /// Remove all network message callbacks
    /// </summary>
    /// <param name="receiver"></param>
    public static void RemoveNetworkCallbacks(object receiver)
    {
        if (!instance || receiver == null) return;
        instance.RemovePacketCallBacks(receiver);
    }

    #endregion

    #region NetStat statistic

#if NETSTAT
    internal PacketReceiver receiver { get { return m_receiver; } }
#endif

    #endregion

    public const string EventLostConnection = "EventLostConnection";

    public enum SessionState { INITIALIZED = 0, CONNECTING = 1, CONNECTED = 2, CLOSED = 3 }

    private class PacketCallBack
    {
        public int priority;
        public object o;
        public MethodInfo method;
        public Type packetType;

        private object[] m_paramCache = { null };

        public PacketCallBack(object _o, MethodInfo _method, Type _packetType, int _priority) { o = _o; method = _method; packetType = _packetType; priority = _priority; }
        public void Invoke(PacketObject p)
        {
            m_paramCache[0] = p;
            try { method.Invoke(o, m_paramCache); }
            #if DEVELOPMENT_BUILD || UNITY_EDITOR
            catch (Exception e) { Logger.LogException(e); }
            #else
            catch { }
            #endif
            m_paramCache[0] = null;
        }
    }

    /// <summary>
    /// 服务器当前时间戳
    /// 注意，该时间戳是标准（UTC）时间
    /// </summary>
    public int serverTimeStamp { get; private set; }
    /// <summary>
    /// 服务器当前时间戳
    /// 注意，该时间戳是服务器本地时间 相当于 serverTimeStamp + serverTimeZoneDiff
    /// </summary>
    public int serverLocalTimeStamp { get; private set; }
    /// <summary>
    /// 服务器本地时间与标准 (UTC) 时间之间的差异值
    /// 比如北京时间 (GMT+8) 差异值为 8 * 3600 = 28800s
    /// </summary>
    public int serverTimeZoneDiff { get; private set; }
    /// <summary>
    /// 用于计时增加接收到目前为止的偏移
    /// </summary>
    public float localTimeOffset { get; set; }

    public bool connecting { get { return state == SessionState.CONNECTING; } }
    public bool connected { get { return state == SessionState.CONNECTED; } }
    public SessionState state { get; protected set; }
    public string host { get { return m_host; } }
    public int port { get { return m_port; } }
    public int ping { get; protected set; }
    public float pingInterval { get; set; }

    private SessionState m_lastState = SessionState.INITIALIZED;

    private PacketSender     m_sender;
    private PacketReceiver   m_receiver;
    private Socket           m_socket;
    private Action<Session>  m_onConnect;
    private Action<Session>  m_delayCall;

    private string m_host;
    private int m_port;

    protected int m_lastPing = 0;
    protected float m_waitPing = 0.0f;

    protected string m_typeName = string.Empty;

    private Session()
    {
        m_typeName = GetType().Name;

        if (m_instance != null)
            throw new Exception($"Can not create [{m_typeName}] twice.");

        m_instance = this;

        pingInterval = 5.0f;
    }

    protected Session(bool guard) { m_typeName = GetType().Name; }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Disconnect();

        if (this == session)
        {
            m_callBacks.Clear();
            m_callBacks = null;
        }

        m_instance = null;
    }

    protected override void OnModuleCreated()
    {
        state = SessionState.INITIALIZED;
        m_lastState = state;

        enableUpdate = true;
    }

    protected override void OnGameStarted()
    {
        var callBacks = m_callBacks.Values;
        foreach (var callBack in callBacks)
            callBack.Sort((a, b) => a.priority > b.priority ? -1 : 1);
    }

    public void UpdateServer(int id)
    {
        UpdateServer(ServerConfigInfo.Get(id));
    }

    public void UpdateServer(ServerConfigInfo info)
    {
        if (info == null) return;

        m_host = info.host;
        m_port = info.port;

        Logger.LogInfo("<b><color=#FFFFFF>[{0}]</color></b> Server info update: [id: {1}, host: {2}, port: {3}, isHttp: {4}]", m_typeName, info.ID, host, port, info.isHttp);
    }

    public void UpdateServer(string _host, int _port)
    {
        m_host = _host;
        m_port = _port;

        Logger.LogInfo("<b><color=#FFFFFF>[{0}]</color></b> Server info update: [id: {1}, host: {2}, port: {3}]", m_typeName, -1, host, port);
    }

    public void SendPing()
    {
        m_lastPing = Level.realTime;

        Send(PacketObject.Create<CsPing>());
    }

    public void RegisterPacketCallBack(object o, MethodInfo method, int priority = 0)
    {
        if (o == null || method == null || destroyed) return;

        var ps = method.GetParameters();
        if (ps.Length != 1 || !ps[0].ParameterType.IsSubclassOf(typeof(PacketObject)))
        {
            Logger.LogWarning("Session::RegisterPacketCallBack: PacketCallBack method has invalid parameters: [module: {0}, method: {1}, paramCount: {2}, paramType: {3}]", o.GetType().Name, method.Name, ps.Length, ps.Length > 0 ? ps[0].ParameterType.Name : "");
            return;
        }

        var info = Packet.GetPacketInfo(ps[0].ParameterType);
        var ms   = m_callBacks.GetDefault(info.ID);
        if (ms.Find(c => c.o == o && c.method == method) != null) return;
        ms.Add(new PacketCallBack(o, method, ps[0].ParameterType, priority));
    }

    public void RemovePacketCallBack(object o, MethodInfo method)
    {
        if (o == null || method == null || destroyed) return;

        var ps = method.GetParameters();
        if (ps.Length != 1 || !ps[0].ParameterType.IsSubclassOf(typeof(PacketObject))) return;

        var info = Packet.GetPacketInfo(ps[0].ParameterType);
        var ms = m_callBacks.GetDefault(info.ID);
        var cb = ms.Find(c => c.o == o && c.method == method);
        if (cb != null) ms.Remove(cb);
    }

    public void RemovePacketCallBacks(object o)
    {
        if (destroyed) return;

        if (o == null) m_callBacks.Clear();

        var keys = m_callBacks.Keys;
        foreach (var key in keys) m_callBacks[key].RemoveAll(c => c.o == o);
    }

    public void Connect(int id, Action<Session> onConnect = null)
    {
        var info = ServerConfigInfo.Get(id);
        if (info == null)
        {
            Logger.LogWarning("<b><color=#FFFFFF>[{0}]</color></b> Could not find server [{1}] from config.", m_typeName, id);
            return;
        }

        UpdateServer(info);
        Connect(onConnect);
    }

    public void Connect(string _host, int _port, Action<Session> onConnect = null)
    {
        UpdateServer(_host, _port);
        Connect(onConnect);
    }

    public void Connect(Action<Session> onConnect = null)
    {
        if (connecting)
        {
            m_onConnect += onConnect;
            return;
        }

        Disconnect();

        m_onConnect += onConnect;

        state = SessionState.CONNECTING;

        var callback = m_onConnect;
        var validHost = host;
        var family = AddressFamily.Unknown;
        var hostInfo = Util.ParseString<string>(SDKManager.GetValidHost(host), false, ',');

        if (hostInfo.Length != 2 || hostInfo[0] != "0" && hostInfo[0] != "1")
        {
            family = AddressFamily.Unknown;
            Logger.LogException("<b><color=#FFFFFF>[{0}]</color></b> Could not resolve host [{1}:{2}]! Family:{3}", m_typeName, host, port, hostInfo.Length > 0 ? hostInfo[0] : "-1");
        }
        else
        {
            family = hostInfo[0] == "0" ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6;
            validHost = hostInfo[1];

            Logger.LogInfo("<b><color=#FFFFFF>[{0}]</color></b> Current Address: <b><color=#FAA000>[{1} -- {2}]</color></b>", m_typeName, family, validHost);
        }

        OnValidateHost(validHost, family, callback);
    }

    private void OnValidateHost(string validHost, AddressFamily family, Action<Session> callback)
    {
        if (family == AddressFamily.Unknown)
        {
            state = SessionState.CLOSED;

            moduleGlobal.ShowMessageNextFrame(1);
            Logger.LogError("<b><color=#FFFFFF>[{0}]</color></b> Connect to server [{1}:{2}] failed! Resolve address failed.", m_typeName, host, port);

            m_delayCall = callback;

            return;
        }

        m_socket = new Socket(family, SocketType.Stream, ProtocolType.Tcp);
        m_socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);

        try { OnCreateSocket(m_socket); }
        catch (Exception e) { Logger.LogException(e); }
        
        m_socket.BeginConnect(validHost, port, a =>
        {
            var socket = a.AsyncState as Socket;
            if (socket != m_socket) // if we recreate a new socket, return
            {
                if (socket != null && socket.Connected) socket.Disconnect(false);
                return;
            }

            try { socket.EndConnect(a); }
            catch (Exception e) { Logger.LogException("<b><color=#FFFFFF>[{0}]</color></b> Exception occured while connecting to server [{1}:{2}]! Exception: {3}", m_typeName, host, port, e); }

            if (socket.Connected)
            {
                state = SessionState.CONNECTED;

                Logger.LogInfo("<b><color=#FFFFFF>[{0}]</color></b> Connected to server [{1}:{2}]!", m_typeName, host, port);

                m_sender = new PacketSender(socket);
                m_receiver = new PacketReceiver(socket);

                SendPing();

                OnConnected();
                DispatchEvent(Events.SESSION_CONNECTED);
            }
            else
            {
                Logger.LogError("<b><color=#FFFFFF>[{0}]</color></b> Connect to server [{1}:{2}] failed!", m_typeName, host, port);
                state = SessionState.CLOSED;
                Disconnect();
            }

            m_delayCall = callback;
        }, m_socket);
    }

    public void Disconnect()
    {
        if (m_receiver != null) m_receiver.Destroy();
        if (m_sender != null) m_sender.Destroy();

        m_sender    = null;
        m_receiver  = null;
        m_onConnect = null;
        m_delayCall = null;

        m_waitPing = 0;

        var s = state;
        state = SessionState.CLOSED;

        if (m_socket != null)
        {
            Logger.LogInfo("<b><color=#FFFFFF>[{0}]</color></b> {1}", m_typeName, s == SessionState.CONNECTING ? "Connecting aborted!" : s == SessionState.CONNECTED ? "Disconnect from server!" : "Socket closed.");

            var tmp = m_socket;
            m_socket = null;
            tmp.Close();
        }
    }

    public virtual void Send(PacketObject packet)
    {
        if (connected && packet != null && !packet.destroyed)
        {
            #if UNITY_EDITOR
            if (LocalServer.instance.AutoHandlePacket(packet.GetType(), this)) return;
            #endif

            var p = packet.BuildPacket();

            #if DEVELOPMENT_BUILD || UNITY_EDITOR
            var f = ConfigManager.Get<ProtocolLoggerFilter>(packet._ID);
            if (!f || !f.disabled)
            {
                if (!f || !f.noDetail)
                {
                    try { Logger.Log(LogType.SEND, "Send: [{0}:{1}-{2},{3}] {4}", p.ID, p.dataSize, Level.realTime, packet._name, LitJson.JsonMapper.ToJson(packet, true, 5, true)); }
                    catch (Exception e)
                    {
                        Logger.Log(LogType.SEND, "Send: [{0}:{1}-{2},{3}]", p.ID, p.dataSize, Level.realTime, packet._name);
                        Logger.LogException(e);
                    }
                }
                else Logger.Log(LogType.SEND, "Send: [{0}:{1}-{2},{3}]", p.ID, p.dataSize, Level.realTime, packet._name);

                if (f && f.cpp) packet.LogCppString();
            }
            #endif

            m_sender.Send(p);
        }

        if (!packet.dontDestroyOnSend) packet.Destroy();
    }

    public override void OnRootUpdate(float diff)
    {
        if (m_delayCall != null)
        {
            var dc = m_delayCall;
            m_delayCall = null;
            dc.Invoke(this);
        }

        if (FightRecordManager.IsRecovering)
        {
            var ps = FightRecordManager.SimulateUpdate(diff);
            if (ps != null && ps.Count > 0)
            {
                foreach (var p in ps)
                {
                    HandlePacket(p);
                }
                ps.Clear();
            }
        }
        else if (m_receiver != null)
        {
            var ps = m_receiver.ReceivedPackets();

            foreach (var p in ps)
            {
                FightRecordManager.Record(p);

                HandlePacket(p);
            }
            ps.Clear();
        }

        if (m_lastState == SessionState.CONNECTED && (m_socket == null || !m_socket.Connected))  // we lost connection!
        {
            Disconnect();
            OnLostConnection();

            DispatchEvent(Events.SESSION_LOST_CONNECTION);
            DispatchModuleEvent(EventLostConnection);
        }
        m_lastState = state;

        if (m_lastState == SessionState.CONNECTED && m_waitPing > 0 && (m_waitPing -= diff) <= 0) SendPing();
        localTimeOffset += diff;
        if (localTimeOffset >= 1)
        {
            localTimeOffset -= 1;
            serverLocalTimeStamp += 1;
            serverTimeStamp += 1;
        }
    }

    public void HandlePacket(PacketObject p)
    {
        if (p == null) return;
        var handlers = m_callBacks.Get(p._ID);
        if (handlers == null || handlers.Count < 1 || (!connected && !FightRecordManager.IsRecovering)) // if we lost connection while processing packets, just destroy packets
        {
            p.Destroy();
            return;
        }

        foreach (var h in handlers) h.Invoke(p);

        if (!p.dontDestroyOnRecv) p.Destroy();
    }

    protected virtual void OnConnected() { }
    protected virtual void OnLostConnection() { }
    protected virtual void OnCreateSocket(Socket socket) { }

    void _Packet(ScPing p)
    {
        if ((p.type & 0x01) == 0) return;

        ping = Level.realTime - m_lastPing;

        serverTimeStamp = (int)p.timestamp;
        serverLocalTimeStamp = serverTimeStamp + serverTimeZoneDiff;

        m_waitPing = pingInterval;

        DispatchEvent(Events.EVENT_PING_UPDATE);
        localTimeOffset = 0;
    }

    void _Packet(ScSystemSetting p)
    {
        serverTimeZoneDiff = (int)p.serverTimeDiff;
        serverLocalTimeStamp = serverTimeStamp + serverTimeZoneDiff;
        localTimeOffset = 0;
    }
}