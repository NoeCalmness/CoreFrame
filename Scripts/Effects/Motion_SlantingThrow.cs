// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-07-17      15:03
//  * LastModify：2018-07-17      15:52
//  ***************************************************************************************************/

#region

using System;

#endregion

public interface ILogicTransform
{
    Vector3_ LogicPosition { get; set; }
}

[Serializable]
public class SlantingThrowData
{
    public double acceleration;
    public double strength;
    public double velocity;
    public Vector3_ offset;
}

public class Motion_SlantingThrow
{
    private readonly SlantingThrowData   data;
    public ILogicTransform               end;
    public ILogicTransform               start;
    private Vector3_                     startPos;
    private double                       timer;

    public Motion_SlantingThrow(ILogicTransform rSource, ILogicTransform rTarget, SlantingThrowData rData)
    {
        start      = rSource;
        end        = rTarget;
        data       = rData;
        Initialize();
    }

    private void Initialize()
    {
        startPos = start.LogicPosition;
    }

    private double CalcSlanteThrowTime()
    {
        return Vector3_.Distance(startPos, end.LogicPosition) / data.velocity * 0.5;
    }

    public Vector3_ Evaluate(double time)
    {
        timer += time;
        var throwTime = CalcSlanteThrowTime();

        Vector3_ endPos = (Vector3_)end.LogicPosition + data.offset;
        //计算水平方向运动
        Vector3_ dir = endPos - (Vector3_)start.LogicPosition;
        dir.Normalize();
        var tp = dir * time * (data.velocity + data.acceleration * timer);

        if (throwTime < timer)
        {
            //向下掉落
            tp += (endPos - (Vector3_)start.LogicPosition).normalized*data.velocity*time;
            //tp += Vector3_.down * data.velocity * data.strength * time;
            tp += (Vector3_)start.LogicPosition;
            tp.y = Mathd.Max(tp.y, endPos.y);
            return tp;
        }

        //向上抛
        tp += Vector3_.up * data.velocity * data.strength * time;
        return tp + (Vector3_)start.LogicPosition;
    }
}