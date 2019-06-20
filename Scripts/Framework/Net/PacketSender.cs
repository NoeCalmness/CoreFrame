/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Async packet sender for Session module.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-27
 * 
 ***************************************************************************************************/

using System.Net.Sockets;

class PacketSender : IDestroyable
{
    public long sendedBytes { get; protected set; }

    public bool destroyed { get; protected set; }
    public bool pendingDestroy { get { return false; } }

    private Socket m_socket;

    public PacketSender(Socket socket)
    {
        m_socket  = socket;
    }

    public void Send(Packet packet)
    {
        if (destroyed || !m_socket.Connected) return;

        packet.Flush();
        var bytes = packet.bytes;

        if (bytes.Length > Packet.maxSize)
        {
            Logger.LogException("PacketSender::Send: Toooooooooo large packet size, discard!! Packet: [ID: {0}, curSize: {1}]", packet.ID, bytes.Length);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPaused = true;
#endif
            packet.Destroy();
            return;
        }

        m_socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, OnSend, packet);
    }

    private void OnSend(System.IAsyncResult a)
    {
        if (m_socket != null)
        {
            var count = m_socket.EndSend(a);
            sendedBytes += count;
        }

        var sended = a.AsyncState as Packet;
        sended.Destroy();
    }

    public void Destroy()
    {
        if (destroyed) return;
        destroyed = true;

        m_socket = null;
    }
}