/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * BigEndian version of System.BitConverter
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-28
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/// <summary>
/// The ByteConverter class contains methods for converting an array of bytes to one of the base data 
/// types, as well as for converting a base data type to an array of bytes.
///
/// Please note this class use BigEndian!!
/// </summary>
public static class ByteConverter
{
    #region Int32|Single Int64|Double Union struct helper

    [StructLayout(LayoutKind.Explicit)]
    struct Int32SingleUnion
    {
        [FieldOffset(0)]
        private int m_int;
        [FieldOffset(0)]
        private float m_single;

        internal Int32SingleUnion(int _int)
        {
            m_single = 0;
            m_int    = _int;
        }

        internal Int32SingleUnion(float _float)
        {
            m_int    = 0;
            m_single = _float;
        }

        internal int intValue { get { return m_int; } }
        internal float singleValue { get { return m_single; } }
    }

    [StructLayout(LayoutKind.Explicit)]
    struct Int64DoubleUnion
    {
        [FieldOffset(0)]
        private long   m_long;
        [FieldOffset(0)]
        private double m_double;

        internal Int64DoubleUnion(long _long)
        {
            m_double = 0;
            m_long   = _long;
        }

        internal Int64DoubleUnion(double _double)
        {
            m_long    = 0;
            m_double  = _double;
        }

        internal long longValue { get { return m_long; } }
        internal double doubleValue { get { return m_double; } }
    }

    #endregion

    #region Double/Single primitive conversions

    public static long DoubleToInt64Bits(double value)
    {
        return new Int64DoubleUnion(value).longValue;
    }

    public static double Int64BitsToDouble(long value)
    {
        return new Int64DoubleUnion(value).doubleValue;
    }

    public static int SingleToInt32Bits(float value)
    {
        return new Int32SingleUnion(value).intValue;
    }

    public static float Int32BitsToSingle(int value)
    {
        return new Int32SingleUnion(value).singleValue;
    }

    #endregion

    #region Convert to byte[] operation

    public static byte[] GetBytes(float value)
    {
        return GetBytes(SingleToInt32Bits(value));
    }

    public static byte[] GetBytes(ulong value)
    {
        return GetBytes(unchecked((long)value));
    }

    public static byte[] GetBytes(uint value)
    {
        return GetBytes(unchecked((int)value));
    }

    public static byte[] GetBytes(ushort value)
    {
        return GetBytes(unchecked((short)value));
    }

    public static byte[] GetBytes(long value)
    {
        var v = new byte[8];
        for (var i = v.Length - 1; i > -1; --i)
        {
            v[i] = (byte)(value & 0xFF);
            value = value >> 8;
        }
        return v;
    }

    public static byte[] GetBytes(double value)
    {
        return GetBytes(DoubleToInt64Bits(value));
    }

    public static byte[] GetBytes(short value)
    {
        return new byte[] { (byte)(value >> 8), (byte)(value & 0xFF) };
    }

    public static byte[] GetBytes(char value)
    {
        return GetBytes((short)value);
    }

    public static byte[] GetBytes(bool value)
    {
        return new byte[] { value ? (byte)1 : (byte)0 };
    }

    public static byte[] GetBytes(int value)
    {
        var v = new byte[4];
        for (var i = v.Length - 1; i > -1; --i)
        {
            v[i] = (byte)(value & 0xFF);
            value >>= 8;
        }
        return v;
    }

    #endregion

    #region Convert from byte[] operation

    public static char ToChar(byte[] value, int startIndex)
    {
        return (char)ToInt16(value, startIndex);
    }

    public static bool ToBoolean(byte[] value, int startIndex)
    {
        return value[startIndex] != 0;
    }

    public static short ToInt16(byte[] value, int startIndex)
    {
        return (short)(value[startIndex] << 8 | value[startIndex + 1]);
    }

    public static ushort ToUInt16(byte[] value, int startIndex)
    {
        return (ushort)(value[startIndex] << 8 | value[startIndex + 1]);
    }

    public static int ToInt32(byte[] value, int startIndex)
    {
        int v = 0;
        for (var i = 0; i < 4; ++i) v = unchecked((v << 8) | value[startIndex + i]);
        return v;
    }

    public static uint ToUInt32(byte[] value, int startIndex)
    {
        uint v = 0;
        for (var i = 0; i < 4; ++i) v = unchecked((v << 8) | value[startIndex + i]);
        return v;
    }

    public static long ToInt64(byte[] value, int startIndex)
    {
        long l = 0;
        for (var i = 0; i < 8; ++i) l = unchecked((l << 8) | value[startIndex + i]);
        return l;
    }

    public static ulong ToUInt64(byte[] value, int startIndex)
    {
        ulong v = 0;
        for (var i = 0; i < 8; ++i) v = unchecked((v << 8) | value[startIndex + i]);
        return v;
    }

    public static float ToSingle(byte[] value, int startIndex)
    {
        return Int32BitsToSingle(ToInt32(value, startIndex));
    }

    public static double ToDouble(byte[] value, int startIndex)
    {
        return Int64BitsToDouble(ToInt64(value, startIndex));
    }

    public static string ToString(byte[] value)
    {
        return BitConverter.ToString(value);
    }

    public static string ToString(byte[] value, int startIndex)
    {
        return BitConverter.ToString(value, startIndex);
    }

    public static string ToString(byte[] value, int startIndex, int length)
    {
        return BitConverter.ToString(value, startIndex, length);
    }

    #endregion

    #region Convert from List<byte> operation

    public static char ToChar(List<byte> value, int startIndex)
    {
        return (char)ToInt16(value, startIndex);
    }

    public static bool ToBoolean(List<byte> value, int startIndex)
    {
        return value[startIndex] != 0;
    }

    public static short ToInt16(List<byte> value, int startIndex)
    {
        return (short)(value[startIndex] << 8 | value[startIndex + 1]);
    }

    public static ushort ToUInt16(List<byte> value, int startIndex)
    {
        return (ushort)(value[startIndex] << 8 | value[startIndex + 1]);
    }

    public static int ToInt32(List<byte> value, int startIndex)
    {
        int v = 0;
        for (var i = 0; i < 4; ++i) v = unchecked((v << 8) | value[startIndex + i]);
        return v;
    }

    public static uint ToUInt32(List<byte> value, int startIndex)
    {
        uint v = 0;
        for (var i = 0; i < 4; ++i) v = unchecked((v << 8) | value[startIndex + i]);
        return v;
    }

    public static long ToInt64(List<byte> value, int startIndex)
    {
        long l = 0;
        for (var i = 0; i < 8; ++i) l = unchecked((l << 8) | value[startIndex + i]);
        return l;
    }

    public static ulong ToUInt64(List<byte> value, int startIndex)
    {
        ulong v = 0;
        for (var i = 0; i < 8; ++i) v = unchecked((v << 8) | value[startIndex + i]);
        return v;
    }

    public static float ToSingle(List<byte> value, int startIndex)
    {
        return Int32BitsToSingle(ToInt32(value, startIndex));
    }

    public static double ToDouble(List<byte> value, int startIndex)
    {
        return Int64BitsToDouble(ToInt64(value, startIndex));
    }

    #endregion

    #region Write to List<byte> operation

    public static void WriteBytes(List<byte> list, float value)
    {
        WriteBytes(list, SingleToInt32Bits(value));
    }

    public static void WriteBytes(List<byte> list, ulong value)
    {
        WriteBytes(list, unchecked((long)value));
    }

    public static void WriteBytes(List<byte> list, uint value)
    {
        WriteBytes(list, unchecked((int)value));
    }

    public static void WriteBytes(List<byte> list, ushort value)
    {
        WriteBytes(list, unchecked((short)value));
    }

    public static void WriteBytes(List<byte> list, long value)
    {
        for (var i = 7; i > -1; --i)
        {
            list.Add((byte)(value & 0xFF));
            value = value >> 8;
        }
    }

    public static void WriteBytes(List<byte> list, double value)
    {
        WriteBytes(list, DoubleToInt64Bits(value));
    }

    public static void WriteBytes(List<byte> list, short value)
    {
        list.Add((byte)(value >> 8));
        list.Add((byte)(value & 0xFF));
    }

    public static void WriteBytes(List<byte> list, char value)
    {
       WriteBytes(list, (short)value);
    }

    public static void WriteBytes(List<byte> list, bool value)
    {
        list.Add(value ? (byte)1 : (byte)0);
    }

    public static void WriteBytes(List<byte> list, int value)
    {
        for (var i = 3; i > -1; --i)
        {
            list.Add((byte)(value >> (i * 8) & 0xFF));
        }
    }

    #endregion
}
