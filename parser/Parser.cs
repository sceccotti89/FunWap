/**
* @author Stefano Ceccotti
*/

using System;
using System.Collections.Generic;
using System.Text;

using Funwap.engine;
using Funwap.expression;
using Funwap.statements;
using Funwap.miscellaneous;

namespace Funwap.parser
{
    public class Parser
    {
        /* the tokenizer */
        private Tokenizer tokenizer;
        /* the current token */
        private int currentToken = -1;
        /* determines if the main function is founded */
        private bool _main = false;
        /* an identifier */
        private Identifier ide;
        /* the return type of the current function */
        private Types r_type;
        /* the current record */
        private Record record = null;
        /* index of the current record */
        private static int index;
        /* index of the async record */
        private int async_index;
        /* determines if we are inside an async block */
        private bool inside_async = false;
        /* the async return statement */
        private IExpr async_return = null;
        /* determines if we are inside a block of a closure */
        private bool inside_closure = false;
        /* the closure return statement */
        private IExpr closure_return = null;
        /* list of functions parameter that must be add */
        private List<Function> functions_to_add;
        /* determines if the return statement is founded */
        private bool _return = false;
        /* list of records */
        private static List<Record> records;

        public Parser( Tokenizer t )
        {
            tokenizer = t;

            records = new List<Record>( 32 );

            // the global record
            records.Add( record = new Function( "", Token.T_VOID ) );

            // loads the activation records for println and readln functions
            Function f;
            records.Add( new Function( "println", Token.T_VOID, new Identifier( "", new Types( Token.T_STRING ) ) ) );
            records.Add( f = new Function( "println", Token.T_VOID, new Identifier( "", new Types( Token.T_INT ) ) ) );
            f.setStaticLink( 1 );
            records.Add( f = new Function( "println", Token.T_VOID, new Identifier( "", new Types( Token.T_BOOL ) ) ) );
            f.setStaticLink( 2 );
            records.Add( f = new Function( "println", Token.T_VOID, new Identifier( "", new Types( Token.T_CHAR ) ) ) );
            f.setStaticLink( 3 );
            records.Add( f = new Function( "println", Token.T_VOID ) );
            f.setStaticLink( 4 );
            records.Add( f = new Function( "readln", Token.T_STRING ) );
            f.setStaticLink( 5 );
            records.Add( f = new Function( "readln", Token.T_URL ) );
            f.setStaticLink( 6 );
            f.setIndex( 7 );

            index = 8;
        }

        /**
         * Parses the funw@p source code.
         * 
         * @return the root of the Abstract Syntax Tree
        */
        public Program parse() {
            return parsePROGRAM();
        }

        /** Skips all the comments. */
        private void skipComments()
        {
            if(currentToken == -1)
                currentToken = tokenizer.nextToken();

            while(currentToken == Token.T_COMMENT)
                currentToken = tokenizer.nextToken();
        }

        /**
         * Checks whether the next token is the expected one.
         * 
         * @param current - the current token
         * @param expected - the expected token
        */
        private void expectedToken( int current, int expected )
        {
            if(current != expected)
                throw new Exception( "Error line " + Tokenizer.getLine() +
                                     ": expected token: \"" + Token.getTokenValue( expected ) + "\", instead of: \"" +  Token.getTokenValue( current ) + "\"." );
        }

        /**
         * Parses the program.
         * 
         * @return the Abstract Syntax Tree of the program
        */
        private Program parsePROGRAM()
        {
            List<Function> functions = new List<Function>();

            // add the global variable to the first record of the stack
            List<IStatement> statements = null;
            while (true) {
                currentToken = tokenizer.nextToken();
                skipComments();

                if (currentToken != Token.T_VAR) {
                    break;
                }

                List<IStatement> declaration_list = parseDECLARATION( true );
                if(declaration_list != null){
                    if(statements == null) statements = new List<IStatement>();
                    for (int i = 0; i < declaration_list.Count; i++) {
                        statements.Add(declaration_list[i]);
                    }
                }
            }

            Function r = (Function) records[0];

            Block b = new Block();
            b.addStatementsList( statements );
            r.addBlock( b );

            functions.Add( r );

            // Parse the functions.
            while(true){
                skipComments();

                if(currentToken == Token.T_FUN){
                    switch( tokenizer.nextToken() ){
                        case( Token.T_IDENTIFIER ):
                            functions.Add( parseFUNCTION( Token.stringValue ) );
                            break;

                        case( Token.T_MAIN ):
                            if(_main)
                                throw new Exception( "Error line: " + Tokenizer.getLine() + ": duplicate main function." );
                            else{
                                _main = true;
                                functions.Add( parseFUNCTION( "main" ) );
                            }

                            break;

                        default:
                            throw new Exception( "Error line: " + Tokenizer.getLine() + ": expected an identifier after the fun Token." );
                    }
                }
                else{
                    if (currentToken == Token.T_EOF) {
                        break;
                    } else {
                        throw new Exception( "Error line: " + Tokenizer.getLine() +
                                             ": token \"" + Token.getTokenValue(currentToken) + "\" not allowed here." );
                    }
                }

                currentToken = tokenizer.nextToken();
            }

            if(!_main)
                throw new Exception( "Error line: " + Tokenizer.getLine() + ": function \"main\" not found." );

            return new Program( functions );
        }

        /** parse a declaration
         * 
         * @return list of identifier
        */
        private List<IStatement> parseDECLARATION( bool is_global )
        {
            List<IStatement> commands = null;
            List<String> ide_list = null;
            String name;

            while(true){
                expectedToken( tokenizer.nextToken(), Token.T_IDENTIFIER );

                if(lookup_ide( name = Token.stringValue, false, false ) != null)
                    throw new Exception( "Error line " + Tokenizer.getLine() +
                                                ": the variable \"" + name + "\" has been already declared." );

                if(ide_list == null) ide_list = new List<String>();
                ide_list.Add( name );

                if((currentToken = tokenizer.nextToken()) != Token.T_COMMA)
                    break;
            }

            Types type = parseTYPE1();

            if(commands == null) commands = new List<IStatement>();
            Identifier id = null;
            int size = ide_list.Count;
            for(int i = 0; i < size; i++){
                id = new Identifier( ide_list[i], type, false );
                record.addIdentifier( ide_list[i], id );
                commands.Add( new Declaration( id, null ) );
            }

            // checks if there is an assignment
            if((currentToken = tokenizer.nextToken()) == Token.T_ASSIGN){
                IExpr e;
                int j = 0;
                while(true){
                    id = record.getIdentifier( ide_list[j] );
                    if((e = parseEXPR( false )) == null){
                        // declaration of closure
                        Function f = parseCLOSURE( id, false );

                        // add the closure to the identifier
                        id.add_function( f );
                        ((Declaration) commands[j]).setExpression( f );

                        currentToken = tokenizer.nextToken();
                    }
                    else{
                        if(e.getTAG() == Token.T_CALL){
                            Call c = (Call) e;

                            if(type.getType() != c.getType().getType())
                                throw new Exception( "Error line " + Tokenizer.getLine() +
                                                            ": is not possibile to assign the type \"" + c.getType() +
                                                            "\" to type \"" + type + "\"" );

                            if(type.getType() == Token.T_FUN){
                                Function f = (Function) lookup_fun( c.getName(), c.getArgs() );

                                int n = c.getNumberOfCalls();
                                for(int i = 0; i < n; i++)
                                    f = (Function) f.getClosure();
                
                                f = new Function( name, f.getParams(), f.getReturnType() );
                                id.add_function( f );
                                addRecord( f, true );
                            }
                        }
                        else{
                            if(e.getTAG() == Token.T_IDENTIFIER){
                                Identifier i = (Identifier) e;

                                if(type.getType() == Token.T_FUN){
                                    // assignment of closures
                                    Function f = (Function) search_fun( i.getName(), null, false );
                                    Function fun = new Function( name, f.getParams(), f.getReturnType() );
                                    addRecord( fun, true );
                                }
                                else{
                                    if(!type.checkType( i.getType(), false ))
                                        throw new Exception( "Error line " + Tokenizer.getLine() + ": is not possibile to assign the type \"" +
                                                                    i.getType() + "\" to type \"" + type + "\"" );
                                }
                            }
                            else{
                                // "constant" assignment or cast
                                Types e_type = typeCheck( e );
                                if(!type.checkType( e_type, false ))
                                    throw new Exception( "Error line " + Tokenizer.getLine() + ": is not possibile to assign the type \"" +
                                                                e_type + "\" to type \"" + id.getType() + "\"." );
                            }
                        }

                        ((Declaration) commands[j]).setExpression( e );
                    }

                    id.setInit();

                    if(currentToken == Token.T_SEMICOLON)
                        break;

                    expectedToken( currentToken, Token.T_COMMA );

                    if(++j == size)
                        throw new Exception( "Error line " + Tokenizer.getLine() + ": the number of identifier is less than the assignments." );
                }
            }

            expectedToken( currentToken, Token.T_SEMICOLON );

            return commands;
        }

        /** parse a function
         * 
         * @param name - name of the function
        */
        private Function parseFUNCTION( String name )
        {
            currentToken = -1;

            skipComments();

            expectedToken( currentToken, Token.T_OPEN_PARENTHESIS );
            currentToken = -1;

            skipComments();

            List<Identifier> parameters = parsePARAM_LIST( false );

            if(search_fun( name, parameters, true ) != null){
                if(parameters == null)
                    throw new Exception( "Error line " + Tokenizer.getLine() + ": the function \"" + name + "\"() has been already declared." );
                else{
                    // obtain the type of the formal parameters
                    StringBuilder param = new StringBuilder( 32 );
                    for(int i = 0; i < parameters.Count; i++)
                        param.Append( ((i == 0) ? "" : ", ") + parameters[i].getType() );

                    throw new Exception( "Error line " + Tokenizer.getLine() + ": the function \"" + name + "( " + param.ToString() + " )\" has been already declared." );
                }
            }

            expectedToken( currentToken, Token.T_CLOSED_PARENTHESIS );
            currentToken = -1;

            skipComments();

            Function f = new Function( name, parameters, r_type = parseTYPE2( false ) );
            r_type.setName( name );

            // add the parameters to the record
            if(parameters != null){
                for(int i = 0; i < parameters.Count; i++)
                    f.addIdentifier( parameters[i].getName(), parameters[i] );
            }

            parseBLOCK( f, name );

            if(f.getReturnType().getType() != Token.T_VOID && !_return)
                throw new Exception( "Error line " + Tokenizer.getLine() +
                                            ": the function \"" + f.getName() + "\" must return a result of type: " + r_type );

            _return = false;

            return f;
        }

        /** parse the formal parameters of a function
         * 
         * @param check - TRUE if we must check the parameters (only for closures), FALSE otherwise
         * 
         * @return the list of formal parameters
        */
        private List<Identifier> parsePARAM_LIST( bool check )
        {
            List<Identifier> param_list = null;

            Types type;
            String name;
            while(currentToken == Token.T_IDENTIFIER){
                ide = new Identifier( name = Token.stringValue );
                if(check && lookup_ide( name, false, false ) != null)
                    throw new Exception( "Error line " + Tokenizer.getLine() + ": the variable \"" + name + "\" has been already declared." );

                currentToken = tokenizer.nextToken();

                ide.setType( type = parseTYPE2( true ) );
                ide.setInit();

                if(param_list == null) param_list = new List<Identifier>();
                param_list.Add( ide );

                if(type.getType() == Token.T_FUN){
                    // create the function that must be added
                    List<Identifier> parameters = null;
                    List<Types> types = type.getArgs();
                    if(types != null){
                        parameters = new List<Identifier>( types.Count );
                        for(int i = 0; i < types.Count; i++)
                            parameters.Add( new Identifier( "", types[i], true ) );
                    }

                    if(functions_to_add == null) functions_to_add = new List<Function>();
                    functions_to_add.Add( new Function( ide.getName(), parameters, type.get_closure_ret() ) );
                }

                if(currentToken != Token.T_COMMA)
                    break;

                currentToken = tokenizer.nextToken();
            }

            return param_list;
        }

        /** parse the type of an expression
         * 
         * @return the type of the expression
        */
        private Types parseTYPE1()
        {
            switch( currentToken ){
                case( Token.T_INT ):
                    return new Types( currentToken );
    
                case( Token.T_FLOAT ):
                    return new Types( currentToken );
    
                case( Token.T_DOUBLE ):
                    return new Types( currentToken );
    
                case( Token.T_CHAR ):
                    return new Types( currentToken );
    
                case( Token.T_BOOL ):
                    return new Types( currentToken );
    
                case( Token.T_URL ):
                    return new Types( currentToken );
    
                case( Token.T_STRING ):
                    return new Types( currentToken );
    
                case( Token.T_FUN ):
                    return new Types( currentToken );
    
                case( Token.T_VOID ):
                    return new Types( currentToken );
            }
    
            return new Types( Token.T_VOID );
        }

        /** parse a block
         * 
         * @param f - the function associated
         * @param name - name of the function
         * 
         * @return the block
        */
        private Block parseBLOCK( Function f, String name )
        {
            Block b = new Block();

            if(f != null){
                f.addBlock( b );

                addRecord( f, f.isClosure() );

                record = f;

                if(functions_to_add != null){
                    int size = functions_to_add.Count;
                    while(size > 0){
                        Record fun = functions_to_add[0];
                        functions_to_add.RemoveAt( 0 );
                        addRecord( fun, true );
                        size--;
                    }

                    functions_to_add = null;
                }
            }
            else{
                addRecord( b, false );
                record = b;
            }

            expectedToken( currentToken, Token.T_OPEN_BRACKET );
            currentToken = -1;

            skipComments();

            b.addStatementsList( parseSTATEMENT_LIST() );

            if(currentToken != Token.T_CLOSED_BRACKET)
                throw new Exception( "Error line " + Tokenizer.getLine() + ": syntax error on token \"" + Token.getTokenValue( currentToken ) + "\"." );

            // remove all the closures
            while(index > 0 && records[index - 1].isClosure())
                records.RemoveAt( --index );

            // checks if we must remove the current record (a block)
            if(name == null)
                records.RemoveAt( --index );

            record = records[index - 1];

            return b;
        }

        /** parse the return type of a function
         * 
         * @param must_be_found - TRUE if the return type must be founded, FALSE otherwise
         * 
         * @return the return type associated
        */
        private Types parseTYPE2( bool must_be_found )
        {
            switch( currentToken ){
                case( Token.T_INT ):
                    currentToken = tokenizer.nextToken();
    
                    return new Types( Token.T_INT );
    
                case( Token.T_FLOAT ):
                    currentToken = tokenizer.nextToken();
    
                    return new Types( Token.T_FLOAT );
    
                case( Token.T_DOUBLE ):
                    currentToken = tokenizer.nextToken();
    
                    return new Types( Token.T_DOUBLE );
    
                case( Token.T_CHAR ):
                    currentToken = tokenizer.nextToken();
    
                    return new Types( Token.T_CHAR );
    
                case( Token.T_BOOL ):
                    currentToken = tokenizer.nextToken();
    
                    return new Types( Token.T_BOOL );
    
                case( Token.T_STRING ):
                    currentToken = tokenizer.nextToken();
    
                    return new Types( Token.T_STRING );
    
                case( Token.T_URL ):                
                    currentToken = tokenizer.nextToken();
    
                    return new Types( Token.T_URL );
    
                case( Token.T_FUN ):
                    expectedToken( tokenizer.nextToken(), Token.T_OPEN_PARENTHESIS );
    
                    Types r_type = new Types( Token.T_FUN );
    
                    // parse the parameters of the function
                    Types type;
                    while(true){
                        currentToken = tokenizer.nextToken();
    
                        if((type = parseTYPE2( false )).getType() != Token.T_VOID)
                            r_type.addType( type );
    
                        if(currentToken != Token.T_COMMA)
                            break;
                    }
    
                    expectedToken( currentToken, Token.T_CLOSED_PARENTHESIS );
    
                    currentToken = tokenizer.nextToken();
    
                    r_type.set_closure_ret( type = parseTYPE2( false ) );
    
                    return r_type;
            }

            if (must_be_found)
            {
                throw new Exception("Error line: " + Tokenizer.getLine() + ": expected a type.");
            }
    
            return new Types( Token.T_VOID );
        }

        /** parse the list of statements
         * 
         * @return the list of statements
        */
        private List<IStatement> parseSTATEMENT_LIST()
        {
            List<IStatement> commands = null;

            while(true){
                skipComments();

                Assignment assignment;
                if((assignment = parseASSIGNMENT()) != null){
                    expectedToken( currentToken, Token.T_SEMICOLON );

                    if(commands == null) commands = new List<IStatement>();
                    commands.Add( assignment );
                }
                else{
                    switch( currentToken ){
                        case( Token.T_OPEN_PARENTHESIS ):
                            // call function
                            String name = this.ide.getName();

                            List<IExpr> args = parseFUN_ARGS();

                            Function f;
                            if((f = (Function) lookup_fun( name, args )) == null){
                                if(args == null)
                                    throw new Exception( "Error line " + Tokenizer.getLine() + ": no function \"" + name + "()\" founded." );
                                else{
                                    // obtain the type of the actual parameters
                                    StringBuilder param = new StringBuilder( 32 );
                                    for(int i = 0; i < args.Count; i++){
                                        if(args[i].getTAG() == Token.T_IDENTIFIER)
                                            param.Append( ((i == 0) ? "" : ", ") + ((Identifier) args[i]).getType() );
                                        else
                                            param.Append( ((i == 0) ? "" : ", ") + typeCheck( args[i] ) );
                                    }

                                    throw new Exception( "Error line " + Tokenizer.getLine() + ": no function \"" + name + "( " + param.ToString() + " )\" founded." );
                                }
                            }

                            expectedToken( currentToken, Token.T_CLOSED_PARENTHESIS );

                            Call c = new Call( name, args, f.getReturnType() );

                            Types t = f.getReturnType();
                            int j = 1;
                            while((currentToken = tokenizer.nextToken()) == Token.T_OPEN_PARENTHESIS){
                                if(t.getType() != Token.T_FUN)
                                    throw new Exception( "Error line " + Tokenizer.getLine() + ": the return type of the " + j + "' invocation is not a function." );

                                List<IExpr> expr_list = parseFUN_ARGS();
                                t.check_param_list( expr_list );

                                c.addCall( expr_list, t = t.get_closure_ret() );

                                j++;
                            }

                            expectedToken( currentToken, Token.T_SEMICOLON );

                            if(commands == null) commands = new List<IStatement>();
                            commands.Add( c );

                            break;

                        case( Token.T_FOR ):
                            if(this.ide != null)
                                throw new Exception( "Error line " + Tokenizer.getLine() + ": \""  + this.ide.getName() + "\" cannot be resolved to a type." );

                            // first assignment
                            currentToken = tokenizer.nextToken();
                            Assignment a1 = parseASSIGNMENT();
                            expectedToken( currentToken, Token.T_SEMICOLON );

                            // check the iteration condition
                            IExpr e = parseEXPR( false );
                            if(e != null && typeCheck( e ).getType() != Token.T_BOOL)
                                throw new Exception( "Error line " + Tokenizer.getLine() + ": the for statement requires a boolean guard." );

                            expectedToken( currentToken, Token.T_SEMICOLON );

                            // get the command executed for each iteration
                            currentToken = tokenizer.nextToken();
                            Assignment a2 = parseASSIGNMENT();

                            // the body
                            Block b = parseBLOCK( null, null );

                            if(commands == null) commands = new List<IStatement>();
                            commands.Add( new For( a1, e, a2, b ) );
    
                            break;

                        case( Token.T_WHILE ):
                            if(this.ide != null)
                                throw new Exception( "Error line " + Tokenizer.getLine() + ": \""  + this.ide.getName() + "\" cannot be resolved to a type." );

                            if((e = parseEXPR( true )) == null)
                                throw new Exception( "Error line " + Tokenizer.getLine() + ": the while statement requires a non-null guard." );

                            if(typeCheck( e ).getType() != Token.T_BOOL)
                                throw new Exception( "Error line " + Tokenizer.getLine() + ": the while statement requires a boolean guard." );

                            b = parseBLOCK( null, null );

                            if(commands == null) commands = new List<IStatement>();
                            commands.Add( new While( e, b ) );
                        
                            break;

                        case( Token.T_IF ):
                            if(this.ide != null)
                                throw new Exception( "Error line " + Tokenizer.getLine() + ": \""  + this.ide.getName() + "\" cannot be resolved to a type." );

                            if((e = parseEXPR( true )) == null)
                                throw new Exception( "Error line " + Tokenizer.getLine() + ": the if statement requires a non-null guard." );

                            if(typeCheck( e ).getType() != Token.T_BOOL)
                                throw new Exception( "Error line " + Tokenizer.getLine() + ": the if statement requires a boolean guard." );

                            b = parseBLOCK( null, null );

                            Block b2 = null;
                            // checks if the else branch is present
                            if((currentToken = tokenizer.nextToken()) == Token.T_ELSE){
                                currentToken = tokenizer.nextToken();
                                b2 = parseBLOCK( null, null );
                            }

                            if(commands == null) commands = new List<IStatement>();
                            commands.Add( new IfThenElse( e, b, b2 ) );

                            if(b2 == null)
                                continue;

                            break;

                        case( Token.T_OPEN_BRACKET ): // block
                            if(this.ide != null)
                                throw new Exception( "Error line " + Tokenizer.getLine() + ": \""  + this.ide.getName() + "\" cannot be resolved to a type." );

                            if(commands == null) commands = new List<IStatement>();
                            commands.Add( parseBLOCK( null, null ) );

                            break;

                        case( Token.T_RETURN ):
                            if(this.ide != null)
                                throw new Exception( "Error line " + Tokenizer.getLine() + ": \""  + this.ide.getName() + "\" cannot be resolved to a type." );

                            _return = true;
                            Return r = null;

                            // check the return type: expression or closure
                            if((e = parseEXPR( false )) == null){
                                if(currentToken == Token.T_SEMICOLON){
                                    if(r_type.getType() != Token.T_VOID)
                                        throw new Exception( "Error line " + Tokenizer.getLine() +
                                                                    ": is not possibile to assign the type \"void\" to type \"" + r_type + "\"." );

                                    if(commands == null) commands = new List<IStatement>();
                                    commands.Add( new Return( null ) );
                                }
                                else{
                                    // return a closure
                                    expectedToken( currentToken, Token.T_FUN );

                                    bool inside_async = this.inside_async;
                                    bool inside_closure = this.inside_closure;

                                    f = parseCLOSURE( null, true );

                                    expectedToken( tokenizer.nextToken(), Token.T_SEMICOLON );

                                    if(inside_async)
                                        async_return = new Return( f );

                                    if(inside_closure)
                                        closure_return = new Return( f );

                                    if(commands == null) commands = new List<IStatement>();
                                    commands.Add( r = new Return( f ) );
                                }
                            }
                            else{
                                if(e.getTAG() == Token.T_IDENTIFIER && ((Identifier) e).getType().getType() == Token.T_FUN){
                                    // a closure identifier
                                    f = ((Identifier) e).get_function();
                                    f.checkType( r_type );

                                    record.addClosure( f );
                                }
                                else{
                                    // expression
                                    Types type = typeCheck( e );
                                    if(!r_type.checkType( type, false ))
                                        throw new Exception( "Error line " + Tokenizer.getLine() + ": the function has type \"" +
                                                                    r_type + "\", while the return has type \"" + type + "\"." );
                                }

                                if(commands == null) commands = new List<IStatement>();
                                commands.Add( r = new Return( e ) );
                            }

                            // obtain the return statment
                            if(inside_async)
                                async_return = r;

                            if(inside_closure)
                                closure_return = r;

                            break;

                        case( Token.T_VAR ): // declaration
                            if(this.ide != null)
                                throw new Exception( "Error line " + Tokenizer.getLine() + ": \""  + this.ide.getName() + "\" cannot be resolved to a type." );

                            List<IStatement> statements = parseDECLARATION( false );
                            if(statements != null){
                                if(commands == null) commands = new List<IStatement>();
                                for(int i = 0; i < statements.Count; i++)
                                    commands.Add( statements[i] );
                            }

                            break;

                        case( Token.T_ASYNC ):
                            if(this.ide != null)
                                throw new Exception( "Error line " + Tokenizer.getLine() + ": \""  + this.ide.getName() + "\" cannot be resolved to a type." );

                            currentToken = tokenizer.nextToken();

                            inside_async = true;
                            async_index = index;
                            b = parseBLOCK( null, null );
                            inside_async = false;

                            if(async_return != null)
                                throw new Exception( "Error line " + Tokenizer.getLine() + ": the return statement is not allowed without an async assignment." );

                            currentToken = tokenizer.nextToken();

                            if(commands == null) commands = new List<IStatement>();
                            commands.Add( new Async( b, async_return ) );

                            break;

                        case( Token.T_DASYNC ):
                            if(this.ide != null)
                                throw new Exception( "Error line " + Tokenizer.getLine() + ": \""  + this.ide.getName() + "\" cannot be resolved to a type." );

                            expectedToken( tokenizer.nextToken(), Token.T_OPEN_BRACKET );

                            // checks if the address is an URL
                            Identifier ide = null;
                            if ((currentToken = tokenizer.nextToken()) != Token.T_IDENTIFIER ||
                                (ide = lookup_ide(Token.stringValue, true, false)).getType().getType() != Token.T_URL) {
                                throw new Exception("Error line " + Tokenizer.getLine() + ": expected an url expression.");
                            }

                            if(!ide.hasValue( index ))
                                throw new Exception( "Error line " + Tokenizer.getLine() + ": the variable \"" + ide.getName() + "\" is used uninitialized." );

                            expectedToken( tokenizer.nextToken(), Token.T_COMMA );

                            currentToken = tokenizer.nextToken();

                            inside_async = true;
                            async_index = index;
                            List<IStatement> statements_list = parseSTATEMENT_LIST();
                            inside_async = false;

                            if(async_return != null)
                                throw new Exception( "Error line " + Tokenizer.getLine() + ": the return statement is not allowed without a dasync assignment." );

                            expectedToken( currentToken, Token.T_CLOSED_BRACKET );

                            currentToken = tokenizer.nextToken();

                            if(commands == null) commands = new List<IStatement>();
                            commands.Add( new DAsync( ide, statements_list, async_return ) );

                            break;

                        case( Token.T_SEMICOLON ):
                            if(this.ide != null)
                                throw new Exception( "Error line " + Tokenizer.getLine() + ": \""  + this.ide.getName() + "\" cannot be resolved to a type." );

                            // ignores all the empty statements
                            break;

                        default:
                            if(this.ide != null)
                                throw new Exception( "Error line " + Tokenizer.getLine() + ": \""  + this.ide.getName() + "\" cannot be resolved to a type." );

                            return commands;
                    }
                }

                currentToken = tokenizer.nextToken();
            }
        }

        /** parse an assignment
         * 
         * @return an assignment if found, null otherwise
        */
        private Assignment parseASSIGNMENT()
        {
            if(currentToken == Token.T_IDENTIFIER){
                String name = Token.stringValue;
                ide = new Identifier( name );

                IExpr e;
                switch( currentToken = tokenizer.nextToken() ){
                    case( Token.T_INCR ):
                    case( Token.T_DECR ):
                        if((ide = lookup_ide( name, true, true )) == null)
                            throw new Exception( "Error line " + Tokenizer.getLine() + ": the identifier \"" + name + "\" has never been declared." );

                        Types type = ide.getType();
                        if(type.getType() != Token.T_INT && type.getType() != Token.T_FLOAT && type.getType() != Token.T_DOUBLE)
                            throw new Exception( "Error line " + Tokenizer.getLine() + ": the variable \"" + name + "\" must be a number." );

                        if(!ide.hasValue( index ))
                            throw new Exception( "Error line " + Tokenizer.getLine() + ": the variable \"" + name + "\" is used uninitialized." );

                        int token = currentToken;
                        currentToken = tokenizer.nextToken();

                        return new Assignment( ide, null, token );
    
                    case( Token.T_ASSIGN ):
                        Identifier id;
                        if((id = lookup_ide( name, true, true )) == null)
                            throw new Exception( "Error line " + Tokenizer.getLine() + ": the identifier \"" + name + "\" has never been declared." );

                        id.setModified( inside_async || inside_closure, index );

                        if((e = parseEXPR( false )) == null){
                            switch( currentToken ){
                                case( Token.T_ASYNC ):
                                    currentToken = tokenizer.nextToken();

                                    inside_async = true;
                                    async_index = index;
                                    Block b = parseBLOCK( null, null );
                                    inside_async = false;

                                    if(async_return == null)
                                        throw new Exception( "Error line " + Tokenizer.getLine() + ": expected a return statement." );

                                    async_return = null;

                                    currentToken = tokenizer.nextToken();

                                    return new Assignment( id, new Async( b, async_return ), Token.T_ASSIGN );

                                case( Token.T_DASYNC ):
                                    expectedToken( tokenizer.nextToken(), Token.T_OPEN_BRACKET );

                                    // checks if the address is an URL
                                    Identifier ide = null;
                                    if((currentToken = tokenizer.nextToken()) != Token.T_IDENTIFIER ||
                                        (ide = lookup_ide( Token.stringValue, true, false )).getType().getType() != Token.T_URL)
                                            throw new Exception( "Error line " + Tokenizer.getLine() + ": expected an url expression." );

                                    if(!ide.hasValue( index ))
                                        throw new Exception( "Error line " + Tokenizer.getLine() + ": the variable \"" + ide.getName() + "\" is used uninitialized." );

                                    expectedToken( tokenizer.nextToken(), Token.T_COMMA );

                                    currentToken = tokenizer.nextToken();

                                    inside_async = true;
                                    async_index = index;
                                    List<IStatement> statements_list = parseSTATEMENT_LIST();
                                    inside_async = false;

                                    if(async_return == null)
                                        throw new Exception( "Error line " + Tokenizer.getLine() + ": expected a return statement." );

                                    async_return = null;

                                    expectedToken( currentToken, Token.T_CLOSED_BRACKET );

                                    currentToken = tokenizer.nextToken();

                                    return new Assignment( id, new DAsync( ide, statements_list, async_return ), Token.T_ASSIGN );

                                case( Token.T_FUN ):
                                    // closure assignment
                                    Function f = parseCLOSURE( id, false );
                                    id.add_function( f );

                                    currentToken = tokenizer.nextToken();

                                    return new Assignment( id, f, Token.T_ASSIGN );

                                default:
                                    throw new Exception( "Error line " + Tokenizer.getLine() + ": syntax error, remove the token " + currentToken + "." );
                            }
                        }
                        else{
                            // identifier or call
                            if(e.getTAG() == Token.T_CALL){
                                Call c = (Call) e;

                                if(id.isInit() && !id.getType().checkType( c.getType(), false ))
                                    throw new Exception( "Error line " + Tokenizer.getLine() + ": is not possibile to assign the type \"" +
                                                                c.getType() + "\" to type \"" + id.getType() + "\"" );

                                if(id.getType().getType() == Token.T_FUN){
                                    Function f = (Function) lookup_fun( c.getName(), c.getArgs() );

                                    int n = c.getNumberOfCalls();
                                    for(int i = 0; i < n; i++)
                                        f = (Function) f.getClosure();
                    
                                    f = new Function( name, f.getParams(), f.getReturnType() );
                                    addRecord( f, true );
                                }
                            }
                            else{
                                if(e.getTAG() == Token.T_IDENTIFIER){
                                    Identifier ide = (Identifier) e;
                                    type = typeCheck( e );

                                    if(type.getType() == Token.T_FUN){
                                        if(!id.isInit()){
                                            id.setType( type );

                                            Function f = (Function) search_fun( ide.getName(), null, false );
                                            Function fun = new Function( name, f.getParams(), f.getReturnType() );
                                            addRecord( fun, true );
                                        }
                                        else{
                                            if(!type.checkType( id.getType(), true ))
                                                throw new Exception( "Error line " + Tokenizer.getLine() + ": is not possibile to assign the type \"" +
                                                                            id.getType() + "\" to type \"" + type + "\"" );
                                        }
                                    }
                                    else{
                                        if(!type.checkType( id.getType(), false ))
                                            throw new Exception( "Error line " + Tokenizer.getLine() + ": is not possibile to assign the type \"" +
                                                                        id.getType() + "\" to type \"" + type + "\"" );
                                    }
                                }
                            }

                            return new Assignment( id, e, Token.T_ASSIGN );
                        }
    
                    case( Token.T_PLUS_EQUALS ):    
                    case( Token.T_MINUS_EQUALS ):
                    case( Token.T_DIVIDE_EQUALS ):
                    case( Token.T_MULTIPLY_EQUALS ):
                        if((id = lookup_ide( name, true, true )) == null)
                            throw new Exception( "Error line " + Tokenizer.getLine() + ": the identifier \"" + name + "\" has never been declared." );

                        if((type = id.getType()).getType() != Token.T_INT && type.getType() != Token.T_FLOAT && type.getType() != Token.T_DOUBLE)
                            throw new Exception( "Error line " + Tokenizer.getLine() + ": the variable \"" + name + "\" must be a number." );

                        if(!id.hasValue( index ))
                            throw new Exception( "Error line " + Tokenizer.getLine() + ": the variable \"" + name + "\" is used uninitialized." );

                        id.setModified( inside_async || inside_closure, index );

                        token = currentToken;
                        if((type = typeCheck( (e = parseEXPR( true )) )).getType() != Token.T_INT && type.getType() != Token.T_FLOAT && type.getType() != Token.T_DOUBLE)
                            throw new Exception( "Error line " + Tokenizer.getLine() +
                                                        ": the right expression must be an integer instead of \"" + type + "\"" );

                        return new Assignment( id, e, token );
                }
            }
            else
                ide = null;

            return null;
        }

        /** parse an expression
         * 
         * @param must_be_found - TRUE if the expression must be found, FALSE otherwise
         * 
         * @return an expression if it's found, null otherwise
        */
        private IExpr parseEXPR( bool must_be_found )
        {
            IExpr e = parseE1( must_be_found );
            if(e != null)
                return parseE3( e );

            return e;
        }

        /** parse the first part of an expression
         * 
         * @param must_be_found - TRUE if the expression must be found, FALSE otherwise
        */
        private IExpr parseE1( bool must_be_found )
        {
            IExpr e = parseEXPRESSION( must_be_found );
            if(e != null)
                return parseE2( e );

            return e;
        }

        /** parse the second part of an expression (comparison operators)
         * 
         * @param e - the input expression
        */
        private IExpr parseE2( IExpr e )
        {
            int TAG = currentToken;
            switch( TAG ){
                case( Token.T_GREATER ):
                case( Token.T_GREATER_OR_EQUAL ):
                case( Token.T_LESS ):
                case( Token.T_LESS_OR_EQUAL ):
                case( Token.T_EQUALS ):
                case( Token.T_DIFFERENT ):
                    e = new Expr( e, parseE1( true ), TAG );
                    break;
            }

            return e;
        }

        /** parse the third part of an expression (AND/OR operators)
         * 
         * @param e - the input expression
        */
        private IExpr parseE3( IExpr e )
        {
            int TAG = currentToken;
            if(TAG == Token.T_AND || TAG == Token.T_OR)
                e = new Expr( e, parseEXPR( true ), TAG );

            return e;
        }

        /** parse an expression
         * 
         * @param founded - TRUE if the expression must be found, FALSE otherwise
         * 
         * @return an expression if it's found, null otherwise
        */
        private IExpr parseEXPRESSION( bool must_be_found )
        {
            IExpr e = null;
            IExpr e1, e2;
            int op = -1;
            int TAG;

            while(true){
                if((e1 = getExpr()) == null){
                    if((e == null && must_be_found) || op != -1)
                        throw new Exception( "Error line " + Tokenizer.getLine() + ": expected an expression." );
                    else
                        break;
                }

                switch( TAG = currentToken ){
                    case( Token.T_INCR ):
                    case( Token.T_DECR ):
                        if((e2 = getExpr()) != null)
                            throw new Exception( "Error line " + Tokenizer.getLine() + ": syntax error, invalid AssignmentOperator" );

                        if(e == null) e = new Expr( e1, null, TAG );
                        else e = new Expr( e, e1, TAG );
                        break;

                    case( Token.T_PLUS_EQUALS ):
                    case( Token.T_MINUS_EQUALS ):
                    case( Token.T_DIVIDE_EQUALS ):
                    case( Token.T_MULTIPLY_EQUALS ):
                        if(e1.getTAG() != Token.T_IDENTIFIER)
                            throw new Exception( "Error line " + Tokenizer.getLine() + ": syntax error, invalid AssignmentOperator" );

                        if((e2 = getExpr()) == null)
                            throw new Exception( "Error line " + Tokenizer.getLine() +
                                                        ": expected an expression. Remove token: " + Token.getTokenValue( currentToken ) );

                        if(e == null) e = new Expr( e1, e2, TAG );
                        else e = new Expr( e, new Expr( e1, e2, TAG ), op );
                        break;

                    case( Token.T_PLUS ):
                    case( Token.T_MINUS ):
                    case( Token.T_DIVIDE ):
                    case( Token.T_MULTIPLY ):
                        if((e2 = getExpr()) == null)
                            throw new Exception( "Error line " + Tokenizer.getLine() +
                                                        ": expected an expression. Remove token: " + Token.getTokenValue( currentToken ) );

                        if(e == null) e = new Expr( e1, e2, TAG );
                        else e = new Expr( e, new Expr( e1, e2, TAG ), op );
                        break;

                    default:
                        if(op == Token.T_PLUS_EQUALS || op == Token.T_MINUS_EQUALS ||
                           op == Token.T_DIVIDE_EQUALS || op == Token.T_MULTIPLY_EQUALS){
                            if(e1.getTAG() != Token.T_IDENTIFIER)
                                throw new Exception( "Error line " + Tokenizer.getLine() + ": syntax error, invalid AssignmentOperator" );
                        }

                        return (e == null) ? e1 : new Expr( e, e1, op );
                }

                op = currentToken;
                if(op != Token.T_INCR && op != Token.T_DECR && op != Token.T_PLUS && op != Token.T_MINUS && op != Token.T_DIVIDE &&
                   op != Token.T_PLUS_EQUALS && op != Token.T_MINUS_EQUALS && op != Token.T_DIVIDE_EQUALS && op != Token.T_MULTIPLY_EQUALS &&
                   op != Token.T_MULTIPLY)
                    break;
            }

            return e;
        }

        /** returns an expression
         * 
         * @return the next expression
        */
        private IExpr getExpr()
        {
            switch( currentToken = tokenizer.nextToken() ){
                case( Token.T_OPEN_PARENTHESIS ):
                    IExpr e1 = parseEXPR( false );
                    if(e1 == null){
                        // it must be a cast
                        Types _type = parseTYPE1();

                        expectedToken( tokenizer.nextToken(), Token.T_CLOSED_PARENTHESIS );

                        if((e1 = getExpr()) == null)
                            throw new Exception( "Error line " + Tokenizer.getLine() + ": expected an expression." );

                        Types t = typeCheck( e1 );
                        if(!t.checkType(_type, false ))
                            throw new Exception( "Error line " + Tokenizer.getLine() +
                                                        ": cannot cast from \"" + t + "\" to \"" + _type + "\"" );

                        e1 = new Cast(_type, e1 );
                    }
                    else{
                        e1.setParenthesis();
                        expectedToken( currentToken, Token.T_CLOSED_PARENTHESIS );
                        currentToken = tokenizer.nextToken();
                    }

                    return e1;

                case( Token.T_IDENTIFIER ):
                    String name = Token.stringValue;
                    if((currentToken = tokenizer.nextToken()) == Token.T_OPEN_PARENTHESIS){
                        // call function
                        List<IExpr> args = parseFUN_ARGS();

                        expectedToken( currentToken, Token.T_CLOSED_PARENTHESIS );

                        Function f;
                        if((f = (Function) lookup_fun( name, args )) == null){
                            if(args == null)
                                throw new Exception( "Error line " + Tokenizer.getLine() + ": no function \"" + name + "()\" founded." );
                            else{
                                // obtain and print the type of the actual parameters
                                String param = "";
                                for(int i = 0; i < args.Count; i++){
                                    if(args[i].getTAG() == Token.T_IDENTIFIER)
                                        param = param + ((i == 0) ? "" : ", ") + ((Identifier) args[i]).getType();
                                    else
                                        param = param + ((i == 0) ? "" : ", ") + getType( args[i].getTAG(), false );
                                }

                                throw new Exception( "Error line " + Tokenizer.getLine() + ": no function \"" + name + "( " + param + " )\" founded." );
                            }
                        }

                        Call c = new Call( name, args, f.getReturnType() );

                        Types t = f.getReturnType();
                        int j = 1;
                        while((currentToken = tokenizer.nextToken()) == Token.T_OPEN_PARENTHESIS){
                            if(t.getType() != Token.T_FUN)
                                throw new Exception( "Error line " + Tokenizer.getLine() + ": the return type of the " + j + "' invocation is not a function." );

                            List<IExpr> expr_list = parseFUN_ARGS();
                            t.check_param_list( expr_list );

                            c.addCall( expr_list, t = t.get_closure_ret() );

                            j++;
                        }

                        return c;
                    }
                    else{
                        // simple identifier
                        Identifier id;
                        if((id = lookup_ide( name, true, false )) == null)
                            throw new Exception( "Error line " + Tokenizer.getLine() + ": the variable \"" + name + "\" has never been declared." );

                        if(!id.hasValue( index ))
                            throw new Exception( "Error line " + Tokenizer.getLine() + ": the variable \"" + name + "\" is used uninitialized." );

                        return id;
                    }

                case( Token.T_MINUS ):
                    Types type = null;
                    if((e1 = getExpr()) == null || ((type = typeCheck( e1 )).getType() != Token.T_INT &&
                                                     type.getType() != Token.T_FLOAT && type.getType() != Token.T_DOUBLE)){
                        if(type == null)
                            throw new Exception( "Error line " + Tokenizer.getLine() + ": expected an integer expression" );
                        else
                            throw new Exception( "Error line " + Tokenizer.getLine() +
                                                        ": expected an integer expression instead of \"" + type + "\"" );
                    }

                    return new Expr( new Identifier( "", type ), e1, Token.T_MINUS );

                case( Token.T_PLUS ):
                    type = null;
                    if((e1 = getExpr()) == null || ((type = typeCheck( e1 )).getType() != Token.T_INT && 
                                                     type.getType() != Token.T_FLOAT && type.getType() != Token.T_DOUBLE)){
                        if(type == null)
                            throw new Exception( "Error line " + Tokenizer.getLine() + ": expected an integer expression" );
                        else
                            throw new Exception( "Error line " + Tokenizer.getLine() +
                                                        ": expected an integer instead of \"" + type + "\"" );
                    }

                    return new Expr( new Identifier( "", type ), e1, Token.T_PLUS );

                case( Token.T_NOT ):
                    type = null;
                    if((e1 = getExpr()) == null || (type = typeCheck( e1 )).getType() != Token.T_BOOL){
                        if(type == null)
                            throw new Exception( "Error line " + Tokenizer.getLine() + ": expected a boolean expression" );
                        else
                            throw new Exception( "Error line " + Tokenizer.getLine() +
                                                        ": expected a boolean expression instead of \"" + type + "\"" );
                    }

                    return new Expr( null, e1, Token.T_NOT );

                case( Token.T_INT ):
                    if(Token.intValue == null)
                        break;

                    int i_value = (int) Token.intValue;
                    Token.intValue = null;
                    currentToken = tokenizer.nextToken();
                    return new Number( i_value );

                case( Token.T_FLOAT ):
                    if(Token.floatValue == null)
                        break;

                    float f_value = (float) Token.floatValue;
                    Token.floatValue = null;
                    currentToken = tokenizer.nextToken();
                    return new Number( f_value );

                case( Token.T_DOUBLE ):
                    if(Token.doubleValue == null)
                        break;

                    double d_value = (double) Token.doubleValue;
                    Token.doubleValue = null;
                    currentToken = tokenizer.nextToken();
                    return new Number( d_value );

                case( Token.T_TRUE ):
                    currentToken = tokenizer.nextToken();
                    return new TBoolean( true );

                case( Token.T_FALSE ):
                    currentToken = tokenizer.nextToken();
                    return new TBoolean( false );

                case( Token.T_URL ):
                    currentToken = tokenizer.nextToken();
                    return new Url();

                case( Token.T_CHAR ):
                    if(Token.stringValue == null)
                        break;

                    char c_token = Token.stringValue[0];
                    Token.stringValue = null;
                    currentToken = tokenizer.nextToken();
                    return new TChar( c_token );

                case( Token.T_STRING ):
                    if(Token.stringValue == null)
                        break;

                    String s_token = Token.stringValue;
                    Token.stringValue = null;
                    currentToken = tokenizer.nextToken();
                    return new TString( s_token );
            }

            return null;
        }

        /** parse the actual parameters
         * 
         * @return the list of parameters
        */
        private List<IExpr> parseFUN_ARGS()
        {
            // generates and returns a vector of expressions (the actual parameters)
            List<IExpr> args = null;
            IExpr e;
            while(true){
                if((e = parseEXPR( false )) == null){
                    // checks if it is a closure
                    if(currentToken == Token.T_FUN){
                        e = parseCLOSURE( null, false );
                        currentToken = tokenizer.nextToken();
                    }
                    else
                        break;
                }

                if(args == null) args = new List<IExpr>();
                args.Add( e );

                if(currentToken != Token.T_COMMA)
                    break;
            }

            return args;
        }

        /** parse a closure function
         * 
         * @param ide_type - return type of the identifier that is associated to the closure
         * @param check_args - TRUE if we must check the type of the arguments (used for return statement), FALSE otherwise
         * 
         * @return the closure
        */
        private Function parseCLOSURE( Identifier ide, bool check_args )
        {
            if(ide != null){
                Types ide_type = ide.getType();

                if(ide_type.getType() != Token.T_FUN)
                    throw new Exception( "Error line " + Tokenizer.getLine() + ": is not possibile to assign the type \"fun\" " +
                                                "to type \"" + ide_type + "\"" );
            }

            expectedToken( tokenizer.nextToken(), Token.T_OPEN_PARENTHESIS );
            currentToken = tokenizer.nextToken();

            List<Identifier> parameters = parsePARAM_LIST( true );

            expectedToken( currentToken, Token.T_CLOSED_PARENTHESIS );

            currentToken = tokenizer.nextToken();
            // gets the return type
            Types type = parseTYPE2( false );

            if(check_args)
                r_type.check_args( parameters, type );

            // save the old type and takes the new one
            Types old_type = r_type;
            r_type = type;

            Function f = new Function( "", parameters, type );
            if(ide != null && ide.isInit())
                f.checkType( ide.getType() );

            // add the parameters to the record
            if(parameters != null){
                for(int i = 0; i < parameters.Count; i++)
                    f.addIdentifier( parameters[i].getName(), parameters[i] );
            }

            record.addClosure( f );

            inside_closure = true;
            parseBLOCK( f, null );
            inside_closure = false;

            // checks if the return statement is founded or not (if the function allows it or not)
            if(type.getType() == Token.T_VOID && closure_return != null)
                throw new Exception( "Error line " + Tokenizer.getLine() + ": the closure doesn't require a return statement." );

            if(type.getType() != Token.T_VOID && closure_return == null)
                throw new Exception( "Error line " + Tokenizer.getLine() + ": the closure requires a return statement." );

            closure_return = null;

            r_type = old_type;

            return f;
        }

        /** add a record to the stack
         * 
         * @param r - the record
         * @param isClosure - TRUE if the record is a closure, FALSE otherwise
        */
        private void addRecord( Record r, bool isClosure )
        {
            r.setStaticLink( index - 1 );
            r.setIndex( index++ );

            if(isClosure){
                r.setClosure();
                r.setEnvironment( index - 2 );
            }
            else{
                if(r.getName() == null || r.getName().Length == 0)
                    r.setEnvironment( record.getIndex() );
                else
                    r.setEnvironment( -1 );
            }
    
            records.Add( r );
        }

        /** search the function in the records stack
         * 
         * @param f_name - the name of the function
         * @param args - list of parameters
         * @param checkParams - TRUE if we must check the params, FALSE otherwise
         * 
         * @return the record if found, null otherwise
        */
        private Record search_fun( String f_name, List<Identifier> args, bool checkParams )
        {
            // if we are in an inner-block we reach the main block of the current block
            int i = index - 1;
            while(records[i].getEnvironment() >= 0){
                String name = records[i].getName();
                if(name != null && name.Equals( f_name )){
                    if(!checkParams || ((Function) records[i]).checkSignature( args ))
                        return records[i];
                }

                i = records[i].getEnvironment();
            }

            for(i = records[i].getIndex(); i >= 0; i = records[i].getStaticLink()){
                if(records[i].getName().Equals( f_name )){
                    if(!checkParams || ((Function) records[i]).checkSignature( args ))
                        return records[i];
                }
            }

            return null;
        }

        /** look for the function in the records stack
         * 
         * @param f_name - the name of the function
         * @param args - list of actual parameters
         * 
         * @return the record if found, null otherwise
        */
        private static Record lookup_fun( String f_name, List<IExpr> args )
        {
            // if we are in an inner-block we reach the main block of the current function
            int i = index - 1;
            while(records[i].getEnvironment() >= 0){
                String name = records[i].getName();
                if(name != null && name.Equals( f_name )){
                    if(((Function)records[i]).checkParameters( args ))
                        return records[i];
                }

                i = records[i].getEnvironment();
            }

            for(i = records[i].getIndex(); i >= 0; i = records[i].getStaticLink()){
                if(records[i].getName().Equals( f_name )){
                    if(((Function)records[i]).checkParameters( args ))
                        return records[i];
                }
            }

            return null;
        }

        /** look for the identifier in the records stack
         * 
         * @param name - the name of the identifier
         * @param check_global - TRUE if we must check for the global variables, FALSE otherwise
         * @param check_async - TRUE if we must compare the current and the async/dasync block index, FALSE otherwise
         * 
         * @return the identifier if found, null otherwise
        */
        private Identifier lookup_ide( String name, bool check_global, bool check_async )
        {
            for(int i = index - 1; i >= 0; i = records[i].getEnvironment()){
                if(!records[i].isClosure() && records[i].checkIdentifier( name )){
                    if(check_async && inside_async && records[i].getIndex() < async_index)
                        throw new Exception( "Error line " + Tokenizer.getLine() + ": side effects are not allowed inside an async/dasync block." );

                    return records[i].getIdentifier( name );
                }
            }

            if(check_global && records[0].checkIdentifier( name )){
                if(check_async && inside_async && records[0].getIndex() < async_index)
                    throw new Exception( "Error line " + Tokenizer.getLine() + ": side effects are not allowed inside an async/dasync block." );

                return records[0].getIdentifier( name );
            }

            return null;
        }

        /** return the string value of the TAG
         * 
         * @param TAG - the int value of the type
         * @param compilation - TRUE if we are compiling the code, FALSE otherwise
         * 
         * @return the string value
        */
        public static String getType( int TAG, bool compilation )
        {
            switch( TAG ){
                case( Token.T_INT ): return "int";
                case( Token.T_FLOAT ): return "float";
                case( Token.T_DOUBLE ): return "double";
                case( Token.T_CHAR ): return "char";
                case( Token.T_STRING ): return "string";

                case( Token.T_BOOL ):
                case( Token.T_TRUE ):
                case( Token.T_FALSE ):
                    return "bool";

                // during the compilation the URL is treated as an Uri
                case( Token.T_URL ): return (compilation) ? "Uri" : "url";
                case( Token.T_FUN ): return (compilation) ? "var" : "fun";
                case( Token.T_VOID ): return "void";
            }

            return "var";
        }

        /** performs the type checking of the expression
         * 
         * @param e - the expression that must be evaluated
         * 
         * @return the type associated to the expression
        */
        public static Types typeCheck( IExpr e )
        {
            int TAG = e.getTAG();
            switch( TAG ){
                case( Token.T_IDENTIFIER ):
                    return ((Identifier) e).getType();

                case( Token.T_TRUE ): case( Token.T_FALSE ):
                    return new Types( Token.T_BOOL );

                case( Token.T_CHAR ): case( Token.T_STRING ):
                case( Token.T_BOOL ): case( Token.T_URL ):
                case( Token.T_INT ): case( Token.T_FLOAT ): case( Token.T_DOUBLE ):
                    return new Types( TAG );

                case( Token.T_CAST ):
                    return ((Cast) e).getType();

                case( Token.T_CALL ):
                    return ((Call) e).getType();

                case( Token.T_ASYNC ):
                    return typeCheck( ((Async) e).getReturn() );

                case( Token.T_DASYNC ):
                    return typeCheck( ((DAsync) e).getReturn() );

                case( Token.T_FUN ):
                    Types type = new Types( Token.T_FUN );
                    Function f = (Function) e;
                    List<Identifier> ide = f.getParams();
                    if(ide != null){
                        for(int i = 0; i < ide.Count; i++)
                            type.addType( ide[i].getType() );
                    }

                    type.set_closure_ret( f.getReturnType() );

                    return type;

                case( Token.T_PLUS ): case( Token.T_MINUS ): case( Token.T_DIVIDE ): case( Token.T_MULTIPLY ):
                case( Token.T_PLUS_EQUALS ): case( Token.T_MINUS_EQUALS ): case( Token.T_DIVIDE_EQUALS ):
                case( Token.T_MULTIPLY_EQUALS ):
                    Expr expr = (Expr) e;
                    Types type1 = typeCheck( expr.getExpr1() ), type2 = typeCheck( expr.getExpr2() );
                    int t1 = type1.getType(), t2 = type2.getType();

                    if((t1 == Token.T_INT || t1 == Token.T_FLOAT || t1 == Token.T_DOUBLE) && (t2 == Token.T_INT || t2 == Token.T_FLOAT || t2 == Token.T_DOUBLE))
                        return new Types( Token.T_INT );
                    else
                        throw new Exception( "Error line " + Tokenizer.getLine() +
                                                    ": the first and second type must be numbers instead of \"" + type1 +
                                                    "\" and \"" + type2 + "\"." );

                case( Token.T_INCR ):
                case( Token.T_DECR ):
                    expr = (Expr) e;
                    return typeCheck( expr.getExpr1() );

                case( Token.T_LESS ): case( Token.T_LESS_OR_EQUAL ):
                case( Token.T_GREATER ): case( Token.T_GREATER_OR_EQUAL ):
                    expr = (Expr) e;
                    type1 = typeCheck( expr.getExpr1() ); type2 = typeCheck( expr.getExpr2() );
                    t1 = type1.getType(); t2 = type2.getType();

                    if((t1 == Token.T_INT || t1 == Token.T_FLOAT || t1 == Token.T_DOUBLE) && (t2 == Token.T_INT || t2 == Token.T_FLOAT || t2 == Token.T_DOUBLE) ||
                       (t1 == Token.T_CHAR && t2 == Token.T_CHAR))
                        return new Types( Token.T_BOOL );
                    else
                        throw new Exception( "Error line " + Tokenizer.getLine() +
                                                    ": the first and second type must be numbers instead of \"" + type1 +
                                                    "\" and \"" + type2 + "\"." );

                case( Token.T_EQUALS ): case( Token.T_DIFFERENT ):
                    expr = (Expr) e;
                    type1 = typeCheck( expr.getExpr1() ); type2 = typeCheck( expr.getExpr2() );
                    t1 = type1.getType(); t2 = type2.getType();

                    if((t1 == Token.T_INT || t1 == Token.T_FLOAT || t1 == Token.T_DOUBLE) && (t2 == Token.T_INT || t2 == Token.T_FLOAT || t2 == Token.T_DOUBLE) ||
                       (t1 == Token.T_CHAR && t2 == Token.T_CHAR) || (t1 == Token.T_STRING && t2 == Token.T_STRING))
                        return new Types( Token.T_BOOL );

                    if((type1 = typeCheck( expr.getExpr1() )).checkType( (type2 = typeCheck( expr.getExpr2() )), false ))
                        return new Types( Token.T_BOOL );
                    else
                        throw new Exception( "Error line " + Tokenizer.getLine() +
                                                    ": the first and second type must be of the same type instead of \"" + type1 +
                                                    "\" and \"" + type2 + "\"." );

                case( Token.T_NOT ):
                    return new Types( Token.T_BOOL );

                case( Token.T_AND ): case( Token.T_OR ):
                    expr = (Expr) e;
                    if((type1 = typeCheck( expr.getExpr1() )).getType() == Token.T_BOOL){
                        if(expr.getExpr2() == null)
                            return new Types( Token.T_BOOL );
                        else{
                            if((type2 = typeCheck( expr.getExpr2() )).getType() == Token.T_BOOL)
                                return new Types( Token.T_BOOL );
                            else
                                throw new Exception( "Error line " + Tokenizer.getLine() +
                                                            ": the second type must be a boolean instead of \"" + type2 + "\"." );
                        }
                    }
                    else
                        throw new Exception( "Error line " + Tokenizer.getLine() +
                                                    ": the first type must be a boolean instead of \"" + type1 + "\"." );

                default:
                    return new Types( Token.T_UNKNOWN );
            }
        }
    }
}
