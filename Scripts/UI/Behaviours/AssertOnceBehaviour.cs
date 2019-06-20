// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-12-14      11:37
//  *LastModify：2018-12-14      11:37
//  ***************************************************************************************************/

using UnityEngine;

public class AssertOnceBehaviour : MonoBehaviour
{
    public bool isInited;
    public void AssertInit()
    {
        if (isInited) return;

        isInited = true;

        Init();
    }

    protected virtual void Awake()
    {
        AssertInit();
    }

    protected virtual void Init() { }
}
