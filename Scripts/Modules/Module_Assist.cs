// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 助战模块
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-12-14      13:13
//  *LastModify：2018-12-14      13:13
//  ***************************************************************************************************/
public class Module_Assist : Module<Module_Assist>
{
    public const string Notify_AssistList = "NotifyAssistList";
    public PAssistInfo[] AssistList;

    #region request

    public void RequestAssistList()
    {
        var p = PacketObject.Create<CsAssistList>();
        session.Send(p);
    }

    #endregion

    #region _Packet

    void _Packet(ScAssistList msg)
    {
        msg.assistList.CopyTo(ref AssistList);
        DispatchModuleEvent(Notify_AssistList, msg);
    }
    #endregion

}
