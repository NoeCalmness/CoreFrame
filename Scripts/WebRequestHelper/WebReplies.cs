/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Define all web API replydata
 * 
 * Author:   Y.Moon
 * Version:  0.2
 * Created:  2017-06-27
 * 
 ***************************************************************************************************/

using UnityEngine;

public class ServerStatus
{
    /// <summary>0 = 正常 1 = 维护中 2 = 测试更新中 3 = 其它</summary>
    public int status;
    /// <summary>当前客户端是否在白名单中</summary>
    public bool white;
    /// <summary>公告</summary>
    public string message;

    public string lowest_version;
    public string latest_version;

    /// <summary>下载服务器</summary>
    public string download_server;
    /// <summary>资源服务器</summary>
    public string asset_server;
}

public class ServerInfo
{
    public string host;
    public int port;
}

public class AccountData
{
    public long   valid_time;
    public string tick;
    public uint   acc_id;
    public string acc_name;
    public bool is_new;
    public ServerInfo server;
}

public class AccountInfo
{
    /// <summary>
    /// 接入平台的类型, 定义在 WebAPI 里面
    /// </summary>
    public int type = 0;
    /// <summary>
    /// 注册名称
    /// </summary>
    public string name = "";
    /// <summary>
    /// 密码
    /// </summary>
    public string password = "";
    /// <summary>
    /// 电话号码
    /// </summary>
    public string phone_number = "";
    /// <summary>
    /// 设备ID
    /// </summary>
    public string devid = "";
    /// <summary>
    /// 客户端版本号
    /// </summary>
    public string version = "";
    /// <summary>
    /// 设备品牌
    /// </summary>
    public string brand = "";
    /// <summary>
    /// 设备名称
    /// </summary>
    public string device = "";
    /// <summary>
    /// 设备型号
    /// </summary>
    public string device_model = "";
    /// <summary>
    /// IMEI 号
    /// </summary
    public string imei = "";
    /// <summary>
    /// 屏幕品牌
    /// </summary>
    //sonProperty("screen")]
    //blic string Screen { get; set; }
    /// <summary>
    /// 屏幕分辨率
    /// </summary>
    public string resolution = "";
    /// <summary>
    /// 网络类型
    /// </summary>
    public string net = "";
    /// <summary>
    /// 网络状态, 2G, 3G, 4G, wifi
    /// </summary>
    public string net_status = "";
    /// <summary>
    /// 设备内存
    /// </summary>
    public int ram = 0;
    /// <summary>
    /// 操作系统
    /// </summary>
    public string os = "";
    /// <summary>
    /// 客户端mac地址
    /// </summary>
    public string mac = "";

    public AccountInfo()
    {
        device_model = SystemInfo.deviceModel;
        device       = SystemInfo.deviceName;
        brand        = SystemInfo.deviceType.ToString();
        ram          = SystemInfo.systemMemorySize;
        os           = SystemInfo.operatingSystem;
        devid        = SystemInfo.deviceUniqueIdentifier;
        resolution   = Util.Format("{0}x{1}", Screen.currentResolution.width, Screen.currentResolution.width);
        version      = Launch.Updater.currentVersion;
    }
}