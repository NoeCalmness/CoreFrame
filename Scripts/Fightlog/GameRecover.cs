// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-09-15      13:34
//  * LastModify：2018-09-15      13:34
//  ***************************************************************************************************/

using System;
using System.IO;

public class GameRecover
{
    public const string RecordFileSuffix = ".gr";
    public enum EnumPlayError
    {
        None,
        FileTypeError,
        FileDontExits,
        FileContentError,
    }

    public IGameRecordData GameRecordDataHandle;
    private string path;

    public GameRecover() { }

    public GameRecover(string rPath)
    {
        path = rPath;
        Load();
    }

    public string Path { get { return path; } }

    public void SetPath(string rPath)
    {
        if (!rPath.EndsWith(RecordFileSuffix))
            return;
        path = rPath;
    }


    public void Start()
    {
        GameRecordDataHandle.StartRecover();
    }

    public EnumPlayError StartLoad()
    {
        return Load();
    }

    public EnumPlayError StartLoad(string rPath)
    {
        SetPath(rPath);
        return Load();
    }

    private EnumPlayError Load()
    {
        if (!File.Exists(path))
            return EnumPlayError.FileDontExits;
        if (!path.EndsWith(RecordFileSuffix))
            return EnumPlayError.FileTypeError;
        try
        {
            var stream = File.OpenRead(path);
            stream.Seek(0, SeekOrigin.Begin);
            string time;
            stream.Read(out time);
            string t;
            stream.Read(out t);
            var dataType = Type.GetType(t);
            GameRecordDataHandle = GameRecorder.CreateRecodeData(dataType);
            GameRecordDataHandle?.Load(stream);
            stream.Close();
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
        }

        if (GameRecordDataHandle?.CommandsCount == 0)
            return EnumPlayError.FileContentError;

        return EnumPlayError.None;
    }

    public void SaveToJson()
    {
        var jsonPath = path.Replace(RecordFileSuffix, "_json.txt");
        var stream = File.CreateText(jsonPath);
        GameRecordDataHandle?.SaveToJson(stream);
        stream.Close();
    }

    public void End()
    {
        GameRecordDataHandle?.FightEnd();
    }
}
