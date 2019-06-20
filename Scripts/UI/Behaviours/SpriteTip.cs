using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteTip : MonoBehaviour
{

    private GameObject tipopen;
    private Text detail;
    private GameObject itemone;
    private GameObject itemtwo;
    private GameObject itemthree;
    private Text progress;
    private Image progress_bar;
    private Button explore;

    private GameObject tipclose;
    private Image costimage;
    private Text havenum;
    private Button sureopen;
    private Text m_restTimes;
    private GameObject m_expendTip;

    private ConfigText pettask_txt;
    private ConfigText publice_txt;

    private List<GameObject> ItemInfo = new List<GameObject>();
    void Get()
    {
        publice_txt = ConfigManager.Get<ConfigText>((int)TextForMatType.PublicUIText);
        pettask_txt = ConfigManager.Get<ConfigText>((int)TextForMatType.PetTaskText);
        if (publice_txt == null)
        {
            publice_txt = ConfigText.emptey;
            Logger.LogError("this id can not");
        }
        if (pettask_txt == null)
        {
            pettask_txt = ConfigText.emptey;
            Logger.LogError("this id can not");
        }
        tipopen = transform.Find("tip_01").gameObject;
        detail = transform.Find("tip_01/content_tip").GetComponent<Text>();
        itemone = transform.Find("tip_01/preview/item_01").gameObject;
        itemtwo = transform.Find("tip_01/preview/item_02").gameObject;
        itemthree = transform.Find("tip_01/preview/item_03").gameObject;
        progress = transform.Find("tip_01/progress_txt").GetComponent<Text>();
        progress_bar = transform.Find("tip_01/exploreprogress_img/progressbar").GetComponent<Image>();
        explore = transform.Find("tip_01/go_btn").GetComponent<Button>();
        tipclose = transform.Find("tip_02").gameObject;
        costimage = transform.Find("tip_02/bg/xiaohao_tip/zuan").GetComponent<Image>();
        havenum = transform.Find("tip_02/bg/xiaohao_tip/remain").GetComponent<Text>();
        sureopen = transform.Find("tip_02/bg/yes").GetComponent<Button>();
        m_restTimes = transform.Find("tip_02/bg/rest_times").GetComponent<Text>();
        m_expendTip = transform.Find("tip_02/bg/xiaohao_tip").gameObject;

        Util.SetText(transform.Find("tip_01/preview_txt").GetComponent<Text>(), pettask_txt[0]);
        Util.SetText(transform.Find("tip_01/equipinfo").GetComponent<Text>(), pettask_txt[1]);
        Util.SetText(transform.Find("tip_02/bg/equipinfo").GetComponent<Text>(), pettask_txt[2]);
        Util.SetText(transform.Find("tip_02/bg/detail_content").GetComponent<Text>(), pettask_txt[3]);
        Util.SetText(transform.Find("tip_02/bg/xiaohao_tip/xiaohao").GetComponent<Text>(), pettask_txt[4]);
        Util.SetText(transform.Find("tip_02/bg/yes/yes_text").GetComponent<Text>(), publice_txt[4]);
        Util.SetText(transform.Find("tip_02/bg/nobtn/no_text").GetComponent<Text>(), publice_txt[5]);
    }

    public void SetInfo()
    {
        Get();
        ScPetCopyinfo AllTask = Module_Home.instance.LocalPetInfo;
        if (AllTask == null)
        {
            Module_Global.instance.ShowMessage("local pet info is null");
            return;
        }
        if (Module_Home.instance.PetTaskopen)
        {
            TaskInfo task = ConfigManager.Get<TaskInfo>(Module_Home.instance.LocalPetInfo.taskid);
            if (task == null)
            {
                Module_Global.instance .ShowMessage("server chase id not found");
                return;
            }

            tipopen.gameObject.SetActive(true);
            tipclose.gameObject.SetActive(false);
            OpenInfo(AllTask.progress, AllTask.taskid);
        }
        else
        {
            tipopen.gameObject.SetActive(false);
            tipclose.gameObject.SetActive(true);

            CloseInfo(AllTask.expend, AllTask.times);
        }
    }

    public void ChangeValue(int value)
    {
        Util.SetText(progress, pettask_txt[5], value);
        progress_bar.fillAmount = (float)value / (float)100;
    }

    private void OpenInfo(int progressvalue, short taskid)
    {
        ItemInfo.Clear();
        ItemInfo.Add(itemone);
        ItemInfo.Add(itemtwo);
        ItemInfo.Add(itemthree);

        TaskInfo task = ConfigManager.Get<TaskInfo>(taskid);
        if (task == null)
        {
            Logger.LogError("can not find config id" + taskid);
            return;
        }
        Util.SetText(detail, task.desc);
        
        Util.SetText(progress, pettask_txt[5], progressvalue);
        progress_bar.fillAmount = (float)progressvalue / (float)100;
        explore.onClick.RemoveAllListeners();
        explore.onClick.AddListener(() =>
        {
            if (Module_Home.instance.m_showBossTip)
                Window_Alert.ShowAlert(pettask_txt[11], true, true, true, () => { Module_Home.instance.Entermodule(taskid); }, null, "", "");
            else
                Module_Home.instance.Entermodule(taskid);
        });

        for (int i = 0; i < ItemInfo.Count; i++)
        {
            ItemInfo[i].gameObject.SetActive(false);
        }
        StageInfo stage = ConfigManager.Get<StageInfo>(task.stageId);
        if (stage == null) return;
        if (stage.previewRewardItemId.Length == 0) return;

        List<PropItemInfo> awardinfo = GetAllAward(stage.previewRewardItemId);
        if (awardinfo.Count <= 0)
        {
            Logger.LogDetail("no award can find ");
            return;
        }

        for (int i = 0; i < awardinfo.Count; i++)
        {
            if (i < ItemInfo.Count)
            {
                Util.SetItemInfo(ItemInfo[i], awardinfo[i]);
                ItemInfo[i].gameObject.SetActive(true);
                OnlickSet(ItemInfo[i], (ushort)awardinfo[i].ID);
            }
        }
    }

    private void OnlickSet(GameObject itemObj, ushort tipId)
    {
        Button clickBtn = itemObj.GetComponent<Button>();
        clickBtn.onClick.RemoveAllListeners();
        clickBtn.onClick.AddListener(delegate
        {
            Module_Global.instance.UpdateGlobalTip(tipId, true);
        });
    }

    private List<PropItemInfo> GetAllAward(int[] awardid)
    {
        List<PropItemInfo> award = new List<PropItemInfo>();

        var professional = Module_Player.instance.proto;

        int awardlength = awardid.Length;
        if (awardlength > 0)
        {
            if (awardid[0] == -1)
            {
                awardlength = 1;
            }
            for (int i = 0; i < awardlength; i++)
            {
                if (awardid[0] == -1)
                {
                    i = 1;
                }
                PropItemInfo items = ConfigManager.Get<PropItemInfo>(awardid[i]);
                if (items == null)
                {
                    Logger.LogError(awardid[i] + "this propitem can not find in config");
                    break;
                }

                bool appropriate = false;
                CreatureVocationType[] proto = items.proto;
                for (int j = 0; j < proto.Length; j++)
                {
                    if (proto[j] == CreatureVocationType.All || (CreatureVocationType)professional == proto[j])
                    {
                        appropriate = true;
                    }
                }
                if (appropriate)
                {
                    award.Add(items);
                }
            }
        }
        return award;
    }

    private void CloseInfo(PExpendItem[] expend, int throughTimes)
    {
        GeneralConfigInfo generalInfo = GeneralConfigInfo.defaultConfig;
        if (generalInfo == null)
        {
            Logger.LogError("pet time is null");
            return;
        }

        var remainTimes = 0;
        if (Module_Home.instance.LocalPetInfo !=null) remainTimes = Module_Home.instance.LocalPetInfo.times;
        var sysnText = pettask_txt[10] + remainTimes + pettask_txt[9];
        Util.SetText(m_restTimes, sysnText);

        sureopen.onClick.RemoveAllListeners();
        sureopen.onClick.AddListener(() =>
        {
            Module_Home.instance.Opentask();
        });

        if (expend.Length == 0)
        {
            Logger.LogDetail("NO expend");
            m_expendTip.gameObject.SetActive(false);
            return;
        }
        m_expendTip.gameObject.SetActive(true);
        int propid = expend[0].expendid;
        int propnum = expend[0].expendnum;
        PropItemInfo item = ConfigManager.Get<PropItemInfo>(propid);//id 是1 或者2 
        if (item == null)
        {
            Logger.LogError("expend prop is null");
            m_expendTip.gameObject.SetActive(false);
            return;
        }
        AtlasHelper.SetItemIcon(costimage, item);
        int num = (int)Module_Player.instance.roleInfo.diamond;
        if (propid == 1)
        {
            num = (int)Module_Player.instance.roleInfo.coin;
        }
        var synthetic = "×" + propnum.ToString() + pettask_txt[8] + num + pettask_txt[9];
        Util.SetText(havenum, synthetic);

    }

}