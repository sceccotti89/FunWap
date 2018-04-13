/**
* @author Stefano Ceccotti
*/

using Funwap.expression;
using Funwap.miscellaneous;

namespace Funwap.statements
{
    public class Assignment : IStatement
    {
        /* The identifier. */
        private Identifier ide;
        /* The expression. */
        private IExpr e;
        /* The TAG. */
        private int TAG;

        public Assignment( Identifier ide, IExpr e, int TAG )
        {
            // The expression could be null (i.e. with increment or decrement).
            this.ide = ide;
            this.e = e;
            this.TAG = TAG;
        }

        /** Returns the identifier of the assignment. */
        public Identifier getIDE() {
            return ide;
        }

        /** Returns the associated expression. */
        public IExpr getExpression() {
            return e;
        }

        public int getTAG() {
            return TAG;
        }
    }
}
