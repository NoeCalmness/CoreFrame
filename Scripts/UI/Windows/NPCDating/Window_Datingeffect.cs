/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-02-19
 * 
 ***************************************************************************************************/

using System;
using UnityEngine;

public class Window_Datingeffect : Window
{
    private RectTransform m_rectEffectNode;

    private bool m_bHideWindow = true;
    protected override void OnOpen()
    {
        isFullScreen = false;
        m_rectEffectNode = GetComponent<RectTransform>("effnode");
    }

    protected override void OnShow(bool forward)
    {
        PlayEffect();
    }

    protected override void OnHide(bool forward)
    {
        moduleNPCDating.ContinueBehaviourCallBack();
    }

    public override void OnRootUpdate(float diff)
    {
        if (!m_bHideWindow && m_rectEffectNode.childCount == 0)
        {
            m_bHideWindow = true;
            Hide<Window_Datingeffect>();
        } 
    }

    private void PlayEffect()
    {
        UIEffectManager.PlayEffectAsync(m_rectEffectNode, moduleNPCDating.tranEffectName,"",0,(e)=> { m_bHideWindow = false; });
    }
}
