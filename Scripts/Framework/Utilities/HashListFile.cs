/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Hash List File Utility
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-05-14
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class HashListFile
{
    public static readonly string hashListFilePath = Util.dataPath + LocalFilePath.HASHLIST + "/";

    public static string GetFileHash(string path)
    {
        var md5 = string.Empty;
        try
        {
            var fs = new FileStream(path, FileMode.Open);
            md5 = fs.GetMD5();
            fs.Close();
        }
        catch (Exception e)
        {
            Logger.LogException("HashListFile::GetFileHash: Get file [{0}] hash failed.", path);
            Logger.LogException(e);

            md5 = string.Empty;
        }

        return md5;
    }

    public static Dictionary<string, string> ParseAssetHashListFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return new Dictionary<string, string>();

        var fp = hashListFilePath + fileName;
        if (!File.Exists(fp)) return new Dictionary<string, string>();

        var data = string.Empty;
        try
        {
            var fs = File.OpenText(fp);
            data = fs.ReadToEnd();
            fs.Close();
        }
        catch (Exception e)
        {
            Logger.LogException("HashListFile::ParseAssetHashListFile: Load hash list file [{0}] failed.", fp);
            Logger.LogException(e);
            return new Dictionary<string, string>();
        }

        return ParseAssetHashListData(data);
    }

    public static Dictionary<string, string> ParseAssetHashListData(string data)
    {
        var list = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(data)) return list;

        var ll = Util.ParseString<string>(data, false, '\n');
        foreach (var l in ll)
        {
            var pair = Util.ParseString<string>(l, false, '|');
            if (pair.Length != 2) continue;
            list.Set(pair[1], pair[0]);
        }
        return list;
    }

    public static string ParseAssetHashListData(Dictionary<string, string> data)
    {
        var sb = new StringBuilder();
        foreach (var p in data)
        {
            sb.Append(p.Value);
            sb.Append('|');
            sb.Append(p.Key);
            sb.Append('\n');
        }
        return sb.ToString();
    }

    public static void ReplaceHashListFile(string fileName, string name, string hash)
    {
        string md5 = null;
        name += "\n";
        hash = hash + "|" + name;

        var fp = hashListFilePath + fileName;
        try
        {
            if (!Directory.Exists(hashListFilePath)) Directory.CreateDirectory(hashListFilePath);
            if (File.Exists(fp))
            {
                var fs = File.OpenText(fp);
                md5 = fs.ReadToEnd();
                fs.Close();

                if (md5.Contains(name)) md5 = System.Text.RegularExpressions.Regex.Replace(md5, @"\w{32}\|" + name, hash);
                else md5 += hash;
            }
            else md5 = hash;

            var ss = File.CreateText(fp);
            ss.Write(md5);
            ss.Close();
        }
        catch (Exception e)
        {
            Logger.LogException("HashListFile::ReplaceHashListFile: Save hash list file [{0}] failed", fp);
            Logger.LogException(e);
        }
    }

    public static void DeleteHashListFile(string fileName)
    {
        var fp = hashListFilePath + fileName;
        try
        {
            if (!Directory.Exists(hashListFilePath)) Directory.CreateDirectory(hashListFilePath);
            if (File.Exists(fp)) File.Delete(fp);
        }
        catch (Exception e)
        {
            Logger.LogException("HashListFile::DeleteHashListFile: Delete hash list file [{0}] failed", fp);
            Logger.LogException(e);
        }
    }
}
