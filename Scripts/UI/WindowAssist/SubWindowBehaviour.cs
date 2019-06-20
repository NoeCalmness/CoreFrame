// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-04-04      17:37
//  *LastModify：2019-04-04      17:37
//  ***************************************************************************************************/

using UnityEngine;

public class SubWindowBehaviour : MonoBehaviour
{
    private SubWindowBase m_subWindow;
    [SerializeField]
    private bool m_uninitalizeWhenDisable = true;

    public void Set(SubWindowBase rWindow, bool rUninitalizeWhenDisable)
    {
        m_subWindow = rWindow;
        m_uninitalizeWhenDisable = rUninitalizeWhenDisable;
    }

    public void Set(bool rUninitalizeWhenDisable)
    {
        m_uninitalizeWhenDisable = rUninitalizeWhenDisable;
    }

    private void OnDisable()
    {
        if(m_uninitalizeWhenDisable)
            m_subWindow.UnInitialize();
    }
}
