/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Login window class
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-04-06
 * 
 ***************************************************************************************************/

using UnityEngine.UI;
using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum LoginState
{
    Normal      = 1,    //只有两个按钮的状态

    LoginInput  = 2,    //输入账号密码的状态

    RegistInput = 3,    //注册的状态

    WaitLogin   = 4,    //等待登录的状态
}

public class Window_Login : Window
{
    #region Static functions

    public const int registerNameLength = 18;//账号最大长度
    public const int registerNameMinLength = 6;//账号最小长度
    public const int registerPassWordLength = 18;//密码长度
    public const int registerPassWordMinLength = 6;//密码最小长度

    // @TODO: 将这部分设置移动到配置中
    private static readonly char[] InvalidCustomChars = new char[] {
    '\'','"','~','`','!','@','#','$','%','^','&','*','(',')','+','=','>','<',
    '|','{','}','/','\\',':',';',',','?','.','。','！','￥','（','）','—','-','《','》','、','·','；','：','’','”','“','‘','【','】',' '};

    private static bool IsValidChar(char[] addedChar)
    {
        for (int i = 0; i < addedChar.Length; i++)
        {
            for (int k = 0; k < InvalidCustomChars.Length; k++)
            {
                if (addedChar[i] == InvalidCustomChars[k])
                {
                    return false;
                }
            }
        }
        return true;
    }

    #endregion

    #region 登录
    private RectTransform loginPanel;
    private Button login;
    private Button visitorLoginBtn;
    private bool isVisitor;
    #endregion

    #region input
    private RectTransform inputPanel;
    private InputField loginName;
    private InputField loginPassword;
    private Button registerBtn;
    private Button realLoginBtn;
    private Button returnToLoginPanel;
    #endregion

    #region 注册
    private RectTransform registerPanel;
    private InputField registerName;
    private InputField registerPassword;
    private Image passRight;
    private Image passWrong;
    private InputField registerSurePassword;
    private Image sureRight;
    private Image sureWrong;
    private Button checkRepetNameBtn;
    private Button realRegisterBtn;
    private Button noRegisterBtn;
    #endregion

    #region 登录等待
    private RectTransform loginTip;
    private Text waitLoginDecription;
    private Button loginOut;
    #endregion

    protected override void OnOpen()
    {
        #region 登录
        loginPanel      = GetComponent<RectTransform>("loginPanel");
        login           = GetComponent<Button>("loginPanel/center/loginBtn");  
        visitorLoginBtn = GetComponent<Button>("loginPanel/center/visitorBtn");

        login.          onClick.RemoveAllListeners();
        login.          onClick.AddListener(()=> OnBtnLogin(login));
        visitorLoginBtn.onClick.RemoveAllListeners();
        visitorLoginBtn.onClick.AddListener(OnVisitorLogin);
        #endregion

        #region input
        inputPanel         = GetComponent<RectTransform>("inputPanel");
        loginName          = GetComponent<InputField>("inputPanel/center/account");
        loginPassword      = GetComponent<InputField>("inputPanel/center/password");
        registerBtn        = GetComponent<Button>("inputPanel/center/registerBtn");
        realLoginBtn       = GetComponent<Button>("inputPanel/center/loginBtnForInput");
        returnToLoginPanel = GetComponent<Button>("inputPanel/center/returnToLoginPanel");

        loginName.text = PlayerPrefs.GetString("loginName");
        loginPassword.text = PlayerPrefs.GetString("password");

        loginName.         onValueChanged.RemoveAllListeners();
        loginName.         onValueChanged.AddListener((str) => OnCheckLoginPassWord(loginName, str));
        loginPassword.     onValueChanged.RemoveAllListeners();
        loginPassword.     onValueChanged.AddListener((str) => OnCheckLoginPassWord(loginPassword, str));
        registerBtn.       onClick.RemoveAllListeners();
        registerBtn.       onClick.AddListener(()=> AllGameObjectState(LoginState.RegistInput));
        realLoginBtn.      onClick.RemoveAllListeners();
        realLoginBtn.      onClick.AddListener(()=> OnBtnLogin(realLoginBtn));
        returnToLoginPanel.onClick.RemoveAllListeners();
        returnToLoginPanel.onClick.AddListener(OnCloseInputPanel);
        #endregion

        #region 注册
        registerPanel        = GetComponent<RectTransform>("RegisterPanel");
        checkRepetNameBtn    = GetComponent<Button>("RegisterPanel/center/checkBtn");
        registerName         = GetComponent<InputField>("RegisterPanel/center/registerAccount");
        registerPassword     = GetComponent<InputField>("RegisterPanel/center/registerPassword");
        passRight            = GetComponent<Image>("RegisterPanel/center/right");
        passWrong            = GetComponent<Image>("RegisterPanel/center/wrong");
        registerSurePassword = GetComponent<InputField>("RegisterPanel/center/surePassword");
        sureRight            = GetComponent<Image>("RegisterPanel/center/right(1)");
        sureWrong            = GetComponent<Image>("RegisterPanel/center/wrong(1)");
        realRegisterBtn      = GetComponent<Button>("RegisterPanel/center/registerBtn");
        noRegisterBtn        = GetComponent<Button>("RegisterPanel/center/returnToLoginBtn");

        noRegisterBtn.       onClick.RemoveAllListeners();
        noRegisterBtn.       onClick.AddListener(() => AllGameObjectState(LoginState.LoginInput));
        realRegisterBtn.     onClick.RemoveAllListeners();
        realRegisterBtn.     onClick.AddListener(OnRegisterLogin);
        registerSurePassword.onEndEdit.RemoveAllListeners();
        registerSurePassword.onEndEdit.AddListener(OnCheckSureWord);
        registerSurePassword.onValueChanged.RemoveAllListeners();
        registerSurePassword.onValueChanged.AddListener(OnSureWordChanged);
        registerName.        onValueChanged.RemoveAllListeners();
        registerName.        onValueChanged.AddListener(OnNameChanged);
        registerPassword.    onEndEdit.RemoveAllListeners();
        registerPassword.    onEndEdit.AddListener(OnCheckPassWord);
        registerPassword.    onValueChanged.RemoveAllListeners();
        registerPassword.    onValueChanged.AddListener(OnPassWordValueChaged);
        checkRepetNameBtn.   onClick.RemoveAllListeners();
        checkRepetNameBtn.   onClick.AddListener(() => OnCheckName(registerName.text));
        #endregion

        #region 等待登录
        loginTip            = GetComponent<RectTransform>("loginTip");
        loginOut            = GetComponent<Button>("loginTip/tip/loginOut");
        waitLoginDecription = GetComponent<Text>("loginTip/tip/decription");

        loginOut.onClick.RemoveAllListeners();
        loginOut.onClick.AddListener(() => { moduleLogin.LogOut(true); });
        #endregion

        moduleGlobal.ShowGlobalLayer(false);
        IniteText();
    }

    private void OnCloseInputPanel()
    {
        string name = PlayerPrefs.GetString("loginName");
        string password = PlayerPrefs.GetString("password");
        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(password))
        {
            if (!string.Equals(loginName.text, name) || !string.Equals(loginPassword.text, password))
            {
                loginName.text = string.Empty;
                loginPassword.text = string.Empty;
            }
        }
        AllGameObjectState(LoginState.Normal);
    }

    private void IniteText()
    {
        var loginText = ConfigManager.Get<ConfigText>((int)TextForMatType.LoginUIText);
        Util.SetText(GetComponent<Text>("bj/tittle/Text"), loginText[33]);
        Util.SetText(GetComponent<Text>("loginPanel/center/loginBtn/text"), loginText[34]);
        Util.SetText(GetComponent<Text>("inputPanel/center/loginBtnForInput/Text"), loginText[48]);
        Util.SetText(GetComponent<Text>("loginPanel/center/visitorBtn/text"), loginText[35]);
        Util.SetText(GetComponent<Text>("loginPanel/Image/text"), loginText[36]);
        Util.SetText(GetComponent<Text>("bj/bottom/Text"), loginText[37]);
        Util.SetText(GetComponent<Text>("inputPanel/center/equip_prop/equipinfo"), loginText[38]);
        Util.SetText(GetComponent<Text>("inputPanel/center/account/Placeholder"), loginText[39]);
        Util.SetText(GetComponent<Text>("RegisterPanel/center/registerAccount/Placeholder"), loginText[39]);
        Util.SetText(GetComponent<Text>("inputPanel/center/password/Placeholder"), loginText[40]);
        Util.SetText(GetComponent<Text>("RegisterPanel/center/registerPassword/Placeholder"), loginText[40]);
        Util.SetText(GetComponent<Text>("RegisterPanel/center/equip_prop/equipinfo"), loginText[41]);
        Util.SetText(GetComponent<Text>("RegisterPanel/center/surePassword/Placeholder"), loginText[42]);
        Util.SetText(GetComponent<Text>("RegisterPanel/center/registerBtn/Text"), loginText[43]);
        Util.SetText(GetComponent<Text>("loginTip/tip/loginOut/Text"), loginText[44]);
        Util.SetText(GetComponent<Text>("inputPanel/center/registerBtn"), loginText[45]);

        Util.SetText(GetComponent<Text>("bj/bottom/Text"), 11);
        Util.SetText(GetComponent<Text>("bj/bottom/version"), string.IsNullOrWhiteSpace(Root.alias) ? Launch.Updater.buildVersion : $"{Launch.Updater.buildVersion}\n{Root.alias}");
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        if (moduleLogin.reLogin)
        {
            AllGameObjectState(LoginState.LoginInput);
            moduleLogin.reLogin = false;
        }
        else AllGameObjectState(LoginState.Normal);
    }

    private void AllGameObjectState(LoginState state, string name = "")
    {
        loginPanel.SafeSetActive(state == LoginState.Normal || state == LoginState.WaitLogin);
        loginTip  .SafeSetActive(state == LoginState.WaitLogin);

        if (state == LoginState.Normal) login.interactable = true;
        else if (state == LoginState.WaitLogin)
        {
            if (isVisitor) Util.SetText(waitLoginDecription, ConfigText.GetDefalutString(TextForMatType.LoginUIText, 32));
            else Util.SetText(waitLoginDecription, Util.Format(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 24), name));
        }

        inputPanel   .SafeSetActive(state == LoginState.LoginInput);
        registerPanel.SafeSetActive(state == LoginState.RegistInput);

        if (state == LoginState.LoginInput) realLoginBtn.interactable = !string.IsNullOrEmpty(loginName.text) && !string.IsNullOrEmpty(loginPassword.text);
        else if (state == LoginState.RegistInput)
        {
            passRight.SafeSetActive(false);
            passWrong.SafeSetActive(false);
            sureRight.SafeSetActive(false);
            sureWrong.SafeSetActive(false);
            registerName.text                 = string.Empty;
            registerPassword.text             = string.Empty;
            registerSurePassword.text         = string.Empty;

            registerPassword.interactable     = false;
            registerSurePassword.interactable = false;
            realRegisterBtn.interactable      = false;
            checkRepetNameBtn.interactable    = false;
        }
    }

    private void RefreshRegBtnIntercable()
    {
        registerPassword.interactable = !string.IsNullOrEmpty(registerName.text);
        registerSurePassword.interactable = !string.IsNullOrEmpty(registerPassword.text);
        realRegisterBtn.interactable = !string.IsNullOrEmpty(registerName.text) && !string.IsNullOrEmpty(registerPassword.text) && !string.IsNullOrEmpty(registerSurePassword.text);
    }

    private void OnCheckLoginPassWord(InputField field, string arg0)
    {
        realLoginBtn.interactable = !string.IsNullOrEmpty(loginName.text) && !string.IsNullOrEmpty(loginPassword.text);
        CheckIsHaveInvalidChar(field);
    }

    private void OnNameChanged(string arg0)
    {
        bool isLawful = CheckIsHaveInvalidChar(registerName);

        if (!isLawful) return;

        RefreshRegBtnIntercable();
        checkRepetNameBtn.interactable = true;
    }

    private void OnPassWordValueChaged(string arg0)
    {
        bool isLawful = CheckIsHaveInvalidChar(registerPassword);

        if (!isLawful) return;

        passRight.SafeSetActive(false);
        passWrong.SafeSetActive(false);
        if (string.IsNullOrEmpty(arg0))
        {
            registerSurePassword.text = string.Empty;
            sureRight.SafeSetActive(false);
            sureWrong.SafeSetActive(false);
        }

        RefreshRegBtnIntercable();
    }

    private void OnSureWordChanged(string arg0)
    {
        bool isLawful = CheckIsHaveInvalidChar(registerSurePassword);

        if (!isLawful) return;

        RefreshRegBtnIntercable();

        sureRight.gameObject.SetActive(false);
        sureWrong.gameObject.SetActive(false);
    }

    private bool CheckIsHaveInvalidChar(InputField field)
    {
        if (field == null) return false;
        string text = field.text;
        char[] chars = text.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            bool lawful = (chars[i] >= '0' && chars[i] <= '9') || (chars[i] >= 'A' && chars[i] <= 'Z') || (chars[i] >= 'a' && chars[i] <= 'z');
            if (!lawful)
            {
                field.text = text.Remove(i, 1);
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 47), 2.5f);
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 检查确认密码
    /// </summary>
    /// <param name="arg0"></param>
    private void OnCheckSureWord(string arg0)
    {
        if (string.IsNullOrEmpty(arg0)) return;
        CheckSureWord(arg0);
    }

    private int CheckSureWord(string arg0)
    {
        int id = !string.Equals(registerPassword.text, arg0) ? 12 : CheckPassWord(arg0);

        sureWrong.gameObject.SetActive(id > 0);
        sureRight.gameObject.SetActive(id == 0);

        if (id > 0)
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.LoginUIText, id), 2.5f);

        return id;
    }

    /// <summary>
    /// 检查密码 
    /// </summary>
    /// <param name="arg0"></param>
    private void OnCheckPassWord(string arg0)
    {
        int passed = CheckPassWord(arg0);
        if (passed == 0) registerSurePassword.ActivateInputField();
    }

    private int CheckPassWord(string arg0)
    {
        int id = string.IsNullOrEmpty(arg0) ? -1 : !IsValidChar(arg0.ToCharArray()) ? 26 : arg0.Length < registerPassWordMinLength ? 23 : arg0.Length > registerPassWordLength ? 27 : 0;

        passWrong.SafeSetActive(id > 0);
        passRight.SafeSetActive(id == 0);

        if (id > 0)
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.LoginUIText, id), 2.5f);
        return id;
    }

    /// <summary>
    /// 检查注册名
    /// </summary>
    private void OnCheckName(string agr0)
    {
        checkRepetNameBtn.interactable = false;

        bool isTrue = CheckName(agr0);
        if (isTrue) moduleLogin.CheckRepetName(agr0);
    }

    private bool CheckName(string agr0)
    {
        bool isvalid = IsValidChar(agr0.ToCharArray());
        if (!isvalid)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 30), 2.5f);
            return false;
        }
        else if (agr0.Length < registerNameMinLength)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 28), 2.5f);
            return false;
        }
        else if (agr0.Length > registerNameLength)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 29), 2.5f);
            return false;
        }
        return true;
    }

    private void OnRegisterLogin()
    {
        if (!CheckName(registerName.text) || CheckPassWord(registerPassword.text) != 0 || CheckSureWord(registerSurePassword.text) != 0)
            return;

        moduleLogin.Register(registerName.text, registerPassword.text);
        realRegisterBtn.interactable = false;
    }

    private void OnVisitorLogin()
    {
        isVisitor = true;
        moduleLogin.LoginGuest();
    }

    private void OnBtnLogin(Button btn)
    {
        if (string.IsNullOrEmpty(loginName.text) || string.IsNullOrEmpty(loginPassword.text))
        {
            AllGameObjectState(LoginState.LoginInput);
            return;
        }

        isVisitor = false;
        moduleLogin.Login(loginName.text, loginPassword.text, false);
        btn.interactable = false;
    }

    void _ME(ModuleEvent<Module_Login> e)
    {
        var code = 0;

        switch (e.moduleEvent)
        {
            case Module_Login.EventFailed://登录游戏服务器失败
            {
                var result = (int)e.param1;
                if (result == 1)
                {
                    Hide();
                    moduleSelectRole.CreateRole();
                    break;
                }

                AllGameObjectState(LoginState.Normal);

                if (result == 2)
                    moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 19) + ConfigText.GetDefalutString(TextForMatType.LoginUIText, 20), 2.5f);
                else if (result == 3)
                    moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 19) + ConfigText.GetDefalutString(TextForMatType.LoginUIText, 21), 2.5f);
                else if (result == 4)
                    moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 19) + ConfigText.GetDefalutString(TextForMatType.LoginUIText, 22), 2.5f);

                login.interactable = true;
                realLoginBtn.interactable = true;
                break;
            }
            case Module_Login.EventAuth://游戏登录验证失败
            {
                code = (int)e.param1;
                //失败
                if (code != 0)
                {
                    login.interactable = true;
                    realLoginBtn.interactable = true;
                    if (code == 1101 || code == 1102 || code == 1104 || code == 1105 || code == 1106 || code == 1107)
                    {
                        if (code == 1101) loginPassword.text = string.Empty;
                        else loginName.text = string.Empty;

                        AllGameObjectState(LoginState.LoginInput);
                    }

                    moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 19) + moduleLogin.Result(code), 2.5f);
                    break;
                }
                //成功
                PlayerPrefs.SetString("loginName", loginName.text);
                PlayerPrefs.SetString("password", loginPassword.text);

                AllGameObjectState(LoginState.WaitLogin, loginName.text);
                moduleLogin.waitLoginTimer = GeneralConfigInfo.swaitLoginTime;
                break;
            }
            case Module_Login.EventConnection://连接游戏服务器失败
            {
                if ((int)e.param1 != 0) AllGameObjectState(LoginState.Normal);
                break;
            }
            case Module_Login.EventRegister:
            {
                code = (int)e.param1;

                realRegisterBtn.interactable = true;
                passRight.SafeSetActive(false);
                sureRight.SafeSetActive(false);
                passWrong.SafeSetActive(false);
                passRight.SafeSetActive(false);

                if (code != 0)
                {
                    if (code == 1101)
                    {
                        registerPassword.text = string.Empty;
                        registerSurePassword.text = string.Empty;
                    }
                    else
                        registerName.text = string.Empty;

                    moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 17) + moduleLogin.Result(code), 2.5f);
                }
                else
                {
                    moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 18), 2.5f);
                    loginName.text = registerName.text;
                    loginPassword.text = registerPassword.text;
                    AllGameObjectState(LoginState.LoginInput);
                }
                break;
            }
            case Module_Login.EventCheckName:
            {
                bool isReceiveFalse = (bool)e.param1;
                if (!isReceiveFalse)
                {
                    registerPassword.ActivateInputField();
                    moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 25), 2.5f);
                }
                else moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 7), 2.5f);
                break;
            }
            case Module_Login.EventReturnToLogin:
            {
                if (actived)
                {
                    AllGameObjectState(moduleLogin.reLogin ? LoginState.LoginInput : LoginState.Normal);
                    moduleLogin.reLogin = false;
                }
                else Show();
                break;
            }
            default: break;
        }
    }
}
