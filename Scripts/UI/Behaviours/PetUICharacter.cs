// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-06-20      11:26
//  * LastModify：2018-07-12      17:20
//  ***************************************************************************************************/

public class PetUICharacter : UICharacter
{
    public string exclude;
    protected override void OnEnable()
    {
        if (!Game.started) return;

        base.OnEnable();

        Level.current.mainCamera.enabled = true;
        if(string.IsNullOrEmpty(exclude))
            Module_Home.instance.HideOthers(Module_Home.PET_OBJECT_NAME);
        else
            Module_Home.instance.HideOthers(exclude);
    }

    protected override void OnDisable()
    {
        if (!Game.started) return;

        base.OnDisable();

        var level = Level.current as Level_Home;
        if (level == null || level.PetGameObject == null) return;
        level.PetGameObject.SetActive(false);
    }
}
