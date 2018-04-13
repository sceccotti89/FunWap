/**
* @author Stefano Ceccotti
*/

using System;
using System.Collections.Generic;
using System.Text;
using Funwap.engine;
using Funwap.parser;
using Funwap.expression;

namespace Funwap.miscellaneous
{
    public class Types
    {
        /* The type. */
        private int type;
        /* List of arguments (for closures). */
        private List<Types> args;
        /* The return type of the closure. */
        private Types closure_ret = null;
        /* Name of the associated function. */
        private String name = "";

        public Types(int type)
        {
            this.type = type;
        }

        /**
         * Sets the name of the function.
         * 
         * @param name    the name of the function
        */
        public void setName(String name)
        {
            this.name = name;
        }

        /**
         * Adds a type (used for closures).
         * 
         * @param type    the added type
        */
        public void addType(Types type)
        {
            if (args == null)
            {
                args = new List<Types>(4);
            }

            args.Add(type);
        }

        /** Returns the type associated. */
        public int getType()
        {
            return type;
        }

        /**
         * Sets the closure return type.
         * 
         * @param type    the closure return type
        */
        public void set_closure_ret(Types type)
        {
            closure_ret = type;
        }

        /** Returns the type of the closure. */
        public Types get_closure_ret()
        {
            return closure_ret;
        }

        /** Returns the list of arguments of the closure. */
        public List<Types> getArgs()
        {
            return args;
        }

        /**
         * Checks whether the list of actual parameters are correct.
         * 
         * @param expr_list    list of expressions
        */
        public void check_param_list(List<IExpr> expr_list)
        {
            if ((expr_list == null && args != null) ||
                (expr_list != null && args == null) ||
                (expr_list != null && args != null && expr_list.Count != args.Count))
            {
                throw new Exception("Error line " + Tokenizer.getLine() + ": the number of argument is different respect to the invoked function.");
            }

            Types t;
            if (expr_list != null && args != null)
            {
                for (int i = 0; i < expr_list.Count; i++)
                {
                    if (!(t = Parser.typeCheck(expr_list[i])).checkType(args[i], true))
                    {
                        throw new Exception("Error line " + Tokenizer.getLine() + ": " +
                                             ": is not possibile to assign the type \"" + t +
                                             "\" to type \"" + args[i] + "\".");
                    }
                }
            }
        }

        /**
        * Checks whether the two types are equals.
         * 
         * @param t                   the other type
         * @param number_as_equals    TRUE if we must check if the number are equals
         * 
         * @return TRUE if they are equal, FALSE otherwise
        */
        public bool checkType(Types t, bool number_as_equals)
        {
            int p_type = t.getType();
            if (p_type == Token.T_FUN)
            {
                List<Types> parameters = t.getArgs();

                if (parameters == null && args == null)
                {
                    return true;
                }

                if ((parameters == null && args != null) ||
                    (parameters != null && args == null) ||
                    (parameters != null && args != null && parameters.Count != args.Count))
                {
                    return false;
                }

                for (int i = 0; i < parameters.Count; i++)
                {
                    if (!parameters[i].checkType(args[i], number_as_equals))
                    {
                        return false;
                    }
                }

                return t.get_closure_ret().checkType(closure_ret, number_as_equals);
            }

            if (!number_as_equals)
            {
                switch (type)
                {
                    case (Token.T_INT):
                        if (p_type == type || p_type == Token.T_CHAR)
                        {
                            return true;
                        }
                        else {
                            return false;
                        }

                    case (Token.T_FLOAT):
                        if (p_type == type || p_type == Token.T_INT || p_type == Token.T_CHAR)
                        {
                            return true;
                        }
                        else {
                            return false;
                        }

                    case (Token.T_DOUBLE):
                        if (p_type == type || p_type == Token.T_INT || p_type == Token.T_FLOAT || p_type == Token.T_CHAR)
                        {
                            return true;
                        }
                        else {
                            return false;
                        }

                    case (Token.T_CHAR):
                        if (p_type == type || p_type == Token.T_INT || p_type == Token.T_FLOAT || p_type == Token.T_CHAR)
                        {
                            return true;
                        }
                        else {
                            return false;
                        }
                }
            }

            if (p_type != type)
            {
                return false;
            }

            return true;
        }

        /**
         * Checks the params of the closure.
         * 
         * @param ide     list of identifier
         * @param type    the return type
        */
        public void check_args(List<Identifier> ide, Types type)
        {
            if ((args == null && ide != null) ||
                (args != null && ide == null) ||
                (args != null && ide != null && args.Count != ide.Count))
            {
                throw new Exception("Error line " + Tokenizer.getLine() + ": the function \"" + name + "\" has type: " + ToString() +
                                     ", but the return type is: " + printTypes(ide, type));
            }

            if (ide != null)
            {
                for (int i = 0; i < ide.Count; i++)
                {
                    if (!ide[i].getType().checkType(args[i], true))
                    {
                        throw new Exception("Error line " + Tokenizer.getLine() + ": type mismatch on " + (i + 1) + "' parameter: cannot convert from \"" +
                                             ide[i].getType() + "\" to \"" + args[i] + "\".");
                    }
                }
            }

            if (this.type == Token.T_FUN)
            {
                if (!type.checkType(closure_ret, true))
                {
                    throw new Exception("Error line " + Tokenizer.getLine() + ": the function \"" + name + "\" has return type: " + closure_ret +
                                         ", but the closure return type is: " + type);
                }
            }
            else {
                if (type.getType() != this.type)
                {
                    throw new Exception("Error line " + Tokenizer.getLine() + ": the function \"" + name + "\" has type: " + ToString() +
                                         ", but the closure type is: " + type);
                }
            }
        }

        /**
         * Prints the type in string format.
         * 
         * @param ide     list of identifier
         * @param type    the type
         * 
         * @return the return type
        */
        private String printTypes(List<Identifier> ide, Types type)
        {
            StringBuilder s_type = new StringBuilder(32);

            s_type.Append("fun(");

            if (ide != null)
            {
                for (int i = 0; i < ide.Count; i++)
                {
                    s_type.Append(((i == 0) ? " " : ", ") + ide[i].getType());
                }

                s_type.Append(" ");
            }

            s_type.Append(") ");

            s_type.Append(type);

            return s_type.ToString();
        }

        override
        public String ToString()
        {
            StringBuilder s_type = new StringBuilder(32);

            if (Compiler.compiling)
            {
                if (type == Token.T_FUN && closure_ret != null)
                {
                    int closure_type;
                    if ((closure_type = closure_ret.getType()) == Token.T_VOID)
                    {
                        s_type.Append("Action<");
                    }
                    else {
                        s_type.Append("Func<");
                    }

                    int i = 0;
                    if (args != null)
                    {
                        for (i = 0; i < args.Count; i++)
                        {
                            s_type.Append(((i == 0) ? "" : ", ") + args[i]);
                        }
                    }

                    if (closure_type != Token.T_VOID)
                    {
                        s_type.Append(((i == 0) ? "" : ", ") + closure_ret);
                    }

                    s_type.Append(">");
                }
                else {
                    s_type.Append(Funwap.parser.Parser.getType(type, true));
                }
            }
            else {
                if (type == Token.T_FUN && closure_ret != null)
                {
                    s_type.Append("fun(");

                    if (args != null)
                    {
                        for (int i = 0; i < args.Count; i++)
                        {
                            s_type.Append(((i == 0) ? " " : ", ") + args[i]);
                        }

                        s_type.Append(" ");
                    }

                    s_type.Append(")");

                    if (closure_ret.getType() != Token.T_VOID)
                    {
                        s_type.Append(" " + closure_ret);
                    }
                }
                else {
                    s_type.Append(Funwap.parser.Parser.getType(type, false));
                }
            }

            return s_type.ToString();
        }
    }
}
