// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-07-30      14:14
//  * LastModify：2018-07-30      14:16
//  ***************************************************************************************************/

#region

using UnityEngine;
using UnityEngine.UI;

#endregion

public class AwakeWindow_TaskComfirm : SubWindowBase<Window>
{
    private ChaseTaskItem   chaseItem;
    private Button          createRoom;
    private Button          joinRoom;
    private Button          close;
    private Transform       btnParent;

    private ChaseTask       task;
    
    protected override void InitComponent()
    {
        base.InitComponent();
        chaseItem     = Root.GetComponentDefault<ChaseTaskItem>();
        btnParent     = Root.GetComponent<Transform>("kuang/normal_Btn/awake");
        createRoom    = Root.GetComponent<Button>("kuang/normal_Btn/awake/create_Btn");
        joinRoom      = Root.GetComponent<Button>("kuang/normal_Btn/awake/join_Btn");
        close         = Root.GetComponent<Button>("close_button");
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;

        task = p[0] as ChaseTask;
        chaseItem    .RefreshDetailPanel(task, null);
        btnParent   ?.gameObject.SetActive(true);
        joinRoom    ?.onClick.AddListener(OnJoinRoomClick);
        createRoom  ?.onClick.AddListener(OnCreateRoomClick);
        close       ?.onClick.AddListener(()=>
        {
            WindowCache.Hide();
            UnInitialize();
        });
        return true;
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (base.UnInitialize(hide))
        {
            joinRoom    ?.onClick.RemoveListener(OnJoinRoomClick);
            createRoom  ?.onClick.RemoveListener(OnCreateRoomClick);
            close       ?.onClick.RemoveListener(()=> UnInitialize());
            return true;
        }
        return false;
    }

    private void OnCreateRoomClick()
    {
        moduleAwakeMatch.Request_EnterRoom(false, task);
    }

    private void OnJoinRoomClick()
    {
        moduleAwakeMatch.Request_EnterRoom(true, task);
    }

    public override bool OnReturn()
    {
        if (isInit)
        {
            UnInitialize();
            return true;
        }
        return false;
    }

    public void RefreshTask(ChaseTask rTask)
    {
        task = rTask;
        chaseItem.RefreshDetailPanel(task, null);
        btnParent   ?.gameObject.SetActive(true);
    }
}