// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-04-26      12:00
//  *LastModify：2019-04-26      12:00
//  ***************************************************************************************************/

using System;
using System.Security.Cryptography;

public struct _Double
{
    private ulong m_encryptedValue;
    private string m_md5Code;
    private bool m_valid;

    private ulong encryptedValue
    {
        get
        {
            if (Module_GameGuard.enableMd5Check && !string.IsNullOrEmpty(m_md5Code))
            {
                var md5Code = Module_GameGuard.GetMD5Code(BitConverter.GetBytes(m_encryptedValue));
                if (string.CompareOrdinal(md5Code, m_md5Code) != 0)
                {
                    Module_GameGuard.InValid();
                }
            }
            return m_encryptedValue;
        }
        set
        {
            m_encryptedValue = value;
            m_md5Code = Module_GameGuard.GetMD5Code(BitConverter.GetBytes(m_encryptedValue));
        }
    }

    public static implicit operator double(_Double rValue)
    {
        return rValue.m_valid ? Decrypt(rValue.encryptedValue) : BitConverter.ToDouble(BitConverter.GetBytes(rValue.encryptedValue), 0);
    }

    public static implicit operator _Double(double rValue)
    {
        bool valid;
        var v = Eecrypt(rValue, out valid);
        return new _Double {encryptedValue = v, m_valid = valid};
    }

    public static double operator +(_Double a, double rValue)
    {
        return (double)a + rValue;
    }

    public static double operator +(_Double a, int rValue)
    {
        return (double)a + rValue;
    }

    public static double operator +(_Double a, _Int b)
    {
        return (double)a + b;
    }

    public static double operator +(_Double a, _Double b)
    {
        return (double)a + Decrypt(b.encryptedValue);
    }

    public static double operator -(_Double a, double rValue)
    {
        return (double)a - rValue;
    }

    public static double operator -(_Double a, int rValue)
    {
        return (double)a - rValue;
    }

    public static double operator -(_Double a, _Double b)
    {
        return (double)a - Decrypt(b.encryptedValue);
    }

    public static double operator -(_Double a, _Int b)
    {
        return (double)a - b;
    }

    public static double operator *(_Double a, double rValue)
    {
        return (double)a * rValue;
    }
    public static double operator *(_Double a, int b)
    {
        return (double)a * b;
    }

    public static double operator *(_Double a, _Int rValue)
    {
        return (double)a * rValue;
    }
    public static double operator *(_Double a, _Double b)
    {
        return (double)a * (double)b;
    }

    public static double operator /(_Double a, double rValue)
    {
        return (double)a/rValue;
    }


    public static double operator /(_Double a, int b)
    {
        return (double)a/b;
    }

    public static double operator /(_Double a, _Int rValue)
    {
        return (double)a / rValue;
    }

    public static double operator /(_Double a, _Double b)
    {
        return (double)a / (double)b;
    }

    public override string ToString()
    {
        return ((double)this).ToString();
    }

    public static ulong Eecrypt(double rValue, out bool rSuccess)
    {
        rSuccess = false;
        if (!Module_GameGuard.enableMemoryEncrypt)
            return BitConverter.ToUInt64(BitConverter.GetBytes(rValue), 0);
        rSuccess = true;
        return BitConverter.ToUInt64(BitConverter.GetBytes(rValue), 0) ^ Module_GameGuard.EncryptSeed;
    }

    public static double Decrypt(ulong rValue)
    {
        return BitConverter.ToDouble(BitConverter.GetBytes(~rValue ^ ~Module_GameGuard.EncryptSeedUlong), 0);
    }
}