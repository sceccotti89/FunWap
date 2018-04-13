/**
* @author Stefano Ceccotti
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Funwap.parser;

namespace Funwap.engine
{
    public class Tokenizer
    {
        /* lettore dello stream */
        private StreamReader reader;
        /* table maps used to reserve words for token type */
        private Dictionary<String, Nullable<int>> table;
        /* the current line of code */
        private static int line = 1;
        /* the next character read */
        private char c;

        /* the regular expressions */
        /*private static final String REGEX[] = {
                                                "[A-z][0|A-z]*",        // IDENTIFIER
                                                "[0-9]+",                // INTEGER
                                                "[0-9]*(\\.[0-9]*f)",    // FLOAT
                                                "[0-9]*\\.[0-9]*"        // DOUBLE
                                              };*/

        public Tokenizer(String file_path)
        {
            String file;
            int index = file_path.LastIndexOf( '/' );
            if (index != -1) {
                file = file_path.Substring( index + 1 );
            } else {
                file = file_path;
            }

            index = file.IndexOf( '.' );
            if (index == -1) {
                throw new Exception( "The input file requires the extension." );
            }

            if (!file.Substring( file.IndexOf( '.' ) + 1 ).Equals( "funwap" )) {
                throw new Exception( "The input file doesn't have the funwap extension." );
            }

            reader = new StreamReader( file_path );

            table = new Dictionary<String, Nullable<int>>( 32 );
            init_table();
        }

        /** initialize the table maps with tokens type */
        private void init_table()
        {
            table.Add("async", Token.T_ASYNC);
            table.Add("dasync", Token.T_DASYNC);
            table.Add("for", Token.T_FOR);
            table.Add("if", Token.T_IF);
            table.Add("else", Token.T_ELSE);
            table.Add("while", Token.T_WHILE);
            table.Add("var", Token.T_VAR);
            table.Add("fun", Token.T_FUN);
            table.Add("main", Token.T_MAIN);
            table.Add("return", Token.T_RETURN);
            table.Add("true", Token.T_TRUE);
            table.Add("false", Token.T_FALSE);
            table.Add("int", Token.T_INT);
            table.Add("float", Token.T_FLOAT);
            table.Add("double", Token.T_DOUBLE);
            table.Add("bool", Token.T_BOOL);
            table.Add("char", Token.T_CHAR);
            table.Add("string", Token.T_STRING);
            table.Add("url", Token.T_URL);
            table.Add("void", Token.T_VOID);
        }

        /** Returns the current line. */
        public static int getLine()
        {
            return line;
        }

        /**
         * Checks whether the next token is an identifier or a number.
         * 
         * @return TRUE if an identifier or a number is found, FALSE otherwise
        */
        private bool isIdeOrNumber()
        {
            StringBuilder ide = new StringBuilder(16);
            bool is_number = true;
            bool number_founded = false;
            bool point = false;
            bool is_float = false;

            c = reader.peek();

            // check if the first element is a character or a number
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_')
            {
                ide.Append(c);
                is_number = false;
            }
            else {
                if ((c >= '0' && c <= '9') || c == '.')
                {
                    if (c == '.') point = true;
                    ide.Append(c);
                    number_founded = true;
                }
                else {
                    return false;
                }
            }

            reader.consume();

            while (!reader.isEoF())
            {
                c = reader.peek();

                // check if it is a character or a number
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_')
                {
                    if (is_number)
                    {
                        ide.Append(c);

                        // it may be a float
                        if (c == 'f')
                        {
                            if (is_float)
                            {
                                throw new Exception("Error line " + getLine() + ": duplication of token \"f\".");
                            }
                            else {
                                is_float = true;
                            }
                        }
                        else {
                            throw new Exception("Error line " + getLine() + ": syntax error on token \"" + c + "\", delete this token.");
                        }
                    }
                    else {
                        ide.Append(c);
                    }
                }
                else {
                    if ((c >= '0' && c <= '9') || c == '.')
                    {
                        ide.Append(c);

                        // it may be a double or a float
                        if (c == '.')
                        {
                            if (!is_number)
                            {
                                throw new Exception("Error line " + getLine() + ": syntax error on token \"" + c + "\", delete this token.");
                            }

                            if (point)
                            {
                                throw new Exception("Error line " + getLine() + ": duplication of token \".\".");
                            }
                            else {
                                point = true;
                            }
                        }

                        if (!number_founded)
                        {
                            number_founded = true;
                        }
                    }
                    else {
                        break;
                    }
                }

                reader.consume();
            }

            if (ide.Length > 0) {
                Token.stringValue = ide.ToString();
                if (is_number) {
                    if (!number_founded) {
                        reader.rewind( 1 );
                        return false;
                    }

                    if (is_float) {
                        Token.type = Token.T_FLOAT;
                        String number = Token.stringValue;
                        Token.floatValue = float.Parse( number.Substring( 0, number.Length - 1 ), CultureInfo.InvariantCulture.NumberFormat);
                    } else {
                        if (point) {
                            Token.type = Token.T_DOUBLE;
                            Token.doubleValue = double.Parse(Token.stringValue, CultureInfo.InvariantCulture.NumberFormat);
                        } else {
                            Token.type = Token.T_INT;
                            Token.intValue = int.Parse(Token.stringValue, CultureInfo.InvariantCulture.NumberFormat);
                        }
                    }
                } else {
                    // It's an identifier.
                    if (!table.TryGetValue( Token.stringValue, out Token.type )) {
                        Token.type = Token.T_IDENTIFIER;
                    } else {
                        Token.stringValue = null;
                    }
                }

                return true;
            }
            else {
                return false;
            }
        }

        /**
         * Checks whether the next token is a char.
         * 
         * @return TRUE if it's a char, FALSE otherwise
        */
        private bool isChar()
        {
            if (reader.peek() == '\'')
            {
                reader.consume();

                if (reader.isEoF() || (c = reader.next()) == '\n' || c == '\'')
                {
                    throw new Exception("Error line " + getLine() + ": invalid character constant");
                }

                Token.stringValue = c + "";

                if (reader.next() != '\'')
                {
                    throw new Exception("Error line " + getLine() + ": invalid character constant");
                }

                return true;
            }

            return false;
        }

        /** checks if the next token is a string
         * 
         * @return TRUE if it's a string, FALSE otherwise
        */
        private bool isString()
        {
            if (reader.peek() == '"')
            {
                reader.consume();

                StringBuilder buffer = new StringBuilder(16);
                while (!reader.isEoF() && (c = reader.next()) != '"' && c != '\n')
                {
                    buffer.Append(c);
                }

                if (c != '"')
                {
                    throw new Exception("Error line " + getLine() + ": string literal is not properly closed by a double-quote");
                }

                Token.stringValue = buffer.ToString();

                return true;
            }

            return false;
        }

        /** Reads and ignores all the white spaces, new lines and tabs. */
        private void whiteSpaces()
        {
            while (!reader.isEoF())
            {
                c = reader.peek();
                if (c == ' ' || c == '\n' || c == '\t')
                {
                    if (c == '\n')
                    {
                        line++;
                    }
                    reader.consume();
                }
                else {
                    break;
                }
            }
        }

        /** Skips the comments on the code. */
        private void skipComments()
        {
            Token.stringValue = "";

            if (c == '/')
            {
                while (!reader.isEoF() && (c = reader.next()) != '\n')
                {
                    Token.stringValue = Token.stringValue + c;
                }

                line++;
            }
            else {
                while (!reader.isEoF())
                {
                    if ((c = reader.next()) == '*')
                    {
                        if (reader.isEoF())
                        {
                            throw new Exception("Error line " + getLine() + ": syntax error, insert \"/\" to complete the comment");
                        }

                        char ch;
                        if ((ch = reader.peek()) == '/')
                        {
                            reader.consume();
                            return;
                        }
                        else {
                            reader.consume();

                            if (ch == '\n')
                            {
                                line++;
                            }

                            Token.stringValue = Token.stringValue + c;
                        }
                    }
                    else {
                        if (c == '\n')
                        {
                            line++;
                        }

                        Token.stringValue = Token.stringValue + c;
                    }
                }

                throw new Exception("Error line " + getLine() + ": syntax error, insert \"*/\" to complete the comment");
            }
        }

        /***/
        /*private String getString() throws IOException
        {
            StringBuilder buffer = new StringBuilder( 16 );

            // TODO se non c'e' uno spazio o un a capo non funziona
            while((c = reader.peek()) != ' ' && c != '\n'){
                buffer.append( c );
                reader.consume();
            }

            return buffer.toString();
        }*/

        /** returns the next token of the stream
         * 
         * @return the next token
        */
        public int nextToken()
        {
            whiteSpaces();

            if (reader.isEoF())
            {
                return Token.T_EOF;
            }

            if (isIdeOrNumber())
            {
                return (int)Token.type;
            }

            if (isChar())
            {
                return Token.T_CHAR;
            }

            if (isString())
            {
                return Token.T_STRING;
            }

            c = reader.next();
            if (reader.isEoF())
            {
                return Token.T_EOF;
            }

            switch (c)
            {
                case ('/'):
                    c = reader.peek();
                    if (c == '/' || c == '*')
                    {
                        reader.consume();
                        skipComments();
                        return Token.T_COMMENT;
                    }
                    else {
                        if (c == '=')
                        {
                            reader.consume();
                            return Token.T_DIVIDE_EQUALS;
                        }
                        else {
                            return Token.T_DIVIDE;
                        }
                    }

                case ('*'):
                    if (reader.peek() == '=')
                    {
                        reader.consume();
                        return Token.T_MULTIPLY_EQUALS;
                    }
                    else {
                        return c;
                    }

                case ('+'):
                    if ((c = reader.peek()) == '+')
                    {
                        reader.consume();
                        return Token.T_INCR;
                    }
                    else {
                        if (c == '=')
                        {
                            reader.consume();
                            return Token.T_PLUS_EQUALS;
                        }
                        else {
                            return Token.T_PLUS;
                        }
                    }

                case ('-'):
                    if ((c = reader.peek()) == '-')
                    {
                        reader.consume();
                        return Token.T_DECR;
                    }
                    else {
                        if (c == '=')
                        {
                            reader.consume();
                            return Token.T_MINUS_EQUALS;
                        }
                        else {
                            return Token.T_MINUS;
                        }
                    }

                case ('='):
                    if (reader.peek() == '=')
                    {
                        reader.consume();
                        return Token.T_EQUALS;
                    }
                    else {
                        return c;
                    }

                case ('!'):
                    if (reader.peek() == '=')
                    {
                        reader.consume();
                        return Token.T_DIFFERENT;
                    }
                    else {
                        return c;
                    }

                case ('|'):
                    if ((c = reader.peek()) == '|')
                    {
                        reader.consume();
                        return Token.T_OR;
                    }
                    else {
                        throw new Exception("Error line " + getLine() + ": invalid Character: '" + c + "'");
                    }

                case ('&'):
                    if ((c = reader.peek()) == '&')
                    {
                        reader.consume();
                        return Token.T_AND;
                    }
                    else {
                        throw new Exception("Error line " + getLine() + ": invalid Character: '" + c + "'");
                    }

                case ('<'):
                    if ((c = reader.peek()) == '=')
                    {
                        reader.consume();
                        return Token.T_LESS_OR_EQUAL;
                    }
                    else {
                        return Token.T_LESS;
                    }

                case ('>'):
                    if ((c = reader.peek()) == '=')
                    {
                        reader.consume();
                        return Token.T_GREATER_OR_EQUAL;
                    }
                    else {
                        return Token.T_GREATER;
                    }

                case (';'):
                case (','):
                case ('('):
                case (')'):
                case ('{'):
                case ('}'):
                    return c;

                default:
                    // TODO inserire qui i controlli
                    // TODO prima pero' bisogna ottenere la stringa (e' qui il vero problema)
                    //String token = getString();

                    // identifier
                    /*if(Pattern.matches( REGEX[0], token ))
                        ;*/

                    // integer
                    /*if(Pattern.matches( REGEX[1], token ))
                        ;*/

                    // float
                    /*if(Pattern.matches( REGEX[2], token ))
                        ;*/

                    // double
                    /*if(Pattern.matches( REGEX[3], token ))
                        ;*/
                    throw new Exception("Error line " + getLine() + ": invalid Character: '" + c + "'");
            }
        }
    }
}
