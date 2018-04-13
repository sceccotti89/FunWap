/**
* @author Stefano Ceccotti
*/

using Funwap.parser;

namespace Funwap.expression
{
    public class TChar : IExpr
    {
        /* The character. */
        private char c;
        /* Determines whether the expression has the parenthesis. */
        private bool hasParenthesis = false;

        public TChar(char c) {
            this.c = c;
        }

        /** Returns the char it holds. */
        public char getChar() {
            return c;
        }

        public void setParenthesis() {
            hasParenthesis = true;
        }

        public bool getParenthesis() {
            return hasParenthesis;
        }

        public int getTAG() {
            return Token.T_CHAR;
        }
    }
}
