// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-09-14      14:43
//  * LastModify：2018-09-14      14:43
//  ***************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

public class FightLog
{
    public const string Path = "GameRecorder/FightLog";

    private const int MAX_FILES = 20;

    public bool writeBinary { get; set;}

    private Dictionary<Type, MethodInfo[]> typeMethodInfos = new Dictionary<Type, MethodInfo[]>();

    public MemoryStream writer;

    public string GetFullPath(string rName)
    {
        return $"{Path}/{rName}.log";
    }

    public void Log(PacketObject rLog, int rFrame, StackTrace rStack)
    {
        if (writer == null)
            return;

        var field = rLog.GetType().GetField("frame");
        field?.SetValue(rLog, (ushort)rFrame);
        var c = Command.Create();
        c.Init(rLog, rFrame);
        c.Serialize(writer);
        c.Destroy();

        if (rStack != null)
        {
            var l = PacketObject.Create<LogStack>();
            var list = new List<LogMethod>();
            for (var i = 0; i < rStack.FrameCount; i++)
            {
                var m = PacketObject.Create<LogMethod>();
                var methodInfo = rStack.GetFrame(i)?.GetMethod();
                var t = methodInfo?.DeclaringType;
                if (t != null)
                {
                    if (t.GetInterface("IGameRecordData") != null || t == typeof (Session)) continue; 
                    m.typeHash = t.Name.GetHashCode();
                    MethodInfo[] arr = null;
                    if (!typeMethodInfos.TryGetValue(t, out arr))
                    {
                        arr = t.GetMethods();
                        typeMethodInfos.Add(t, arr);
                    }
                    m.methodIndex = (ushort)Array.FindIndex(arr, item => item == methodInfo);

                }
                list.Add(m);
            }
            l.method = list.ToArray();
            var com = Command.Create();
            com.Init(l, rFrame);
            com.Serialize(writer);
            com.Destroy();
        }
    }

    public void Start()
    {
        if (writer != null)
        {
            writer.Close();
            writer = null;
        }
        writer = new MemoryStream();
    }

    public void End()
    {
    }

    public void SaveToFile(string rFileName)
    {
        if (writer == null)
            return;

        var path = Util.GetDataPath() + Path;
        if (Directory.Exists(path))
        {
            //日志过多。删除
            DirectoryInfo dInfo = new DirectoryInfo(path);
            var files = dInfo.GetFiles();
            if (files.Length > MAX_FILES)
                Directory.Delete(path, true);
        }

        var p = GetFullPath(rFileName);
        try
        {
            writer.Seek(0, SeekOrigin.Begin);
            byte[] data = new byte[writer.Length];
            writer.Read(data, 0, (int)writer.Length);
            Util.SaveFile(p, Util.CompressData(data));
            writer.Close();
            writer = null;
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
        }
    }
}
