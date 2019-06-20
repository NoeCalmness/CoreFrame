/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Login module
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-04-06
 * 
 ***************************************************************************************************/

using UnityEngine;

public class Module_Login : Module<Module_Login>
{
    #region Events

    /// <summary>
    /// 账户验证  param1 = code
    /// </summary>
    public const string EventAuth                  = "EventLoginAuth";
    /// <summary>
    /// 连接游戏服务器 param1 = result  0 = successed 1 = failed
    /// </summary>
    public const string EventConnection            = "EventLoginConnection";
    /// <summary>
    /// 登陆游戏服务器成功
    /// </summary>
    public const string EventSuccessed             = "EventLoginSuccessed";
    /// <summary>
    /// 登陆游戏服务器失败
    /// </summary>
    public const string EventFailed                = "EventLoginFailed";
    /// <summary>
    /// 账户注册 param1 = code
    /// </summary>
    public const string EventRegister              = "EventLoginRegister";
    /// <summary>
    /// 进入游戏
    /// </summary>
    public const string EventEnterGame             = "EventLoginEnterGame";
    /// <summary>
    /// 当执行返回到登陆界面行为时
    /// </summary>
    public const string EventReturnToLogin         = "EventLoginReturnToLogin";
    public const string EventCreatePlayerFailed    = "EventLoginCreatePlayer";
    public const string EventCreatPlayerSuccess    = "EventLoginCreatPlayerSuccess";
    public const string EventCheckName             = "EventLoginCheckName";
    public const string EventGetNewTick            = "EventGetNewTick";

    #endregion

    #region Static functions

    /// <summary>
    /// 游客账户名
    /// </summary>
    public static string guestName
    {
        get
        {
            if (string.IsNullOrEmpty(m_guestName))
            {
                m_guestName = PlayerPrefs.GetString(SystemInfo.deviceUniqueIdentifier);
                if (string.IsNullOrEmpty(m_guestName))
                    m_guestName = System.Guid.NewGuid().ToString("N");  // @TODO: 不应本地生成随机账户，游客账户应当直接由服务器生成
            }
            return m_guestName;
        }
    }
    private static string m_guestName = null;

    #endregion

    /// <summary>
    /// 登陆是否已经就绪？
    /// 只有当玩家成功登陆，并收到 ScEnterGame 之后才会就绪
    /// </summary>
    public bool ok { get; private set; } = false;
    /// <summary>
    /// 玩家是否已经成功登陆
    /// </summary>
    public bool loggedIn { get; private set; } = false;
    /// <summary>
    /// 是否是第一次进入游戏？
    /// </summary>
    public bool firstEnter { get; private set; } = false;
    /// <summary>
    /// 玩家账号信息
    /// </summary>
    public AccountData account { get; private set; }
    /// <summary>
    /// 标记下一次进入登陆场景时的默认行为
    /// 如果为 true 登陆界面将默认显示输入账户和密码的输入框 否则不显示
    /// 这一标记在每次进入登陆场景后重置
    /// </summary>
    public bool reLogin { get; set; }

    public int waitLoginTimer = 3000;

    private int m_sendLoginTime = 0;
    private int m_waitEventHandler = 0;

    private int m_preventLoginMessage = 0;

    #region Need rewrite

    /// <summary>
    /// 获取经验条的进度
    /// </summary>
    public float expProcess
    {
        get
        {
            CreatureEXPInfo expinfoTo = ConfigManager.Get<CreatureEXPInfo>(modulePlayer.roleInfo.level + 1);
            //last level
            if (expinfoTo == null)
                return 1f;

            CreatureEXPInfo expinfoBefore = ConfigManager.Get<CreatureEXPInfo>(modulePlayer.roleInfo.level);
            if (expinfoBefore == null)
                return 0f;

           return (float)(modulePlayer.roleInfo.expr - expinfoBefore.exp) / (expinfoTo.exp - expinfoBefore.exp);
        }
    }

    /// <summary>
    /// 前一步的经验条
    /// </summary>
    public float lastExpProcess { get; set; }

    public string Result(int result)
    {
        string resultMes = "";
        //登录返回值对应错误
        switch (result)
        {
            case 1101: resultMes = ConfigText.GetDefalutString(TextForMatType.LoginUIText, 1); break;
            case 1102: resultMes = ConfigText.GetDefalutString(TextForMatType.LoginUIText, 2); break;
            case 1103: resultMes = ConfigText.GetDefalutString(TextForMatType.LoginUIText, 3); break;
            case 1104: resultMes = ConfigText.GetDefalutString(TextForMatType.LoginUIText, 4); break;
            case 1105: resultMes = ConfigText.GetDefalutString(TextForMatType.LoginUIText, 5); break;
            case 1106: resultMes = ConfigText.GetDefalutString(TextForMatType.LoginUIText, 6); break;
            case 1107: resultMes = ConfigText.GetDefalutString(TextForMatType.LoginUIText, 7); break;
            case 1201: resultMes = ConfigText.GetDefalutString(TextForMatType.LoginUIText, 8); break;
            case 1202: resultMes = ConfigText.GetDefalutString(TextForMatType.LoginUIText, 9); break;
            case 1203: resultMes = ConfigText.GetDefalutString(TextForMatType.LoginUIText, 10); break;
            case 1204: resultMes = ConfigText.GetDefalutString(TextForMatType.LoginUIText, 11); break;
            case -2: resultMes = Util.GetString(1, 4); break;
            default: resultMes = Util.GetString(1, 5); break;
        }
        return resultMes;
    }

    #endregion

    #region create role cache
    public int createGender { get; set; }
    public string createName { get; set; }
    public int roleProto { get; set; }

    public void CreatePlayer()
    {
        CreatePlayer(createName, roleProto);
    }
    #endregion

    protected override void OnGameStarted()
    {
        session.AddEventListener(Events.SESSION_LOST_CONNECTION, OnLostConnection);
    }

    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        lastExpProcess = expProcess;
    }

    protected void ClearData()
    {
        account  = null;
        ok       = false;
        loggedIn = false;

        moduleGlobal.Hidepao();

        DelayEvents.Remove(m_waitEventHandler);
        m_waitEventHandler = -1;
    }

    private void OnLostConnection()
    {
        loggedIn = false;

        CGManager.Stop();

        if (m_preventLoginMessage == 0 && (Level.currentLevel != 0 || moduleStory.inStoryMode || moduleStory.loading))
        {
            Game.paused = true;
            moduleGlobal.ShowMessageBox_(10, 1, b => { ReturnToLogin(true); });
        }
        else if (m_preventLoginMessage != 1) ReturnToLogin(true);
    }

    private void ReturnToLogin(bool _reLogin)
    {
        reLogin = _reLogin;

        if (Level.loading)
        {
            Level.onLoadOperationComplete -= ReturnToLoginGuard;
            Level.onLoadOperationComplete += ReturnToLoginGuard;
            return;
        }
        else ReturnToLoginGuard();
    }

    private void ReturnToLoginGuard()
    {
        DelayEvents.Remove(m_waitEventHandler);
        m_waitEventHandler = -1;

        if (session.connected)    session.Disconnect();
        if (modulePVP.connected)  modulePVP.Disconnect();
        if (moduleTeam.connected) moduleTeam.Disconnect();
        if (moduleChat.connected) moduleChat.Disconnect();

        Game.paused = false;
        ObjectManager.timeScale = 1.0;

        ClearData();
        Game.ResetGameData();
        Game.LoadLevelGuard(0);

        DispatchModuleEvent(EventReturnToLogin);
    }

    public void CheckRepetName(string name)
    {
        WebRequestHelper.CheckAccount(name, reply =>
        {
            DispatchModuleEvent(EventCheckName, reply.data);
        });
    }

    public void Register(string accName, string password, bool lockUI = true)
    {
        var accountInfo = new AccountInfo
        {
            name = accName,
            password = password.GetMD5()
        };

        if (lockUI) moduleGlobal.LockUI("", 0.1f);

        WebRequestHelper.Register(accountInfo, reply =>
        {
            if (lockUI) moduleGlobal.UnLockUI();

            DispatchModuleEvent(EventRegister, reply.code);

            if (reply.code == 0) SDKManager.Register(reply.data.ToString());
        });
    }

    public void LoginGuest()
    {
        Login(guestName, null, true);
        moduleSet.account_IS = false;
    }

    public void Login(string username, string password, bool isGuest = false, bool lockUI = false)
    {
        moduleSet.account_IS = true;
       
        if (Root.serverInfo.isHttp)
        {
            if (lockUI) moduleGlobal.LockUI(0.5f);

            WebRequestHelper.Login(username, password, isGuest, reply =>
            {
                if (lockUI) moduleGlobal.UnLockUI();

                DispatchModuleEvent(EventAuth, reply.code);

                if (reply.code != 0)
                {
                    Logger.LogError($"Login: Error reply code : {reply.code}");
                    
                    return;
                }

                if (isGuest)
                {
                    string name = PlayerPrefs.GetString(SystemInfo.deviceUniqueIdentifier);
                    if (string.IsNullOrEmpty(name))
                        PlayerPrefs.SetString(SystemInfo.deviceUniqueIdentifier, username);
                }

                account = reply.data;
                m_waitEventHandler = 0;

                if (account.is_new) SDKManager.Register(account.acc_id.ToString());

                Login();
            });
        }
        else
        {
            account = new AccountData() { acc_id = 1, acc_name = username, tick = username.GetMD5(), valid_time = 0 };
            account.server = new ServerInfo() { host = session.host, port = session.port };

            waitLoginTimer = 0;
            m_waitEventHandler = 0;

            Login();
        }
    }

    private void Login()
    {
        if (session.connected) session.Disconnect();

        moduleGlobal.LockUI(0.5f);
        session.Connect(account.server.host, account.server.port, s =>
        {
            moduleGlobal.UnLockUI();

            DispatchModuleEvent(EventConnection, s.connected ? 0 : 1);

            if (!s.connected || account == null) return; // Connect aborted if account is null

            m_preventLoginMessage = 0;

            DelayEvents.Remove(m_waitEventHandler);
            m_sendLoginTime = Level.realTime;

            var p = PacketObject.Create<CsLogin>();
            p.accName       = account.acc_name;
            p.accId         = account.acc_id;
            p.tick          = account.tick;
            p.type          = WebAPI.PLATFORM_TYPE;

            s.Send(p);
        });
    }

    public void LogOut(bool _reLogin)
    {
        if (session.connected)
        {
            m_preventLoginMessage = 2;

            session.Send(PacketObject.Create<CsLogout>());
            session.Disconnect();

            return;
        }

        ReturnToLogin(_reLogin);
    }

    public void CreatePlayer(string name, int _roleProto)
    {
        var p = PacketObject.Create<CsCreateRole>();
        p.platform      = 2;
        p.gender        = (sbyte)createGender;
        p.roleProto     = (byte)_roleProto;
        p.roleName      = name;
        p.gameVersion   = Launch.Updater.currentVersion;
        p.machineNumber = SystemInfo.deviceUniqueIdentifier;
        p.producer      = SystemInfo.deviceModel;
        p.sourceHash    = AssetBundles.AssetManager.dataHash;

        session.Send(p);
    }

    public void GoHome(bool firstEnter = false)
    {
        ok = false;

        moduleGlobal.Hidepao();

        moduleSelectRole.RoleStartEnterGame(moduleSelectRole.RoleCount - 1, firstEnter);
    }

    private void LoginEnd(int result)
    {
        if (m_waitEventHandler < 0) return;
        m_waitEventHandler = 0;
        
        if (result == 0)
        {
            DispatchModuleEvent(EventSuccessed);

            Game.LoadLevel(GeneralConfigInfo.sroleLevel);

            SDKManager.Login(account.acc_id.ToString());
        }
        else DispatchModuleEvent(EventFailed, result);
    }

    void _Packet(ScLogin p)
    {
        firstEnter = p.result == 1;
        loggedIn = p.result < 2;

        if (p.result > 1)
        {
            waitLoginTimer = 0;
            LoginEnd(p.result);
            return;
        }

        moduleSelectRole.SetRoleSummary(p.roleList);

        var wait = Level.realTime - m_sendLoginTime;
        if (wait >= waitLoginTimer) LoginEnd(p.result);
        else
        {
            var result = p.result;
            m_waitEventHandler = DelayEvents.Add(() => { LoginEnd(result); }, (waitLoginTimer - wait) * 0.001f);
        }

        waitLoginTimer = 0;
    }

    void _Packet(ScLogout p)
    {
        if (p.result == 9)
        {
            moduleGlobal.ShowMessageBox_(10, p.result, b => { moduleSelectRole.OnReturnSwitchRole(); });
            return;
        }

        loggedIn = false;

        if (p.result == 0)
        {
            ReturnToLogin(true);
            return;
        }

        Game.paused = true;
        m_preventLoginMessage = 1;

        moduleGlobal.ShowMessageBox_(10, p.result == 20 ? 8 : p.result, b => { ReturnToLogin(true); });
    }
    
    void _Packet(ScEnterGame p)
    {
        ok = true;

        DispatchModuleEvent(EventEnterGame);
    }

    void _Packet(ScCreateRole p)
    {
        if (p.result == 0)
        {
            DispatchModuleEvent(EventCreatPlayerSuccess);

            moduleSelectRole.OnCreateRoleSuccess(p.role);
            //wait for story dialog end
            //GoHome(true); // 一旦创建角色成功，即开始加载主场景，相关协议会由服务器主动推送
        }
        else DispatchModuleEvent(EventCreatePlayerFailed, p);
    }
}
