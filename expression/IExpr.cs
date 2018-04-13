/**
* @author Stefano Ceccotti
*/

namespace Funwap.expression
{
    public interface IExpr
    {
        /**
            Returns an integer representing the expression.
        */
        int getTAG();

        /**
            Sets TRUE the parenthesis state of the expression.
        */
        void setParenthesis();

        /** Checks whether the expression has the parenthesis.
         * 
         * @return TRUE if it has the parenthesis, FALSE otherwise
        */
        bool getParenthesis();
    }
}
