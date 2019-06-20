// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-12-24      14:05
//  *LastModify：2018-12-24      14:21
//  ***************************************************************************************************/

#region

using System.Collections.Generic;

#endregion

namespace NoeExpression
{

    public class ExpressionOperator
    {
        // 操作符的优先级
        public static Dictionary<string, int> TableOperator = new Dictionary<string, int>
        {
            { "<<",6 }, { ">>",6 },
            { "|" ,5 }, { "&" ,5 },        
            { "*" ,4 }, { "/" ,4 },{ "%",4 },
            { "+" ,3 }, { "-" ,3 },        
            { ">" ,2 }, { ">=",2 },{ "<",2 },{"<=",2},{"!=",2},{"==",2},
            { "||",1 }, { "&&",1 },{ "!",1 }
        };

        // 操作符映射
        public static Dictionary<string, string> TableMapping = new Dictionary<string, string>
        {
		    { "dy", ">" }, { "dydy", ">=" },
		    { "xy", "<" }, { "xydy", "<=" },
		    { "and", "&&" }, { "or", "||" },
            { "dd", "=="}
	    };

        public static int GetOperatorLevel(string strOperator)
        {
            var nResult = -1;
            if (TableOperator.TryGetValue(strOperator, out nResult))
                return nResult;

            return -1;
        }
    }

}