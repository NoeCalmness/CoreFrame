/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 主要用于标识所有任务的枚举（不仅针对追捕）
 * 
 * Author:   Lee
 * Version:  0.1
 * Created:  2017-12-05
 * 
 ***************************************************************************************************/

public enum TaskType
{
    None = -1,

    Emergency,  //紧急

    Easy,       //简单

    Difficult,  //困难

    Active,     //活动

    Awake,      //觉醒副本

    Nightmare,  //噩梦关卡

    Train,      //精灵历练

    GaidenChapterOne        = 200,         //外传第一章
    GaidenChapterTwo        = 201,         //外传第二章
    GaidenChapterThree      = 202,         //外传第三章
    GaidenChapterFour       = 203,         //外传第四章
    GaidenChapterFive       = 204,         //外传第五章
    GaidenChapterSix        = 205,         //外传第六章

    Count,      
}

/// <summary>
/// 任务完成状态
/// </summary>
public enum EnumChaseTaskFinishState
{
    None = 0,

    Accept,

    Finish,

    NoEnterAgain,       //某些关卡只能挑战一次，挑战之后就不能再次挑战了
}
