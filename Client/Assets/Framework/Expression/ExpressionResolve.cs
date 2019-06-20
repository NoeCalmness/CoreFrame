// 表达式解析器

using System;
using System.Collections.Generic;

namespace NoeExpression
{
    internal class ExpressionResolve
    {
        protected ExpressionToken[] ArrayToken;
        protected int Index;

        protected Stack<ExpressionToken> StackOperator = new Stack<ExpressionToken>();
        protected Stack<ExpressionToken> StackToken = new Stack<ExpressionToken>();

        public ExpressionResolve(ExpressionToken[] rArrayToken)
        {
            ArrayToken = rArrayToken;
            Index = 0;
        }

        public ExpressionToken Last
        {
            get
            {
                if (Index - 1 < 0 || Index - 1 >= ArrayToken.Length)
                {
                    return null;
                }

                return ArrayToken[Index - 1];
            }
        }

        public ExpressionToken Current
        {
            get
            {
                if (Index >= ArrayToken.Length)
                {
                    return null;
                }

                return ArrayToken[Index];
            }
        }

        public ExpressionToken Peek
        {
            get
            {
                if (Index + 1 >= ArrayToken.Length)
                {
                    return new ExpressionToken(ExpressionToken.eType.End, "");
                }

                return ArrayToken[Index + 1];
            }
        }

        public ExpressionToken Read()
        {
            Index++;

            if (Index >= ArrayToken.Length)
            {
                return new ExpressionToken(ExpressionToken.eType.End, "");
            }

            return ArrayToken[Index];
        }


        // 解析表达式
        private Expression Resolve()
        {
            // 直接退出
            if (null == Current)
            {
                return GenerateSubMatch();
            }

            if (ResolveIdentifier())
                return Resolve();
            
            if (ResolveMatch())
            {
                return Resolve();
            }
            if (Current.Type == ExpressionToken.eType.Operand)
            {
                StackToken.Push(Current);
                Read();

                return Resolve();
            }
            if (Current.Type == ExpressionToken.eType.String)
            {
                StackToken.Push(Current);
                Read();

                return Resolve();
            }

            return Resolve();
        }
         
        /// <summary>
        /// 解析函数和变量
        /// </summary>
        /// <returns></returns>
        private bool ResolveIdentifier()
        {
            Expression e;
            // 标示符打头解析(变量赋值及函数调用)
            if (Resolve_Identifier(out e))
            {
                var rExpressionToken = new ExpressionToken(ExpressionToken.eType.Expression, null);
                rExpressionToken.Expression = e;

                StackToken.Push(rExpressionToken);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 开始构建表达式
        /// </summary>
        /// <returns></returns>
        public Expression ResolveMatchBegin()
        {
            StackOperator.Push(new ExpressionToken(ExpressionToken.eType.End, ""));

            return Resolve();
        }
        
        /// <summary>
        /// 根据断句获得表达式
        /// </summary>
        /// <param name="rToken"></param>
        /// <returns></returns>
        private Expression GetExpression(ExpressionToken rToken)
        {
            if (rToken.Type == ExpressionToken.eType.Identifier)
            {
                return new ExpressionVariable(rToken.Value);
            }
            if (rToken.Type == ExpressionToken.eType.Operand)
            {
                return new ExpressionValue(new NValue(BehaviourTreeAssists.ToValue(rToken.Value)));
            }
            if (rToken.Type == ExpressionToken.eType.String)
            {
                var rValue = new NValue();
                rValue.SetValueString(rToken.Value);
                return new ExpressionValue(rValue);
            }
            if (rToken.Type == ExpressionToken.eType.Expression)
            {
                return rToken.Expression;
            }

            return null;
        }

        /// <summary>
        /// 生成表达式
        /// </summary>
        /// <returns></returns>
        private Expression GenerateSubMatch()
        {
            ExpressionMatch rCurrentMatch = null;

            if (StackOperator.Count == 0 || StackOperator.Peek().Type == ExpressionToken.eType.End)
            {
                if (StackToken.Count == 1)
                {
                    return GetExpression(StackToken.Pop());
                }
                throw new Exception("语法解析失败");
            }

            while (StackOperator.Count > 0)
            {
                var rToken = StackOperator.Pop();

                if (rToken.Type == ExpressionToken.eType.End || rToken.Value == "(")
                {
                    break;
                }

                var rMatch = new ExpressionMatch();
                rMatch.Operator = rToken.Value;

                if (null == rCurrentMatch)
                {
                    rMatch.Right = GetExpression(StackToken.Pop());
                }
                else
                {
                    rMatch.Right = rCurrentMatch;
                }

                rMatch.Left = GetLeft(rMatch.Operator);

                rCurrentMatch = rMatch;
            }

            return rCurrentMatch;
        }
        private Expression GetLeft(string rOperator)
        {
            if (StackToken.Count == 0)
            {
                if (rOperator == "-")
                    return null;
                throw new Exception($"操作符{rOperator} 参数不匹配");
            }

            if (StackOperator.Count == 0)
            {
                return GetExpression(StackToken.Pop());
            }
            var level = ExpressionOperator.GetOperatorLevel(rOperator);
            var lLevel = ExpressionOperator.GetOperatorLevel(StackOperator.Peek().Value);
            if (level > lLevel)
                return GetExpression(StackToken.Pop());

            var rToken = StackOperator.Pop();
            ExpressionMatch rMatch = new ExpressionMatch();
            rMatch.Operator = rToken.Value;

            rMatch.Right = GetExpression(StackToken.Pop());

            rMatch.Left = GetLeft(rMatch.Operator);
            return rMatch;
        }

        /// <summary>
        /// 开始解析匹配表达式
        /// </summary>
        /// <returns></returns>
        private bool ResolveMatch()
        {
            if (Current.Type != ExpressionToken.eType.Operator)
                return false;

            Expression expression;
            if (Resolve_Block(out expression) || Resolve_FirstOrder(out expression))
            {
                var token = new ExpressionToken(expression);
                StackToken.Push(token);
                return true;
            }

            if (StackOperator.Count == 0 || StackOperator.Peek().Type == ExpressionToken.eType.End ||
                StackOperator.Peek().Value == "(")
            {
                StackOperator.Push(Current);
                Read();
                return true;
            }

            // 放入了后括号
            if (")" == Current.Value)
            {
                throw new Exception("表达式解析失败,( )不匹配");
            }

            if (Last != null && Last.Type == ExpressionToken.eType.Operator && Last.Value != ")")
                throw new Exception($"表达式解析失败。两个操作符{Last.Value}与{Current.Value}之间没有操作数");

            StackOperator.Push(Current);
            Read();

            return true;
        }
        
        /// <summary>
        /// 解析一则表达式
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool Resolve_FirstOrder(out Expression e )
        {
            e = null;
            bool firstOrder = false;
            switch (Current.Value)
            {
                case "+":
                case "-":
                    firstOrder = Last == null || (Last.Type == ExpressionToken.eType.Operator && Last.Value != ")");
                    break;
                case "!":
                    firstOrder = true;
                    break;
            }
            if (firstOrder)
            {
                var tokens = new List<ExpressionToken>();
                Read();
                if (Current == null)
                    throw new Exception($"解析表达式异常，操作符{Last.Value}参数错误");
                tokens.Add(Current);
                var m = new ExpressionMatch()
                {
                    Operator = Last.Value,
                    Left = null,
                };
                if (!Resolve_Identifier(out m.Right))
                {
                    if (!Resolve_Block(out m.Right))
                    {
                        m.Right = Expression.Resolve(tokens.ToArray());
                        Read();
                    }
                }
                e = m;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 解析表达式块
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool Resolve_Block(out Expression e)
        {
            if (Current.Value == "(")
            {
                var nCount = 0;
                List<ExpressionToken> tokens = new List<ExpressionToken>();
                while (true)
                {
                    if (Current == null)
                        throw new Exception("表达式解析失败,( )不匹配");
                    if (Current.Value == "(")
                        nCount++;
                    else if (Current.Value == ")")
                        nCount--;
                    tokens.Add(Current);
                    Read();
                    if (nCount == 0)
                    {
                        if (tokens.Count < 2)
                        {
                            throw new Exception("表达式解析失败,( )不匹配");
                        }

                        tokens.RemoveAt(0);
                        tokens.RemoveAt(tokens.Count - 1);
                        if (tokens.Count == 0)
                            throw new Exception("表达式解析失败, 无效的( ), 没有任何意义");

                        e = new ExpressionResolve(tokens.ToArray()).Resolve();
                        return true;
                    }
                }
            }
            e = null;
            return false;
        }
        
        /// <summary>
        /// 解析方法
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool Resolve_Function(out Expression e)
        {
            e = null;
            if (Current.Type != ExpressionToken.eType.Identifier)
                return false;

            if (Peek.Value != "(")
                return false;

            var rFunction = new ExpressionFunction(Current.Value);

            // 所有参数列表
            var rListToken = new List<ExpressionToken>();
            var rListParameter = new List<ExpressionToken>();

            Read();

            var nCount = 0;

            while (true)
            {
                if (Current.Value == "(")
                {
                    nCount++;
                }
                else if (Current.Value == ")")
                {
                    nCount--;

                    if (0 == nCount)
                    {
                        Read();
                        break;
                    }
                }

                rListToken.Add(Current);

                Read();
            }

            // 移除开始与结束的括号
            rListToken.RemoveAt(0);
            rListToken.Add(new ExpressionToken(ExpressionToken.eType.End, null));

            foreach (var rToken in rListToken)
            {
                if (rToken.Value == "(")
                    nCount++;
                else if (rToken.Value == ")")
                    nCount--;
                if ((rToken.Value == "," && nCount == 0) || rToken.Type == ExpressionToken.eType.End)
                {
                    if (rListParameter.Count > 0)
                    {
                        var rResolve = new ExpressionResolve(rListParameter.ToArray());
                        var rExpression = rResolve.Resolve();

                        // 添加到参数列表中
                        rFunction.Parameter.Add(rExpression);

                        rListParameter.Clear();
                    }
                }
                else
                {
                    rListParameter.Add(rToken);
                }
            }

            if (nCount != 0)
            {
                throw new Exception("方法（ )不匹配");
            }
            e = rFunction;
            return true;
        }

        /// <summary>
        /// 解析特殊标识符
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool Resolve_Identifier(out Expression e)
        {
            e = null;
            if (Current.Type != ExpressionToken.eType.Identifier)
                return false;

            // 标示符打头解析(变量赋值及函数调用)
            if (Resolve_Function(out e))
            {
                return true;
            }

            e = new ExpressionVariable(Current.Value);
            Read();
            return true;
        }
    }
}