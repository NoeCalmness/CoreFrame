/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Async packet receiver for Session module.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-27
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
#if NETSTAT
using System.Diagnostics;
#endif

class PacketReceiver : IDestroyable
{
    public long receivedBytes { get; protected set; }
    public bool destroyed { get; protected set; }
    public bool pendingDestroy { get { return false; } }

    private Socket              m_socket;
    private byte[]              m_buffer;
    private int                 m_rpos;
    private int                 m_wpos;
    private List<PacketObject>  m_back;
    private List<PacketObject>  m_front;
    private object              m_guard_write;
    private Packet              m_packet;
    private object              m_guard_packet;
    private Thread              m_thread;

    #if DEVELOPMENT_BUILD || UNITY_EDITOR
        private LitJson.JsonWriter m_jsonWriter = new LitJson.JsonWriter() { CsFormat = true, PrettyPrint = true };
        private System.Text.StringBuilder m_sb = new System.Text.StringBuilder();
    #endif

    #region NetStat statistic

    #if NETSTAT
        public float maxNet = -1;
        public float maxParse = -1;
        public List<int> networkDelay = new List<int>(1000);
        public List<int> parseDelay = new List<int>(1000);

        public bool netStat = false;
        public bool pauseNetStatistic = false;
        public bool pauseParseStatistic = false;

        private Stopwatch m_net, m_parse;

        public void ClearStat(int type = -1)
        {
            if (type == 0 || type != 1)
            {
                networkDelay.Clear();
                m_net = null;
                maxNet = -1;
            }

            if (type == 1 || type != 0)
            {
                parseDelay.Clear();
                m_parse = null;
                maxParse = -1;
            }
        }
    #endif

    #endregion

    public PacketReceiver(Socket socket)
    {
        m_socket  = socket;
        m_buffer  = new byte[2048];
        m_rpos    = 0;
        m_wpos    = 0;
        m_back    = new List<PacketObject>();
        m_front   = new List<PacketObject>();

        m_guard_write   = new object() { };
        m_guard_packet  = new object() { };

        m_packet  = Packet.Build(0);

        m_thread = new Thread(StartReceive);

        m_thread.Start();
    }

    private void StartReceive()
    {
        while (true)
        {
            if (m_socket == null) break;
            Receive();
        }
    }

    private void Receive()
    {
        try
        {
            if (destroyed) return;

            if (m_wpos == m_buffer.Length)
            {
                if (m_rpos > 0)  // If the buffer still has free space, move received data to buffer start
                {
                    var received = m_wpos - m_rpos;
                    var tmp = new byte[received];
                    Array.Copy(m_buffer, m_rpos, tmp, 0, received);
                    Array.Copy(tmp, m_buffer, received);

                    m_rpos = 0;
                    m_wpos = received;
                }
                else // or increase buffer size
                {
                    Array.Resize(ref m_buffer, m_buffer.Length * 2);
                    Logger.LogDetail("Buffer is full, increase to {0} ", m_buffer.Length);
                }
            }

            var count = m_socket.Receive(m_buffer, m_wpos, m_buffer.Length - m_wpos, SocketFlags.None);
            if (count > 0) OnReceive(count);
            else Close();
        }
        catch (SocketException e)
        {
            Close();
            Logger.LogException(e);
        }
        catch (ThreadAbortException)
        {
            Close();

            Logger.LogInfo("PacketReceiver::StartReceive: Thread aborted!");
            Thread.ResetAbort();
        }
        catch (Exception e)
        {
            Close();
            Logger.LogException(e);
        }
    }

    private void OnReceive(int count)
    {
        receivedBytes += count;
        m_wpos += count;

        #region NetStat statistic

        #if NETSTAT
        if (!pauseNetStatistic && netStat)
        {
            if (m_net == null)
            {
                m_net = new Stopwatch();
                m_net.Start();
            }

            var diff = (int)m_net.ElapsedMilliseconds + 5;
            m_net.Restart();

            if (diff + 10 > maxNet) maxNet = diff + 10;
            if (maxNet > 1000) maxNet = 1000;
            if (networkDelay.Count >= 1000) networkDelay.RemoveAt(0);
            networkDelay.Add(diff);
        }

        if (!pauseParseStatistic && netStat)
        {
            if (m_parse == null)
            {
                m_parse = new Stopwatch();
                m_parse.Start();
            }

            m_parse.Restart();

            ProcessData();

            m_parse.Stop();

            var diff = (int)m_parse.ElapsedMilliseconds + 5;

            if (diff + 10 > maxParse) maxParse = diff + 10;
            if (maxParse > 1000) maxParse = 1000;
            if (parseDelay.Count >= 1000) parseDelay.RemoveAt(0);
            parseDelay.Add(diff);
        }
        else
        #endif

        #endregion

        ProcessData();
    }

    private void ProcessData()
    {
        var received = m_wpos - m_rpos;
        while (received >= Packet.headerSize)
        {
            if (destroyed || !m_socket.Connected) return; // if the session is disconnected or receiver destroyed, stop processing data immdiately

            var mask   = ByteConverter.ToUInt32(m_buffer, m_rpos); // low 24 bit = length, hight 8 bit = bitmask
            var length = (int)((mask & 0xFFFFFF) + 4);  // server side packet length does not contain mask length
            //var bimask = mask >> 24;

            if (length < Packet.headerSize) // got invalid packet data, stop processing data immdiately and disconnect from server
            {
                Logger.LogException("PacketReceiver got invalid packet data header, header length is {0} but required {1}", length, Packet.headerSize);

                Close();
                return;
            }

            if (received >= length)
            {
                var id     = ByteConverter.ToUInt16(m_buffer, m_rpos + 4);
                var header = id | (long)length << 16;

                var bytes = new byte[length];
                Array.Copy(m_buffer, m_rpos, bytes, 0, length);
                m_rpos += length;
                received -= length;

                PacketObject o;

                lock (m_guard_packet)
                {
                    m_packet.Set(header, bytes);
                    o = PacketObject.Create(m_packet);

                    #if DEVELOPMENT_BUILD || UNITY_EDITOR
                    if (o != null)
                    {
                        var f = ConfigManager.Get<ProtocolLoggerFilter>(o._ID);
                        if (!f || !f.disabled)
                        {
                            if (!f || !f.noDetail)
                            {
                                m_jsonWriter.Reset();
                                try
                                {
                                    LitJson.JsonMapper.ToJson(o, m_jsonWriter, 5);
                                    Logger.Log(LogType.RECV, "Recv: [{0}:{1}-{2},{3}] {4}", m_packet.ID, m_packet.dataSize, Level.realTime, o._name, m_jsonWriter);
                                }
                                catch (Exception ee)
                                {
                                    m_jsonWriter = new LitJson.JsonWriter()  { CsFormat = true, PrettyPrint = true };
                                    Logger.Log(LogType.RECV, "Recv: [{0}:{1}-{2},{3}]", m_packet.ID, m_packet.dataSize, Level.realTime, o._name);
                                    Logger.LogException(ee);
                                }
                            }
                            else Logger.Log(LogType.RECV, "Recv: [{0}:{1}-{2},{3}]", m_packet.ID, m_packet.dataSize, Level.realTime, o._name);

                            if (f && f.cpp) o.LogCppString(m_sb);
                        }
                    }
                    else Logger.LogError("Receive unknow packet: [{0}:{1}-{2}], packet not registered.", m_packet.ID, m_packet.dataSize, Level.realTime);
                    #endif

                    m_packet.Reset();
                }

                if (o != null)
                {
                    lock (m_guard_write)
                    {
                        m_front.Add(o);
                    }
                }
            }
            else break;
        }
    }

    public List<PacketObject> ReceivedPackets()
    {
        lock (m_guard_write)
        {
            var curr = m_front;
            m_front = m_back;
            m_back = curr;

            return m_back;
        }
    }

    public void Destroy()
    {
        if (destroyed) return;
        destroyed = true;

        m_thread.Abort();
        m_thread = null;

        m_wpos         = 0;
        m_rpos         = 0;
        m_buffer       = null;
        m_socket       = null;

        #region NetStat statistic

        #if NETSTAT
        m_net = null;
        m_parse = null;
        networkDelay.Clear();
        parseDelay.Clear();
        #endif

        #endregion

        lock (m_guard_packet)
        {
            #if DEVELOPMENT_BUILD || UNITY_EDITOR
                m_jsonWriter.Reset();
                m_jsonWriter = null;

                m_sb.Clear();
                m_sb = null;
            #endif

            m_packet.Destroy();
            m_packet = null;
        }

        lock (m_guard_write)
        {
            m_front.Clear(true);
            //m_back.Clear();  // Back queue will clear by Session

            m_front = null;
            m_back = null;
        }

        m_guard_write  = null;
        m_guard_packet = null;
    }

    private void Close()
    {
        if (m_socket == null) return;

        m_socket.Disconnect(false);
        m_socket.Shutdown(SocketShutdown.Both);
        m_socket.Close();
    }
}