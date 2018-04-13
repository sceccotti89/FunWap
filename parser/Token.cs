/**
* @author Stefano Ceccotti
*/

using System;

namespace Funwap.parser
{
    public static class Token
    {
        public const int
                // operational tokens

                T_ASSIGN = '=', // 61
                T_DIVIDE = '/',
                T_MULTIPLY = '*', // 42
                T_PLUS = '+', // 43
                T_MINUS = '-', // 45
                T_INCR = 0,
                T_DECR = 1,
                T_PLUS_EQUALS = 2,
                T_MINUS_EQUALS = 3,
                T_DIVIDE_EQUALS = 4,
                T_MULTIPLY_EQUALS = 5,
                T_EQUALS = 6,
                T_LESS = '<', // 60
                T_LESS_OR_EQUAL = 7,
                T_GREATER = '>', // 62
                T_GREATER_OR_EQUAL = 8,
                T_DIFFERENT = 9,

                // boolean tokens

                T_AND = 10,
                T_OR = 11,
                T_NOT = '!', // 33
                T_TRUE = 12,
                T_FALSE = 13,

                // command tokens

                T_WHILE = 14,
                T_FOR = 15,
                T_IF = 16,
                T_ELSE = 17,
                T_ASYNC = 18,
                T_DASYNC = 19,
                T_RETURN = 20,
                T_READLN = 21,
                T_PRINTLN = 22,
                T_CALL = 23,

                // miscellaneous tokens

                T_SEMICOLON = ';', // 59
                T_COMMA = ',', // 44
                T_OPEN_BRACKET = '{', // 123
                T_CLOSED_BRACKET = '}', // 125
                T_OPEN_PARENTHESIS = '(', // 40
                T_CLOSED_PARENTHESIS = ')', // 41
                T_IDENTIFIER = 24,
                T_VAR = 25,
                T_MAIN = 26,
                T_COMMENT = 27,
                T_BLOCK = 28,
                T_CAST = 29,

                // type tokens

                T_INT = 30,
                T_FLOAT = 31,
                T_DOUBLE = 32,
                T_CHAR = 34,
                T_STRING = 35,
                T_BOOL = 36,
                T_URL = 37,
                T_FUN = 38,
                T_VOID = 39,

                // exception tokens

                T_EOF = 46,
                T_UNKNOWN = 47;

        /* One of the token codes from above. */
        public static int? type;
        /* Holds value if string/identifier. */
        public static String stringValue = null;
        /* Holds value if integer. */
        public static int? intValue = null;
        /* Holds value if float. */
        public static float? floatValue = null;
        /* Holds value if double. */
        public static double? doubleValue = null;

        /**
         * Returns the string value of a token.
         * 
         * @param TAG    ID of the token
         * 
         * @return the string representation
        */
        public static String getTokenValue( int TAG )
        {
            switch (TAG) {
                case (T_ASSIGN): return "=";
                case (T_DIVIDE): return "/";
                case (T_MULTIPLY): return "*";
                case (T_PLUS): return "+";
                case (T_MINUS): return "-";
                case (T_INCR): return "++";
                case (T_DECR): return "--";
                case (T_PLUS_EQUALS): return "+=";
                case (T_MINUS_EQUALS): return "-=";
                case (T_DIVIDE_EQUALS): return "/=";
                case (T_MULTIPLY_EQUALS): return "*=";
                case (T_EQUALS): return "==";
                case (T_LESS): return "<";
                case (T_LESS_OR_EQUAL): return "<=";
                case (T_GREATER): return ">";
                case (T_GREATER_OR_EQUAL): return ">=";
                case (T_DIFFERENT): return "!=";

                // Boolean tokens.

                case (T_AND): return "&&";
                case (T_OR): return "||";
                case (T_NOT): return "!";
                case (T_TRUE): return "true";
                case (T_FALSE): return "false";

                // Command tokens.

                case (T_WHILE): return "while";
                case (T_FOR): return "for";
                case (T_IF): return "if";
                case (T_ELSE): return "else";
                case (T_ASYNC): return "async";
                case (T_DASYNC): return "dasync";
                case (T_RETURN): return "return";
                case (T_READLN): return "readln";
                case (T_PRINTLN): return "println";
                case (T_CALL): return "call";

                // Miscellaneous tokens.

                case (T_SEMICOLON): return ";";
                case (T_COMMA): return ",";
                case (T_OPEN_BRACKET): return "{";
                case (T_CLOSED_BRACKET): return "}";
                case (T_OPEN_PARENTHESIS): return "(";
                case (T_CLOSED_PARENTHESIS): return ")";
                case (T_IDENTIFIER): return "Identifier";
                case (T_VAR): return "var";
                case (T_MAIN): return "main";
                case (T_COMMENT): return "comment";
                case (T_BLOCK): return "block";
                case (T_CAST): return "cast";

                // Type tokens.

                case (T_INT):    return "int";
                case (T_FLOAT):  return "float";
                case (T_DOUBLE): return "double";
                case (T_CHAR):   return "char";
                case (T_STRING): return "string";
                case (T_BOOL):   return "bool";
                case (T_URL):    return "url";
                case (T_FUN):    return "fun";
                case (T_VOID):   return "void";
            }

            return null;
        }
    }
}
