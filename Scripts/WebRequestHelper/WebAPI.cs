/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Web API definitions
 * 
 * Author:   Hardy Huang, Y.Moon <chglove@live.cn>
 * Version:  0.2
 * Created:  2017-06-05
 * 
 ***************************************************************************************************/

 /// <summary>
 /// Define all web API address
 /// </summary>
public class WebAPI
{
    /// <summary>客户端发行平台的唯一标识</summary>
    public const int PLATFORM_TYPE = 0;

    /// <summary>获取服务器状态信息</summary>
    public const string API_SERVER_STATUS = "res/update_info";

    /// <summary>注册地址</summary>
    public const string API_REGISTER = "user";

    /// <summary>登录授权地址</summary>
    public const string API_AUTH = "user/auth";

    /// <summary>游客授权地址</summary>
    public const string API_AUTH_GUEST = "user/guest";

    /// <summary>游客绑定地址</summary>
    public const string API_ACCOUNT_BIAND = "user/bind";

    /// <summary>检测非法字符</summary>
    public const string API_ACCOUNT_CHECK = "user/exist";

    /// <summary>头像存储地址</summary>
    public const string RES_HEAD_AVATAR = "res/head";

    /// <summary>不同步日志上传地址</summary>
    public const string RES_FIGHT_DATA = "res/fight_data";

    /// <summary>Banner信息</summary>
    public const string API_BANNER_INFO = "res/banners";

    /// <summary>客户端发行平台子渠道 如果为空表示是母包，应当在游戏启动前（SDKManager 的 Tracking 初始化之前设置）</summary>
    public static string platformSubType = null;
    
    public static string FullApiUrl(string api)
    {
        return string.Concat(Root.fullHost, api);
    }

    public static string FullApiUrl(string host, string api)
    {
        return string.Concat(host, api);
    }
}
