/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-11-08
 * 
 ***************************************************************************************************/

using UnityEngine.UI;
using UnityEngine;
using System.Text.RegularExpressions;

public class Window_System : Window
{
    public const int registerNameLength = 18;//账号最大长度
    public const int registerNameMinLength = 6;//账号最小长度
    public const int registerPassWordLength = 18;//密码长度
    public const int registerPassWordMinLength = 6;//密码最小长度

    // @TODO: 将这部分设置移动到配置中
    private static readonly char[] InvalidCustomChars = new char[] {
    '\'','"','~','`','!','@','#','$','%','^','&','*','(',')','+','=','>','<',
    '|','{','}','/','\\',':',';',',','?','.','。','！','￥','（','）','—','-','《','》','、','·','；','：','’','”','“','‘','【','】',' '};

    //判断账号密码是否为数字
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

    #region 基本信息设置
    private RectTransform details_plane;
    private Button Return_back;//点击关闭这个界面         

    private Text m_name;
    private Button name_change;
    private Text play_level;
    private Text IDTxt;
    private Button introduce_btn;//修改介绍
    private Text introduce_txt;

    //提示界面 所有的
    private Button bg_plane_close;//点击关闭提示小窗口
    private Button head_plane_close;//点击关闭提示小窗口

    //兑换 绑定 设置 登出
    private Button award_btn;
    private Button Accont_click;//绑定
    private Button Set_btn;
    private Button logout_account;//登出
    private Button switch_account;//切换账号

    //选择头像框界面
    private RectTransform head_plane;
    private Button sure_change;//确定改变
    //提示背景出现
    private RectTransform details_panel;
    //修改介绍界面
    private RectTransform introduce_plane;
    private InputField introduce_plane_txt;
    private Button introduce_plane_change;
    //修改名字界面
    private RectTransform change_name;
    private InputField name_txt;
    private Text textname;
    private Button first_change;
    private Button two_change;
    private Text two_txt_num;//50
    private Text zuannow;//现有的钻石数
    private Image spend_name_img1;
    //确定修改名字界面
    private RectTransform sure_name_change;
    private Button sure_change_free;
    private Button sure_change_two;
    private Text sure_change_num;//(50
    private Image spend_name_img2;
    //钱不够界面
    private RectTransform moneyless;
    private Button shop_btn;//前往商店按钮
    private Button m_shopCancle;
    private Button m_changeCancle;
    private Button m_sureCancle;
    private Button m_exchangeCancle;
    private Button m_outCancle;
    private Button m_showCancle;

    private RectTransform award_plane;//兑奖码
    private InputField award_txt;
    private Button award_sure_btn;

    //退出账号
    private RectTransform Logout_plane;
    private Button caccount_sure;

    //绑定账号信息
    private RectTransform Account_Plane;//绑定界面
    private InputField registerName;//帐号
    private InputField registerPassword;//密码
    private InputField registerSurePassword;//密码确定
    private Image passRight;
    private Image passWrong;
    private Image sureRight;
    private Image sureWrong;
    private Button checkRepetNameBtn;
    private Button realRegisterBtn;
    //private string lastRegisterName;
    private bool isClickRegisterBtn;
    #endregion

    #region 头像框
    private Button head_btn;//头像框按钮 
    #endregion

    private DataSource<PropItemInfo> AllHeadData;
    private ConfigText system_txt;
    private ConfigText public_txt;
    private ConfigText friend_txt;
    private ConfigText login_txt;

    protected override void OnOpen()
    {
        isFullScreen = false;
        isClickRegisterBtn = false;
        system_txt = ConfigManager.Get<ConfigText>((int)TextForMatType.SetUIText);
        public_txt = ConfigManager.Get<ConfigText>((int)TextForMatType.PublicUIText);
        friend_txt = ConfigManager.Get<ConfigText>((int)TextForMatType.FriendUIText);
        login_txt = ConfigManager.Get<ConfigText>((int)TextForMatType.LoginUIText);

        if (system_txt == null)
        {
            system_txt = ConfigText.emptey;
            Logger.LogError("this id can not 221");
        }
        if (public_txt == null)
        {
            public_txt = ConfigText.emptey;
            Logger.LogError("this id can not 200");
        }
        if (friend_txt == null)
        {
            friend_txt = ConfigText.emptey;
            Logger.LogError("this id can not 218");
        }
        if (login_txt == null)
        {
            login_txt = ConfigText.emptey;
            Logger.LogError("this id can not 217");
        }
        SetText();

        #region 基本信息设置

        details_plane = GetComponent<RectTransform>("details");
        Return_back = GetComponent<Button>("details/close");
        Return_back.onClick.AddListener(delegate { Hide(); });

        #region 头像框
        head_btn = GetComponent<Button>("details/avatar_back");
        head_btn.onClick.AddListener(delegate
        {
            //头像框 每次打开默认选中自己穿戴的
            details_plane.gameObject.SetActive(false);
            head_plane.gameObject.SetActive(true);

            short lastcheck = moduleSet.SelectBoxID;
            moduleSet.SelectBoxID = modulePlayer.roleInfo.headBox;
            UpdateIndex(lastcheck);
            UpdateIndex(moduleSet.SelectBoxID);

        });
        #endregion

        bg_plane_close = GetComponent<Button>("tip_Panel/detail_panel/bg/equip_prop/top/button");
        head_plane_close = GetComponent<Button>("tip_Panel/head_plane/equip_prop/top/button");
        details_panel = GetComponent<RectTransform>("tip_Panel/detail_panel");
        head_plane_close.onClick.AddListener(() =>
        {
            Close_tip();
        });
        bg_plane_close.onClick.AddListener(Close_tip);

        m_name = GetComponent<Text>("details/playerInfo_plane/name");
        name_change = GetComponent<Button>("details/playerInfo_plane/name/Button");
        name_change.onClick.AddListener(OPen_Change_name);
        play_level = GetComponent<Text>("details/playerInfo_plane/play_level");
        IDTxt = GetComponent<Text>("details/playerInfo_plane/ID");
        introduce_btn = GetComponent<Button>("details/playerInfo_plane/introducte_btn");
        introduce_txt = GetComponent<Text>("details/playerInfo_plane/introducte_btn/introduce_myself");
        introduce_btn.onClick.AddListener(delegate
        {
            Open_tip();
            introduce_plane.gameObject.SetActive(true);
            introduce_plane_txt.text = modulePlayer.roleInfo.sign;//简介按钮
        });
        award_btn = GetComponent<Button>("details/award_btn");
        award_btn.onClick.AddListener(delegate
        {
            Open_tip();//打开兑换奖励界面
            award_plane.gameObject.SetActive(true);
            award_txt.text = string.Empty;
        });
        logout_account = GetComponent<Button>("details/Logout_btn");
        logout_account.onClick.AddListener(delegate
        {
            Open_tip();//打开退出界面
            Logout_plane.gameObject.SetActive(true);
        });

        switch_account = GetComponent<Button>("details/switchCharater_btn");
        switch_account?.onClick.AddListener(delegate
        {
            moduleSelectRole.OnReturnSwitchRole();
        });

        Accont_click = GetComponent<Button>("details/iphone_btn");
        Accont_click.onClick.AddListener(delegate
        {
            Open_tip();//打开绑定界面
            registerName.text = string.Empty;
            registerPassword.text = string.Empty;
            registerSurePassword.text = string.Empty;
            details_plane.gameObject.SetActive(false);
            Account_Plane.gameObject.SetActive(true);
        });
        Set_btn = GetComponent<Button>("details/Gameset_btn");
        Set_btn.onClick.AddListener(() => { SetWindowParam<Window_Settings>(0); ShowAsync<Window_Settings>(); Hide(); });

        head_plane = GetComponent<RectTransform>("tip_Panel/head_plane");

        sure_change = GetComponent<Button>("tip_Panel/head_plane/sure");
        sure_change.onClick.AddListener(delegate
        {
            //确定更换头像 发给服务器消息
            if (modulePlayer.roleInfo.headBox != moduleSet.SelectBoxID) moduleSet.Change_HeadKuang();
            else moduleGlobal.ShowMessage(system_txt[1]);
        });

        introduce_plane = GetComponent<RectTransform>("tip_Panel/detail_panel/introud_plane");
        introduce_plane_txt = GetComponent<InputField>("tip_Panel/detail_panel/introud_plane/inputname");
        introduce_plane_change = GetComponent<Button>("tip_Panel/detail_panel/introud_plane/introudsure");
        introduce_plane_change.onClick.AddListener(delegate
        {
            if (string.IsNullOrEmpty(introduce_plane_txt.text)) moduleGlobal.ShowMessage(system_txt[6]);
            else
            {
                if (Util.ContainsSensitiveWord(introduce_plane_txt.text)) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(227, 5));
                else moduleSet.Change_Introud(introduce_plane_txt.text);//简介更改确定按钮发送给服务器
            }
        });
        zuannow = GetComponent<Text>("tip_Panel/detail_panel/changename/cnamesuremoney/remain");
        change_name = GetComponent<RectTransform>("tip_Panel/detail_panel/changename");
        name_txt = GetComponent<InputField>("tip_Panel/detail_panel/changename/inputname");
        textname = GetComponent<Text>("tip_Panel/detail_panel/changename/inputname/Text");
        first_change = GetComponent<Button>("tip_Panel/detail_panel/changename/cnamesurefree");
        two_change = GetComponent<Button>("tip_Panel/detail_panel/changename/cnamesuremoney");
        two_txt_num = GetComponent<Text>("tip_Panel/detail_panel/changename/cnamesuremoney/kuo");//需要花费的金额
        spend_name_img1 = GetComponent<Image>("tip_Panel/detail_panel/changename/cnamesuremoney/zuan");//花费的是钻石
        first_change.onClick.AddListener(Change_name_plane);
        two_change.onClick.AddListener(Change_name_plane);

        sure_name_change = GetComponent<RectTransform>("tip_Panel/detail_panel/surename_plane");
        sure_change_free = GetComponent<Button>("tip_Panel/detail_panel/surename_plane/surefree");
        sure_change_two = GetComponent<Button>("tip_Panel/detail_panel/surename_plane/suremoney");

        sure_change_num = GetComponent<Text>("tip_Panel/detail_panel/surename_plane/suremoney/kuo");
        spend_name_img2 = GetComponent<Image>("tip_Panel/detail_panel/surename_plane/suremoney/zuan");
        sure_change_free.onClick.AddListener(delegate
        {
            moduleSet.Change_Name(name_txt.text);//向服务器发送更改名字请求
        });
        sure_change_two.onClick.AddListener(Sure_change_name);

        moneyless = GetComponent<RectTransform>("tip_Panel/detail_panel/moneyless_plane");

        shop_btn = GetComponent<Button>("tip_Panel/detail_panel/moneyless_plane/shopsure");//前往商城
        m_shopCancle = GetComponent<Button>("tip_Panel/detail_panel/moneyless_plane/cancel");
        m_sureCancle = GetComponent<Button>("tip_Panel/detail_panel/surename_plane/cancel");
        m_changeCancle = GetComponent<Button>("tip_Panel/detail_panel/changename/cancel");
        m_exchangeCancle = GetComponent<Button>("tip_Panel/detail_panel/award_plane/cancel");
        m_outCancle = GetComponent<Button>("tip_Panel/detail_panel/Logout_plane/cancel");
        m_showCancle = GetComponent<Button>("tip_Panel/detail_panel/introud_plane/cancel");
        m_shopCancle.onClick.AddListener(Close_tip);
        m_changeCancle.onClick.AddListener(Close_tip);
        m_sureCancle.onClick.AddListener(Close_tip);
        m_exchangeCancle.onClick.AddListener(Close_tip);
        m_outCancle.onClick.AddListener(Close_tip);
        m_showCancle.onClick.AddListener(Close_tip);
        shop_btn.onClick.AddListener(delegate
        {
            moduleAnnouncement.OpenWindow(32);
            Hide();
            moduleSet.openChangeName = true;
        });
        award_plane = GetComponent<RectTransform>("tip_Panel/detail_panel/award_plane");
        award_txt = GetComponent<InputField>("tip_Panel/detail_panel/award_plane/InputField");
        award_sure_btn = GetComponent<Button>("tip_Panel/detail_panel/award_plane/sure");
        award_sure_btn.onClick.AddListener(delegate
        {
            if (award_txt.text != null && award_txt.text != string.Empty)
            {
                moduleSet.GetReward(award_txt.text); //确定兑换 发送给服务器str
            }
            else moduleGlobal.ShowMessage(system_txt[6]);
        });
        Logout_plane = GetComponent<RectTransform>("tip_Panel/detail_panel/Logout_plane");
        caccount_sure = GetComponent<Button>("tip_Panel/detail_panel/Logout_plane/yes_btn");
        caccount_sure.onClick.AddListener(delegate
        {
            Close_tip();
            moduleLogin.LogOut(true);//发送给服务器断线请求
        });
        Account_Plane = GetComponent<RectTransform>("tip_Panel/detail_panel/account_plane");
        checkRepetNameBtn = GetComponent<Button>("tip_Panel/detail_panel/account_plane/checkBtn");//向服务器发送是否存在该账号
        registerName = GetComponent<InputField>("tip_Panel/detail_panel/account_plane/registerAccount");//账号
        registerPassword = GetComponent<InputField>("tip_Panel/detail_panel/account_plane/registerPassword");//密码
        registerSurePassword = GetComponent<InputField>("tip_Panel/detail_panel/account_plane/surePassword");//确认密码
        passRight = GetComponent<Image>("tip_Panel/detail_panel/account_plane/right");
        passWrong = GetComponent<Image>("tip_Panel/detail_panel/account_plane/wrong");
        sureRight = GetComponent<Image>("tip_Panel/detail_panel/account_plane/right(1)");
        sureWrong = GetComponent<Image>("tip_Panel/detail_panel/account_plane/wrong(1)");
        realRegisterBtn = GetComponent<Button>("tip_Panel/detail_panel/account_plane/registerBtn");//确定 向服务器发送
        registerName.onValueChanged.RemoveAllListeners();
        registerName.onValueChanged.AddListener(OnNameChanged);

        registerPassword.onEndEdit.RemoveAllListeners();
        registerPassword.onEndEdit.AddListener(OnCheckPassWord);
        registerPassword.onValueChanged.RemoveAllListeners();
        registerPassword.onValueChanged.AddListener(OnPassWordValueChaged);

        registerSurePassword.onEndEdit.RemoveAllListeners();
        registerSurePassword.onEndEdit.AddListener(OnCheckSureWord);
        registerSurePassword.onValueChanged.RemoveAllListeners();
        registerSurePassword.onValueChanged.AddListener(OnSureWordChanged);

        realRegisterBtn.onClick.RemoveAllListeners();
        realRegisterBtn.onClick.AddListener(OnRegisterLogin);

        checkRepetNameBtn.onClick.RemoveAllListeners();
        checkRepetNameBtn.onClick.AddListener(() => OnCheckName(registerName.text));

        #endregion
        name_txt.characterLimit = GeneralConfigInfo.MAX_NAME_LEN;

        AddAllHead();
        Name_Spend();
    }

    private void SetText()
    {
        Util.SetText(GetComponent<Text>("details/playerInfo_plane/bg/anguan_text"), system_txt[38]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/surename_plane/Text"), system_txt[3]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/moneyless_plane/Text"), system_txt[4]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/Logout_plane/tip_txt"), system_txt[7]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/account_plane/account"), system_txt[8]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/account_plane/password"), system_txt[9]);
        Util.SetText(GetComponent<Text>("tip_Panel/head_plane/equip_prop/top/equipinfo"), system_txt[0]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/account_plane/surePassWord"), system_txt[10]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/surename_plane/surefree/free"), system_txt[2]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/changename/cnamesurefree/free"), system_txt[2]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/moneyless_plane/shopsure/Text"), system_txt[5]);
        Util.SetText(GetComponent<Text>("details/Logout_btn/Gameset_text"), system_txt[44]);
        Util.SetText(GetComponent<Text>("details/iphone_btn/click"), system_txt[45]);
        Util.SetText(GetComponent<Text>("details/award_btn/click"), system_txt[46]);
        Util.SetText(GetComponent<Text>("details/Gameset_btn/Gameset_text"), system_txt[47]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/changename/notice"), system_txt[48]);
        Util.SetText(GetComponent<Text>("details/switchCharater_btn/switchCharater_txt"), system_txt[74]);
        var str = ConfigText.GetDefalutString(227, 4);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/changename/inputname/placeholder"), Util.Format(str, GeneralConfigInfo.MIN_NAME_LEN, GeneralConfigInfo.MAX_NAME_LEN));
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/award_plane/InputField/placeholder"), system_txt[49]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/changename/cnamesuremoney/xiaohao"), system_txt[50]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/bg/equip_prop/top/equipinfo"), system_txt[51]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/account_plane/equip_prop/equipinfo"), system_txt[52]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/account_plane/registerAccount/Placeholder"), system_txt[53]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/account_plane/registerPassword/Placeholder"), system_txt[54]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/account_plane/surePassword/Placeholder"), system_txt[55]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/award_plane/notice"), system_txt[49]);
        Util.SetText(GetComponent<Text>("details/playerInfo_plane/introducte_btn/introduce_myself"), system_txt[57]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/award_plane/sure/Text"), public_txt[4]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/Logout_plane/yes_btn/Text"), public_txt[4]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/surename_plane/suremoney/sure"), public_txt[4]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/account_plane/registerBtn/Text"), public_txt[4]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/introud_plane/introudsure/sure"), public_txt[4]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/changename/cnamesuremoney/sure"), public_txt[4]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/surename_plane/surefree/free_sure"), public_txt[4]);
        Util.SetText(GetComponent<Text>("tip_Panel/detail_panel/changename/cnamesurefree/free_sure"), public_txt[4]);
        Util.SetText(GetComponent<Text>("details/playerInfo_plane/play_leveltext"), friend_txt[2]);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        Close_tip();
        Base_info();
        Accont_click.gameObject.SetActive(!moduleSet.account_IS && WebAPI.PLATFORM_TYPE == 0); // 只有白包才能使用账号绑定功能
        if (moduleSet.openChangeName)
        {
            OPen_Change_name();
            moduleSet.openChangeName = false;
        }
    }

    private void Open_tip()//打开非头像框的tip
    {
        details_plane.gameObject.SetActive(false);
        details_panel.gameObject.SetActive(true);
        head_plane.gameObject.SetActive(false);
    }

    private void Close_tip()//关闭tips设置
    {
        details_plane.gameObject.SetActive(true);
        details_panel.gameObject.SetActive(false);
        head_plane.gameObject.SetActive(false);
        change_name.gameObject.SetActive(false);
        sure_name_change.gameObject.SetActive(false);
        moneyless.gameObject.SetActive(false);
        Account_Plane.gameObject.SetActive(false);
        award_plane.gameObject.SetActive(false);
        introduce_plane.gameObject.SetActive(false);
        Logout_plane.gameObject.SetActive(false);
    }

    private void OPen_Change_name()//打开改名字界面
    {
        Open_tip();
        change_name.gameObject.SetActive(true);
        name_txt.text = string.Empty;
        textname.text = null;
        if (!moduleSet.changedname)
        {
            sure_change_free.gameObject.SetActive(true);
            sure_change_two.gameObject.SetActive(false);
            first_change.gameObject.SetActive(true);//从未更改过
            two_change.gameObject.SetActive(false);
        }
        else
        {
            string formattext = Util.Format(system_txt[43], modulePlayer.roleInfo.diamond.ToString());
            Util.SetText(zuannow, formattext);
            sure_change_free.gameObject.SetActive(false);
            sure_change_two.gameObject.SetActive(true);
            first_change.gameObject.SetActive(false);
            two_change.gameObject.SetActive(true);
        }
    }

    private void Change_name_plane()//打开确定修改名字界面
    {
        if (name_txt.text != null && name_txt.text != string.Empty)
        {
            if (name_txt.text.Length < GeneralConfigInfo.MIN_NAME_LEN || name_txt.text.Length > GeneralConfigInfo.MAX_NAME_LEN)
            {
                moduleGlobal.ShowMessage(Util.Format(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 46), GeneralConfigInfo.MIN_NAME_LEN, GeneralConfigInfo.MAX_NAME_LEN));
            }
            else if (name_txt.text == modulePlayer.name_)
            {
                moduleGlobal.ShowMessage(system_txt[69]);
            }
            else
            {
                var onlyNum = Regex.IsMatch(name_txt.text, Window_Name.MATCH_NUM);
                if (onlyNum)
                {
                    moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.NameUIText, 6));
                    name_txt.text = string.Empty;
                    return;
                }

                bool islowful = Regex.IsMatch(name_txt.text, Window_Name.MATCH_STRING);//是否是汉字 英文 或者数字
                if (islowful)
                {
                    if (Util.ContainsSensitiveWord(name_txt.text))
                    {
                        moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.NameUIText, 5));
                    }
                    else
                    {
                        change_name.gameObject.SetActive(false);
                        sure_name_change.gameObject.SetActive(true);
                    }
                }
                else
                {
                    name_txt.text = string.Empty;
                    textname.text = null;
                    moduleGlobal.ShowMessage(system_txt[36]);
                }
            }
        }
        else
        {
            name_txt.text = string.Empty;
            textname.text = null;
            moduleGlobal.ShowMessage(system_txt[6]);
        }
    }

    private void Sure_change_name()//发送给服务器更改名字（花钱时候判断钱够不够）
    {
        int mymonry = 0;
        if (moduleSet.spend_num.itemTypeId == 2)
        {
            mymonry = (int)modulePlayer.roleInfo.diamond;
        }
        else if (moduleSet.spend_num.itemTypeId == 1)
        {
            mymonry = (int)modulePlayer.roleInfo.coin;
        }
        if (mymonry < moduleSet.spend_num.price)//这里要换掉
        {
            sure_name_change.gameObject.SetActive(false); //钱不够 
            moneyless.gameObject.SetActive(true);
        }
        else
        {
            moduleSet.Change_Name(name_txt.text);//向服务器发送更改名字请求//钱够想服务器发送名字是否含有特殊符号 成功发送换名字
        }
    }

    private void Name_Spend()//从服务器获取更改名字需要花费的钻石
    {
        if (moduleSet.spend_num == null) return;

        PropItemInfo info = ConfigManager.Get<PropItemInfo>(moduleSet.spend_num.itemTypeId);
        AtlasHelper.SetItemIcon(spend_name_img1, info);
        AtlasHelper.SetItemIcon(spend_name_img2, info);
        two_txt_num.text = moduleSet.spend_num.price.ToString();
        sure_change_num.text = moduleSet.spend_num.price.ToString();
    }

    private void Base_info()//基本信息
    {
        Util.SetText(m_name, modulePlayer.roleInfo.roleName);
        Util.SetText(play_level, modulePlayer.level.ToString());
        Util.SetText(IDTxt, ConfigText.GetDefalutString(218, 36), modulePlayer.roleInfo.index);
        Util.SetText(introduce_txt, moduleSet.SetSignTxt(modulePlayer.roleInfo.sign));

        //每次进入界面进行头像全体刷新
        moduleSet.SelectBoxID = modulePlayer.roleInfo.headBox;
        AllHeadData.SetItems(moduleSet.AllheadItems);
    }
    
    private void AddAllHead()
    {
        moduleSet.GetAllHead();

        AllHeadData = new DataSource<PropItemInfo>(moduleSet.AllheadItems, GetComponent<ScrollView>("tip_Panel/head_plane/scrollView"), SetheadInfo, HeadClick);
    }

    private void SetheadInfo(RectTransform rt, PropItemInfo info)
    {
        HeadBoxItme item = rt.gameObject.transform.gameObject.GetComponentDefault<HeadBoxItme>();

        if (item != null && info != null)
        {
            item.RefreshHeadBoxItem(info);
        }
    }

    private void HeadClick(RectTransform rt, PropItemInfo info)
    {
        GameObject slecton = rt.gameObject.transform.Find("click").gameObject;
        Image newLockImage = rt.gameObject.transform.Find("newUnLock").GetComponent<Image>();
        string SaveID = Util.Format("{0},{1}", modulePlayer.roleInfo.roleId, info.ID);
        PlayerPrefs.SetInt(SaveID, info.ID);

        slecton.gameObject.SetActive(true);
        newLockImage.gameObject.SetActive(false);

        short lastcheck = moduleSet.SelectBoxID;
        moduleSet.SelectBoxID = (short)info.ID;
        UpdateIndex(lastcheck);
    }

    private void UpdateIndex(int itemid)
    {
        for (int i = 0; i < moduleSet.AllheadItems.Count; i++)
        {
            if (moduleSet.AllheadItems[i].ID == itemid)
            {
                AllHeadData.UpdateItem(i);
            }
        }
    }

    private void _ME(ModuleEvent<Module_Set> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Set.EventBindAccount:
                {
                    var code = (int)e.param1;
                    if (code == 0)
                    {
                        isClickRegisterBtn = true;
                        moduleGlobal.ShowMessage(system_txt[31]);
                        Close_tip();
                        Accont_click.gameObject.SetActive(false);
                    }
                    else
                    {
                        var rstr = moduleLogin.Result(code);
                        moduleGlobal.ShowMessage(rstr);
                    }
                    break;
                }
            case Module_Set.EventHeadChange:
                {
                    AllHeadData.UpdateItems();
                    Close_tip();
                    break;
                }
            case Module_Set.EventIntroudChange:
                var bb = Util.Parse<int>(e.param1.ToString());
                if (bb == 0)
                {
                    modulePlayer.roleInfo.sign = introduce_plane_txt.text;
                    introduce_txt.text = moduleSet.SetSignTxt(modulePlayer.roleInfo.sign);
                    Close_tip();
                }
                else moduleGlobal.ShowMessage(system_txt[33]);
                break;
            case Module_Set.EventNameChange:
                //更改成功
                AudioManager.PlaySound(AudioInLogicInfo.audioConst.clickToSucc);//更改成功的音效
                var result = Util.Parse<int>(e.param2.ToString());

                if (result == 0)
                {
                    var formatText = e.param1 as string;
                    Util.SetText(m_name, formatText);
                    Close_tip();
                }
                else
                {
                    Nametips(result);
                    change_name.gameObject.SetActive(true);
                    sure_name_change.gameObject.SetActive(false);
                    name_txt.text = string.Empty;
                }
                break;
            case Module_Set.EventBangAward:
                var reward = e.param1 as PReward;
                var result1 = Util.Parse<int>(e.param2.ToString());
                award_txt.text = string.Empty;

                if (result1 == 0)//出现奖励条例
                {
                    Window_ItemTip.Show(system_txt[70], reward);
                    Close_tip();
                }
                break;
        }
    }

    private void Nametips(int result)
    {
        if (result == 1) moduleGlobal.ShowMessage(system_txt[58]);
        else if (result == 2) moduleGlobal.ShowMessage(system_txt[59]);
        else if (result == 3) moduleGlobal.ShowMessage(system_txt[60]);
    }

    #region//绑定账号

    public override void OnRenderUpdate()
    {
        if (Account_Plane.gameObject.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                //检查密码
                OnCheckPassWord(registerPassword.text);
                //检查确认密码
                OnCheckSureWord(registerSurePassword.text);
            }
            realRegisterBtn.interactable = !string.IsNullOrEmpty(registerName.text) && !string.IsNullOrEmpty(registerPassword.text) && !string.IsNullOrEmpty(registerSurePassword.text) && sureRight.gameObject.activeInHierarchy && passRight.gameObject.activeInHierarchy;
            registerSurePassword.interactable = !string.IsNullOrEmpty(registerPassword.text) && passRight.gameObject.activeInHierarchy;
        }
    }

    private void _ME(ModuleEvent<Module_Login> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Login.EventCheckName://检查账号结果
                bool isReceiveFalse = (bool)e.param1;
                if (!isReceiveFalse) moduleGlobal.ShowMessage(login_txt[25]);
                else moduleGlobal.ShowMessage(login_txt[7]);
                break;
            default: break;
        }
    }

    private void OnCheckName(string agr0)
    {
        if (!isClickRegisterBtn)
        {
            bool isTrue = CheckName(agr0);
            if (isTrue)
            {
                if (Util.ContainsSensitiveWord(agr0)) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(227, 5));
                else moduleLogin.CheckRepetName(agr0);
            }
        }
    }
    private bool CheckName(string agr0)
    {
        bool isvalid = IsValidChar(agr0.ToCharArray());
        if (!isvalid)
        {
            moduleGlobal.ShowMessage(login_txt[30]);
            return false;
        }
        else if (agr0.Length < registerNameMinLength)
        {
            moduleGlobal.ShowMessage(login_txt[28]);
            return false;
        }
        else if (agr0.Length > registerNameLength)
        {
            moduleGlobal.ShowMessage(login_txt[29]);
            return false;
        }
        return true;
    }

    private void OnRegisterLogin()
    {
        if (!CheckName(registerName.text))
        {
            isClickRegisterBtn = false;
            return;
        }
        if (CheckPassWord(registerPassword.text) != 0)
        {
            isClickRegisterBtn = false;
            return;
        }
        if (CheckSureWord(registerSurePassword.text) != 0)
        {
            isClickRegisterBtn = false;
            return;
        }
        moduleSet.Bang_Acount(registerName.text, registerPassword.text, true);
    }

    private void OnCheckPassWord(string arg0)
    {
        if (!isClickRegisterBtn)
        {
            var passed = CheckPassWord(arg0);
            passWrong.gameObject.SetActive(passed > 0);
            passRight.gameObject.SetActive(passed == 0);

            if (passed == 0) registerSurePassword.ActivateInputField(); // 少跳转确认密码
            else if (passed > 0) moduleGlobal.ShowMessage(login_txt[passed]);
        }
    }

    private int CheckPassWord(string arg0)
    {
        return string.IsNullOrEmpty(arg0) ? -1 : !IsValidChar(arg0.ToCharArray()) ? 26 : arg0.Length < registerPassWordMinLength ? 23 : arg0.Length > registerPassWordLength ? 27 : 0;
    }

    /// <summary>
    /// 检查确认密码
    /// </summary>
    private void OnCheckSureWord(string arg0)
    {
        if (!isClickRegisterBtn)
        {
            var passed = CheckSureWord(arg0);
            sureWrong.gameObject.SetActive(passed > 0);
            sureRight.gameObject.SetActive(passed == 0);

            if (passed > 0) moduleGlobal.ShowMessage(login_txt[passed]);
        }
    }
    private int CheckSureWord(string arg0)
    {
        return !string.Equals(registerPassword.text, arg0) ? 12 : CheckPassWord(arg0);
    }
    private void OnSureWordChanged(string arg0)
    {
        sureRight.gameObject.SetActive(false);
        sureWrong.gameObject.SetActive(false);
    }
    private void OnNameChanged(string arg0)
    {
        checkRepetNameBtn.interactable = true;
    }
    private void OnPassWordValueChaged(string arg0)
    {
        passRight.gameObject.SetActive(false);
        passWrong.gameObject.SetActive(false);
        if (string.IsNullOrEmpty(arg0)) registerSurePassword.text = string.Empty;
    }
    #endregion
}