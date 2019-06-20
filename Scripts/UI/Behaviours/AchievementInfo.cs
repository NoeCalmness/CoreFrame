using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementInfo : BaseRestrain
{
    private Text taskname;
    private Text taskdetail;
    private GameObject typegroups;//四种不同的等级
    private Image progress;//进度条 可能显示
    private Image progress_value;
    private Text my_value;
    private Text should_value;
    private GameObject reward_group;
    private Button get_btn;//领取按钮
    private Image alreadly_getimg; //领取过图标
    private Action<ushort> m_awardget;
    private ushort ID;

    private List<GameObject> alltype = new List<GameObject>();
    private List<GameObject> child = new List<GameObject>();

    private void Get()
    {
        transform.Find("get_butotn/get_text").GetComponent<Text>().text = ConfigText.GetDefalutString(TextForMatType.ActiveUIText, 6);
        taskname = transform.Find("name_text").GetComponent<Text>();
        taskdetail = transform.Find("describe_text").GetComponent<Text>();
        typegroups = transform.Find("typesgroup").gameObject;
        progress = transform.Find("progress_bg").GetComponent<Image>();
        progress_value = transform.Find("progress_bg/progress").GetComponent<Image>();
        my_value = transform.Find("progress_bg/current_text").GetComponent<Text>();
        should_value = transform.Find("progress_bg/max_text").GetComponent<Text>();
        reward_group = transform.Find("reward_grids").gameObject;
        get_btn = transform.Find("get_butotn").GetComponent<Button>();
        alreadly_getimg = transform.Find("get_already").GetComponent<Image>();

        child.Clear();
        foreach (Transform item in reward_group.transform)
        {
            child.Add(item.gameObject);
        }
    }
    public void Click(Action<ushort> get, ushort id)
    {
        Get();
        m_awardget = get;
        ID = id;
    }

    public void SetInfo(PAchieve info)
    {
        restrainId = info.id;
        
        Util.SetText(taskname, Util.Parse<int>(info.name));
        Util.SetText(taskdetail, info.desc);

        Settype(info.achieveLv);//显示等级
        progress.gameObject.SetActive(true);
        my_value.text = info.finishVal.ToString();
        should_value.text = info.condition.ToString();

        if (info.type == 17)
        {
            double a = info.finishVal / 3600;
            double b = info.condition / 3600;
            my_value.text = (Math.Floor(a)).ToString();
            should_value.text = (Math.Floor(b)).ToString();
        }
        else should_value.text = info.condition.ToString();
        
        if (!info.hasBar)
        {
            should_value.text = "1";
            if (!Module_Active.instance.CanGetList[info.id])  my_value.text = "0";
            else  my_value.text = "1";
        }

        ChangeValue(Util.Parse<int>(my_value.text));//设置进度条
        ChangeState(info);

        get_btn.onClick.RemoveAllListeners();
        get_btn.onClick.AddListener(AwardGet);

        //奖励
        AwardGetSucced succed = reward_group.GetComponentDefault<AwardGetSucced>();
        succed.SetAward(info.reward, child);
    }

    private void AwardGet()
    {
        m_awardget?.Invoke(ID);
    }

    private void Settype(int type)
    {
        alltype.Clear();
        // 成就的背景
        foreach (Transform child in typegroups.transform)
        {
            alltype.Add(child.gameObject);
        }
        for (int i = 0; i < alltype.Count; i++)
        {
            alltype[i].gameObject.SetActive(i == type);
        }
    }
    public void ChangeValue(int value)
    {
        my_value.text = value.ToString();
        if (value < Util.Parse<int>(should_value.text))
        {
            if (Util.Parse<int>(should_value.text) == 0)
            {
                Logger.LogError("should value is 0 ");
                return;
            }
            progress_value.fillAmount = (float)value / (float)Util.Parse<int>(should_value.text);
        }
        else  progress_value.fillAmount = 1.0f;
    }
    public void ChangeState(PAchieve info)
    {
        if (info.isDraw)
        {
            //已经领取
            my_value.text = should_value.text;
            get_btn.gameObject.SetActive(false);
            alreadly_getimg.gameObject.SetActive(true);
            progress_value.fillAmount = 1.0f;
        }
        else
        {
            if (Module_Active.instance.CanGetList[info.id])
            {
                //可领取未领取
                get_btn.gameObject.SetActive(true);
                alreadly_getimg.gameObject.SetActive(false);
            }
            else
            {
                get_btn.gameObject.SetActive(false);
                alreadly_getimg.gameObject.SetActive(false);
            }
        }
    }
}