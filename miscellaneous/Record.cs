/**
* @author Stefano Ceccotti
*/

using System;
using System.Collections.Generic;

namespace Funwap.miscellaneous
{
    /** The definition of an activation record. */
    public class Record
    {
        /* variables of the record */
        protected Dictionary<String, Identifier> variables;
        /* name of the record */
        protected String name;
        /* determines if it is a closure */
        protected bool _isClosure = false;
        /* pointer to the closure */
        protected Record closure = null;
        /* pointer of the current record */
        protected int index = -1;
        /* the next environment of the stack */
        protected int static_link = -1;
        /* the scope of the variables */
        protected int environment = -1;

        public Record()
        {
            variables = new Dictionary<String, Identifier>(8);
        }

        /** Returns the name of the record. */
        public String getName()
        {
            return name;
        }

        /**
         * Adds the closure to this record.
         * 
         * @param r    pointer to the closure
        */
        public void addClosure(Record r)
        {
            closure = r;
        }

        /** Returns the closure of this record. */
        public Record getClosure()
        {
            return closure;
        }

        /** Sets TRUE that this record is a closure. */
        public void setClosure()
        {
            _isClosure = true;
        }

        /**
         * Determines if it is a closure.
         * 
         * @return TRUE if it's a closure, FALSE otherwise
        */
        public bool isClosure()
        {
            return _isClosure;
        }

        /** Returns the pointer of this record. */
        public int getIndex()
        {
            return index;
        }

        /**
         * Sets the pointer of this record.
         * 
         * @param index    the pointer
        */
        public void setIndex(int index)
        {
            this.index = index;
        }

        /**
         * Sets the environment.
         * 
         * @param index    index of actual environment
        */
        public void setEnvironment(int index)
        {
            this.environment = index;
        }

        /** Returns the index of the associated environment. */
        public int getEnvironment()
        {
            return environment;
        }

        /**
         * Sets the static environment.
         * 
         * @param index    the static link to the environment
        */
        public void setStaticLink(int index)
        {
            static_link = index;
        }

        /** Returns the index of the static environment. */
        public int getStaticLink()
        {
            return static_link;
        }

        /**
         * Checks if an identifier has been already declared.
         * 
         * @param name    the name of the identifier
         * 
         * @return TRUE if the identifier is found, FALSE otherwise
        */
        public bool checkIdentifier(String name)
        {
            return variables.ContainsKey(name);
        }

        /**
         * Adds an identifier to the function.
         * 
         * @param name    name of the identifier
         * @param ide     the identifier
        */
        public void addIdentifier(String name, Identifier ide)
        {
            variables.Add(name, ide);
        }

        /**
         * Returns the requested identifier.
         * 
         * @param name   the name of the identifier
         * 
         * @return the identifier if found, null otherwise
        */
        public Identifier getIdentifier(String name)
        {
            return variables[name];
        }
    }
}
