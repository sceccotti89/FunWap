/**
* @author Stefano Ceccotti
*/

using Funwap.expression;
using Funwap.parser;

namespace Funwap.statements
{
    public class Return : IExpr, IStatement
    {
        /* The expression that is returned. */
        private IExpr e;

        public Return( IExpr e ) {
            this.e = e;
        }

        /**
         * Returns the associated expression.
         * 
         * @return the expression if there is, null otherwise
        */
        public IExpr getExpr() {
            return e;
        }

        public void setParenthesis() {
            // Empty body.
        }

        public bool getParenthesis() {
            return false;
        }

        public int getTAG() {
            return Token.T_RETURN;
        }
    }
}