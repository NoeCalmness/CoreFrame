/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Mathd use double params
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-09-06
 * 
 ***************************************************************************************************/
 
using System;

/// <summary>
/// Custom Mathf implementation with double params
/// </summary>
public static class Mathd
{
    public const double PI               = 3.14159274;
    public const double Infinity         = double.PositiveInfinity;
    public const double NegativeInfinity = double.NegativeInfinity;
    public const double Deg2Rad          = 0.0174532924;
    public const double Rad2Deg          = 57.29578;

    public static readonly double Epsilon = 1.401298E-45;

    public static double Sin(double f)
    {
        return Math.Sin(f);
    }

    public static double Cos(double f)
    {
        return Math.Cos(f);
    }

    public static double Tan(double f)
    {
        return Math.Tan(f);
    }

    public static double Asin(double f)
    {
        return Math.Asin(f);
    }

    public static double Acos(double f)
    {
        return Math.Acos(f);
    }

    public static double Atan(double f)
    {
        return Math.Atan(f);
    }

    public static double Atan2(double y, double x)
    {
        return Math.Atan2(y, x);
    }

    public static double Sqrt(double f)
    {
        return Math.Sqrt(f);
    }

    public static double Abs(double f)
    {
        return Math.Abs(f);
    }

    public static int Abs(int value)
    {
        return Math.Abs(value);
    }

    public static double Min(double a, double b)
    {
        return a >= b ? b : a;
    }

    public static double AbsMin(double a, double b)
    {
        return Abs(a) >= Abs(b) ? b : a;
    }

    public static double Min(params double[] values)
    {
        var len = values.Length;

        if (len == 0) return 0;

        var min = values[0];
        for (var i = 1; i < len; ++i)
        {
            if (values[i] < min)
                min = values[i];
        }
        return min;
    }

    public static int Min(int a, int b)
    {
        return a >= b ? b : a;
    }

    public static int Min(params int[] values)
    {
        var len = values.Length;

        if (len == 0) return 0;

        var min = values[0];
        for (var i = 1; i < len; ++i)
        {
            if (values[i] < min)
                min = values[i];
        }
        return min;
    }

    public static double Max(double a, double b)
    {
        return a <= b ? b : a;
    }

    public static double MaxForAbs(double a, double b)
    {
        return Abs(a) > Abs(b) ? a : b;
    }

    public static double Max(params double[] values)
    {
        var len = values.Length;

        if (len == 0) return 0;

        var max = values[0];
        for (var i = 1; i < len; ++i)
        {
            if (values[i] > max)
                max = values[i];
        }
        return max;
    }

    public static int Max(int a, int b)
    {
        return (a <= b) ? b : a;
    }

    public static int Max(params int[] values)
    {
        var len = values.Length;

        if (len == 0) return 0;

        var max = values[0];
        for (var i = 1; i < len; ++i)
        {
            if (values[i] > max)
                max = values[i];
        }
        return max;
    }

    public static double Pow(double f, double p)
    {
        return Math.Pow(f, p);
    }

    public static double Exp(double power)
    {
        return Math.Exp(power);
    }

    public static double Log(double f, double p)
    {
        return Math.Log(f, p);
    }

    public static double Log(double f)
    {
        return Math.Log(f);
    }

    public static double Log10(double f)
    {
        return Math.Log10(f);
    }

    public static double Ceil(double f)
    {
        return Math.Ceiling(f);
    }

    public static double Floor(double f)
    {
        return Math.Floor(f);
    }

    public static double Round(double f)
    {
        return Math.Round(f);
    }

    public static int CeilToInt(double f)
    {
        return (int)Math.Ceiling(f);
    }

    public static int FloorToInt(double f)
    {
        return (int)Math.Floor(f);
    }

    public static int RoundToInt(double f)
    {
        return (int)Math.Round(f);
    }

    public static double Sign(double f)
    {
        return f < 0 ? -1 : 1;
    }

    public static double Clamp(double value, double min, double max)
    {
        if (min > max) min = max;

        if (value < min) return min;
        if (value > max) return max;

        return value;
    }

    public static int Clamp(int value, int min, int max)
    {
        if (min > max) min = max;

        if (value < min) return min;
        if (value > max) return max;

        return value;
    }

    public static double Clamp01(double value)
    {
        if (value < 0) return 0;
        if (value > 1) return 1;

        return value;
    }

    public static double Lerp(double a, double b, double t)
    {
        return a + (b - a) * Clamp01(t);
    }

    public static double LerpUnclamped(double a, double b, double t)
    {
        return a + (b - a) * t;
    }

    public static double LerpAngle(double a, double b, double t)
    {
        var num = Repeat(b - a, 360);
        if (num > 180) num -= 360;

        return a + num * Clamp01(t);
    }

    public static double MoveTowards(double current, double target, double maxDelta)
    {
        if (Abs(target - current) <= maxDelta) return target;

        return current + Sign(target - current) * maxDelta;
    }

    public static double MoveTowardsAngle(double current, double target, double maxDelta)
    {
        var num = DeltaAngle(current, target);

        if (-maxDelta < num && num < maxDelta) return target;

        target = current + num;
        return MoveTowards(current, target, maxDelta);
    }

    public static double SmoothStep(double from, double to, double t)
    {
        t = Clamp01(t);
        t = -2 * t * t * t + 3 * t * t;
        return to * t + from * (1 - t);
    }

    public static bool Approximately(double a, double b)
    {
        return Abs(b - a) < Max(1E-06 * Max(Abs(a), Abs(b)), Epsilon * 8);
    }

    public static double Repeat(double t, double length)
    {
        return t - Floor(t / length) * length;
    }

    public static double PingPong(double t, double length)
    {
        t = Repeat(t, length * 2);
        return length - Abs(t - length);
    }

    public static double InverseLerp(double a, double b, double value)
    {
        return a != b ? Clamp01((value - a) / (b - a)) : 0;
    }

    public static double DeltaAngle(double current, double target)
    {
        var num = Repeat(target - current, 360);
        if (num > 180) num -= 360;
        return num;
    }

    public static double ClampAngle(double angle)
    {
        var num = Repeat(angle, 360);
        if (num > 180) num -= 360;
        return num;
    }

    public static double AngToRad(double deg)
    {
        return ClampAngle(deg) * Deg2Rad;
    }

    public static double RegToAng(double rad)
    {
        return rad * Rad2Deg;
    }
}