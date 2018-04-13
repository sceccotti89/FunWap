/**
* @author Stefano Ceccotti
*/

using System;
using System.Collections.Generic;
using System.Text;

using Funwap.engine;
using Funwap.parser;
using Funwap.statements;
using Funwap.expression;

namespace Funwap.miscellaneous
{
    public class Function : Record, IExpr
    {
        /* The formal parameters. */
        private List<Identifier> parameters = null;
        /* The return type. */
        private Types ret_type;
        /* The block. */
        private Block b = null;

        /**
         * Creates a "static" function.
         * 
         * @param name            name of the function
         * @param ret_type        return type
         * @param parameters      list of parameters
        */
        public Function(String name, int ret_type, params Identifier[] parameters) : base()
        {
            this.name = name;
            this.ret_type = new Types(ret_type);

            if (parameters.Length > 0)
            {
                this.parameters = new List<Identifier>(parameters);
            }
        }

        /**
         * Creates a "dynamic" function.
         * 
         * @param name            name of the function
         * @param parameters      list of parameters
         * @param ret_type        return type
        */
        public Function(String name, List<Identifier> parameters, Types ret_type) : base()
        {
            this.name = name;
            this.parameters = parameters;
            this.ret_type = ret_type;
        }

        /**
         * Adds the block of the function.
         * 
         * @param b    the block
        */
        public void addBlock(Block b)
        {
            this.b = b;
        }

        /**
         * Checks the signature of this function.
         * 
         * @param args    the arguments that must be checked
         * 
         * @return TRUE if the arguments are the same, FALSE otherwise
        */
        public bool checkSignature(List<Identifier> args)
        {
            if (args == null && parameters == null)
            {
                return true;
            }

            if ((args == null && parameters != null) ||
                (args != null && parameters == null) ||
                (args != null && parameters != null && args.Count != parameters.Count))
            {
                return false;
            }

            for (int i = 0; i < args.Count; i++)
            {
                if (!args[i].getType().checkType(parameters[i].getType(), true))
                {
                    return false;
                }
            }

            return true;
        }

        /**
         * Checks the number and the type of the formal and actual parameters.
         * 
         * @param actual_parameters    the actual parameters
         *
         * @return TRUE if the number and type of the parameters coincide, FALSE otherwise
        */
        public bool checkParameters(List<IExpr> actual_parameters)
        {
            if (actual_parameters == null && parameters == null)
            {
                return true;
            }

            if ((actual_parameters == null && parameters != null) ||
                (actual_parameters != null && parameters == null) ||
                (actual_parameters != null && parameters != null && actual_parameters.Count != parameters.Count))
            {
                return false;
            }

            // checks if the type of the variables are the same
            for (int i = 0; i < parameters.Count; i++)
            {
                if (!parameters[i].getType().checkType(Parser.typeCheck(actual_parameters[i]), true) )
                {
                    return false;
                }
            }

            return true;
        }

        /**
         * Checks the type of this function and the input.
         * 
         * @param type    the input type
         * 
         * @return TRUE if they are equals, FALSE otherwise
        */
        public void checkType(Types type)
        {
            bool is_null = true;
            String f_type = null;

            // obtain the string of the type
            if (parameters != null)
            {
                is_null = false;
                StringBuilder b_type = new StringBuilder(32);

                b_type.Append("fun( ");
                for (int i = 0; i < parameters.Count; i++)
                {
                    b_type.Append(((i == 0) ? "" : ", ") + parameters[i].getType());
                }
                b_type.Append(" ) " + ret_type);

                f_type = b_type.ToString();
            }

            List<Types> args = type.getArgs();

            if ((args == null && !is_null) ||
                (args != null && is_null) ||
                (args != null && !is_null && args.Count != parameters.Count))
            {
                throw new Exception("Error line " + Tokenizer.getLine() +
                                     ": is not possibile to assigne the type \"" + ((f_type == null) ? ("fun() " + ret_type) : f_type) +
                                     ")\" to type \"" + type + "\".");
            }

            // checks if the types are the same
            if (args != null && !is_null)
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    if (!parameters[i].getType().checkType(args[i], true))
                    {
                        throw new Exception("Error line " + Tokenizer.getLine() +
                                             ": is not possibile to assigne the type \"" + ((f_type == null) ? ("fun() " + ret_type) : f_type) +
                                             "\" to type \"" + type + "\".");
                    }
                }
            }

            if (!type.get_closure_ret().checkType(ret_type, true))
            {
                throw new Exception("Error line " + Tokenizer.getLine() +
                                     ": is not possibile to assigne the type \"" + ((f_type == null) ? ("fun() " + ret_type) : f_type) +
                                     "\" to type \"" + type + "\".");
            }
        }

        /** Returns the list of parameters. */
        public List<Identifier> getParams()
        {
            return parameters;
        }

        /** Returns the function's return type. */
        public Types getReturnType()
        {
            return ret_type;
        }

        /** Returns the block of the function. */
        public Block getBlock()
        {
            return b;
        }

        public void setParenthesis()
        {
            // Empty body.
        }

        public bool getParenthesis()
        {
            return false;
        }

        public int getTAG()
        {
            return Token.T_FUN;
        }
    }
}