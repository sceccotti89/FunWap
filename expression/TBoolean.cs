/**
* @author Stefano Ceccotti
*/

using Funwap.parser;

namespace Funwap.expression
{
    class TBoolean : IExpr
    {
        /* The boolean value. */
        private bool value;
        /* Indicates whether the boolean value is true or false. */
        private int TAG;
        /* Determines whether the expression is inside the parenthesis. */
        private bool hasParenthesis = false;

        public TBoolean( bool value ) {
            TAG = (value) ? Token.T_TRUE : Token.T_FALSE;
            this.value = value;
        }

        public bool getParenthesis() {
            return hasParenthesis;
        }

        public void setParenthesis() {
            hasParenthesis = true;
        }

        public int getTAG() {
            return TAG;
        }
    }
}
