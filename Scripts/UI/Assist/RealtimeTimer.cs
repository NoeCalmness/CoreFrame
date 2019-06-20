// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-03-19      9:51
//  *LastModify：2019-03-19      9:51
//  ***************************************************************************************************/

using UnityEngine;

public class RealtimeTimer
{
    private float m_startTime;

    public float RealTime { get { return Time.realtimeSinceStartup - m_startTime; } }

    public int IntRealTime { get { return Mathf.RoundToInt(RealTime); } }

    public RealtimeTimer()
    {
        m_startTime = Time.realtimeSinceStartup;
    }

    public void Start()
    {
        m_startTime = Time.realtimeSinceStartup;
    }
}

public class RealtimeCountDownTimer : RealtimeTimer
{
    private float m_countDownTime;

    public float RealCountDownTime { get { return m_countDownTime - RealTime; } }

    public int IntRealCountDownTime { get { return Mathf.RoundToInt(RealCountDownTime); } }

    public RealtimeCountDownTimer(float rCountDownTime) : base()
    {
        m_countDownTime = rCountDownTime;
    }

    public void Reset(int rCountDownTime)
    {
        m_countDownTime = rCountDownTime;
        Start();
    }
}
