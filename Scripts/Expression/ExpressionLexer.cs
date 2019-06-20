// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-12-24      14:05
//  *LastModify：2018-12-24      14:22
//  ***************************************************************************************************/

#region

using System.Collections;
using System.Text;

#endregion

namespace NoeExpression
{
// 词法分析器
    public class ExpressionLexer
    {
        protected ExpressionReader Reader;

        public ExpressionLexer(string strExpression)
        {
            Reader = new ExpressionReader(strExpression);
        }

        public ExpressionToken[] GetToken()
        {
            var rArray = new ArrayList();

            ExpressionToken rToken = null;

            while (null != (rToken = GetNextToken(rToken)))
            {
                rArray.Add(rToken);
            }

            return rArray.ToArray(typeof (ExpressionToken)) as ExpressionToken[];
        }

        public ExpressionToken GetNextToken(ExpressionToken currentToken)
        {
            var nChar = Reader.Read();

            while (true)
            {
                // 无效字符串跳过处理
                if (nChar == '\r' || nChar == '\t' || nChar == ' ' || nChar == '\n')
                {
                    nChar = Reader.Read();
                }
                // 结束检测
                else if (nChar == '\0')
                {
                    return null;
                }
                else
                {
                    break;
                }
            }

            // 运算符检测
            switch (nChar)
            {
                case '+':
                {
                    if (Reader.NextIs('='))
                    {
                        Reader.Read();
                        return new ExpressionToken(ExpressionToken.eType.Operator, "+=");
                        }
                    if (Reader.NextIs('+'))
                    {
                        Reader.Read();
                        return new ExpressionToken(ExpressionToken.eType.Operator, "++");
                    }
                    return new ExpressionToken(ExpressionToken.eType.Operator, "+");
                }
                case '-':
                {
                    if (Reader.NextIs('='))
                    {
                        Reader.Read();
                        return new ExpressionToken(ExpressionToken.eType.Operator, "-=");
                    }
                    if (Reader.NextIs('-'))
                    {
                        Reader.Read();
                        return new ExpressionToken(ExpressionToken.eType.Operator, "--");
                    }
                    return new ExpressionToken(ExpressionToken.eType.Operator, "-");
                }
                case '*':
                {
                    if (Reader.NextIs('='))
                    {
                        return new ExpressionToken(ExpressionToken.eType.Operator, "*=");
                    }
                    return new ExpressionToken(ExpressionToken.eType.Operator, "*");
                }
                case '/':
                {
                    if (Reader.NextIs('='))
                    {
                        return new ExpressionToken(ExpressionToken.eType.Operator, "/=");
                    }
                    return new ExpressionToken(ExpressionToken.eType.Operator, "/");
                }
                case '|':
                {
                    if (Reader.NextIs('|'))
                    {
                        return new ExpressionToken(ExpressionToken.eType.Operator, "||");
                    }
                    return new ExpressionToken(ExpressionToken.eType.Operator, "|");
                }
                case '&':
                {
                    if (Reader.NextIs('&'))
                    {
                        return new ExpressionToken(ExpressionToken.eType.Operator, "&&");
                    }
                    return new ExpressionToken(ExpressionToken.eType.Operator, "&");
                }
                case '<':
                {
                    if (Reader.NextIs('='))
                    {
                        return new ExpressionToken(ExpressionToken.eType.Operator, "<=");
                    }
                    return new ExpressionToken(ExpressionToken.eType.Operator, "<");
                }
                case '>':
                {
                    if (Reader.NextIs('='))
                    {
                        return new ExpressionToken(ExpressionToken.eType.Operator, ">=");
                    }
                    return new ExpressionToken(ExpressionToken.eType.Operator, ">");
                }
                case '=':
                {
                    if (Reader.NextIs('='))
                    {
                        return new ExpressionToken(ExpressionToken.eType.Operator, "==");
                    }
                    return new ExpressionToken(ExpressionToken.eType.Operator, "=");
                }
                case '!':
                {
                    if (Reader.NextIs('='))
                    {
                        return new ExpressionToken(ExpressionToken.eType.Operator, "!=");
                    }
                    return new ExpressionToken(ExpressionToken.eType.Operator, "!");
                }
                case '(':
                {
                    return new ExpressionToken(ExpressionToken.eType.Operator, "(");
                }
                case ')':
                {
                    return new ExpressionToken(ExpressionToken.eType.Operator, ")");
                }
                case ',':
                {
                    return new ExpressionToken(ExpressionToken.eType.Separator, ",");
                }
            }
            // 检测是否是字符串
            if (nChar == '"')
            {
                var String = new StringBuilder();

                while ('"' != Reader.Peek())
                {
                    String.Append(Reader.Read());
                }

                // 去掉后面的引号
                Reader.Read();

                return new ExpressionToken(ExpressionToken.eType.String, String.ToString());
            }

            // 检测是否是字符串
            if (nChar == '\'')
            {
                var String = new StringBuilder();

                while ('\'' != Reader.Peek())
                {
                    String.Append(Reader.Read());
                }

                // 去掉后面的引号
                Reader.Read();

                return new ExpressionToken(ExpressionToken.eType.String, String.ToString());
            }

            // 是否是数字
            if (char.IsDigit(nChar))
            {
                var String = new StringBuilder();

                String.Append(nChar);

                while (char.IsDigit(Reader.Peek()) || Reader.Peek() == '.')
                {
                    String.Append(Reader.Read());
                }

                return new ExpressionToken(ExpressionToken.eType.Operand, String.ToString());
            }

            // 是否是字符串
            if (char.IsLetter(nChar))
            {
                var String = new StringBuilder();

                String.Append(nChar);

                while (char.IsLetter(Reader.Peek()))
                {
                    String.Append(Reader.Read());
                }


                if (ExpressionOperator.TableMapping.ContainsKey(String.ToString().ToLower()))
                {
                    return new ExpressionToken(ExpressionToken.eType.Operator,
                        ExpressionOperator.TableMapping[String.ToString().ToLower()]);
                }
                return new ExpressionToken(ExpressionToken.eType.Identifier, String.ToString());
            }

            throw new System.Exception($"解析表达式失败。无效的字符：{nChar}");
        }
    }
}