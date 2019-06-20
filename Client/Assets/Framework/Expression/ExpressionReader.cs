using System.IO;

namespace NoeExpression
{
// 字符串操作接口
    internal class ExpressionReader
    {
        protected StringReader Reader;

        public ExpressionReader(string strString)
        {
            Reader = new StringReader(strString);
        }

        public char Peek()
        {
            var nChar = Reader.Peek();

            return nChar > -1 ? (char) nChar : '\0';
        }

        public char Read()
        {
            var nChar = Reader.Read();

            return nChar > -1 ? (char) nChar : '\0';
        }

        public bool NextIs(char nChar)
        {
            if (nChar == Peek())
            {
                Read();

                return true;
            }

            return false;
        }
    }
}