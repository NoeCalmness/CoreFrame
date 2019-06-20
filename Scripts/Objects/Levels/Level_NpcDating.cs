/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-01-11
 * 
 ***************************************************************************************************/

using System.Collections.Generic;

public class Level_NpcDating : Level
{
    protected override List<string> BuildPreloadAssets()
    {
        var asstes = base.BuildPreloadAssets();
        if(!moduleNPCDating.isDebug) asstes.AddRange(Module_NPCDating.GetDatingPreAssets(moduleNPCDating.enterSceneData.eventId));
        asstes.AddRange(Module_Battle.BuildPlayerSimplePreloadAssets());//预加载主角资源
        return asstes;
    }

    protected override void OnLoadComplete()
    {
        var curSceneType = moduleNPCDating.GetDatingSceneType(current.levelID);

        moduleNPCDating.SetCurDatingScene(curSceneType);

        if (moduleNPCDating.isDebug) moduleNPCDating.GMDatingEventCallBack();
        else
        {
            bool bCheckReConnect = moduleNPCDating.CheckDatingReconnect(curSceneType);

            if (!bCheckReConnect) moduleNPCDating.DoDatingEvent(moduleNPCDating.enterSceneData.eventId);
        }

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Module_Story.DestoryStory();
    }

}
