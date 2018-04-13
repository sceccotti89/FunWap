/**
* @author Stefano Ceccotti
*/

using Funwap.parser;
using Funwap.statements;

namespace Funwap.expression
{
    public class Async : IExpr, IStatement
    {
        /* The return expression. */
        private IExpr e_return;
        /* The block of the async statement. */
        private Block b;
        /* Determines whether the expression has the parenthesis. */
        private bool hasParenthesis = false;

        public Async( Block b, IExpr ret )
        {
            this.b = b;
            e_return = ret;
        }

        /** Returns the 'return' expression. */
        public IExpr getReturn() {
            return e_return;
        }

        /** Returns the block. */
        public Block getBlock() {
            return b;
        }

        public void setParenthesis() {
            hasParenthesis = true;
        }

        public bool getParenthesis() {
            return hasParenthesis;
        }

        public int getTAG() {
            return Token.T_ASYNC;
        }
    }
}