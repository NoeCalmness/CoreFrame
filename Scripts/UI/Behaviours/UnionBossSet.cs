using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnionBossSet : MonoBehaviour
{
    private Transform m_diffectType;
    private Toggle m_automatic;

    private Transform m_weekPalne;
    private Text m_weekTip;

    private Slider m_timeBar;
    private Text m_timeTxt;

    private Button m_saveBtn;
    private Button m_cancleBtn;

    private int m_diffcutType;
    private int m_amtic;
    private List<sbyte> m_openDay = new List<sbyte>();
    private List<sbyte> m_realDay = new List<sbyte>();

    private List<UIToggle> m_weekToggle = new List<UIToggle>();
    private List<UIToggle> m_diffectToggle = new List<UIToggle>();
    
    private PBossSet m_bossSet;
    void GetPath()
    {
        m_weekToggle.Clear();
        m_diffectToggle.Clear();

        m_diffectType = transform.Find("bossDiff_group");
        m_automatic = transform.Find("openAutoBeginActice_Toggle").GetComponent<Toggle>();

        m_weekPalne = transform.Find("bossWeek_group");
        m_weekTip = transform.Find("m_weekTip").GetComponent<Text>();

        m_timeBar = transform.Find("timeSettingBar").GetComponent<Slider>();
        m_timeTxt = transform.Find("timeSettingBar/timeSettingBarPointer_Img/timeSettingBar_Txt").GetComponent<Text>();

        m_saveBtn = transform.Find("savetimeSetting_Btn").GetComponent<Button>();
        m_cancleBtn = transform.Find("canceltimeSetting_Btn").GetComponent<Button>();

        foreach (Transform item in m_weekPalne.transform)
        {
            m_weekToggle.Add(item.gameObject.GetComponent<UIToggle>());
        }
        foreach (Transform item in m_diffectType.transform)
        {
            m_diffectToggle.Add(item.gameObject.GetComponent<UIToggle>());
        }
        Util.SetText(m_weekPalne?.gameObject, 242, 188);

    }
    //set 会使用module 存的
    public void BossSetInfo()
    {
        m_bossSet = Module_Union.instance.BossSet;
        if (m_bossSet == null) return;

        m_realDay.Clear();
        m_realDay.AddRange(m_bossSet.openday);
        GetPath();
        SetInfo();
        SetBtnclick();
        m_cancleBtn.interactable = false;
    }

    private void SetInfo()
    {
        m_openDay.Clear();
        m_openDay.AddRange(m_bossSet.openday);

        m_diffcutType = m_bossSet.diffcuttop;

        for (int i = 0; i < m_diffectToggle.Count; i++)
        {
            var index = i + 1;
            m_diffectToggle[i].isOn = index == m_bossSet.diffculttype ? true : false;
        }
        SetAutomatic();

        m_automatic.isOn = true;
        if (m_bossSet.bossautomatic == 0)
        {
            m_amtic = 0;
            m_automatic.isOn = false;
            m_weekPalne.gameObject.SetActive(false);
            m_weekTip.gameObject.SetActive(false);
            m_timeBar.gameObject.SetActive(false);
        }

        m_cancleBtn.interactable = false;
    }

    private void SetAutomatic()
    {
        m_amtic = 1;
        if (m_bossSet.openday.Length >= 7)
        {
            m_weekPalne.gameObject.SetActive(false);
            m_weekTip.gameObject.SetActive(true);
        }
        else
        {
            m_weekPalne.gameObject.SetActive(true);
            m_weekTip.gameObject.SetActive(false);
        }

        m_timeBar.gameObject.SetActive(true);
        for (int i = 0; i < m_weekToggle.Count; i++)
        {
            m_weekToggle[i].isOn = false;
        }

        for (int j = 0; j < m_bossSet.openday.Length; j++)
        {
            int day = m_bossSet.openday[j];
            if (day > 7 || day < 1) {  Logger.LogError("data is error"); continue; }
            var index = m_bossSet.openday[j] - 1;
            m_weekToggle[index].isOn = true;
        }

        m_timeBar.value = (float)m_bossSet.opentime;

        string hour = (m_bossSet.opentime / 3600).ToString();
        int ss = (int)m_timeBar.value % 3600;
        string mintue = (ss / 60).ToString();
        if (mintue.Length < 2)  mintue = "0" + mintue;

        m_timeTxt.text = hour + ":" + mintue;
    }

    private void SetBtnclick()
    {
        m_saveBtn.onClick.RemoveAllListeners();
        m_cancleBtn.onClick.RemoveAllListeners();
        m_automatic.onValueChanged.RemoveAllListeners();
        m_automatic.onValueChanged.AddListener(delegate
        {
            m_amtic = 0;
            m_weekPalne.gameObject.SetActive(false);
            m_weekTip.gameObject.SetActive(false);
            m_timeBar.gameObject.SetActive(false);
            if (m_automatic.isOn) SetAutomatic();
            m_cancleBtn.interactable = SetUndo();
        });
        m_timeBar.onValueChanged.AddListener(delegate
        {
            string hour = ((int)m_timeBar.value / 3600).ToString();
            int ss = (int)m_timeBar.value % 3600;
            string mintue = (ss / 60).ToString();
            if (mintue.Length < 2) mintue = "0" + mintue;
            m_timeTxt.text = hour + ":" + mintue;

            m_cancleBtn.interactable = SetUndo();
        });
        m_saveBtn.onClick.AddListener(delegate
        {
            //发送保存
            sbyte[] by = new sbyte[m_openDay.Count];
            for (int i = 0; i < m_openDay.Count; i++) by[i] = m_openDay[i];
            float second = m_timeBar.value % 60;
            int last = (int)(m_timeBar.value - second);
            Module_Union.instance.ChangeBossSet(m_diffcutType, m_amtic, by, last);

            m_cancleBtn.interactable = false;
        });

        m_cancleBtn.onClick.AddListener(delegate { SetInfo(); Module_Global.instance.ShowMessage(ConfigText.GetDefalutString(242, 204));
        });

        for (int i = 0; i < m_weekToggle.Count; i++)
        {
            var item = m_weekToggle[i];
            var index = i + 1;
            item.onValueChanged.RemoveAllListeners();
            item.onValueChanged.AddListener(delegate
            {
                bool have = m_openDay.Exists(a => a == index);
                if (item.isOn)
                {
                    if (have) return;
                    if (m_openDay.Count < Module_Union.instance.BossInfo.weektimes) m_openDay.Add((sbyte)index);
                }
                else if (have) m_openDay.Remove((sbyte)index);
                
                m_cancleBtn.interactable = SetUndo();
            });
        }
        for (int i = 0; i < m_diffectToggle.Count; i++)
        {
            var item = m_diffectToggle[i];
            var index = i + 1;
            item.onValueChanged.RemoveAllListeners();
            item.onValueChanged.AddListener(delegate { if (item.isOn) m_diffcutType = index; m_cancleBtn.interactable = SetUndo(); });

            item.interactable = m_bossSet.diffcuttop >= index ? true : false;
        }
    }

    private bool SetUndo()
    {
        m_openDay.Sort();
        m_realDay.Sort();
        if (m_openDay.Count != m_realDay.Count) return true;
        else
        {
            for (int i = 0; i < m_openDay.Count; i++) if (m_openDay[i] != m_realDay[i]) return true;
        }
        if (m_bossSet.diffculttype != m_diffcutType || m_bossSet.opentime != m_timeBar.value || m_bossSet.bossautomatic != m_amtic) return true;

        return false;
    }

}
