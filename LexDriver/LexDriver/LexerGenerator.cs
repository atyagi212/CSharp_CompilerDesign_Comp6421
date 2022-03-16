using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexDriver
{
    public class LexerGenerator
    {
        public static char currentChar = ' ';
        public static int currentCharIndex = 0;
        public static int line = 1;
        public Token TokenGenerationbyReadingCharacters()
        {
            for (; ; ReadCharcter())
            {
                if (currentCharIndex == Common.fileContent.Length)
                {
                    Common.continueRead = false;
                    break;
                }
                else if (currentChar == ' ' || currentChar == '\t') continue;
                else if (currentChar == '\n') line = line + 1;
                else if ((int)currentChar == 65535) return null;
                else break;
            }

            switch (currentChar)
            {
                case '=':
                    if (ReadCharcter('='))
                    {
                        ReadCharcter();
                        return new Token("eq", "==", line);
                    }
                    else
                    {
                        return new Token("assign", "=", line);
                    }
                case '<':
                    ReadCharcter();
                    if (currentChar == '=')
                    {
                        ReadCharcter();
                        return new Token("leq", "<=", line);
                    }
                    else if (currentChar == '>')
                    {
                        ReadCharcter();
                        return new Token("noteq", "<>", line);
                    }
                    else return new Token("lt", "<", line);
                case '>':
                    if (ReadCharcter('='))
                    {
                        ReadCharcter();
                        return new Token("geq", ">=", line);
                    }
                    else
                    {
                        return new Token("gt", ">", line);
                    }
                case ':':
                    if (ReadCharcter(':'))
                    {
                        ReadCharcter();
                        return new Token("coloncolon", "::", line);
                    }
                    else
                    {
                        return new Token("colon", ":", line);
                    }
                case '+':
                    ReadCharcter(); return new Token("plus", "+", line);
                case '-':
                    if (ReadCharcter('>'))
                    {
                        ReadCharcter();
                        return new Token("arrow", "->", line);
                    }
                    else
                    {
                        return new Token("minus", "-", line);
                    }
                case '*':
                    ReadCharcter(); return new Token("mult", "*", line);
                case '/':
                    ReadCharcter();
                    if (currentChar == '/')
                    {
                        string inLineComment = "//";
                        while (!ReadCharcter('\n'))
                        {
                            inLineComment = inLineComment + currentChar.ToString();
                        }
                        int startLine = line;
                        line++;
                        return new Token("inlinecmt", inLineComment.ToString(), startLine);
                    }
                    else if (currentChar == '*')
                    {
                        int open = 1;
                        string blockComment = "/*";
                        while (true)
                        {
                            ReadCharcter();
                            blockComment = blockComment + currentChar.ToString();

                            string cmtstr = blockComment.ToString();
                            if (currentChar == '/' && cmtstr[cmtstr.Length - 2] == '*')
                            {
                                open--;
                            }
                            else if (currentChar == '*' && cmtstr[cmtstr.Length - 2] == '/')
                            {
                                open++;
                            }
                            if (open == 0)
                            {
                                ReadCharcter();
                                int startLine = line;
                                foreach (char ch in blockComment.ToString().ToCharArray())
                                {
                                    if (ch == '\n') line++;
                                }
                                return new Token("blockcmt", blockComment.ToString(), startLine);
                            }
                        }
                    }
                    else
                    {
                        ReadCharcter(); return new Token("div", "/", line);
                    }
                case '(':
                    ReadCharcter(); return new Token("openpar", "(", line);
                case ')':
                    ReadCharcter(); return new Token("closepar", ")", line);
                case '{':
                    ReadCharcter(); return new Token("opencubr", "{", line);
                case '}':
                    ReadCharcter(); return new Token("closecubr", "}", line);
                case '[':
                    ReadCharcter(); return new Token("opensqbr", "[", line);
                case ']':
                    ReadCharcter(); return new Token("closesqbr", "]", line);
                case '.':
                    ReadCharcter(); return new Token("dot", ".", line);
                case ',':
                    ReadCharcter(); return new Token("comma", ",", line);
                case ';':
                    ReadCharcter(); return new Token("semi", ";", line);
                case '|':
                    ReadCharcter(); return new Token("or", "|", line);
                case '&':
                    ReadCharcter(); return new Token("and", "&", line);
                case '!':
                    ReadCharcter(); return new Token("not", "!", line);
            }

            if (Common.lstDigits.Contains(currentChar.ToString().ToLower()))
            {
                string num = "";
                if (currentChar == '0')
                {
                    num = num + currentChar.ToString();
                    ReadCharcter();
                    if (currentChar != '.')
                    {
                        return new Token("intnum", "0", line);
                    }
                }
                if (currentChar != '.')
                {
                    do
                    {
                        num = num + currentChar.ToString();
                        ReadCharcter();
                    } while (Common.lstDigits.Contains(currentChar.ToString().ToLower()));

                }

                if (currentChar != '.')
                {
                    return new Token("intnum", num.ToString(), line);
                }
                else
                {
                    do
                    {
                        num = num + currentChar.ToString();
                        ReadCharcter();
                    } while (Common.lstDigits.Contains(currentChar.ToString().ToLower()));
                    if (currentChar == 'e' && (num[num.Length - 1] != '0' || (num[num.Length - 1] == '0' && num[num.Length - 2] == '.')))
                    {
                        num = num + currentChar.ToString();
                        ReadCharcter();
                        if (currentChar == '+' || currentChar == '-' || Common.lstDigits.Contains(currentChar.ToString().ToLower()))
                        {
                            num = num + currentChar.ToString();
                            if (currentChar == '0')
                            {
                                ReadCharcter();
                                return new Token("floatnum", num.ToString(), line);
                            }
                            ReadCharcter();
                            if (Common.lstDigits.Contains(currentChar.ToString().ToLower()))
                            {
                                if (currentChar == '0' && (num[num.Length - 1] == '+' || num[num.Length - 1] == '-'))
                                {
                                    num = num + currentChar.ToString();
                                    ReadCharcter();
                                    return new Token("floatnum", num.ToString(), line);
                                }
                                do
                                {
                                    num = num + currentChar.ToString();
                                    ReadCharcter();
                                } while (Common.lstDigits.Contains(currentChar.ToString().ToLower()));
                                return new Token("floatnum", num.ToString(), line);
                            }
                        }
                    }
                    if (num[num.Length - 1] != '0')
                    {
                        return new Token("floatnum", num.ToString(), line);
                    }
                    else if (num.Length >= 2 && num[num.Length - 1] == '0' && num[num.Length - 2] == '.')
                    {
                        return new Token("floatnum", num.ToString(), line);
                    }
                }
                return new Token("invalidnum", num.ToString(), line);
            }

            if (Common.lstAlphabets.Contains(currentChar.ToString().ToLower()))
            {
                string b = "";
                do
                {
                    b = b + currentChar.ToString();
                    ReadCharcter();
                } while (Common.lstAlphabets.Contains(currentChar.ToString().ToLower()) || Common.lstDigits.Contains(currentChar.ToString().ToLower()) || currentChar == '_' || Common.lstDigits.Contains(currentChar.ToString().ToLower()));
                string s = b.ToString();
                if (Common.lstReserverdKeyWords.Contains(s))
                {
                    return new Token(s, s, line);
                }
                return new Token("id", s, line);
            }

            if ((int)currentChar == 13)
            {
                ReadCharcter();
                return TokenGenerationbyReadingCharacters();
            }
            Token t = new Token("invalidchar", currentChar + "", line);
            currentChar = ' ';
            return t;
        }

        private void ReadCharcter()
        {
            if (currentCharIndex < Common.fileContent.Length)
            {
                currentChar = Common.fileContent[currentCharIndex];
                currentCharIndex = currentCharIndex + 1;
            }
            else
            {
                Common.continueRead = false;
            }
        }

        private bool ReadCharcter(char c)
        {
            ReadCharcter();
            if (currentChar != c)
                return false;
            currentChar = ' ';
            return true;
        }

        public static void Initializer()
        {
            currentChar = ' ';
            currentCharIndex = 0;
            line = 1;
            Common.continueRead = true;
            SyntaxAnalysis.st = new Stack<ASTNode>();
        }
    }
}