/**
* @author Stefano Ceccotti
*/

using System;
using System.Collections.Generic;

using Funwap.parser;
using Funwap.statements;
using Funwap.miscellaneous;

namespace Funwap.expression
{
    public class Call : IExpr, IStatement
    {
        /* The name of the function. */
        private String name;
        /* List of list of actual parameters. */
        private List<List<IExpr>> args = new List<List<IExpr>>();
        /* Number of calls. */
        private int size = 1;
        /* Determines whether the expression has the parenthesis. */
        private bool hasParenthesis = false;
        /* Type of the call. */
        private Types type;

        public Call(String name, List<IExpr> args, Types type)
        {
            this.name = name;
            this.args.Add(args);
            this.type = type;
        }

        /**
         * Returns the name of the function.
         * 
         * @return the name
        */
        public String getName()
        {
            return name;
        }

        /**
         * Returns the list of the actual parameters.
         * 
         * @return the list of actual parameters
        */
        public List<IExpr> getArgs()
        {
            return args[0];
        }

        /**
         * Adds a new list of actual parameters.
         * 
         * @param values - list of expressions
         * @param type - the new type of the called function
        */
        public void addCall(List<IExpr> values, Types type)
        {
            args.Add(values);
            size++;

            this.type = type;
        }

        /** Returns the next list of actual parameters. */
        public List<IExpr> getCall()
        {
            if (size-- == 1)
            {
                return null;
            }

            List<IExpr> e = args[1];
            args.Remove(e);
            return e;
        }

        /** Returns the number of calls. */
        public int getNumberOfCalls()
        {
            return size;
        }

        /** Returns the type of the called function. */
        public Types getType()
        {
            return type;
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
            return Token.T_CALL;
        }
    }
}
