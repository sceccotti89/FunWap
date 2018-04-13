/**
* @author Stefano Ceccotti
*/

using Funwap.parser;

using Funwap.expression;

namespace Funwap.miscellaneous
{
    public class Url : IExpr
    {
        public Url() {
            // Empty constructor.
        }

        public void setParenthesis() {
            // Empty body.
        }

        public bool getParenthesis() {
            return false;
        }

        public int getTAG() {
            return Token.T_URL;
        }
    }
}
