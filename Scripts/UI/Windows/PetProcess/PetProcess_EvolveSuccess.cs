// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 宠物进化成功界面.
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-06-13      15:08
//  * LastModify：2018-07-11      14:23
//  ***************************************************************************************************/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetProcess_EvolveSuccess : SubWindowBase<Window_Sprite>
{
    [Widget("evolve_panel/evolve_success/attr_group")]
    private Transform AttrGroupRoot;

    [Widget("evolve_panel/evolve_success/attr_group/content", false)]
    private Transform AttrTemplete;

    private readonly Dictionary<CreatureFields, Transform> cache = new Dictionary<CreatureFields, Transform>();

    [Widget("evolve_panel/evolve_success/stage_txt")]
    private Text gradeText;

    [Widget("evolve_panel/evolve_success/skill_group/skill_01")]
    private Image leftIcon;

    [Widget("evolve_panel/evolve_success/skill_group/skill_01/Text")]
    private Text leftName;

    [Widget("evolve_panel/evolve_success/skill_group/skill_02")]
    private Image rightIcon;

    [Widget("evolve_panel/evolve_success/skill_group/skill_02/Text")]
    private Text rightName;

    [Widget("evolve_panel/evolve_success/skill_group/skill_01")]
    private Button skillLeftButton;

    [Widget("evolve_panel/evolve_success/skill_group/skill_02")]
    private Button skillRightButton;

    [ArrayWidget("evolve_panel/evolve_success/stage_star_group/star_01",
                "evolve_panel/evolve_success/stage_star_group/star_02",
                "evolve_panel/evolve_success/stage_star_group/star_03")]
    private Transform[] stars;

    private Effect m_evolveEffect;

    private PetSelectModule petSelectModule { get { return (WindowCache as Window_Sprite)?.SelectModule; } }

    public override bool Initialize(params object[] p)
    {
        if(base.Initialize(p))
        {
            Level.PrepareAsset<GameObject>(GeneralConfigInfo.defaultConfig.evolveEffect.effect, (go) =>
            {
                if (!isInit)
                    return;
                m_evolveEffect = Effect.Create(GeneralConfigInfo.defaultConfig.evolveEffect,
                    ((Level_Home) Level.current)?.PetGameObject?.transform);
            });
            moduleGlobal.ShowGlobalLayerDefault(1, false);
            Refresh();
            var pet = petSelectModule.selectInfo;

            skillLeftButton?.onClick.AddListener(() => { moduleGlobal.UpdateSkillTip(pet.GetSkill(pet.GetUpGradeInfo(pet.Grade - 1)), pet.AdditiveLevel - 1, pet.Mood); });
            skillRightButton?.onClick.AddListener(() => { moduleGlobal.UpdateSkillTip(pet.GetSkill(), pet.AdditiveLevel, pet.Mood); });
        }
        return true;
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (base.UnInitialize(hide))
        {
            moduleGlobal.ShowGlobalLayerDefault();
            Util.ClearChildren(AttrGroupRoot);
            cache.Clear();
            skillLeftButton?.onClick.RemoveAllListeners();
            skillRightButton?.onClick.RemoveAllListeners();

            m_evolveEffect?.Destroy();
            return true;
        }
        return false;
    }

    private void Refresh()
    {
        var petInfo = petSelectModule.selectInfo;
        Util.SetText(gradeText, petInfo.UpGradeInfo.CombineGradeName(petInfo.Star));
        SetUIStar(petInfo.Star);
        RefreshPrev();
        RefreshNow();

        parentWindow.RefreshModel();
    }

    private void RefreshNow()
    {
        var petInfo = petSelectModule.selectInfo;
        if (petInfo == null) return;
        var grade = petInfo.GetUpGradeInfo(petInfo.Grade);

        var skill = petInfo.GetSkill(grade);
        if (skill != null)
        {
            Util.SetText(rightName, petInfo.SkillName);
            AtlasHelper.SetShared(rightIcon, skill.skillIcon);
        }

        var showList = petInfo.Attribute;
        for (var i = 0; i < showList.Count; i++)
        {
            if (cache.ContainsKey((CreatureFields)showList[i].id))
            {
                var t = cache[(CreatureFields)showList[i].id].GetComponent<Text>("attr_txt_03");
                Util.SetText(t, showList[i].ValueString());
            }
        }
    }

    private void RefreshPrev()
    {
        var petInfo = petSelectModule.selectInfo;
        if (petInfo == null) return;
        var grade = petInfo.GetUpGradeInfo(petInfo.Grade - 1);
        var skill = petInfo.GetSkill(grade);
        if (skill != null)
        {
            Util.SetText(leftName, Util.Format(ConfigText.GetDefalutString(skill.skillName), petInfo.AdditiveLevel - 1));
            AtlasHelper.SetShared(leftIcon, skill.skillIcon);
        }

        if (AttrGroupRoot == null)
            return;
        //创建属性条
        var rList = petInfo.GetAttribute(petInfo.Level, petInfo.Grade - 1);
        foreach (var aShow in rList)
        {
            var t = AttrGroupRoot.AddNewChild(AttrTemplete);
            t.SafeSetActive(true);
            BindProperty(t, aShow);
            cache.Add((CreatureFields)aShow.id, t);
        }
    }

    private void BindProperty(Transform t, ItemAttachAttr show)
    {
        Util.SetText(t.GetComponent<Text>("attr_txt_02"), show.ValueString());
        Util.SetText(t.GetComponent<Text>("attr_txt_01"), show.TypeString());
    }

    private void SetUIStar(int star)
    {
        if(stars == null) return;
        for (var i = 0; i < stars.Length; i++)
        {
            stars[i].SafeSetActive(i < star);
        }
    }
}
