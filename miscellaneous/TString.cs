/**
* @author Stefano Ceccotti
*/

using Funwap.parser;
using System;

using Funwap.expression;

namespace Funwap.miscellaneous
{
    public class TString : IExpr
    {
        /* The string. */
        private String s;
        /* Determine whether the expression has the parenthesis. */
        private bool hasParenthesis = false;

        public TString(String s)
        {
            this.s = s;
        }

        /** Returns the string it holds. */
        public String getString()
        {
            return s;
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
            return Token.T_STRING;
        }
    }
}
