/**
* @author Stefano Ceccotti
*/

namespace Funwap.expression
{
    public class Expr : IExpr
    {
        /* The two expressions. */
        private IExpr expr1, expr2;
        /* Determines whether the expression ha the parenthesis. */
        private bool hasParenthesis = false;
        /* The TAG associated to the expression. */
        private int TAG;

        public Expr( IExpr e1, IExpr e2, int TAG )
        {
            this.TAG = TAG;
            expr1 = e1;
            expr2 = e2;
        }

        /** Returns the first expression. */
        public IExpr getExpr1() {
            return expr1;
        }

        /** Returns the second expression. */
        public IExpr getExpr2() {
            return expr2;
        }

        public void setParenthesis() {
            hasParenthesis = true;
        }

        public bool getParenthesis() {
            return hasParenthesis;
        }

        public int getTAG() {
            return TAG;
        }
    }
}