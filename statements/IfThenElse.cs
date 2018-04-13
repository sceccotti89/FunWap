/**
* @author Stefano Ceccotti
*/

using Funwap.expression;
using Funwap.parser;

namespace Funwap.statements
{
    public class IfThenElse : IStatement
    {
        /* The if condition. */
        private IExpr e;
        /* Then and else blocks. */
        private Block b1, b2;

        public IfThenElse(IExpr e, Block b1, Block b2)
        {
            this.e = e;
            this.b1 = b1;
            this.b2 = b2;
        }

        /** Returns the condition. */
        public IExpr getCondition() {
            return e;
        }

        /** Returns the 'then' block. */
        public Block getThen() {
            return b1;
        }

        /** Returns the 'else' block. */
        public Block getElse() {
            return b2;
        }

        public int getTAG() {
            return Token.T_IF;
        }
    }
}
