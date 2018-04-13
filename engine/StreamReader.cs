/**
* @author Stefano Ceccotti
*/

using System;
using System.IO;
using System.Text;

using Funwap.parser;

namespace Funwap.engine
{
    public class StreamReader
    {
        /* The expression. */
        private StringBuilder expr;
        /* I/O pointer to the source file. */
        private System.IO.StreamReader reader;
        /* The index and the length of the expression. */
        private int index, length;

        public StreamReader( String file )
        {
            reader = new System.IO.StreamReader( file );
            expr = new StringBuilder( 1024 );
            index = 0;
            length = 0;
        }

        /**
         * Checks whether we are arrived at the end of the stream.
         * 
         * @return TRUE if End of File, FALSE otherwise 
        */
        public bool isEoF()
        {
            if (index >= length) {
                String line;
                try {
                    if ((line = reader.ReadLine()) == null) {
                        return true;
                    } else {
                        expr.Append( line + "\n" );
                        length = length + line.Length + 1;
                    }
                }
                catch ( IOException ) {
                    return true;
                }
            }

            return false;
        }

        /**
         * Returns the next character of the stream.
         * 
         * @return the next character
        */
        public char peek()
        {
            if (isEoF()) {
                return (char) Token.T_EOF;
            }

            return expr[index];
        }

        /**
         * Returns the next character of the stream and consumes it.
         * 
         * @return the next character
        */
        public char next()
        {
            if (isEoF()) {
                return (char) Token.T_EOF;
            }

            return expr[index++];
        }

        /** Consumes one character of the stream. */
        public void consume() {
            index++;
        }

        /**
         * Rewinds one or more characters of the stream.
         * 
         * @param num_char    of how many characters must go back
        */
        public void rewind( int num_char ) {
            index -= num_char;
        }
    }
}
