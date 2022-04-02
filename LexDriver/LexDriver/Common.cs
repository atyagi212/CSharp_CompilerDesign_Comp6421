using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexDriver
{
    public static class Common
    {
        public static string fileContent = string.Empty;
        public static List<string> lstDigits = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        public static List<string> lstAlphabets = new List<string> { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
        public static List<string> lstReserverdKeyWords = new List<string> { "if", "public", "read", "then", "private", "write", "else", "func", "return", "integer", "var", "self", "float", "struct", "inherits", "void", "while", "let", "func", "impl" };

        public static bool continueRead = true;
        internal static string tokenContent;
        public static string[] arrString;
        public static List<string> lstIgnoreToken = new List<string> { "inlinecmt", "blockcmt" };

        public static Dictionary<string, int> dictMemSize = new Dictionary<string, int>() { { "integer", 4 }, { "float", 8 } };
        public static Dictionary<string, string> dictOprName = new Dictionary<string, string>() { { "+", "add" }, { "-", "sub" }, { "*", "mul" }, { "/", "div" } };
        public static Dictionary<string, string> dictCompareOpr = new Dictionary<string, string>() { { " and ", "and" }, { " or ", "or" }, { " not ", "not" }, { " == ", "ceq" }, { " != ", "cne" }, { " <= ", "cle" }, { " < ", "clt" }, { " >= ", "cge" }, { " > ", "cgt" }, };
    }
}
