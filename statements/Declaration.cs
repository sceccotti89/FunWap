/**
* @author Stefano Ceccotti
*/

using Funwap.expression;
using Funwap.miscellaneous;
using Funwap.parser;

namespace Funwap.statements
{
    public class Declaration : IStatement
    {
        /* The identifier. */
        private Identifier ide;
        /* The associated expression. */
        private IExpr e;

        public Declaration( Identifier ide, IExpr e )
        {
            this.ide = ide;
            this.e = e;
        }

        /** Returns the identifier of the assignment. */
        public Identifier getIdentifier() {
            return ide;
        }

        /**
         * Sets the associated expression.
         * 
         * @param e    the expression
        */
        public void setExpression( IExpr e ) {
            this.e = e;
        }

        /** Returns the associated expression. */
        public IExpr getExpression() {
            return e;
        }

        public int getTAG() {
            return Token.T_VAR;
        }
    }
}