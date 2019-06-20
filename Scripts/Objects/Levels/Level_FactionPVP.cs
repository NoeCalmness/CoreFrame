// /**************************************************************************************************
//  * Copyright (C) 2017-2018 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-04-29      16:30
//  *LastModify：2019-04-29      16:30
//  ***************************************************************************************************/

public class Level_FactionPVP : Level_PVP
{
    protected override string endWindow
    {
        get
        {
            return CombatConfig.sdefaultFactionEndWindow;
        }
    }


    protected override bool WaitEndState()
    {
        if (modulePVP.settlementData != null)
        {
            moduleGlobal.UnLockUI();

            return true;
        }

        moduleGlobal.LockUI("等待结算信息...");
        return false;
    }
}