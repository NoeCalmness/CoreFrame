using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CooperationInfo : MonoBehaviour
{
    private Text m_name;
    private Transform m_coop;//协作是否显示
    private Transform m_monsterImg;
    private Button m_monsterBtn;
    private Transform m_invate;
    private Button m_invateBtn;
    private Transform m_player;
    private Button m_playerBtn;
    private Transform m_HeadBox;
    private GameObject m_HeadAvatar;
    private Transform m_reward;
    private Button m_rewardBtn;
    private Text m_invateName;
    private Text m_invateTxt;
    private Transform m_taskOne;
    private Transform m_taskTwo;
    private Text m_oneTxt;
    private Text m_twoTxt;
    private Image m_oneSlider;
    private Image m_twoSlider;
    private Text m_oneValue;
    private Text m_twoValue;
    private Transform m_showReward;
    private Transform m_offPlane;//领取过才会显示
    private Action<CooperationTask> monShowList;//出现关卡

    List<GameObject> showReward = new List<GameObject>();

    private void Get()
    {
        m_name = transform.Find("title/title_txt").GetComponent<Text>();
        m_coop = transform.Find("txt");
        m_monsterImg = transform.Find("boss/boss");
        m_monsterBtn = transform.Find("boss/boss").GetComponent<Button>();
        m_invate = transform.Find("player/invinte");
        m_invateBtn = transform.Find("player/invinte/resetBtn").GetComponent<Button>();
        m_player = transform.Find("player/player");
        m_playerBtn = transform.Find("player/player/bg").GetComponent<Button>();
        m_HeadBox = transform.Find("player/player/bg");
        m_HeadAvatar = transform.Find("player/player/bg/mask").gameObject;
        m_invateName = transform.Find("player/player/name").GetComponent<Text>();
        m_reward = transform.Find("player/reward");
        m_rewardBtn = transform.Find("player/reward/rewardBtn").GetComponent<Button>();
        m_taskOne = transform.Find("task_1");
        m_taskTwo = transform.Find("task_2");
        m_oneTxt = transform.Find("task_1/task_txt").GetComponent<Text>();
        m_twoTxt = transform.Find("task_2/task_txt").GetComponent<Text>();
        m_oneSlider = transform.Find("task_1/bg/progress").GetComponent<Image>();
        m_twoSlider = transform.Find("task_2/bg/progress").GetComponent<Image>();
        m_oneValue = transform.Find("task_1/bg/value").GetComponent<Text>();
        m_twoValue = transform.Find("task_2/bg/value").GetComponent<Text>();
        m_offPlane = transform.Find("off");
        m_invateTxt = transform.Find("player/invinte/Text").GetComponent<Text>();

        m_showReward = transform.Find("pre_award_parent");
        Util.SetText(transform.Find("txt/Text").GetComponent<Text>(), ConfigText.GetDefalutString(223, 39));
        Util.SetText(transform.Find("player/reward/Text").GetComponent<Text>(), ConfigText.GetDefalutString(223, 45));
        showReward.Clear();
        foreach (Transform item in m_showReward)
        {
            item.gameObject.SetActive(false);
            showReward.Add(item.gameObject);
        }
    }

    private void SetText()
    {
        Util.SetText(transform.Find("txt/Text").GetComponent<Text>(), 223,47);
        Util.SetText(transform.Find("player/invinte/Text").GetComponent<Text>(),223,48);
        Util.SetText(transform.Find("player/reward/Text").GetComponent<Text>(), 223,45);
    }

    public void SetInfo(PCooperateInfo info, Action<CooperationTask> monShow)
    {
        if (info == null)
        {
            Logger.LogError("server info is null");
            return;
        }
        CooperationTask task = Module_Active.instance.coopTaskBase.Find(a => a.ID == info.taskId);
        if (task == null)
        {
            Logger.LogError("configuration is not have this id");
            return;
        }
        Get();
        monShowList = monShow;

        Util.SetText(m_name, task.name);
        if (string.IsNullOrEmpty (task.icon)) Logger.LogError(" this task no icon");
        UIDynamicImage.LoadImage(m_monsterImg, task.icon);

        Util.SetText(m_invateTxt, ConfigText.GetDefalutString(223, 38), task.limitLevel);
        m_taskOne.gameObject.SetActive(true);
        m_coop.gameObject.SetActive(task.type == 1);
        m_taskTwo.gameObject.SetActive(task.type == 1);
        m_invate.gameObject.SetActive(info.friendId == 0 && task.type == 1 && info.state == 0);
        m_player.gameObject.SetActive(info.friendId != 0 && task.type == 1 && info.state == 0);
        m_reward.gameObject.SetActive(info.state == 1);
        m_offPlane.gameObject.SafeSetActive(info.state == 2);

        AwardGetSucced succed = m_showReward.GetComponentDefault<AwardGetSucced>();
        succed.SetAward(task.reward, showReward, false);

        if (task.conditions == null || task.conditions.Length < 1)
        {
            Logger.LogError("monster info is error");
            return;
        }
        if (task.type == 2 && task.conditions.Length >= 1) SetValue(task.conditions[0], 0, m_oneTxt, m_oneSlider, m_oneValue, info.selfFinishVal);
        else if (task.type == 1 && task.conditions.Length > 1)
        {
            var self = task.conditions[0];
            var friend = task.conditions[1];
            var ids = info.friendId;
            var selfValue = info.selfFinishVal;
            var fValue = info.assistFinishVal;
            if (Module_Player.instance.id_ != info.ownerId)
            {
                self = task.conditions[1];
                friend = task.conditions[0];
                ids = info.ownerId;
                selfValue = info.assistFinishVal;
                fValue = info.selfFinishVal;
            }
            SetValue(self, 0, m_oneTxt, m_oneSlider, m_oneValue, selfValue);
            SetValue(friend, 1, m_twoTxt, m_twoSlider, m_twoValue, fValue);
            if (ids != 0)
            {
                var f = Module_Friend.instance.FriendList.Find(a => a.roleId == ids);
                if (f != null)
                {
                    m_invateName.text = f.name;
                    headBoxFriend headBox = m_HeadBox.gameObject.GetComponentDefault<headBoxFriend>();
                    headBox.HeadBox(f.headBox);
                    Module_Avatar.SetClassAvatar(m_HeadAvatar, f.proto, false, f.gender);
                }
            }
        }
        m_invateBtn.onClick.RemoveAllListeners();
        m_invateBtn.onClick.AddListener(delegate
        {
            Module_Active.instance.CheckTaskID = info.uid;
            Module_Active.instance.GetCanInvate(info.taskId);
        });
        m_monsterBtn.onClick.RemoveAllListeners();
        m_monsterBtn.onClick.AddListener(delegate
        {
            //出现怪物详情界面
            if (!Module_Active.instance.collTask.ContainsKey(task.ID))
            {
                var idnex = 0;
                if (Module_Player.instance.id_ != info.ownerId) idnex = 1;
                Module_Active.instance.GetMonsterShow(task.ID, task.conditions[idnex].monsterId);
            }
            Module_Active.instance.CoopShowList.Clear();
            Module_Active.instance.GetAllMonsterStage(task.ID);

            monShowList(task);
        });
        m_rewardBtn.onClick.RemoveAllListeners();
        m_rewardBtn.onClick.AddListener(delegate
        {
            Module_Active.instance.GetCoopReward(info.uid);
        });
        m_playerBtn.onClick.RemoveAllListeners();
        m_playerBtn.onClick.AddListener(delegate
        {
            if (Module_Player.instance.id_ != info.ownerId) Module_Global.instance.ShowMessage(223, 46);//无权踢出
            else
            {
                if (CanKickedOut(info.acceptTime) && info.state == 0)
                {
                    Window_Alert.ShowAlert(ConfigText.GetDefalutString(223, 40), true, true, true, () =>
                    {
                        Module_Active.instance.KickedOutFriend(info.uid, info.friendId);
                    }, null, "", "");
                }
                else Module_Global.instance.ShowMessage(223, 43);//未到时间 或者 已经可领取
            }
        });
    }

    private bool CanKickedOut(uint accpetTime)
    {
        var haveTime = Util.GetDateTime(accpetTime);
        var now = DateTime.Now;
        TimeSpan span = now - haveTime;
        var t = 0;
        if (Module_Global.instance.system != null) t = Module_Global.instance.system.coopicedTime;
        return span.TotalHours > t ? true : false;
    }

    private void SetValue(CooperationTask.KillMonster coll, int type, Text txt, Image slider, Text value, int killNum)
    {
        var str = ConfigText.GetDefalutString(coll.text);
        var t = string.Format(ConfigText.GetDefalutString(223, 41), str);
        if (type == 1) t = string.Format(ConfigText.GetDefalutString(223, 42), str);
        Util.SetText(txt, t);
        slider.fillAmount = (float)killNum / coll.number;
        value.text = killNum + "/" + coll.number;
    }

}