namespace NoeExpression
{
    internal class ExpressionToken
    {
        public enum eType
        {
            // 操作数
            Operand,

            // 字符串
            String,

            // 操作符
            Operator,

            // 标示符
            Identifier,

            // 关键字
            Keyword,

            // 分割符号
            Separator,

            // 结束
            End,

            // 表达式
            Expression
        }

        public ExpressionToken(Expression rExpression)
        {
            Type = eType.Expression;
            Expression = rExpression;
        }


        public ExpressionToken(eType rType, string strValue)
        {
            Type = rType;
            Value = strValue;
        }

        public eType Type { get; set; }
        public string Value { get; set; }
        public Expression Expression { get; set; }
    }
}