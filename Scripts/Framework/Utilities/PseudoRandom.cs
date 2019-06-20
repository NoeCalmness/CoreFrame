/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * A pseudo random generator
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-09-18
 * 
 ***************************************************************************************************/

using System;

/// <summary>
/// 伪随机数生成器
/// 每一个随机数生成器都使用其独立的随机种子生成随机序列，相互之间不受影响
/// 尽管在生成器创建后仍然可以更改其中子，但这样会使当前序列中断，重新开始新序列
/// 算法：
///     N(j + 1) = (A * N(j) + B) % M
///
///     其中, A, B, M 是常数, 并且应当满足以下条件:
///         1. B, M 互质
///         2. M 的所有质子的积能整除 A - 1
///         3. 若 M 是 4 的倍数, 则 A - 1 也是
///         4. A, B, N0 都比 M 小, 即种子不能超过回归周期 (此算法的随机回归周期最大为 M)
///         5. A, B 都是正整数
/// </summary>
public class PseudoRandom
{
    private UInt64 m_seed   = 0;
    private UInt64 m_nSeed  = 0;
    private UInt32 m_count  = 0;

    public UInt64 seed { get { return m_seed; } set { m_seed = value; m_nSeed = m_seed; m_count = 0; } }
    public UInt64 nSeed { get { return m_nSeed; } }
    public UInt32 count { get { return m_count; } }

    /// <summary>
    /// 使用一个随机种子创建一个随机数发生器
    /// </summary>
    public PseudoRandom()
    {
        seed = (UInt64)UnityEngine.Random.Range(0, int.MaxValue); // 使用随机种子
    }

    /// <summary>
    /// 使用指定种子创建一个随机数发生器
    /// </summary>
    /// <param name="_seed"></param>
    public PseudoRandom(UInt64 _seed)
    {
        seed = _seed;
    }

    /// <summary>
    /// 获取下一个随机数
    /// </summary>
    /// <param name="max">最大值</param>
    /// <returns>范围从 0 - max - 1 的随机数</returns>
    public UInt32 Next(UInt32 max)
    {
        return (UInt32)(max * NextRange());
    }

    /// <summary>
    /// 获取一个介于 min 和 max 之间的浮点数
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public float Range(float min, float max)
    {
        return (float)(min + (max - min) * NextRange());
    }

    /// <summary>
    /// 获取一个介于 min 和 max 之间的双精度浮点数
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public double Range(double min, double max)
    {
        return min + (max - min) * NextRange();
    }

    /// <summary>
    /// 获取一个介于 min 和 max 之间的整数
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public int Range(int min, int max)
    {
        return (int)(min + (max - min) * NextRange());
    }

    /// <summary>
    /// 下一个随机范围  介于 0 - 1 的一个随机浮点数
    /// </summary>
    /// <returns></returns>
    private double NextRange()
    {
        ++m_count;

        UInt64 x = 0xfffffffffffffful; x += 1;
        m_nSeed *= 134775813;
        m_nSeed += 1;
        m_nSeed = m_nSeed % x;
        var result = m_nSeed / (double)0xffffffffffffff;

        FightRecordManager.RecordLog<LogDouble>(log =>
        {
            log.tag = (byte)TagType.Random;
            log.value = result;
        });

        return result;
    }
}
