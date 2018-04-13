/**
* @author Stefano Ceccotti
*/

using System.Collections.Generic;
using Funwap.parser;
using Funwap.miscellaneous;

namespace Funwap.statements
{
    public class Block : Record, IStatement
    {
        /* List of statements. */
        private List<IStatement> statements;

        public Block() : base() {
            name = null;
        }

        public int getTAG() {
            return Token.T_BLOCK;
        }

        /**
         * Sets the list of commands.
         * 
         * @param commands - the list
        */
        public void addStatementsList( List<IStatement> commands ) {
            this.statements = commands;
        }

        /** Returns the list of commands */
        public List<IStatement> getStatementsList() {
            return statements;
        }
    }
}
