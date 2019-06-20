/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Utility
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-09-21
 * 
 ***************************************************************************************************/

using UnityEngine;
using SevenZip;
using SevenZip.Compression.LZMA;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static partial class Util
{
    #region Path helpers

    public static string GetPlatformName()
    {
#if UNITY_EDITOR
        return GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
#else
        return GetPlatformForAssetBundles(Application.platform);
#endif
    }

#if UNITY_EDITOR
    public static string GetPlatformForAssetBundles(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.Android:
                return "Android";
            case BuildTarget.iOS:
                return "iOS";
            case BuildTarget.WebGL:
                return "WebGL";
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "Windows";
            case BuildTarget.StandaloneOSX:
                return "OSX";
            // Add more build targets for your own.
            // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
            default:
                return null;
        }
    }
#endif

    public static string GetPlatformForAssetBundles(RuntimePlatform platform)
    {
        switch (platform)
        {
            case RuntimePlatform.Android:
                return "Android";
            case RuntimePlatform.IPhonePlayer:
                return "iOS";
            case RuntimePlatform.WebGLPlayer:
                return "WebGL";
            case RuntimePlatform.WindowsPlayer:
                return "Windows";
            case RuntimePlatform.OSXPlayer:
                return "OSX";
            // Add more build targets for your own.
            // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
            default:
                return null;
        }
    }

    /// <summary>
    /// Application data file path (read & write)
    /// Path ends with '/'
    /// </summary>
    public static string dataPath { get { return m_dataPath ?? GetDataPath(); } }
    private static string m_dataPath = null;
    /// <summary>
    /// Get external data path (read & write)
    /// Path ends with '/'
    /// </summary>
    public static string externalDataPath { get { return m_externalDataPath ?? GetExternalDataPath(); } }
    private static string m_externalDataPath = null;

    /// <summary>
    /// Get application data file path (read & write)
    /// Path ends with '/'
    /// </summary>
    /// <returns></returns>
    public static string GetDataPath()
    {
        if (m_dataPath == null)
        {
            if (Application.isEditor)
                m_dataPath = Environment.CurrentDirectory;
            else if (Application.isMobilePlatform || Application.isConsolePlatform)
                m_dataPath = Application.persistentDataPath;
            else // For standalone player.
                m_dataPath = Application.dataPath;

            m_dataPath = m_dataPath.Replace("\\", "/");
            if (!m_dataPath.EndsWith("/")) m_dataPath += "/";

            Logger.LogInfo("Data path set to: <color=#00DDFF><b>{0}</b></color>", m_dataPath);
        }

        return m_dataPath;
    }

    /// <summary>
    /// Get application external data path (read & write)
    /// Path ends with '/'
    /// </summary>
    /// <returns></returns>
    public static string GetExternalDataPath()
    {
        if (m_externalDataPath == null)
        {
            if (Application.isEditor) m_externalDataPath = dataPath;
            else if (Application.isMobilePlatform || Application.isConsolePlatform)
            {
                #if UNITY_ANDROID
                m_externalDataPath = SDKManager.CallAliya<string>("GetExternalDataPath", "kzwg");
                if (string.IsNullOrEmpty(m_externalDataPath))
                #endif
                m_externalDataPath = Application.persistentDataPath;
            }
            else m_externalDataPath = Application.dataPath; // For standalone player.

            m_externalDataPath = m_externalDataPath.Replace("\\", "/");
            if (!m_externalDataPath.EndsWith("/")) m_externalDataPath += "/";

            Logger.LogInfo("External data path set to: <color=#00DDFF><b>{0}</b></color>", m_externalDataPath);
        }

        return m_externalDataPath;
    }

    #endregion

    #region File helpers

    /// <summary>
    /// Load text data from file (Encoding with utf8)
    /// </summary>
    /// <param name="path">Path relative to data path</param>
    /// <param name="useLastAutoIncrease">See Util.SaveFile</param>
    /// <param name="external">Use external data path ?</param>
    /// <returns>If failed, return empty string else return text data</returns>
    public static string LoadTextFile(string path, bool useLastAutoIncrease = false, bool external = false)
    {
        var data = LoadFile(path, useLastAutoIncrease, external);
        if (data == null) return "";

        return System.Text.Encoding.UTF8.GetString(data);
    }

    /// <summary>
    /// Load data from file
    /// </summary>
    /// <param name="path">Path relative to data path</param>
    /// <param name="data"></param>
    /// <param name="useLastAutoIncrease">See Util.SaveFile</param>
    /// <param name="external">Use external data path ?</param>
    /// <returns>If failed, return null else return binary data</returns>
    public static byte[] LoadFile(string path, bool useLastAutoIncrease = false, bool external = false)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
        {
            Logger.LogError("Util::LoadFile: Invalid path [{0}], path can not be null or empty or whitespace", path);
            return null;
        }

        var p = path.Replace("\\", "/").Replace("//", "/");
        if (p[0] == '/') p = p.Length > 1 ? p.Substring(1) : "";

        if (string.IsNullOrEmpty(p) || string.IsNullOrWhiteSpace(p))
        {
            Logger.LogError("Util::LoadFile: Invalid path [{0}], path can not be null or empty or whitespace", path);
            return null;
        }

        var r = external ? externalDataPath : dataPath;
        var d = r + Path.GetDirectoryName(p).Replace("\\", "/") + "/";

        if (!Directory.Exists(d)) return null;

        var fp = r + p;
        var fn = Path.GetFileName(fp);

        if (string.IsNullOrEmpty(fn))
        {
            Logger.LogError("Util::LoadFile: Invalid path [{0}], file name can not be null or empty", path);
            return null;
        }

        byte[] data = null;

        try
        {
            if (useLastAutoIncrease)
            {
                fn = Path.GetFileNameWithoutExtension(fp);
                var n = 0;
                var ex = Path.GetExtension(fp);
                var fss = Directory.GetFiles(d, "*" + ex);
                
                foreach (var nf in fss)
                {
                    var ffn = Path.GetFileNameWithoutExtension(nf);
                    if (!ffn.StartsWith(fn + "_")) continue;
                    var nn = Parse<int>(ffn.Replace(fn + "_", ""));
                    if (nn > n) n = nn;
                }

                fp = fp.Replace(fn + ex, fn + "_" + n + ex);
            }

            if (!File.Exists(fp)) return null;

            var fs = File.OpenRead(fp);
            data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            fs.Close();
        }
        catch (Exception e)
        {
            Logger.LogException(e);
            return null;
        }

        return data;
    }

    /// <summary>
    /// Save text data to file (Encoding with utf8)
    /// </summary>
    /// <param name="path">Path relative to data path</param>
    /// <param name="data"></param>
    /// <param name="autoIncreaseSuffix">Auto append a number suffix after file name ?</param>
    /// <param name="external">Use external data path ?</param>
    /// <returns>If failed, return null else return full file path</returns>
    public static string SaveFile(string path, string data, bool autoIncreaseSuffix = false, bool external = false)
    {
        return SaveFile(path, System.Text.Encoding.UTF8.GetBytes(data), autoIncreaseSuffix, external);
    }

    /// <summary>
    /// Save data to file
    /// </summary>
    /// <param name="path">Path relative to data path</param>
    /// <param name="data"></param>
    /// <param name="autoIncreaseSuffix">Auto append a number suffix after file name ?</param>
    /// <param name="external">Use external data path ?</param>
    /// <returns>If failed, return null else return full file path</returns>
    public static string SaveFile(string path, byte[] data, bool autoIncreaseSuffix = false, bool external = false)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
        {
            Logger.LogError("Util::SaveFile: Invalid path [{0}], path can not be null or empty or whitespace", path);
            return null;
        }

        var p = path.Replace("\\", "/").Replace("//", "/");
        if (p[0] == '/') p = p.Length > 1 ? p.Substring(1) : "";

        if (string.IsNullOrEmpty(p) || string.IsNullOrWhiteSpace(p))
        {
            Logger.LogError("Util::SaveFile: Invalid path [{0}], path can not be null or empty or whitespace", path);
            return null;
        }

        var r  = external ? externalDataPath : dataPath;
        var d  = r + Path.GetDirectoryName(p).Replace("\\", "/") + "/";
        var fp = r + p;
        var fn = Path.GetFileName(fp);

        if (string.IsNullOrEmpty(fn))
        {
            Logger.LogError("Util::SaveFile: Invalid path [{0}], file name can not be null or empty", path);
            return null;
        }

        try
        {
            if (!Directory.Exists(d)) Directory.CreateDirectory(d);

            if (autoIncreaseSuffix)
            {
                fn = Path.GetFileNameWithoutExtension(fp);
                var n = 0;
                var ex = Path.GetExtension(fp);

                while (File.Exists(d + fn + "_" + n + ex)) ++n;

                fp = fp.Replace(fn + ex, fn + "_" + n + ex);
            }

            var fs = File.Create(fp, data.Length);
            fs.Write(data, 0, data.Length);
            fs.Close();
        }
        catch (Exception e)
        {
            Logger.LogException(e);
            return null;
        }

        return fp;
    }

    /// <summary>
    /// Load an image from local image file
    /// </summary>
    /// <param name="path">File path</param>
    /// <param name="external">Use external data path ?</param>
    /// <returns></returns>
    public static Texture2D LoadImage(string path, bool external = false)
    {
        var data = LoadFile(path, false, external);
        if (data != null)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.LoadImage(data, true);
            texture.name = Path.GetFileNameWithoutExtension(path);
            return texture;
        }
        return null;
    }

    #endregion

    #region Compression

    private static MemoryStream m_sIStream = new MemoryStream();
    private static MemoryStream m_sOStream = new MemoryStream();
    private static Encoder      m_sEncoder = new Encoder();

    private static CoderPropID[] m_ids   = { CoderPropID.DictionarySize, CoderPropID.PosStateBits, CoderPropID.LitContextBits, CoderPropID.LitPosBits, CoderPropID.Algorithm, CoderPropID.NumFastBytes, CoderPropID.MatchFinder, CoderPropID.EndMarker };
    private static object[]      m_props = { 1 << 16,                    0,                        0,                          0,                      0,                     64,                       "bt4",                   false };

    /// <summary>
    /// 压缩数据流
    /// 压缩数据将使用缓存的静态数据流作为缓冲区
    /// 即此函数只能在单线程环境下使用
    /// </summary>
    /// <param name="buffer">待压缩的数据</param>
    /// <param name="flag">若为非负数，该标志将添加到压缩后的数据头部</param>
    /// <returns>若压缩成功 返回压缩后的数据 否则返回 null</returns>
    public static byte[] CompressData(byte[] buffer, int flag = -1)
    {
        if (buffer == null || buffer.Length < 1) return null;

        var watcher = TimeWatcher.Watch("Util.CompressData");

        m_sOStream.Position = 0;
        m_sIStream.Position = 0;
        m_sOStream.SetLength(0);
        m_sIStream.SetLength(buffer.Length);
        m_sIStream.Write(buffer, 0, buffer.Length);

        if (flag > -1) m_sOStream.WriteByte((byte)flag);

        m_sEncoder.SetCoderProperties(m_ids, m_props);
        m_sEncoder.WriteCoderProperties(m_sOStream);

        long dataSize = m_sIStream.Length;

        for (int i = 0; i < 8; i++)
            m_sOStream.WriteByte((byte)(dataSize >> (8 * i)));
        m_sIStream.Position = 0;
        m_sEncoder.Code(m_sIStream, m_sOStream, -1, -1, null);

        m_sOStream.Position = 0;
        var data = new byte[m_sOStream.Length];
        m_sOStream.Read(data, 0, data.Length);

        watcher.UnWatch();

        return data;
    }

    /// <summary>
    /// 解压缩数据
    /// </summary>
    /// <param name="buffer">要解压的数据</param>
    /// <returns>若解压成功 返回解压后的数据 否则返回 null</returns>
    public static byte[] DecompressData(byte[] buffer, int off = 0)
    {
        var watcher = TimeWatcher.Watch("Util.DecompressData");

        var inStream   = new MemoryStream(buffer, off, buffer.Length - 1);
        var decoder    = new Decoder();
        var properties = new byte[5];

        if (inStream.Read(properties, 0, 5) != 5)
        {
            Logger.LogError("Util::DecompressData: Invalid compress data.");
            inStream.Dispose();
            watcher.UnWatch(false);
            return null;
        }

        decoder.SetDecoderProperties(properties);

        long outSize = 0;
        for (int i = 0; i < 8; i++)
        {
            var v = inStream.ReadByte();
            if (v < 0)
            {
                inStream.Dispose();
                watcher.UnWatch(false);
                return null;
            }
            outSize |= ((long)(byte)v) << (8 * i);
        }

        var outStream  = new MemoryStream();
        var compressedSize = inStream.Length - inStream.Position;

        decoder.Code(inStream, outStream, compressedSize, outSize, null);

        outStream.Position = 0;
        var data = new byte[outStream.Length];
        outStream.Read(data, 0, data.Length);

        inStream.Dispose();
        outStream.Dispose();

        watcher.UnWatch();

        return data;
    }

    #endregion

    // @TODO: Implement common list pool manager
    public static IReadOnlyList<T> EmptyList<T>()
    {
        var code = typeof (T).GetHashCode();
        if (!cache.ContainsKey(code))
            cache.Add(code, new List<T>());
        return cache[code] as List<T>;
    }

    static Dictionary<int, IList> cache = new Dictionary<int, IList>();
}
