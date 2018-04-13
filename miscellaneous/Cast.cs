/**
* @author Stefano Ceccotti
*/

using Funwap.expression;
using Funwap.parser;

namespace Funwap.miscellaneous
{
    public class Cast : IExpr
    {
        /* The type of the cast. */
        private Types type;
        /* The associated expression. */
        private IExpr e;

        public Cast(Types type, IExpr e)
        {
            this.type = type;
            this.e = e;
        }

        /** Returns the type of the cast operation. */
        public Types getType() {
            return type;
        }

        /** Returns the expression. */
        public IExpr getExpression() {
            return e;
        }

        public void setParenthesis() {
            // Empty body.
        }

        public bool getParenthesis() {
            return false;
        }

        public int getTAG() {
            return Token.T_CAST;
        }
    }
}
