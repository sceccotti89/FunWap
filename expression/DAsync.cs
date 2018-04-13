/**
* @author Stefano Ceccotti
*/

using Funwap.parser;
using Funwap.statements;
using System.Collections.Generic;

namespace Funwap.expression
{
    public class DAsync : IExpr, IStatement
    {
        /* The the remote address. */
        private IExpr address;
        /* The return expression. */
        private IExpr e_return;
        /* The block of the async statement. */
        private List<IStatement> statements_list;
        /* Determines whether the expression has the parenthesis. */
        private bool hasParenthesis = false;

        public DAsync( IExpr address, List<IStatement> statements_list, IExpr e )
        {
            this.address = address;
            this.statements_list = statements_list;
            e_return = e;
        }

        /** Returns the remote address. */
        public IExpr getRemoteAddress() {
            return address;
        }

        /** Returns the return expression. */
        public IExpr getReturn() {
            return e_return;
        }

        /** Returns the list of statements. */
        public List<IStatement> getStatementsList() {
            return statements_list;
        }

        public void setParenthesis() {
            hasParenthesis = true;
        }

        public bool getParenthesis() {
            return hasParenthesis;
        }

        public int getTAG() {
            return Token.T_DASYNC;
        }
    }
}
