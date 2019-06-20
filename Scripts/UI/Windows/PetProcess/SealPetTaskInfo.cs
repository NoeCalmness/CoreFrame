// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-06-30      14:33
//  *LastModify：2018-12-07      15:05
//  ***************************************************************************************************/

#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

[Serializable]
public class SealPetTaskInfo
{
    public  float               RecieveTime;

    public readonly PetTask     Task;
    private PPetTaskInfo        TaskInfo;

    public SealPetTaskInfo(int rTaskId)
    {
        Task = ConfigManager.Get<PetTask>(rTaskId);
    }

    public SealPetTaskInfo(PetTask rTask)
    {
        Task = rTask;
    }

    public ushort ID { get { return (ushort)Task.ID; } }
    public int Count { get { return TaskInfo?.count ?? 0; } }
    public int State { get { return TaskInfo?.state ?? 0; } }

    public bool TimesUseUp
    {
        get
        {
            if (TaskInfo == null)
                return false;
            return LimitCount > 0 && TaskInfo.count == LimitCount;
        }
    }

    public int LimitCount { get { return Mathf.Max(Task.daily, Task.week); } }

    public bool IsWeekTask { get { return Task.week > 0; } }


    public float RestTime()
    {
        if (TaskInfo == null)
            return 0;
        return TaskInfo.restTime - (Time.realtimeSinceStartup - RecieveTime);
    }

    public ulong GetTrainingPet(int rIndex)
    {
        if (null == TaskInfo?.trainPetId) return 0;

        if (rIndex < 0 || rIndex >= TaskInfo.trainPetId.Length)
            return 0;

        return TaskInfo.trainPetId[rIndex];
    }

    public int GetTrainingPetCount()
    {
        if (null == TaskInfo || null == TaskInfo.trainPetId) return 0;
        return TaskInfo.trainPetId.Length;
    }

    public void SetPetTaskInfo(PPetTaskInfo rInfo)
    {
        TaskInfo = rInfo.Clone();
        RecieveTime = Time.realtimeSinceStartup;
    }

    public float CalcSuccessRate(PetInfo[] rList)
    {
        float v = 0;
        for (var i = 0; i < rList.Length; i++)
        {
            var pet = rList[i];
            if (pet == null) continue;
            v += pet.Star * pet.Level;
        }
        return Mathf.Clamp01(v / Task.diffcult);
    }

    public float CalcSuccessRate(List<PetInfo> rList)
    {
        return CalcSuccessRate(rList.ToArray());
    }
}