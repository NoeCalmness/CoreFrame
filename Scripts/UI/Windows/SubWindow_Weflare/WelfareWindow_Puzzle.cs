using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WelfareWindow_Puzzle : SubWindowBase<Window_Welfare>
{
    private Button m_saveBtn;
    private Button m_totalBtn;
    private Text m_totalTxt;
    private Image m_photo;
    private RectTransform m_lockMask;
    private RectTransform m_totalGroup;
    private RectTransform m_checkGroup;
    private ScrollView m_puzzleScroll;
    private DataSource<PWeflareAward> m_puzzleData;
    List<Transform> m_totalObj = new List<Transform>();//集齐之后的奖励列表
    List<Toggle> m_newCheck = new List<Toggle>();//七日
    List<Image> m_lockObj = new List<Image>();//锁

    protected override void InitComponent()
    {
        base.InitComponent();

        m_saveBtn      = Root.GetComponent<Button>("left/Button");
        m_totalBtn     = Root.GetComponent<Button>("right/bottom/Button");
        m_totalTxt     = Root.GetComponent<Text>("right/bottom/Button/Text");
        m_photo        = Root.GetComponent<Image>("left/bg/photo");
        m_lockMask     = Root.GetComponent<RectTransform>("left/mask");
        m_totalGroup   = Root.GetComponent<RectTransform>("right/bottom/present");
        m_checkGroup   = Root.GetComponent<RectTransform>("checkBox");
        m_puzzleScroll = Root.GetComponent<ScrollView>("right/scrollView");
        m_puzzleData   = new DataSource<PWeflareAward>(null, m_puzzleScroll, SetPuzzleInfo);
        GetPuzzleGroup();

        Util.SetText(Root.GetComponent<Text>("left/Button/Text"), 211, 26);
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p)) return false;
        m_totalBtn.onClick.RemoveAllListeners();
        m_totalBtn.onClick.AddListener(delegate
        {
            parentWindow.GetAward(moduleWelfare.chooseInfo?.rewardid[0], 1);
        });
        return true;
    }

    protected override void RefreshData(params object[] p)
    {
        if (moduleWelfare.chooseInfo == null) return;
        SetCheckClick();
    }

    private void GetPuzzleGroup()
    {
        m_lockObj.Clear();
        foreach (Transform item in m_lockMask)
        {
            var img = item.GetComponentDefault<Image>();
            if (img == null) return;
            m_lockObj.Add(img);
        }
        m_newCheck.Clear();
        foreach (Transform item in m_checkGroup)
        {
            Toggle tog = item.GetComponentDefault<Toggle>();
            m_newCheck.Add(tog);
        }
        m_totalObj.Clear();
        foreach (Transform item in m_totalGroup)
        {
            m_totalObj.Add(item);
        }
    }

    private void SetPuzzleHint(PWeflareInfo info)
    {
        for (int i = 0; i < moduleWelfare.puzzleList.Count; i++)
        {
            if (i >= m_newCheck.Count || m_newCheck[i] == null) continue;
            if (!m_newCheck[i].isOn) continue;

            var hint = m_newCheck[i].transform.Find("hint");
            hint.SafeSetActive(moduleWelfare.GetWelfareHint(info));
        }
    }

    private void SetCheckClick()
    {
        for (int i = 0; i < m_newCheck.Count; i++)
        {
            var tog = m_newCheck[i];
            tog.SafeSetActive(false);
            tog.isOn = false;
            if (i >= moduleWelfare.puzzleList.Count) continue;
            var puzzleInfo = moduleWelfare.puzzleList[i];
            tog.onValueChanged.RemoveAllListeners();
            tog.onValueChanged.AddListener(delegate
            {
                if (!tog.isOn) return;
                SetPuzzlePlane(puzzleInfo);
                parentWindow.LableClickHint(puzzleInfo);
            });
        }
        SetPuzzleLableState(true);
    }

    private void SetPuzzleLableState(bool setIndex)
    {
        var checkIndex = 0;
        for (int i = 0; i < moduleWelfare.puzzleList.Count; i++)
        {
            if (i >= m_newCheck.Count || m_newCheck[i] == null) continue;
            PWeflareInfo info = moduleWelfare.puzzleList[i];
            if (info == null) continue;
            m_newCheck[i].SafeSetActive(true);
            var togTxt = m_newCheck[i].transform.Find("Label")?.GetComponentDefault<Text>();
            Util.SetText(togTxt, info.title);
            var hint = m_newCheck[i].transform.Find("hint");
            hint.SafeSetActive(moduleWelfare.GetWelfareHint(info));

            if (m_newCheck.Count > 0 && i == 0) m_newCheck[i].interactable = true;
            if (i >= m_newCheck.Count - 1) continue;
            if (info.rewardid != null && info.rewardid.Length > 0)
            {
                if (info.rewardid[0] != null && m_newCheck[i + 1] != null)
                {
                    m_newCheck[i + 1].interactable = false;
                    if (info.rewardid[0].state == 2)
                    {
                        m_newCheck[i + 1].interactable = true;
                        checkIndex = i + 1;
                    }
                }
            }
        }
        if(setIndex && checkIndex < m_newCheck.Count) m_newCheck[checkIndex].isOn = true;
    }

    private void SetPuzzlePlane(PWeflareInfo info)
    {
        if (info == null) return;
        Util.SetText(WindowCache.GetComponent<Text>("jigsaw_Panel/bg/Text"), info.title.Substring(0, 1));
        Util.SetText(WindowCache.GetComponent<Text>("jigsaw_Panel/bg/content_txt"), info.title.Substring(1));

        moduleWelfare.chooseInfo = info;
        for (int i = 0; i < m_lockObj.Count; i++)
        {
            m_lockObj[i].saturation = 0;
        }
        List<PWeflareAward> award = new List<PWeflareAward>();
        for (int i = 0; i < info.rewardid.Length; i++)
        {
            if (info.rewardid[i] == null) continue;
            //约定 拼图活动的第一个奖励为最终的整体奖励 第一个奖励的第一个物品为拼图图片
            if (i == 0)
            {
                var save = info.rewardid[0].state != 0 ? true : false;
                m_saveBtn.interactable = save;
                m_photo.gameObject.SetActive(save);
                m_lockMask.gameObject.SetActive(!save);

                Util.SetText(WindowCache.GetComponent<Text>("jigsaw_Panel/right/bottom/title"), info.rewardid[0].rewardname);
                parentWindow.SetBtnState(info.rewardid[0].state, m_totalBtn, m_totalTxt);
                SetRewardInfo(info.rewardid[0].reward, m_totalObj, true);
            }
            else
            {
                var index = i - 1;
                if (index < 0) index = 0;
                if (info.rewardid[i].state == 2 && index < m_lockObj.Count) m_lockObj[index].saturation = 1;
                award.Add(info.rewardid[i]);
            }
        }
        m_puzzleData.SetItems(award);
    }

    private void SetRewardInfo(PItem2[] itemlist, List<Transform> tranList, bool first = false)
    {
        for (int i = 0; i < tranList.Count; i++)
        {
            tranList[i].SafeSetActive(false);
        }
        var index = 0;
        for (int i = 0; i < itemlist.Length; i++)
        {
            var info = itemlist[i];
            if (info == null || index >= tranList.Count) continue;
            var prop = ConfigManager.Get<PropItemInfo>(info.itemTypeId);
            if (!prop || !prop.IsValidVocation(modulePlayer.proto)) continue;

            if (first && prop.itemType == PropType.WallPaper && prop.subType == 1)
            {
                if (prop == null || prop.mesh == null || prop.mesh.Length < 1) continue;
                m_saveBtn.onClick.RemoveAllListeners();
                m_saveBtn.onClick.AddListener(delegate { TakePhoto(prop.mesh[0]); });
                UIDynamicImage.LoadImage(m_photo.transform, prop.mesh[0]);
                for (int j = 0; j < m_lockObj.Count; j++)
                {
                    if (j + 1 >= prop.mesh.Length) continue;
                    AtlasHelper.SetPuzzle(m_lockObj[j].gameObject, prop.mesh[j + 1], null, true);
                }
                continue;
            }
            tranList[index].SafeSetActive(true);
            Util.SetItemInfo(tranList[index], prop, info.level, (int)info.num, false, info.star);
            if (!(prop.itemType == PropType.WallPaper && prop.subType == 1))
            {
                var btn = tranList[index].GetComponentDefault<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(delegate
                {
                    moduleGlobal.UpdateGlobalTip(info, true, false);
                });
            }
            index++;
        }
    }

    private void SetPuzzleInfo(RectTransform rt, PWeflareAward info)
    {
        if (info == null) return;
        var get = rt.Find("get");
        get.SafeSetActive(info.state != 0);
        var getTxt = rt.Find("get/icon/txt").GetComponentDefault<Text>();
        Util.SetText(getTxt, (info.index - 1).ToString());
        var title = rt.Find("txt").GetComponent<Text>();
        Util.SetText(title, info.rewardname);
        var desc = rt.Find("Text").GetComponent<Text>();
        Util.SetText(desc, info.desc);

        List<Transform> awardList = new List<Transform>();
        var objList = rt.Find("present");
        foreach (Transform item in objList)
        {
            awardList.Add(item);
        }
        SetRewardInfo(info.reward, awardList);

        var getBtn = rt.Find("Button").GetComponentDefault<Button>();
        var getBtnTxt = rt.Find("Button/Text").GetComponentDefault<Text>();
        var recived = rt.Find("received");
        Util.SetText(rt.Find("received/Text").GetComponent<Text>(), 211, 32);

        parentWindow.SetBtnState(info.state, getBtn, getBtnTxt);
        getBtn.gameObject.SetActive(info.state != 2);
        recived.gameObject.SetActive(info.state == 2);
        
        SetInfoProgress(rt, info);

        getBtn.onClick.RemoveAllListeners();
        getBtn.onClick.AddListener(delegate
        {
            parentWindow.GetAward(info, info.index);
        });
    }

    private void SetInfoProgress(RectTransform rt, PWeflareAward info)
    {
        if (info.reachnum == null && info.reachnum.Length == 0 || info.reachnum[0] == null) return;

        var progress = rt.Find("process");
        var valueTxt = rt.Find("process/process_txt").GetComponent<Text>();
        var targetTxt = rt.Find("process/value").GetComponent<Text>();
        var fill = rt.Find("process/fill").GetComponent<Image>();

        var target = info.reachnum[0].progress;
        var value = 0;
        PWeflareData dataProgress = null;
        dataProgress = moduleWelfare.GetProgress(moduleWelfare.chooseInfo, info.reachnum[0].type);
        if (dataProgress != null) value = moduleWelfare.GetValueByReachType(dataProgress, info.reachnum[0].days);

        progress.gameObject.SetActive(moduleWelfare.ShowPuzzleProgress(info.reachnum[0].type));
        if (value > target) value = target;
        Util.SetText(valueTxt, value.ToString());
        Util.SetText(targetTxt, target.ToString());

        fill.fillAmount = (float)value / (float)target;
    }

    #region save wallpaper

    private void TakePhoto(string icon)
    {
        if (moduleWelfare.chooseInfo?.rewardid[0].state == 0)
        {
            moduleGlobal.ShowMessage("not get all icon");
            return;
        }
        if (!icon.EndsWith(".png")) icon += ".png";
        var data = TextureToTexture2D(m_photo.mainTexture);
        if (data == null) return;
        var r = Util.SaveFile(LocalFilePath.SCREENSHOT + "/" + icon, data.EncodeToPNG(), true, true);
        if (r == null) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(211, 30));
        else moduleGlobal.ShowMessage(ConfigText.GetDefalutString(211, 31));
    }

    private Texture2D TextureToTexture2D(Texture texture)
    {
        if (texture.GetType() != (typeof(Texture2D))) return null;

        Texture2D newTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        RenderTexture current = RenderTexture.active;
        RenderTexture render = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        Graphics.Blit(texture, render);
        RenderTexture.active = render;

        newTexture.ReadPixels(new Rect(0, 0, render.width, render.height), 0, 0);
        newTexture.Apply();

        RenderTexture.active = current;
        RenderTexture.ReleaseTemporary(render);

        return newTexture;
    }
    #endregion
    
    void _ME(ModuleEvent<Module_Welfare> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Welfare.EventWelfareGetSucced:
                var info = e.param1 as PWeflareInfo;
                var index = Util.Parse<int>(e.param2.ToString());
                if (info == null) return;
                RefreshGetSucced(info, index);
                parentWindow.WelfareSuccedShow(info.rewardid, index, info.type);
                break;
            case Module_Welfare.EventWelfareMoneyChange:
                var money = e.msg as PWeflareInfo;
                RefreshChangeInfo(money, false);
                break;
            case Module_Welfare.EventWelfareCanGet:
            case Module_Welfare.EventWelfareSpecificInfo:
                var newInfo = e.msg as PWeflareInfo;
                RefreshChangeInfo(newInfo, true);
                break;
        }
    }

    private void RefreshGetSucced(PWeflareInfo info, int index)
    {
        if (!parentWindow.RestWelfarePlane(info, index)) return;

        var sub = -1;
        var lockId = 0;
        for (int i = 0; i < info.rewardid.Length; i++)
        {
            if (info.rewardid[i].index == index)
            {
                sub = i;
                if (index == 1) parentWindow.SetBtnState(info.rewardid[i].state, m_totalBtn, m_totalTxt);
                lockId = i - 1;
                if (lockId < 0) lockId = 0;
                if (info.rewardid[i].state == 2 && lockId < m_lockObj.Count) m_lockObj[lockId].saturation = 1;

            }
        }
        if (sub < 0) return;
        var aIndex = moduleWelfare.puzzleList.FindIndex(a => a.id == info.id);
        if (aIndex != -1)
        {
            if (aIndex >= 0 && aIndex < m_newCheck.Count - 1)
            {
                if (info.rewardid[0] != null && m_newCheck[aIndex + 1] != null)
                    m_newCheck[aIndex + 1].interactable = info.rewardid[0].state == 2 ? true : false;
            }
        }
        m_puzzleData.UpdateItem(lockId);
        SetPuzzleHint(moduleWelfare.chooseInfo);
        if (parentWindow.m_welfareTog.isOn) parentWindow.m_lableShow.UpdateItems();
    }
    
    private void RefreshChangeInfo(PWeflareInfo info, bool refresh)
    {
        if (refresh) parentWindow.RefreshLablePlane();
        if (moduleWelfare.chooseInfo == null) return;

        if (info.id == moduleWelfare.chooseInfo.id)
        {
            SetPuzzleHint(info);
            SetPuzzlePlane(info);
        }
        else if (info.type == moduleWelfare.chooseInfo.type) SetPuzzleLableState(false);
    }

}


