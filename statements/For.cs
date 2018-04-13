/**
* @author Stefano Ceccotti
*/

using Funwap.expression;
using Funwap.parser;

namespace Funwap.statements
{
    public class For : IStatement
    {
        /* The two assignment. */
        private Assignment a1, a2;
        /* The guard. */
        private IExpr e;
        /* The block. */
        private Block b;

        public For( Assignment a1, IExpr e, Assignment a2, Block b )
        {
            // All the values except the block could be null.
            this.a1 = a1;
            this.e = e;
            this.a2 = a2;
            this.b = b;
        }

        /** Returns the first assignment. */
        public Assignment getFirstAssignment() {
            return a1;
        }

        /** Returns the guard. */
        public IExpr getGuard() {
            return e;
        }

        /** Returns the second assignment. */
        public Assignment getSecondAssignment() {
            return a2;
        }

        /** Returns the block. */
        public Block getBlock() {
            return b;
        }

        public int getTAG() {
            return Token.T_FOR;
        }
    }
}
