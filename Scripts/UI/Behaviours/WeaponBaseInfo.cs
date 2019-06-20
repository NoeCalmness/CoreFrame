using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class WeaponBaseInfo : MonoBehaviour
{
    // 锻造中的基础信息
    private GameObject nothing_plane;
    private GameObject weapon_plane;
    private RectTransform Weapon_icon;
    private Text Weaponname;
    private Text attacknum;
    private Text elementnum;
    private Text WeaponLevel;
    private Button Btn;
    private Text m_id;
    private Text EleType;
    private GameObject Hight_back;
    private GameObject Gary;
    //private Atlas m_PropAltas;//图集
    private Action<GameObject> objlick;
    private ConfigText Foring_Text;

    List<GameObject> IConType = new List<GameObject>();

    private void Get()
    {
        IConType.Clear();
        Foring_Text = ConfigManager.Get<ConfigText>((int)TextForMatType.ForingUIText);

        if (Foring_Text == null)
        {
            Foring_Text = ConfigText.emptey;
            Logger.LogError("this id can not" );
        }
        //text
        Util.SetText(transform.Find("gotnothing_panel/weijiesuo_text").GetComponent<Text>(), Foring_Text[20]);
        Util.SetText(transform.Find("gotweapon_panel/addattack_text").GetComponent<Text>(), Foring_Text[21]);

        Gary = transform.Find("Gary").gameObject;
        m_id = transform.Find("ID").GetComponent<Text>();
        nothing_plane = transform.Find("gotnothing_panel").gameObject;
        weapon_plane = transform.Find("gotweapon_panel").gameObject;
        Weapon_icon = transform.Find("gotweapon_panel/weapon_icon").GetComponent<RectTransform>();
        Weaponname = transform.Find("gotweapon_panel/info/Text").GetComponent<Text>();
        attacknum = transform.Find("gotweapon_panel/addattack_number").GetComponent<Text>();
        elementnum = transform.Find("gotweapon_panel/addelementattack_number").GetComponent<Text>();
        WeaponLevel = transform.Find("gotweapon_panel/weaponlevel/Text").GetComponent<Text>();
        Btn = transform.GetComponent<Button>();
        Hight_back = transform.Find("checkon_background").gameObject;
        EleType = transform.Find("gotweapon_panel/addelementattack_text").GetComponent<Text>();

        GameObject one = transform.Find("gotweapon_panel/info/type/fist").gameObject;
        GameObject one1 = transform.Find("gotweapon_panel/info/type/fist1").gameObject;
        GameObject one2 = transform.Find("gotweapon_panel/info/type/fist2").gameObject;
        GameObject one3 = transform.Find("gotweapon_panel/info/type/fist3").gameObject;
        GameObject one5 = transform.Find("gotweapon_panel/info/type/fist4").gameObject;
        IConType.Add(one);
        IConType.Add(one1);
        IConType.Add(one5);//这个现在未解锁
        IConType.Add(one2);
        IConType.Add(one3);
    }
    public void Click(Action<GameObject> btnclick)
    {
        Get();
        //m_PropAltas = m_prop;
        objlick = btnclick;
    }

    public void SetInfo(PropItemInfo Info, string weaponAttack, string eleAttck, string weaLevel, bool use)
    {
        if (Info ==null )
        {
            Logger.LogError("info can not find in propitem " + Info.ID);
            return;
        }
        WeaponAttribute dInfo = ConfigManager.Get<WeaponAttribute>(Info.ID);
        Util.SetText(EleType, Show(dInfo.elementType));
        for (int i = 0; i < IConType.Count; i++)
        {
            IConType[i].gameObject.SetActive(false);
            if (i == (Info.subType - 1))
            {
                IConType[i].gameObject.SetActive(true);
            }
        }

        //拥有该武器
        m_id.text = Info.ID.ToString();

        PropItemInfo prpos = ConfigManager.Get<PropItemInfo>(Info.ID);
        if (prpos.mesh.Length <= 0)
        {
            Logger.LogError("can not find mesh name " + Info.ID);
            return;
        }
        string iconname = prpos.mesh[0];
        UIDynamicImage.LoadImage(Weapon_icon.transform, iconname, (d, t) => { Weapon_icon.gameObject.SetActive(true); }, false);

        Weaponname.text = Info.itemName;
        attacknum.text = weaponAttack;
        elementnum.text = eleAttck;
        string format = weaLevel + Foring_Text[23];
        Util.SetText(WeaponLevel, format);

        Hight_back.gameObject.SetActive(false);
        Gary.gameObject.SetActive(true);
        if (use)
        {
            Gary.gameObject.SetActive(false);
            Hight_back.gameObject.SetActive(true);
        }

        Btn.onClick.RemoveListener(ObjClick);
        Btn.onClick.AddListener(ObjClick);

    }

    public void Have(bool have, int idid)
    {
        m_id.text = idid.ToString();
        if (have)
        {
            nothing_plane.gameObject.SetActive(false);
            weapon_plane.gameObject.SetActive(true);
        }
        else
        {
            nothing_plane.gameObject.SetActive(true);
            weapon_plane.gameObject.SetActive(false);
        }
        Btn.onClick.RemoveListener(ObjClick);
        Btn.onClick.AddListener(ObjClick);
    }

    private void ObjClick()
    {
        if (objlick != null)
        {
            objlick(gameObject);
            Hight_back.gameObject.SetActive(true);
            Gary.gameObject.SetActive(false);
        }
    }

    private string Show(int type)
    {
        string str = "";
        switch (type)
        {
            case 1:
                str = Foring_Text[24];
                break;
            case 2:
                str = Foring_Text[25];
                break;
            case 3:
                str = Foring_Text[26];
                break;
            case 4:
                str = Foring_Text[27];
                break;
            case 5:
                str = Foring_Text[28];
                break;
        }
        return str;
    }
    public void ClockOpen()
    {
        //解锁
        nothing_plane.gameObject.SetActive(false);
        weapon_plane.gameObject.SetActive(true);
    }
    public void Clock()
    {
        //上锁
        nothing_plane.gameObject.SetActive(true);
        weapon_plane.gameObject.SetActive(false);
    }
}
