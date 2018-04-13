/**
* @author Stefano Ceccotti
*/

using Funwap.parser;

namespace Funwap.expression
{
    public class Number : IExpr
    {
        /* The integer value of the number. */
        private int i_value;
        /* The float value of the number. */
        private float f_value;
        /* The double value of the number. */
        private double d_value;
        /* Determines whether the expression has the parenthesis. */
        private bool hasParenthesis = false;
        /* ID of the token. */
        private int TAG;

        public Number(int n)
        {
            i_value = n;
            TAG = Token.T_INT;
        }

        public Number(float n)
        {
            f_value = n;
            TAG = Token.T_FLOAT;
        }

        public Number(double n)
        {
            d_value = n;
            TAG = Token.T_DOUBLE;
        }

        public int getIntValue()
        {
            return i_value;
        }

        public float getFloatValue()
        {
            return f_value;
        }

        public double getDoubleValue()
        {
            return d_value;
        }

        public void setParenthesis()
        {
            hasParenthesis = true;
        }

        public bool getParenthesis()
        {
            return hasParenthesis;
        }

        public int getTAG()
        {
            return TAG;
        }
    }
}
