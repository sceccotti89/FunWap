/**
* @author Stefano Ceccotti
*/

using System.Collections.Generic;

namespace Funwap.miscellaneous
{
    public class Program
    {
        /* The list of functions. */
        private List<Function> functions;

        public Program( List<Function> functions ) {
            this.functions = functions;
        }

        /**
         * Returns the list of functions of the program.
         * 
         * @return the list of functions
        */
        public List<Function> getFunctions() {
            return functions;
        }
    }
}