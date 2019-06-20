/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Web Request Helper
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.3
 * Created:  2017-06-27
 * 
 ***************************************************************************************************/

using System.Collections.Generic;

public class WebRequestHelper : Singleton<WebRequestHelper>
{
    #region Server status

    public static WebRequest<ServerStatus> GetServerStatus(OnRequestComplete<ServerStatus> onComplete = null)
    {
        return WebRequestManager.Request(WebAPI.FullApiUrl(WebAPI.API_SERVER_STATUS), new { clientVersion = Launch.Updater.currentVersion }, onComplete);
    }

    #endregion

    #region Account

    public static WebRequest<AccountData> Login(string username, string password, bool guest, OnRequestComplete<AccountData> onComplete = null)
    {
        if (guest) return WebRequestManager.Request(WebAPI.FullApiUrl(WebAPI.API_AUTH_GUEST), new { name = username }, onComplete);
        return WebRequestManager.Request(WebAPI.FullApiUrl(WebAPI.API_AUTH), new { name = username, password = password.GetMD5() }, onComplete);
    }

    public static WebRequest<uint> Register(string name, string password, OnRequestComplete<uint> onComplete = null)
    {
        var accountInfo = new AccountInfo { name = name, password = password.GetMD5() };

        return WebRequestManager.Request(WebAPI.FullApiUrl(WebAPI.API_REGISTER), accountInfo, onComplete);
    }

    public static WebRequest<uint> Register(AccountInfo info, OnRequestComplete<uint> onComplete = null)
    {
        return WebRequestManager.Request(WebAPI.FullApiUrl(WebAPI.API_REGISTER), info, onComplete);
    }

    public static WebRequest BindAcount(int accId, string name, string password, string devId = "", OnRequestComplete onComplete = null)
    {
        var accountInfo = new
        {
            account  = accId,
            new_name = name,
            password = password.GetMD5()
        };

        return WebRequestManager.Request(WebAPI.FullApiUrl(WebAPI.API_ACCOUNT_BIAND), accountInfo, onComplete);
    }

    public static WebRequest<bool> CheckAccount(string m_name, OnRequestComplete<bool> onComplete = null)
    {
        var accountInfo = new { name = m_name };

        return WebRequestManager.Request(WebAPI.FullApiUrl(WebAPI.API_ACCOUNT_CHECK), accountInfo, onComplete);
    }

    #endregion

    public static WebRequest<List<BannerInfo>> GetBanner(OnRequestComplete<List<BannerInfo>> onComplete = null)
    {
        return WebRequestManager.Request(WebAPI.FullApiUrl(WebAPI.API_BANNER_INFO), onComplete);
    }

}