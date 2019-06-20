/****************************************************************************************************
 * Copyright (C) 2017-2017 FengYunChuanShuo
 * 
 * URL list
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-06-11
 * 
 ***************************************************************************************************/

public class URLs
{
#if USE_EXTERNAL_SERVER_URL
    /// <summary> 下载地址 </summary>
    private static readonly string m_downloadUrl = "http://119.23.52.209:8087/download/";
    /// <summary> 版本校验 </summary>
    private static readonly string m_versionUrl  = "http://119.23.52.209:8087/{0}";
    /// <summary> 资源地址 </summary>
    private static readonly string m_assetUrl    = "http://119.23.52.209:8087/{0}assets/";
    /// <summary> 头像地址 </summary>
    private static readonly string m_avatarUrl   = "http://119.23.52.209:8087/{0}assets/Avatars/";
    /// <summary> 上传日志地址 </summary>
    private static readonly string m_uploadUrl   = "http://192.168.3.88:8080/res/fight_data";
#elif USE_PUBLIC_SERVER_URL
    /// <summary> 下载地址 </summary>
    private static readonly string m_downloadUrl = "http://kzwgdownload01.jingzheshiji.com/download_test01/";
    /// <summary> 版本校验 </summary>
    private static readonly string m_versionUrl  = "http://kzwgupdate01.jingzheshiji.com/update_test01/{0}";
    /// <summary> 资源地址 </summary>
    private static readonly string m_assetUrl    = "http://kzwgupdate01.jingzheshiji.com/update_test01/{0}assets/";
    /// <summary> 头像地址 </summary>
    private static readonly string m_avatarUrl   = "http://kzwgupdate01.jingzheshiji.com/update_test01/{0}assets/Avatars/";
    /// <summary> 上传日志地址 </summary>
    private static readonly string m_uploadUrl   = "http://192.168.3.88:8080/res/fight_data";
#else
    /// <summary> 下载地址 </summary>
    private static readonly string m_downloadUrl = "http://192.168.3.254:8089/download/";
    /// <summary> 版本校验 </summary>
    private static readonly string m_versionUrl  = "http://192.168.3.254:8089/{0}";
    /// <summary> 资源地址 </summary>
    private static readonly string m_assetUrl    = "http://192.168.3.254:8089/{0}assets/";
    /// <summary> 头像地址 </summary>
    private static readonly string m_avatarUrl   = "http://192.168.3.254:8089/{0}assets/Avatars/";

    private static readonly string m_uploadUrl   = "http://192.168.3.88:8080/res/fight_data";
#endif

    /// <summary>
    /// 版本校验地址
    /// </summary>
    public static string version
    {
        get
        {
            if (m_version == null) m_version = Util.Format(m_versionUrl, alias);
            return m_version;
        }
    }
    /// <summary>
    /// 更新包下载地址
    /// </summary>
    public static string download
    {
        get
        {
            if (m_download == null) m_download = Util.Format(m_downloadUrl, alias);
            return m_download;
        }
    }
    /// <summary>
    /// 资源更新地址
    /// </summary>
    public static string asset
    {
        get
        {
            if (m_asset == null) m_asset = Util.Format(m_assetUrl, tag);
            return m_asset;
        }
    }
    /// <summary>
    /// 头像更新地址
    /// </summary>
    public static string avatar
    {
        get
        {
            if (m_avatar == null) m_avatar = Util.Format(m_avatarUrl, tag);
            return m_avatar;
        }
    }

    public static string upload
    {
        get
        {
            if (m_upload == null) m_upload = m_uploadUrl;
            return m_upload;
        }
    }

    public static void OverrideUrls(string downloadServer, string assetServer)
    {
        if (!string.IsNullOrEmpty(downloadServer) && !string.IsNullOrWhiteSpace(downloadServer))
        {
            if (!downloadServer.EndsWith("/")) downloadServer += "/";
            m_download = downloadServer;

            Logger.LogInfo("Override Download URL:[<color=#00DDFF><b>{0}</b></color>]", m_download);
        }

        if (!string.IsNullOrEmpty(assetServer) && !string.IsNullOrWhiteSpace(assetServer))
        {
            if (!assetServer.EndsWith("/")) assetServer += "/";

            var t = tag;

            m_version = ValidateURL(assetServer + alias);
            m_asset   = ValidateURL(assetServer + t + "assets/");
            m_avatar  = ValidateURL(assetServer + t + "assets/Avatars/");
            m_upload  = ValidateURL(assetServer + t + "res/fight_data");

            Logger.LogInfo("Override Version  URL:[<color=#00DDFF><b>{0}</b></color>]", m_version);
            Logger.LogInfo("Override Asset    URL:[<color=#00DDFF><b>{0}</b></color>]", m_asset);
            Logger.LogInfo("Override Avatar   URL:[<color=#00DDFF><b>{0}</b></color>]", m_avatar);
        }
    }

    public static string ValidateURL(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return string.Empty;
        return m_validate1.Replace(m_validate0.Replace(url, "/"), "://");
    }

    private static System.Text.RegularExpressions.Regex m_validate0 = new System.Text.RegularExpressions.Regex(@"\/{2,}");
    private static System.Text.RegularExpressions.Regex m_validate1 = new System.Text.RegularExpressions.Regex(@":\/*");

    private static string alias { get { return string.IsNullOrEmpty(Root.alias) ? string.Empty : Root.alias + "/"; } }

    private static string tag { get { return string.IsNullOrEmpty(alias) ? Launch.Updater.build + "/" : alias + Launch.Updater.build + "/"; } }

    private static string m_version  = null;
    private static string m_download = null;
    private static string m_asset    = null;
    private static string m_avatar   = null;
    private static string m_upload   = null;
}
