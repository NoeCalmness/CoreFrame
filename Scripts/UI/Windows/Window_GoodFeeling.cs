// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-10-30      15:57
//  * LastModify：2018-10-30      15:57
//  ***************************************************************************************************/

using UnityEngine.UI;

public class Window_GoodFeeling : Window
{
    private Image avatar;

    protected override void OnOpen()
    {
        base.OnOpen();
        InitComponent();
    }

    private void InitComponent()
    {
        avatar = GetComponent<Image>("panel_dialog_goodfeeling/background/player_avtar");
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault(1, false);

        UIDynamicImage.LoadImage(avatar.transform, $"dialog_avatar_player_0{modulePlayer.proto}_01");
    }
}
