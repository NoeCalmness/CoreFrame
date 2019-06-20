// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-12-24      14:05
//  *LastModify：2018-12-24      14:24
//  ***************************************************************************************************/

#region

using System;
using System.Collections.Generic;
using System.Text;
#if UNITY_EDITOR
using System.Xml;
#endif

#endregion

namespace NoeExpression
{
    public class Expression
    {
        public static Expression Create(string strType)
        {
            switch (strType)
            {
                case "operator":
                    return new ExpressionMatch();
                case "function":
                    return new ExpressionFunction(null);
                default:
                    throw new Exception(string.Format("BehaviourTreeExpression::Create( {0} )", strType));
            }
        }

        public static Expression Resolve(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return null;
            var lexer = new ExpressionLexer(expression);
            var resolve = new ExpressionResolve(lexer.GetToken());
            return resolve.ResolveMatchBegin();
        }

        internal static Expression Resolve(ExpressionToken[] rToken)
        {
            return new ExpressionResolve(rToken).ResolveMatchBegin();
        }


        public virtual bool IsTrue(DataHandler rData)
        {
            return GetValue(rData) != NValue.Zero;
        }

        public virtual NValue GetValue(DataHandler rHandle)
        {
#if UNITY_EDITOR
		Logger.LogError("BehaviourTreeExpression, can't find the left value!");
#endif
            return null;
        }

        public virtual void SetValue(DataHandler rData, string strValue)
        {
        }

        public virtual Expression Clone()
        {
            return null;
        }

#if UNITY_EDITOR
    public virtual bool FromXml(XmlElement rElement)
    {
        return true;
    }
#endif

#if UNITY_EDITOR
    public virtual bool ToXml(XmlDocument rDocument, XmlElement rParent)
    {
        return true;
    }
#endif
    }

    internal enum BehaviourTreeOperator
    {
        /* nop */
        NOP,
        /* >  */
        GreaterThan,
        /* >= */
        GreaterEqual,
        /* <  */
        LessThan,
        /* <= */
        LessEqual,
        /* == */
        EqualEuqal,
        /* != */
        Unequal,
        /* /  */
        Division,
        /* %  */
        Percent,
        /* *  */
        Multiply,
        /* +  */
        Addition,
        /* -  */
        Subtraction,
        /* =  */
        Equal,
        /* && */
        And,
        /* || */
        Or,
        /* ! */
        Revert,
    }

    public class ExpressionMatch : Expression
    {
        public Expression Left;

        private BehaviourTreeOperator operatorValue = BehaviourTreeOperator.NOP;
        public Expression Right;

        public string Operator
        {
            set
            {
                switch (value)
                {
                    case ">":
                        operatorValue = BehaviourTreeOperator.GreaterThan;
                        break;
                    case ">=":
                        operatorValue = BehaviourTreeOperator.GreaterEqual;
                        break;
                    case "<":
                        operatorValue = BehaviourTreeOperator.LessThan;
                        break;
                    case "<=":
                        operatorValue = BehaviourTreeOperator.LessEqual;
                        break;
                    case "==":
                        operatorValue = BehaviourTreeOperator.EqualEuqal;
                        break;
                    case "!=":
                        operatorValue = BehaviourTreeOperator.Unequal;
                        break;
                    case "/":
                        operatorValue = BehaviourTreeOperator.Division;
                        break;
                    case "%":
                        operatorValue = BehaviourTreeOperator.Percent;
                        break;
                    case "*":
                        operatorValue = BehaviourTreeOperator.Multiply;
                        break;
                    case "+":
                        operatorValue = BehaviourTreeOperator.Addition;
                        break;
                    case "-":
                        operatorValue = BehaviourTreeOperator.Subtraction;
                        break;
                    case "=":
                        operatorValue = BehaviourTreeOperator.Equal;
                        break;
                    case "and":
                    case "&&":
                        operatorValue = BehaviourTreeOperator.And;
                        break;
                    case "||":
                        operatorValue = BehaviourTreeOperator.Or;
                        break; // Not impl
                    case "!":
                        operatorValue = BehaviourTreeOperator.Revert;
                        break;
                    default:
                        throw new Exception("表达式解析失败，未处理的操作符: " + value);
                }
            }
            get
            {
                switch (operatorValue)
                {
                    case BehaviourTreeOperator.GreaterThan:
                        return ">";
                    case BehaviourTreeOperator.GreaterEqual:
                        return ">=";
                    case BehaviourTreeOperator.LessThan:
                        return "<";
                    case BehaviourTreeOperator.LessEqual:
                        return "<=";
                    case BehaviourTreeOperator.EqualEuqal:
                        return "==";
                    case BehaviourTreeOperator.Unequal:
                        return "!=";
                    case BehaviourTreeOperator.Division:
                        return "/";
                    case BehaviourTreeOperator.Percent:
                        return "%";
                    case BehaviourTreeOperator.Multiply:
                        return "*";
                    case BehaviourTreeOperator.Addition:
                        return "+";
                    case BehaviourTreeOperator.Subtraction:
                        return "-";
                    case BehaviourTreeOperator.Equal:
                        return "=";
                    case BehaviourTreeOperator.And:
                        return "&&";
                    case BehaviourTreeOperator.Or:
                        return "||"; // Not impl
                    case BehaviourTreeOperator.Revert:
                        return "!";
                }
                return "";
            }
        }

        public override NValue GetValue(DataHandler rHandle)
        {
            switch (operatorValue)
            {
                case BehaviourTreeOperator.GreaterThan:
                {
                    return Left.GetValue(rHandle) > Right.GetValue(rHandle) ? NValue.True : NValue.False;
                }
                case BehaviourTreeOperator.GreaterEqual:
                {
                    return Left.GetValue(rHandle) >= Right.GetValue(rHandle) ? NValue.True : NValue.False;
                }
                case BehaviourTreeOperator.LessThan:
                {
                    return Left.GetValue(rHandle) < Right.GetValue(rHandle) ? NValue.True : NValue.False;
                }
                case BehaviourTreeOperator.LessEqual:
                {
                    return Left.GetValue(rHandle) <= Right.GetValue(rHandle) ? NValue.True : NValue.False;
                }

                case BehaviourTreeOperator.EqualEuqal:
                {
                    return Left.GetValue(rHandle) == Right.GetValue(rHandle) ? NValue.True : NValue.False;
                }
                case BehaviourTreeOperator.Unequal:
                {
                    return Left.GetValue(rHandle) != Right.GetValue(rHandle) ? NValue.True : NValue.False;
                }
                case BehaviourTreeOperator.Division:
                {
                    return Left.GetValue(rHandle)/Right.GetValue(rHandle);
                }
                case BehaviourTreeOperator.Percent:
                {
                    return Left.GetValue(rHandle)%Right.GetValue(rHandle);
                }
                case BehaviourTreeOperator.Multiply:
                {
                    return Left.GetValue(rHandle)*Right.GetValue(rHandle);
                }
                case BehaviourTreeOperator.Addition:
                {
                    return Left.GetValue(rHandle) + Right.GetValue(rHandle);
                }
                case BehaviourTreeOperator.Subtraction:
                {
                    if (Left == null)
                        return 0 - Right.GetValue(rHandle);
                    return Left.GetValue(rHandle) - Right.GetValue(rHandle);
                }
                case BehaviourTreeOperator.Equal:
                {
                    var rRightValue = Right.GetValue(rHandle);

                    Left.GetValue(rHandle).Value = rRightValue.Value;

                    return NValue.True;
                }
                case BehaviourTreeOperator.And:
                {
                    return Left.IsTrue(rHandle) && Right.IsTrue(rHandle) ? NValue.True : NValue.False;
                }
                case BehaviourTreeOperator.Or:
                {
                    return Left.IsTrue(rHandle) || Right.IsTrue(rHandle) ? NValue.True : NValue.False;
                }
                case BehaviourTreeOperator.Revert:
                    return Right.IsTrue(rHandle) ? NValue.False : NValue.True;
            }
            // 没有处理的直接返回0
            return NValue.Zero;
        }


        public override Expression Clone()
        {
            var expression = new ExpressionMatch();
            expression.Left = Left.Clone();
            expression.Right = Right.Clone();
            expression.Operator = Operator;
            return expression;
        }

        public override string ToString()
        {
            if(Left != null)
                return $"__ {Operator} __";
            return $"{Operator} __";
        }

#if UNITY_EDITOR
        public override bool FromXml(XmlElement rElement)
    {
        return true;
    }
#endif

#if UNITY_EDITOR
    public override bool ToXml(XmlDocument rDocument, XmlElement rParent)
    {
        var rElement = rDocument.CreateElement("expression");
        rElement.Attributes.Append(rDocument.CreateAttribute("type")).Value = "operator";

        var rElementLeft = rDocument.CreateElement("left");

        if (false == Left.ToXml(rDocument, rElementLeft))
        {
            return false;
        }

        var rElementRight = rDocument.CreateElement("right");

        if (false == Right.ToXml(rDocument, rElementRight))
        {
            return false;
        }

        rElement.AppendChild(rElementLeft);
        rElement.AppendChild(rElementRight);
        rParent.AppendChild(rElement);

        return base.ToXml(rDocument,rParent);
    }
#endif
    }

    // 变量表达式
    public class ExpressionVariable : Expression
    {
        public string Variable;

        public ExpressionVariable(string strVariable)
        {
            Variable = strVariable;
        }

        public override NValue GetValue(DataHandler rHandle)
        {
            return rHandle.GetVariable(Variable);
        }

        public override void SetValue(DataHandler rData, string strValue)
        {
            var rValue = rData.GetVariable(Variable);

            if (null != rValue)
            {
                rValue.Value = strValue;
            }
        }

        public override Expression Clone()
        {
            return new ExpressionVariable(Variable);
        }

        public override string ToString()
        {
            return Variable;
        }

#if UNITY_EDITOR
        public override bool ToXml(XmlDocument rDocument, XmlElement rParent)
    {
        var rElement = rDocument.CreateElement("expression");
        rElement.Attributes.Append(rDocument.CreateAttribute("type")).Value = "variable";
        rElement.Attributes.Append(rDocument.CreateAttribute("function")).Value = Variable;

        rParent.AppendChild(rElement);

        return base.ToXml(rDocument, rParent);
    }
#endif
    }

    // 数值表达式
    public class ExpressionValue : Expression
    {
        public ExpressionValue(NValue strValue)
        {
            Value = strValue;
        }

        public NValue Value { get; set; }

        public override NValue GetValue(DataHandler rHandle)
        {
            return Value;
        }

        public override Expression Clone()
        {
            return new ExpressionValue(Value.Clone());
        }

        public override string ToString()
        {
            return Value.GetStringValue();
        }

#if UNITY_EDITOR
        public override bool ToXml(XmlDocument rDocument, XmlElement rParent)
    {
        var rElement = rDocument.CreateElement("expression");
        rElement.Attributes.Append(rDocument.CreateAttribute("type")).Value = "value";
        rElement.Attributes.Append(rDocument.CreateAttribute("function")).Value = Value.Value.ToString();

        rParent.AppendChild(rElement);

        return base.ToXml(rDocument, rParent);
    }
#endif
    }

    // 函数
    public class ExpressionFunction : Expression
    {
        protected NValue[] ParameterValue = new NValue[0];

        public ExpressionFunction(string strFunction)
        {
            Function = strFunction;
            Parameter = new List<Expression>();
        }

        public string Function { get; set; }
        public List<Expression> Parameter { get; set; }

        public override NValue GetValue(DataHandler rHandle)
        {
            if (rHandle == null)
            {
                throw new Exception(string.Format("无法调用函数{0}", Function));
            }

            if (ParameterValue.Length != Parameter.Count)
                ParameterValue = new NValue[Parameter.Count];

            var nCount = Parameter.Count;
            for (var nIndex = 0; nIndex < nCount; ++ nIndex)
                ParameterValue[nIndex] = Parameter[nIndex].GetValue(rHandle);

            // foreach (BehaviourTreeExpression rExpression in Parameter)
            // {
            //     ParameterValue.Add(rExpression.GetValue( rBehaviourTree));
            // }

            return rHandle.Method(Function, ParameterValue);
        }

        public override Expression Clone()
        {
            var expression = new ExpressionFunction(Function);
            var nCount = Parameter.Count;
            for (var nIndex = 0; nIndex < nCount; ++ nIndex)
            {
                expression.Parameter.Add(Parameter[nIndex].Clone());
            }
            // foreach (BehaviourTreeExpression rExpression in Parameter)
            // {
            //     rBehaviourTreeExpression.Parameter.Add(rExpression.Clone());
            // }
            return expression;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Function).Append("(");
            var nCount = Parameter.Count;
            for (var nIndex = 0; nIndex < nCount; ++nIndex)
            {
                sb.Append(Parameter[nIndex].ToString()).Append(",");
            }
            sb.Append(")");
            sb.Replace(",)", ")");
            return sb.ToString();
        }

#if UNITY_EDITOR
        public override bool FromXml(XmlElement rElement)
    {
        return true;
    }
#endif
#if UNITY_EDITOR
    public override bool ToXml(XmlDocument rDocument, XmlElement rParent)
    {
        var rElement = rDocument.CreateElement("expression");
        rElement.Attributes.Append(rDocument.CreateAttribute("type")).Value = "function";
        rElement.Attributes.Append(rDocument.CreateAttribute("function")).Value = Function;

        foreach( var rExpression in Parameter )
        {
            var rElementParameter = rDocument.CreateElement( "parameter" );

            if( false == rExpression.ToXml( rDocument, rElementParameter ) )
            {
                return false;
            }

            rElement.AppendChild(rElementParameter);
        }

        rParent.AppendChild(rElement);

        return base.ToXml(rDocument,rParent);
    }
#endif
    }
}