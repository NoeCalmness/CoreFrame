// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-07-23      20:04
//  * LastModify：2018-07-24      14:20
//  ***************************************************************************************************/
public class PetSkillData : LogicObject
{
    private PetSkillData() { }

    #region static functions

    public static PetSkillData Create(PetSkill.Skill rSkill)
    {
        var skillData = _Create<PetSkillData>();
        skillData.skillInfo = rSkill;

        skillData.Initialized();
        return skillData;
    }

    #endregion

    #region private functions

    private void Reset()
    {
        if (skillInfo != null)
            cd = skillInfo.cd;
    }

    #endregion

    #region Fields

    private int             cd;

    public  PetSkill.Skill  skillInfo;
    private int             useCount;
    private int             useCountMax;

    #endregion

    #region public fuction

    public void OnUseSkill()
    {
        useCount ++;
        Reset();
    }

    public void ResetUseCountMax(int rMax)
    {
        useCountMax = rMax;
        useCount = 0;
    }


    public void Initialized()
    {
        enableUpdate = true;
        useCountMax = skillInfo.limitCount > 0 ? skillInfo.limitCount : int.MaxValue;
    }

    public override void OnUpdate(int diff)
    {
        base.OnUpdate(diff);
        cd -= diff;
    }

    public void ResetCD(int rCd)
    {
        cd = rCd;
    }

    #endregion

    #region properties

    public bool IsReadly {get { return cd <= 0; } }

    public bool CanUse { get { return IsReadly && useCount < useCountMax; } }

    public int SkillName { get { return skillInfo != null ? skillInfo.skillName : 0; } }

    #endregion
}
