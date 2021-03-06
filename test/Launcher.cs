/**
* @author Stefano Ceccotti
*/

using System;

using Funwap.engine;
using Funwap.parser;

namespace Funwap.test
{
    public class Launcher
    {
        public static void Main( String[] argv )
        {
            if(argv.Length == 0) {
                throw new Exception( "Missing the source file.\nUsage: Launcher file_path.funwap" );
            }
            
            String file_path = argv[0];
            
            // Parse the code.
            Parser p = new Parser( new Tokenizer( file_path ) );
            
            // Compile the code.
            Compiler c = new Compiler( file_path );
            c.compile( p.parse() );
        }
    }
}
