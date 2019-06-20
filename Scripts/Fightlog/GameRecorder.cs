// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-10-07      14:36
//  * LastModify：2018-10-07      14:49
//  ***************************************************************************************************/

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#endregion

public class GameRecorder
{
    public static IGameRecordData CreateRecodeData<T>() where T : IGameRecordData
    {
        var data = Activator.CreateInstance<T>();
        data.DataType = typeof (T);
        return data;
    }

    public static IGameRecordData CreateRecodeData(Type rType)
    {
        var data = Activator.CreateInstance(rType) as IGameRecordData;
        if(data != null)
            data.DataType = rType;
        return data;
    }

    const int MAX_FILES = 20;

    private bool isRecording;

    public IGameRecordData         GameRecordDataHandle;

    public bool reward;


    public bool IsRecording { get { return isRecording; } }
    public const string Path = "GameRecorder/Recorder";

    public static string GetFullPath(string rName)
    {
        return $"{Path}/{rName}.gr";
    }

    public void Start()
    {
        if (GameRecordDataHandle == null)
            return;
        reward = false;
        isRecording = true;
    }

    public void End()
    {
        isRecording = false;
    }

    public void Record(PacketObject msg, int rFrame)
    {
        if (null == GameRecordDataHandle || msg == null)
            return;
        
        var command = Command.Create();
        command.Init(msg, rFrame);
        GameRecordDataHandle.EnqueueCommand(command);
    }

    private void SaveToFile(string rFileName)
    {
        if (!reward || IsRecording)
            return;
        _SaveToFile(rFileName);
    }

    public void SaveToFileAsync(string rFileName)
    {
        if (null == GameRecordDataHandle)
            return;
        Root.instance.StartCoroutine(WaitReward(rFileName));
    }

    private IEnumerator WaitReward(string rFileName)
    {
        while (!reward)
            yield return 0;
        _SaveToFile(rFileName);
    }

    public void _SaveToFile(string rFileName)
    {
        if (null == GameRecordDataHandle)
        {
            return;
        }
        var dirPath = Util.GetDataPath() + Path;
        if (Directory.Exists(dirPath))
        {
            //日志过多。删除
            DirectoryInfo dInfo = new DirectoryInfo(dirPath);
            var files = dInfo.GetFiles();
            if (files.Length > MAX_FILES)
                Directory.Delete(dirPath, true);
        }
        var path = GetFullPath(rFileName);
        var stream = new MemoryStream();
        stream.Write(FightRecordManager.NowTimeToFileName());
        //记录关卡ID
        stream.Write(GameRecordDataHandle.DataType.ToString());
        GameRecordDataHandle.Save(stream);

        stream.Seek(0, SeekOrigin.Begin);
        var bytes = new byte[stream.Length];
        stream.Read(bytes, 0, (int)stream.Length);
        Logger.LogInfo($" save file : {path}" );
        Util.SaveFile(path, bytes);

        stream?.Close();
    }

    public bool InstanceHandle<T>() where T : IGameRecordData
    {
        GameRecordDataHandle = CreateRecodeData<T>();
        return GameRecordDataHandle != null;
    }

    public void Set<T>(T rMsg) where T : PacketObject<T>
    {
        if (null == GameRecordDataHandle)
        {
            return;
        }
        var t = typeof (T);
        GameRecordDataHandle?.Set(t, rMsg.Clone());

        if (rMsg is ScRoomReward || rMsg is ScChaseTaskOver)
        {
            reward = true;
        }
    }
}
