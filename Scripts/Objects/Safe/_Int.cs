// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-04-25      17:46
//  *LastModify：2019-04-26      15:37
//  ***************************************************************************************************/

public struct _Int
{
    private int m_encryptedValue;
    private string m_md5Code;
    private bool m_valid;

    private int encryptedValue
    {
        get
        {
            if (Module_GameGuard.enableMd5Check && !string.IsNullOrEmpty(m_md5Code))
            {
                var md5Code = Module_GameGuard.GetMD5Code(ByteConverter.GetBytes(m_encryptedValue));
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
            m_md5Code = Module_GameGuard.GetMD5Code(ByteConverter.GetBytes(m_encryptedValue));
        }
    }


    public static implicit operator int(_Int rValue)
    {
        return rValue.m_valid ? Decrypt(rValue.encryptedValue) : rValue.encryptedValue;
    }

    public static implicit operator _Int(int rValue)
    {
        bool valid;
        int a = Eecrypt(rValue, out valid);
        return new _Int {encryptedValue = a, m_valid = valid};
    }

    public static int operator+(_Int a, int rValue)
    {
        return (int)a + rValue;
    }

    public static int operator -(_Int a, int rValue)
    {
        return (int)a - rValue;
    }

    public static double operator+(_Int a, double rValue)
    {
        return (int)a + rValue;
    }

    public static double operator -(_Int a, double rValue)
    {
        return (int)a - rValue;
    }


    public static int operator +(_Int a, _Int b)
    {
        return (int)a + (int)b;
    }

    public static int operator -(_Int a, _Int b)
    {
        return (int)a - (int)b;
    }


    public static double operator +(_Int a, _Double b)
    {
        return (int)a + b;
    }

    public static double operator -(_Int a, _Double b)
    {
        return (int)a - b;
    }


    public static int operator *(_Int a, int rValue)
    {
        return (int)a * rValue;
    }

    public static int operator /(_Int a, int rValue)
    {
        return (int)a / rValue;
    }

    public static int operator *(_Int a, _Int b)
    {
        return (int)a * (int)b;
    }

    public static int operator /(_Int a, _Int b)
    {
        return (int)a / (int)b;
    }

    public static double operator *(_Int a, double b)
    {
        return (int)a * b;
    }

    public static double operator /(_Int a, double b)
    {
        return (int)a / b;
    }

    public static double operator *(_Int a, _Double b)
    {
        return (int)a * b;
    }

    public static double operator /(_Int a, _Double b)
    {
        return (int)a / b;
    }

    public static int operator %(_Int a, int rValue)
    {
        return (int)a % rValue;
    }

    public static int operator %(_Int a, _Int b)
    {
        return (int)a % (int)b;
    }

    public override string ToString()
    {
        return ((int)this).ToString();
    }

    public static int Eecrypt(int rValue, out bool rSuccess)
    {
        rSuccess = false;
        if (!Module_GameGuard.enableMemoryEncrypt)
            return rValue;
        rSuccess = true;
        return rValue ^ Module_GameGuard.EncryptSeed;
    }

    public static int Decrypt(int rValue)
    {
        return ~rValue ^ ~Module_GameGuard.EncryptSeed;
    }
}
