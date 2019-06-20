/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Login scene.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-04-06
 * 
 ***************************************************************************************************/

public class Level_Login : Level
{
    private static bool m_firstEnter = true;

    protected override void OnLoadComplete()
    {
        Window.ShowAsync<Window_Login>();
        if (m_firstEnter) Launch.Updater.Show(1, () => { });

        m_firstEnter = false;
    }
}
