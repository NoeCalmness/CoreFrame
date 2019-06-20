// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-12-27      16:52
//  *LastModify：2018-12-27      16:52
//  ***************************************************************************************************/

using System;
using System.Reflection;

namespace NoeExpression
{
    public static class CommonMath
    {
        public static void AttachCommonMath(DataHandler rHandler)
        {
            rHandler.AttachMethod("Max", Max);
            rHandler.AttachMethod("Min", Min);
        }

        public static void DetachCommonMath(DataHandler rHandler)
        {
            rHandler.DetachMethod("Max", Max);
            rHandler.DetachMethod("Min", Min);
        }

        private static NValue Max(NValue[] rParams)
        {
            if (rParams == null || rParams.Length == 0)
                return NValue.Zero;

            var v = rParams[0];
            for (var i = 1; i < rParams.Length; i++)
            {
                if (rParams[i] > v)
                    v = rParams[i];
            }
            return v;
        }

        private static NValue Min(NValue[] rParams)
        {
            if (rParams == null || rParams.Length == 0)
                return NValue.Zero;

            var v = rParams[0];
            for (var i = 1; i < rParams.Length; i++)
            {
                if (rParams[i] < v)
                    v = rParams[i];
            }
            return v;
        }
    }
}