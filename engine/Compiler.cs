/**
* @author Stefano Ceccotti
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Funwap.expression;
using Funwap.miscellaneous;
using Funwap.parser;
using Funwap.statements;

namespace Funwap.engine
{
    public class Compiler
    {
        /* I/O pointer to the target file. */
        private StreamWriter f_code;
        /* Buffer used to store the compiled code. */
        private StringBuilder buffer;
        /* Hash map for the system functions. */
        private Dictionary<String, String> map;
        /* Determines whether we are inside an async block. */
        private bool inside_async = false;
        /* Determines whether we are compiling the global block. */
        private bool is_global = true;
        /* Determines whether we are compiling. */
        public static bool compiling = false;

        public Compiler( String file_name )
        {
            String file;
            int index = file_name.LastIndexOf( '.' );
            if (index != -1) {
                file = file_name.Substring( 0, index );
            } else {
                file = file_name;
            }

            Console.WriteLine( "FILE: " + file + ".cs" );
            f_code = new StreamWriter( file + ".cs" );

            buffer = new StringBuilder( 1024 );

            map = new Dictionary<String, String>();
            init();
        }

        /** Initializes the map structure. */
        private void init()
        {
            map.Add("println", "Console.WriteLine");
            map.Add("readln", "Console.ReadLine");
        }

        /**
         * Compiles the source code into the target one.
         * 
         * @param p    the program
        */
        public void compile( Program p )
        {
            compiling = true;

            buffer.Append("\nusing System;\n");
            buffer.Append("using System.Threading.Tasks;\n\n");

            buffer.Append("using Remoting;\n");

            buffer.Append("\nnamespace Funwap\n{\n");

            buffer.Append("\tclass Program\n\t{\n");

            buffer.Append("\t\t/* client for remote operations */\n");
            buffer.Append("\t\tprivate static ClientRMI client;\n\n");

            List<Function> functions = p.getFunctions();

            // the first record contains the global variables
            Function f = functions[0];
            compileBLOCK(f.getBlock(), 2);

            is_global = false;

            for (int i = 1; i < functions.Count; i++)
            {
                f = functions[i];
                buffer.Append("\n\t\t");

                if (f.getName().Equals("main"))
                {
                    buffer.Append("static void Main( string[] args )\n\t\t{\n");

                    buffer.Append("\t\t\tclient = new ClientRMI();\n");
                }
                else {
                    buffer.Append("private static " + f.getReturnType() + " " + f.getName() + "(");

                    if (f.getParams() != null)
                        compilePARAMS(f.getParams());

                    buffer.Append(")\n\t\t{\n");
                }

                compileBLOCK(f.getBlock(), 3);

                buffer.Append("\n\t\t}\n");
            }

            buffer.Append("\t}\n}");

            f_code.WriteLine(buffer.ToString());

            f_code.Close();
        }

        /** 
         * Compiles the parameters of a function.
         * 
         * @param params   list of parameters
        */
        private void compilePARAMS( List<Identifier> parameters )
        {
            buffer.Append( " " );

            int size = parameters.Count;
            for (int i = 0; i < size; i++) {
                buffer.Append( parameters[i].getType() + " " + parameters[i].getName() );
                if (i < size - 1) {
                    buffer.Append( ", " );
                }
            }

            buffer.Append(" ");
        }

        /**
         * Compiles a block.
         * 
         * @param b       the block
         * @param tabs    number of tabulations
        */
        private void compileBLOCK( Block b, int tabs )
        {
            List<IStatement> statements = b.getStatementsList();
            if (statements != null) {
                int size = statements.Count;
                for (int j = 0; j < size; j++) {
                    compileSTATEMENT( statements[j], tabs );
                }
            }
        }

        /**
         * Compiles a statement.
         * 
         * @param s       the statement
         * @param tabs    number of tabulations
        */
        private void compileSTATEMENT( IStatement s, int tabs )
        {
            switch (s.getTAG())
            {
                case (Token.T_VAR):
                    Declaration d = (Declaration)s;
                    Identifier ide = d.getIdentifier();

                    if (ide.getType().getType() != Token.T_FUN)
                    {
                        addTabs(tabs);

                        buffer.Append(((is_global) ? "private static " : "") + ide.getType() + " " + ide.getName());

                        if (ide.isInit())
                        {
                            if (ide.getType().getType() == Token.T_URL)
                            {
                                buffer.Append(" = new Uri( ");
                                compileEXPR(d.getExpression(), 0);
                                buffer.Append(" )");
                            }
                            else {
                                buffer.Append(" = ");
                                compileEXPR(d.getExpression(), 0);
                            }
                        }

                        buffer.Append(";\n");

                        ide.setFounded();
                    }
                    else {
                        if (ide.isInit())
                        {
                            addTabs(tabs);

                            Console.WriteLine( "IDE: " + ide.getName() );
                            buffer.Append(((is_global) ? "private static var " : "var ") + ide.getName() + " = ");
                            compileEXPR(d.getExpression(), 0);
                            buffer.Append(";\n\n");

                            ide.setFounded();
                        }
                        else {
                            if (ide.isModified())
                            {
                                addTabs(tabs);
                                buffer.Append(((is_global) ? "private static var " : "var ") + ide.getName() + " = " +
                                                "(" + ide.getType() + ") null;\n");

                                ide.setFounded();
                            }
                        }
                    }

                    return;

                case (Token.T_FOR):
                    addTabs(tabs);

                    For f = (For)s;

                    buffer.Append("for(");

                    compileASSIGNMENT(f.getFirstAssignment(), false, false, 0);
                    buffer.Append("; ");

                    compileEXPR(f.getGuard(), 0);
                    buffer.Append("; ");

                    compileASSIGNMENT(f.getSecondAssignment(), false, false, 0);
                    buffer.Append("){\n");

                    compileBLOCK(f.getBlock(), tabs + 1);

                    addTabs(tabs);
                    buffer.Append("}\n\n");

                    return;

                case (Token.T_WHILE):
                    addTabs(tabs);

                    While w = (While)s;

                    buffer.Append("while(");
                    compileEXPR(w.getGuard(), tabs);
                    buffer.Append("){\n");

                    compileBLOCK(w.getBlock(), tabs + 1);

                    addTabs(tabs);
                    buffer.Append("}\n\n");

                    return;

                case (Token.T_IF):
                    IfThenElse ite = (IfThenElse)s;

                    buffer.Append("\n");
                    addTabs(tabs);

                    buffer.Append("if(");

                    compileEXPR(ite.getCondition(), tabs);

                    buffer.Append("){\n");
                    compileBLOCK(ite.getThen(), tabs + 1);
                    buffer.Append("\n");
                    addTabs(tabs);
                    buffer.Append("}");

                    if (ite.getElse() != null)
                    {
                        buffer.Append("\n");
                        addTabs(tabs);
                        buffer.Append("else{\n");
                        compileBLOCK(ite.getElse(), tabs + 1);
                        buffer.Append("\n");
                        addTabs(tabs);
                        buffer.Append("}\n");
                    }

                    buffer.Append("\n");

                    return;

                case (Token.T_BLOCK):
                    addTabs(tabs);

                    buffer.Append("\n");
                    addTabs(tabs);
                    buffer.Append("{\n");
                    compileBLOCK((Block)s, tabs + 1);
                    addTabs(tabs);
                    buffer.Append("}\n\n");

                    return;

                case (Token.T_CALL):
                    addTabs(tabs);

                    Call c = (Call)s;

                    String name;
                    if (!map.TryGetValue(c.getName(), out name)) {
                        name = c.getName();
                    }

                    buffer.Append(name + "(");

                    List<IExpr> args = c.getArgs();
                    if (args != null)
                    {
                        buffer.Append(" ");

                        for (int i = 0; i < args.Count; i++)
                        {
                            buffer.Append((i == 0) ? "" : ", ");
                            compileEXPR(args[i], tabs);
                        }

                        buffer.Append(" ");
                    }

                    buffer.Append(")");

                    while ((args = c.getCall()) != null)
                    {
                        buffer.Append("( ");
                        for (int i = 0; i < args.Count; i++)
                        {
                            buffer.Append((i == 0) ? "" : ", ");
                            compileEXPR(args[i], 0);
                        }
                        buffer.Append(" )");
                    }

                    buffer.Append(";\n");

                    return;

                case (Token.T_RETURN):
                    addTabs(tabs);

                    Return r = (Return)s;

                    buffer.Append("return ");
                    compileEXPR(r.getExpr(), tabs - 1);
                    buffer.Append(";\n");

                    return;

                case (Token.T_ASYNC):
                    buffer.Append("\n");

                    addTabs(tabs);

                    buffer.Append("Task.Factory.StartNew(() =>\n");

                    addTabs(tabs + 1);
                    buffer.Append("{\n");

                    Async a = (Async)s;
                    inside_async = true;
                    compileBLOCK(a.getBlock(), tabs + 2);
                    inside_async = false;

                    buffer.Append("\n");
                    addTabs(tabs + 1);
                    buffer.Append("});\n");

                    return;

                case (Token.T_DASYNC):
                    buffer.Append("\n");

                    addTabs(tabs);

                    buffer.Append("Task.Factory.StartNew(() =>\n");

                    addTabs(tabs + 1);
                    buffer.Append("{\n");

                    DAsync da = (DAsync)s;

                    inside_async = true;

                    List<IStatement> statements = da.getStatementsList();
                    if (statements != null)
                    {
                        for (int i = 0; i < statements.Count; i++)
                        {
                            if (statements[i].getTAG() == Token.T_RETURN)
                            {
                                addTabs(tabs + 2);
                                buffer.Append("return client.getRemoteServer( " + ((Identifier)da.getRemoteAddress()).getName() + " ).");
                                compileEXPR(((Return)statements[i]).getExpr(), 0);
                                buffer.Append(";");
                            }
                            else
                                compileSTATEMENT(statements[i], tabs + 2);
                            buffer.Append("\n");
                        }
                    }

                    inside_async = false;

                    addTabs(tabs + 1);
                    buffer.Append("});\n");

                    return;
            }

            addTabs(tabs);

            compileASSIGNMENT( s, true, true, tabs );
        }

        /**
         * Compiles an expression.
         * 
         * @param e       the expression
         * @param tabs    number of tabulations
        */
        private void compileEXPR( IExpr e, int tabs )
        {
            if (e == null)
                return;

            int TAG = e.getTAG();

            switch (TAG)
            {
                case (Token.T_INT):
                    buffer.Append(((Number)e).getIntValue() + "");
                    break;

                case (Token.T_FLOAT):
                    buffer.Append((((Number)e).getFloatValue() + "f").Replace( ',', '.' ));
                    break;

                case (Token.T_DOUBLE):
                    buffer.Append((((Number)e).getDoubleValue() + "").Replace(',', '.'));
                    break;

                case (Token.T_CHAR):
                    buffer.Append("'" + ((TChar)e).getChar() + "'");
                    break;

                case (Token.T_STRING):
                    buffer.Append("\"" + ((TString)e).getString() + "\"");
                    break;

                case (Token.T_TRUE):
                    buffer.Append("true");
                    break;

                case (Token.T_FALSE):
                    buffer.Append("false");
                    break;

                case (Token.T_CAST):
                    Cast cast = (Cast)e;
                    buffer.Append("(" + cast.getType());
                    buffer.Append(") ");
                    compileEXPR(cast.getExpression(), tabs);

                    break;

                case (Token.T_IDENTIFIER):
                    Identifier ide = (Identifier)e;

                    if (ide.isWaitingAsync() && !inside_async)
                    {
                        ide.setWaitingAsync(false);
                        buffer.Append("(" + ide.getName() + " = task_" + ide.getName() + ".Result)");
                    }
                    else
                        buffer.Append(ide.getName());

                    break;

                case (Token.T_CALL):
                    Call c = (Call)e;

                    String name;
                    if (!map.TryGetValue( c.getName(), out name )) {
                        name = c.getName();
                    }

                    buffer.Append(name + "(");

                    List<IExpr> args = c.getArgs();
                    if (args != null)
                    {
                        buffer.Append(" ");

                        for (int i = 0; i < args.Count; i++)
                        {
                            buffer.Append((i == 0) ? "" : ", ");
                            compileEXPR(args[i], tabs);
                        }

                        buffer.Append(" ");
                    }

                    buffer.Append(")");

                    while ((args = c.getCall()) != null)
                    {
                        buffer.Append("( ");
                        for (int i = 0; i < args.Count; i++)
                        {
                            buffer.Append((i == 0) ? "" : ", ");
                            compileEXPR(args[i], 0);
                        }
                        buffer.Append(" )");
                    }

                    break;

                case (Token.T_ASYNC):
                    buffer.Append("Task.Factory.StartNew(() =>\n");

                    addTabs(tabs + 1);
                    buffer.Append("{\n");

                    Async a = (Async)e;
                    inside_async = true;
                    compileBLOCK(a.getBlock(), tabs + 2);
                    inside_async = false;

                    buffer.Append("\n");
                    addTabs(tabs + 1);
                    buffer.Append("})");

                    break;

                case (Token.T_DASYNC):
                    buffer.Append("Task.Factory.StartNew(() =>\n");

                    addTabs(tabs + 1);
                    buffer.Append("{\n");

                    DAsync da = (DAsync)e;

                    inside_async = true;

                    List<IStatement> statements = da.getStatementsList();
                    if (statements != null)
                    {
                        for (int i = 0; i < statements.Count; i++)
                        {
                            if (statements[i].getTAG() == Token.T_RETURN)
                            {
                                addTabs(tabs + 2);
                                buffer.Append("return client.getRemoteServer( " + ((Identifier)da.getRemoteAddress()).getName() + " ).");
                                compileEXPR(((Return)statements[i]).getExpr(), 0);
                                buffer.Append(";");
                            }
                            else
                                compileSTATEMENT(statements[i], tabs + 2);
                            buffer.Append("\n");
                        }
                    }

                    inside_async = false;

                    addTabs(tabs + 1);
                    buffer.Append("})");

                    break;

                case (Token.T_FUN):
                    Function f = (Function)e;

                    Types type = f.getReturnType();
                    if (type.getType() == Token.T_VOID)
                        buffer.Append("new Action<");
                    else
                        buffer.Append("new Func<");

                    List<Identifier> parameters = f.getParams();
                    int size = 0;
                    if (parameters != null)
                    {
                        size = parameters.Count;
                        for (int i = 0; i < size; i++)
                            buffer.Append(((i > 0) ? ", " : "") + parameters[i].getType());
                    }

                    if (type.getType() != Token.T_VOID)
                        buffer.Append(((size > 0) ? ", " : "") + type);

                    buffer.Append(">((");

                    if (parameters != null)
                    {
                        for (int i = 0; i < parameters.Count; i++)
                        {
                            if (i > 0) buffer.Append(", ");

                            compileEXPR(parameters[i], tabs + 1);
                        }
                    }

                    buffer.Append(") =>\n");
                    addTabs(tabs + 3);
                    buffer.Append("{\n");

                    bool backup_global = is_global;
                    is_global = false;
                    compileBLOCK(f.getBlock(), tabs + 4);
                    is_global = backup_global;

                    buffer.Append("\n");
                    addTabs(tabs + 3);
                    buffer.Append("})");

                    break;

                case (Token.T_INCR):
                case (Token.T_DECR):
                    Expr expr = (Expr)e;
                    bool par = e.getParenthesis();

                    if (par)
                        buffer.Append("(");

                    compileEXPR(expr.getExpr1(), tabs);

                    buffer.Append(Token.getTokenValue(TAG));

                    if (par)
                        buffer.Append(")");

                    break;

                case (Token.T_AND):
                case (Token.T_OR):
                case (Token.T_NOT):
                case (Token.T_EQUALS):
                case (Token.T_DIFFERENT):
                case (Token.T_GREATER):
                case (Token.T_GREATER_OR_EQUAL):
                case (Token.T_LESS):
                case (Token.T_LESS_OR_EQUAL):
                case (Token.T_PLUS):
                case (Token.T_PLUS_EQUALS):
                case (Token.T_MINUS):
                case (Token.T_MINUS_EQUALS):
                case (Token.T_DIVIDE):
                case (Token.T_DIVIDE_EQUALS):
                case (Token.T_MULTIPLY):
                case (Token.T_MULTIPLY_EQUALS):
                    expr = (Expr)e;
                    par = e.getParenthesis();

                    if (par)
                        buffer.Append("(");

                    compileEXPR(expr.getExpr1(), tabs);

                    buffer.Append(" " + Token.getTokenValue(TAG) + " ");

                    compileEXPR(expr.getExpr2(), tabs);

                    if (par)
                        buffer.Append(")");

                    break;
            }
        }

        /**
         * Compiles an assignment.
         * 
         * @param s            the statement
         * @param new_lines    TRUE to add a new line after the statement, FALSE otherwise
         * @param semicolon    TRUE to add a semicolon after the statement, FALSE otherwise
         * @param tabs         number of tabulations
        */
        private void compileASSIGNMENT( IStatement s, bool new_lines, bool semicolon, int tabs )
        {
            if (s == null) {
                return;
            }

            int TAG = s.getTAG();
            switch (TAG) {
                case (Token.T_ASSIGN):
                    Assignment a = (Assignment)s;
                    TAG = a.getExpression().getTAG();
                    Identifier ide = a.getIDE();

                    if (TAG == Token.T_ASYNC || TAG == Token.T_DASYNC) {
                        inside_async = true;

                        buffer.Append( "\n" );
                        addTabs( tabs );
                        buffer.Append( "var task_" + ide.getName() + " = " );

                        compileEXPR( a.getExpression(), tabs );

                        ide.setWaitingAsync( true );

                        inside_async = false;
                    } else {
                        if (!ide.isFounded() && !ide.isInit()) {
                            buffer.Append("var ");
                            ide.setFounded();
                        }

                        if (ide.getType().getType() == Token.T_URL) {
                            buffer.Append( ide.getName() + " = new Uri( " );
                            compileEXPR( a.getExpression(), tabs );
                            buffer.Append( " )" );
                        } else {
                            buffer.Append( ide.getName() + " = " );
                            compileEXPR( a.getExpression(), tabs );
                        }
                    }

                    break;

                case (Token.T_INCR):
                case (Token.T_DECR):
                    a = (Assignment)s;
                    ide = a.getIDE();

                    // if the variable is waiting the result we transform the assignment in a +- 1
                    if (ide.isWaitingAsync() && !inside_async) {
                        ide.setWaitingAsync( false );
                        buffer.Append( ide.getName() + " = (" + ide.getName() + " = task_" + ide.getName() + ".Result) " + Token.getTokenValue( TAG ).Substring( 0, 1 ) + " 1" );
                    } else {
                        buffer.Append( ide.getName() + Token.getTokenValue( TAG ) );
                    }

                    break;

                case (Token.T_PLUS_EQUALS):
                case (Token.T_MINUS_EQUALS):
                case (Token.T_DIVIDE_EQUALS):
                case (Token.T_MULTIPLY_EQUALS):
                    a = (Assignment)s;
                    ide = a.getIDE();

                    if (!ide.isFounded() && !ide.isInit()) {
                        buffer.Append("var ");
                        ide.setFounded();
                        buffer.Append( ide.getName() + " " + Token.getTokenValue( TAG ) + " " );
                    } else {
                        if (ide.isWaitingAsync() && !inside_async) {
                            ide.setWaitingAsync( false );
                            // First we perform the assignment of the value.
                            buffer.Append( ide.getName() + " = task_" + ide.getName() + ".Result;\n" );
                            addTabs( tabs );

                            // Then we perform the statement.
                            buffer.Append( ide.getName() + " " + Token.getTokenValue( TAG ) + " " );
                        } else {
                            buffer.Append( ide.getName() + " " + Token.getTokenValue( TAG ) + " " );
                        }
                    }

                    compileEXPR( a.getExpression(), tabs );

                    break;
            }

            if (semicolon) {
                buffer.Append(";");
            }

            if (new_lines) {
                buffer.Append("\n");
            }
        }

        /**
         * Inserts tabs in the code.
         * 
         * @param tabs - number of tabs
        */
        private void addTabs( int tabs )
        {
            for (int i = 0; i < tabs; i++) {
                buffer.Append( "\t" );
            }
        }
    }
}