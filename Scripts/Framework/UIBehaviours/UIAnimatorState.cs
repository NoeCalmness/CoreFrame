/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * UI Animator State behaviour
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-05-10
 * 
 ***************************************************************************************************/

using System;
using UnityEngine;

public class UIAnimatorState : StateMachineBehaviour
{
    public static readonly int hashIn  = Animator.StringToHash("In");
    public static readonly int hashOut = Animator.StringToHash("Out");
    public static readonly int hashEnd = Animator.StringToHash("End");

    private WindowBehaviour m_window = null;

    public Action<bool, bool> onStateChange;

    private void Initialize(Animator animator)
    {
        if (m_window) return;
        m_window = animator.GetComponent<WindowBehaviour>();
        m_window?.SetAnimatorCallback(this);
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Initialize(animator);

        var hash = stateInfo.shortNameHash;
        if (hash == hashEnd) return;

        onStateChange?.Invoke(hash == hashIn, false);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var hash = stateInfo.shortNameHash;
        if (hash == hashEnd || stateInfo.normalizedTime < 1) return;

        onStateChange?.Invoke(hash == hashIn, true);
    }

    private void OnDestroy()
    {
        m_window = null;
        onStateChange = null;
    }
}
