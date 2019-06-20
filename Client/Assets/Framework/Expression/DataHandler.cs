// 表达式数据处理接口

using System.Collections.Generic;

namespace NoeExpression
{
    public class BehaviourTreeAssists
    {
        public static object ToValue(string strValue)
        {
            double nSingleValue = 0;
            var nBooleanValue = false;
            if (double.TryParse(strValue, out nSingleValue))
                return nSingleValue;
            if (bool.TryParse(strValue, out nBooleanValue))
                return nBooleanValue;
            return strValue;
        }
    }

    public class DataHandler
    {
        public delegate NValue OnHandlerMethod(NValue[] rArrayValue);

        public delegate NValue OnHandlerVariable(string strVariable);

        protected Dictionary<string, OnHandlerMethod> TableMethod;
        public Dictionary<string, NValue> TableVariable;


        public DataHandler()
        {
            TableMethod = new Dictionary<string, OnHandlerMethod>();
            TableVariable = new Dictionary<string, NValue>();

            CommonMath.AttachCommonMath(this);
        }

        public OnHandlerVariable HandlerVariable { get; set; }

        // 获取变量
        public NValue GetVariable(string strVariable)
        {
            NValue rResult = null;
            if (!TableVariable.TryGetValue(strVariable, out rResult))
                return NValue.Zero;

            return rResult;
        }

        // 设置变量
        public void SetVariable(string strVariable, NValue rValue)
        {
            if (false == TableVariable.ContainsKey(strVariable))
            {
                TableVariable.Add(strVariable, rValue);
            }
            else
            {
                TableVariable[strVariable] = rValue;
            }
        }

        public void RemoveVariable(string strVariable)
        {
            TableVariable.Remove(strVariable);
        }

        public void AttachMethod(string strMethod, OnHandlerMethod rMethod)
        {
            if (false == TableMethod.ContainsKey(strMethod))
            {
                TableMethod.Add(strMethod, rMethod);
            }
        }

        public void DetachMethod(string strMethod, OnHandlerMethod rMethod)
        {
            if (false == TableMethod.ContainsKey(strMethod))
            {
                TableMethod.Remove(strMethod);
            }
        }

        public void ClearMethod()
        {
            TableMethod.Clear();
        }

        public void ClearVariable()
        {
            TableVariable.Clear();
        }

        public void Clear()
        {
            TableMethod.Clear();
            TableVariable.Clear();
        }

        // 调用函数
        public virtual NValue Method(string strMethod, NValue[] rArrayValue)
        {
            OnHandlerMethod rMethod = null;
            if (TableMethod.TryGetValue(strMethod, out rMethod))
                return rMethod(rArrayValue);

            return NValue.True;
        }
    }
}