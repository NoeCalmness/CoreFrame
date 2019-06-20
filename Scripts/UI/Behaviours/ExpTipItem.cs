using System;
using UnityEngine;
using UnityEngine.UI;

public class ExpTipItem : MonoBehaviour
{
    private Text tip_text;
    private Button cancle_btn;
    private Transform wupin;
    private Button dropBtn;
    private Text m_rTime;
    private Text m_petSkill;

    private int expRemainTime;
    private float addtime;
    private bool isInited;
    
    private Action<int> m_jump;
    private Action<PItem2> m_normalJump;
    private int _itemTypeId;
    private PItem2 m_item;

    void IniteCompent()
    {
        if (isInited) return;
        isInited = true;
        Util.SetText(transform.Find("top/equipinfo").GetComponent<Text>(), (int)TextForMatType.PublicUIText, 25);
        tip_text = transform.Find("text_back/detail_viewport/detail_content").GetComponent<Text>();
        cancle_btn = transform.Find("top/global_tip_button").GetComponent<Button>();
        wupin = transform.Find("middle");
        dropBtn = transform.Find("get_Btn").GetComponent<Button>();
        m_rTime = transform.Find("remainTime/time").GetComponent<Text>();
        m_petSkill = transform.GetComponent<Text>("middle/name/addtive");
    }

    public void RefreshTip(PetSkill.Skill rSkill, int rLevel, EnumPetMood rMood)//技能
    {
        IniteCompent();
        expRemainTime = -1;
        dropBtn.gameObject.SetActive(false);
        m_petSkill?.gameObject.SetActive(true);

        m_rTime.transform.parent.gameObject.SetActive(false);
        Util.DisableAllChildren(wupin, new[] { "icon", "name" });
        AtlasHelper.SetShared(wupin.GetComponent<Image>("icon"), rSkill.skillIcon);
        Util.SetText(wupin.GetComponent<Text>("name"), Util.Format(ConfigText.GetDefalutString(rSkill.skillName), rLevel));
        Util.SetText(tip_text, rSkill.GetDesc(rLevel));
        Util.SetText(m_petSkill, Util.Format(ConfigText.GetDefalutString(105000), (int) rMood));
    }

    public void RefreshTip(ushort itemTypeId, PItem item, Action<int> jump, bool drop, int haveNum)//普通
    {

        IniteCompent();
        _itemTypeId = itemTypeId;
        m_jump = jump;
        expRemainTime = -1;

        Refresh(itemTypeId, drop, item, haveNum);
        if (item == null || item.timeLimit == -1) return;
        if (!Module_Cangku.instance.PitemTime.ContainsKey(item.itemId)) return;

        var get = Module_Cangku.instance.PitemTime[item.itemId];
        expRemainTime = item.timeLimit - (int)Module_Active.instance.AlreadyLossTime(get);
        if (expRemainTime <= 0) return;

        setRemainTime(expRemainTime);
    }

    public void RefreshTip(ushort itemTypeId, uint exp, Action<int> jump, bool drop = true)//经验卡
    {
        IniteCompent();
        Refresh(itemTypeId, drop);
        _itemTypeId = itemTypeId;
        m_jump = jump;
        expRemainTime = (int)exp;
        if (expRemainTime <= 0) return;
        
        setRemainTime(expRemainTime);
    }

    public void RefreshTip(ushort itemTypeId, int day, Action<int> jump, bool drop = true)//月季卡
    {
        IniteCompent();
        m_jump = jump;
        _itemTypeId = itemTypeId;
        expRemainTime = -1;
        Refresh(itemTypeId, drop);

        m_rTime.transform.parent.gameObject.SetActive(true);
        Util.SetText(m_rTime, ConfigText.GetDefalutString(6043), day.ToString());
    }
    public void RefreshTip(PItem2 item, Action<PItem2> jump, bool drop)//普通
    {
        IniteCompent();
        m_item = item;
        _itemTypeId = item.itemTypeId;
        m_normalJump = jump;
        expRemainTime = -1;

        Refresh(item, drop);
    }

    private void Refresh(ushort itemTypeId, bool drop, PItem item = null, int haveNum = -1, bool showStar = false)
    {
        m_petSkill?.gameObject.SetActive(false);

        var info = PropItemInfo.Get(itemTypeId);
        if (info == null) return;

        int count = (int)Module_Player.instance.GetCount(itemTypeId);
        if (item != null) count = Module_Cangku.instance.GetItemCount(item.itemId);
        if (haveNum != -1) count = haveNum;

        Util.SetItemInfo(wupin, info, 0, count);
        var ct = wupin.GetComponent<Text>("numberdi/count");
        ct.gameObject.SetActive(count > 0);
        Util.SetText(ct, ConfigText.GetDefalutString(204, 30), count);

        var TextInfo = ConfigManager.Get<ConfigText>(info.desc);
        tip_text.text = TextInfo ? TextInfo.text[0].Replace("\\n", "\n") : string.Empty;

        m_rTime.transform.parent.gameObject.SetActive(false);

        dropBtn.gameObject.SetActive(false);
        var dropOpen = Module_Guide.instance.HasFinishGuide(GeneralConfigInfo.defaultConfig.dropOpenID);
        if (drop && dropOpen)
        {
            Module_Global.instance.m_dropList = Module_Global.instance.GetDropJump(itemTypeId);
            dropBtn.gameObject.SetActive(Module_Global.instance.m_dropList != null && Module_Global.instance.m_dropList.Count > 0);
            dropBtn.onClick.RemoveAllListeners();
            if (showStar) dropBtn.onClick.AddListener(NormalJumpClick);
            else dropBtn.onClick.AddListener(JumpClick);
        }

        cancle_btn.onClick.RemoveAllListeners();
        cancle_btn.onClick.AddListener(() => { Destroy(this); });
    }

    private void Refresh(PItem2 item, bool drop)
    {
        Refresh(item.itemTypeId, drop,null ,-1,true);

        var prop = ConfigManager.Get<PropItemInfo>(item.itemTypeId);
        Util.SetItemInfo(wupin, prop, item.level, Module_Cangku.instance.GetItemCount(item.itemTypeId), true, item.star);

        var ct = wupin.GetComponent<Text>("numberdi/count");
        Util.SetText(ct, ConfigText.GetDefalutString(204, 30), Module_Cangku.instance.GetItemCount(item.itemTypeId));
    }

    private void JumpClick()
    {
        if (m_jump != null && _itemTypeId != 0) m_jump(_itemTypeId);
    }
    private void NormalJumpClick()
    {
        if (m_normalJump != null && m_item != null) m_normalJump(m_item);
    }

    private void setRemainTime(int remain)
    {
        m_rTime.transform.parent.SafeSetActive(true);
        var day = expRemainTime / 86400;
        if (day > 0)
        {
            var s = expRemainTime % 86400;
            var h = s / 3600;
            Util.SetText(m_rTime, ConfigText.GetDefalutString(204,35), day.ToString(), h.ToString());
        }
        else
        {
            var formatText = Util.GetTimeMarkedFromSec(expRemainTime);
            Util.SetText(m_rTime, formatText);
        }
        if (expRemainTime == 0) m_rTime.transform.parent.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (expRemainTime >= 0)
        {
            addtime += Time.deltaTime;
            if (addtime >= 1)
            {
                expRemainTime -= (int)addtime;
                addtime = 0;
                setRemainTime(expRemainTime);
            }
        }
    }
}
