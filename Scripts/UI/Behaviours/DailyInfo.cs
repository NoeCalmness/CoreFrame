using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyInfo : BaseRestrain
{
    private Text taskname;
    private Text taskdetail;

    private GameObject rewardContent;

    private Button get_btn;//领取按钮
    private Button go_btn;//前往按钮
    private Text remaintime;//剩余时间
    private Image progress;//进度条 可能显示
    private Image progress_value;
    private Text my_value;
    private Text should_value;
    private Action<int> dailyget;
    private Action<int> dailygo;

    private int type;
    private int ID;
    private ConfigText ActiveText;
    private List<string> awardinfo = new List<string>();
    private List<GameObject> child = new List<GameObject>();
    private float m_time = 0;
    private int timeNow;
    private bool Start = false;

    private void Get()
    {
        ActiveText = ConfigManager.Get<ConfigText>((int)TextForMatType.ActiveUIText);
        if (ActiveText == null)
        {
            ActiveText = ConfigText.emptey;
            Logger.LogError("this id can not");
        }

        taskname = transform.Find("name_text").GetComponent<Text>();
        taskdetail = transform.Find("describe_text").GetComponent<Text>();

        rewardContent = transform.Find("rewardgrid").gameObject;
        get_btn = transform.Find("get_btn").GetComponent<Button>();
        go_btn = transform.Find("go_btn").GetComponent<Button>();
        remaintime = transform.Find("remain_text/time_text").GetComponent<Text>();
        progress = transform.Find("progress_bg").GetComponent<Image>();
        progress_value = transform.Find("progress_bg/progress").GetComponent<Image>();
        my_value = transform.Find("progress_bg/current_text").GetComponent<Text>();
        should_value = transform.Find("progress_bg/max_text").GetComponent<Text>();

        Util.SetText(transform.Find("get_btn/get_text").GetComponent<Text>(), ActiveText[6]);
        Util.SetText(transform.Find("go_btn/get_text").GetComponent<Text>(), ActiveText[18]);
        Util.SetText(transform.Find("remain_text").GetComponent<Text>(), ActiveText[7]);

        child.Clear();
        foreach (Transform item in rewardContent.transform)
        {
            child.Add(item.gameObject);
        }
    }

    public void Click(int ids, Action<int> get, Action<int> go)
    {
        Get();
        ID = ids;
        dailyget = get;
        dailygo = go;
    }

    private void Dailyget()
    {
        dailyget?.Invoke(ID);
    }

    private void Dailygo()
    {
        dailygo?.Invoke(type);
    }

    public void DetailsInfo(PDailyInfo Dailyinfo, PDailyTask info, uint losstime)
    {
        restrainId = info.id;

        Start = false;
        //如果是开启状态 才会进行时间计算
        if (Dailyinfo.isOpen && Dailyinfo.state < 2)
        {
            timeNow = (int)(Dailyinfo.restTime - losstime);
            m_time = 0;
            if (timeNow > 0)
            {
                ShowTime();
                Start = true;
            }
            else remaintime.text = "0";
        }

        //显示具体信息
        type = info.type;
        awardinfo.Clear();
        Util.SetText(taskname, info.name);
        Util.SetText(taskdetail, info.desc);
        if (!info.hasBar)
        {
            progress.gameObject.SetActive(false);
        }
        else
        {
            progress.gameObject.SetActive(true);
            should_value.text = info.condition.ToString();
            ChangeValue((int)Dailyinfo.finishVal);
        }

        if (Dailyinfo.state == 0)
        {
            get_btn.gameObject.SetActive(false);
            go_btn.gameObject.SetActive(true);
        }
        else if (Dailyinfo.state == 1)
        {
            get_btn.gameObject.SetActive(true);
            go_btn.gameObject.SetActive(false);
        }
        if (info.type == 1) go_btn.gameObject.SetActive(false);
        get_btn.onClick.RemoveAllListeners();
        go_btn.onClick.RemoveAllListeners();
        get_btn.onClick.AddListener(Dailyget);
        go_btn.onClick.AddListener(Dailygo);
        
        if (info.reward == null) return;
        
        AwardGetSucced succed = rewardContent.GetComponentDefault<AwardGetSucced>();
        succed.SetAward(info.reward,child);

    }

    public void ChangeValue(int value)
    {
        my_value.text = value.ToString();
        if (value < Util.Parse<int>(should_value.text))
        {
            if (Util.Parse<int>(should_value.text)==0)
            {
                Logger.LogError("should value is 0 ");
                return;
            }
            progress_value.fillAmount = (float)value / (float)Util.Parse<int>(should_value.text);
        }
        else
        {
            my_value.text = should_value.text;
            progress_value.fillAmount = 1.0f;
        }
    }

    private void ShowTime()
    {
        if (ActiveText.text .Length <12)
        {
            Logger.LogError("can not find active text ");
        }
        TimeSpan t = new TimeSpan(0, 0, timeNow);
        var rTime = "0";
        if (t.Days > 0) rTime = t.Days.ToString() + ActiveText[8];
        else if (t.Hours > 0) rTime = t.Hours.ToString() + ActiveText[9];
        else if (t.Minutes > 0) rTime = t.Minutes.ToString() + ActiveText[10];
        else
        {
            rTime = timeNow.ToString() + ActiveText[11];
            if (timeNow < 1)
            {
                remaintime.text = "0";
                Start = false;
            }
        }
        Util.SetText(remaintime, rTime);
    }

    private void Update()
    {
        if (Start)
        {
            m_time += Time.unscaledDeltaTime;
            if (m_time > 1)
            {
                m_time = 0;
                timeNow--;
                ShowTime();
            }
        }
    }
}
