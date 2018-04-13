/**
* @author Stefano Ceccotti
*/

using System;
using System.Collections.Generic;

using Funwap.expression;
using Funwap.parser;

namespace Funwap.miscellaneous
{
    public class Identifier : IExpr
    {
        /* the name of the identifier */
        private String name;
        /* the type of the identifier */
        private Types type;
        /* TRUE if it has been initialized, FALSE otherwise */
        private bool init = false;
        /* TRUE if it has been modified, FALSE otherwise */
        private bool _isModified = false;
        /* index of the record where this identifier is modified */
        private int m_index = -1;
        /* an identifier can be associated to a function */
        private Function f;
        /* determines if the expression ha the parenthesis */
        private bool hasParenthesis = false;
        /* determines if this identifier is waiting an async result */
        private bool waitingAsync = false;
        /* determines if this identifies has been founded during the compilation */
        private bool founded = false;

        public Identifier(String name)
        {
            this.name = name;
        }

        public Identifier(String name, Types type)
        {
            this.name = name;
            this.type = type;
        }

        public Identifier(String name, Types type, bool init)
        {
            this.name = name;
            this.type = type;
            this.init = init;
        }

        /** Sets true the initialized value. */
        public void setInit()
        {
            init = true;
        }

        /**
         * Checks if this identifier has been initialized.
         * 
         * @return TRUE if it has been initialized, FALSE otherwise
        */
        public bool isInit()
        {
            return init;
        }

        /**
         * Returns the modified state of the identifier.
         * 
         * @return TRUE if the identifier has been modified, FALSE otherwise
        */
        public bool isModified()
        {
            return _isModified;
        }

        /**
         * Sets TRUE the modified value.
         * 
         * @param temp - TRUE if the current record is an async or closure block, FALSE otherwise
         * @param index - index of the current record
        */
        public void setModified(bool temp, int index)
        {
            if (temp)
            {
                m_index = index;
            }
            else {
                _isModified = true;
            }
        }

        /**
         * Changes the status of waiting an async value.
         * 
         * @param waiting - TRUE if it's waiting a result, FALSE otherwise
        */
        public void setWaitingAsync(bool waiting)
        {
            waitingAsync = waiting;
        }

        /**
         * Returns the status of waiting an async value.
         * 
         * @return TRUE if it's waiting a result, FALSE otherwise
        */
        public bool isWaitingAsync()
        {
            return waitingAsync;
        }

        /**
         * Returns the founded state during the compilation.
         * 
         * @return TRUE if the identifier is founded, FALSE otherwise
        */
        public bool isFounded()
        {
            return founded;
        }

        /** Sets TRUE the founded state of the identifier (during the compilation). */
        public void setFounded()
        {
            founded = true;
        }

        /**
         * Determines if the identifier has a value.
         * 
         * @param index - index of the current record
         * 
         * @return TRUE if the identifier has a value, FALSE otherwise
        */
        public bool hasValue(int index)
        {
            if (init || _isModified)
            {
                return true;
            }
            else {
                if (m_index == -1 || index < m_index)
                {
                    return false;
                }
            }

            return true;
        }

        /**
         * Sets the type of the identifier.
         * 
         * @param type    the type of the identifier
        */
        public void setType(Types type)
        {
            this.type = type;
        }

        /**
         * Sets the function.
         * 
         * @param f    the function
        */
        public void add_function(Function f)
        {
            this.f = f;

            // create the new type
            type = new Types(Token.T_FUN);

            List<Identifier> parameters = f.getParams();
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    type.addType(parameters[i].getType());
                }
            }

            type.set_closure_ret(f.getReturnType());
        }

        /** Returns the function associated. */
        public Function get_function()
        {
            return f;
        }

        /**
         * Modifies the name of the identifier.
         * 
         * @param name - the new name
        */
        public void setName(String name)
        {
            this.name = name;
        }

        /** Returns the name of the identifier. */
        public String getName()
        {
            return name;
        }

        /** Returns the type of the identifier. */
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
            return Token.T_IDENTIFIER;
        }
    }
}
