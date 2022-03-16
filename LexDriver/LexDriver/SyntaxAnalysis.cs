using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexDriver
{
    public class SyntaxAnalysis
    {
        private string lookahead = null;
        private int currentTokenIndex = 0;
        List<string> tokenTypesToIgnore = new List<string>();
        Token token = null;
        private string fileName = string.Empty;

        Node root = null;
        public static Stack<ASTNode> st = new Stack<ASTNode>();
        Token prevToken = null;

        public bool PushInStack(string label)
        {
            //st.Push(new ASTNode(label));
            if (prevToken != null) st.Push(new ASTNode(label, prevToken._content));
            else st.Push(new ASTNode(label));
            return true;
        }

        public bool PopFromStack(string label, int noOfChildren)
        {
            //ASTNode parent = new ASTNode(label);
            ASTNode parent = new ASTNode(label, token._content);
            Console.WriteLine("PopFromStack: " + label + ": ");
            while (noOfChildren > 0 && st.Count() > 0)
            {
                parent.Children.Add(st.Peek());
                Console.WriteLine(st.Peek().label + " ");
                st.Pop();
                noOfChildren--;
            }
            Console.WriteLine();
            st.Push(parent);
            return true;
        }

        public bool PopFromStackUntilEpsilon(string label)
        {
            //ASTNode parent = new ASTNode(label);
            ASTNode parent = new ASTNode(label, token._content);
            Console.WriteLine("PopFromStackUntilEpsilon: " + label + ": ");
            while (!st.Peek().label.Equals("epsilon"))
            {
                parent.Children.Add(st.Peek());
                Console.WriteLine(st.Peek().label + " ");
                st.Pop();
            }
            st.Pop();
            Console.WriteLine();
            st.Push(parent);
            return true;
        }

        public static void printAST(ASTNode Node, int depth, string fileName)
        {
            for (int i = 0; i < depth; i++)
                File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outast"), "|\t");
            //File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outast"), Node.label + "\n");
            File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outast"), Node.label + " - " + Node.value + "\n");
            foreach (ASTNode child in Node.Children)
            {
                printAST(child, depth + 1, fileName);
            }
        }
        public SyntaxAnalysis()
        {
            tokenTypesToIgnore.Add("inlinecmt");
            tokenTypesToIgnore.Add("blockcmt");

        }

        public bool Parse(string fileName)
        {
            this.fileName = fileName;
            try
            {
                GetNextToken();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            if (START())
                return true;
            else
                return false;
        }

        public void writeOutDerivation(string str)
        {

            try
            {
                WriteDerivation(str + "\n");
                Console.WriteLine(str + "\n");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }
        private void writeOutDerivationNoLineBreak(string str)
        {
            try
            {
                WriteDerivation(str);
                Console.WriteLine(str);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        public void writeOutSyntaxErrors(string str)
        {
            try
            {
                WriteSyntaxErrors(str + "\n");
                Console.WriteLine(str + "\n");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        private bool Match(string v)
        {
            if (lookahead.Equals(v))
            {
                writeOutDerivationNoLineBreak(lookahead + " ");
                try
                {
                    GetNextToken();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
                return true;
            }
            else
            {
                writeOutSyntaxErrors("Syntax error at line " + token._lineNumber + ", expected token: " + v);
                try
                {
                    GetNextToken();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
                return false;
            }
        }



        private bool START()
        {
            List<string> firstSet = new List<string> { "func", "impl", "struct" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, true)) return false;
            bool error = false;
            if (firstSet.Contains(lookahead))
            {
                if (PROG() && PopFromStack("prog", 1))
                {
                    writeOutDerivation("START -> PROG");
                }
                //if (PROG())
                //    {}//writeOutDerivation("START -> PROG");
                else
                    error = true;
            }
            else
                error = true;
            return !error;
        }

        private bool skipErrors(List<string> firstSet, List<string> followSet, bool epsilon)
        {
            if (firstSet.Contains(lookahead) || (epsilon && followSet.Contains(lookahead))) return true;
            else
            {
                while (!firstSet.Contains(lookahead) && !followSet.Contains(lookahead))
                {
                    try
                    {
                        GetNextToken();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.StackTrace);
                    }
                    if (epsilon && followSet.Contains(lookahead)) return true;
                    else
                    {

                        writeOutSyntaxErrors("Syntax error at: " + token._lineNumber + " " + token._content);
                        return false;
                    }
                }
            }
            return true;
        }

        private void GetNextToken()
        {
            prevToken = token;
            if (currentTokenIndex < Common.arrString.Length)
            {
                SetCurrentTokenIndex(currentTokenIndex);
                lookahead = Common.arrString[currentTokenIndex].Split(',')[0].Split('[')[1].Trim();
                if (lookahead == "inlinecmt")
                {
                    currentTokenIndex = currentTokenIndex + 1;
                    if (currentTokenIndex < Common.arrString.Length)
                    {
                        SetCurrentTokenIndex(currentTokenIndex);
                        lookahead = Common.arrString[currentTokenIndex].Split(',')[0].Split('[')[1].Trim();
                        SetToken(Common.arrString[currentTokenIndex].Trim().ToString());
                        currentTokenIndex = currentTokenIndex + 1;
                    }
                    else
                    {
                        lookahead = "$";
                        return;
                    }
                }
                else if (lookahead == "blockcmt")
                {
                    if (Common.arrString[currentTokenIndex].Last() == ']' && Common.arrString[currentTokenIndex].ToString().Contains("[blockcmt,"))
                    {
                        currentTokenIndex = currentTokenIndex + 1;
                        if (currentTokenIndex < Common.arrString.Length)
                        {
                            SetCurrentTokenIndex(currentTokenIndex);
                            lookahead = Common.arrString[currentTokenIndex].Split(',')[0].Split('[')[1].Trim();
                            SetToken(Common.arrString[currentTokenIndex]);
                            currentTokenIndex = currentTokenIndex + 1;
                        }
                        else
                        {
                            lookahead = "$";
                            return;
                        }
                    }
                    else
                    {
                        string strblockcmnt = "/*";
                        while (true)
                        {
                            if (Common.arrString[currentTokenIndex] != string.Empty && Common.arrString[currentTokenIndex].Last() == ']' && Common.arrString[currentTokenIndex].ToString().Contains("*/, "))
                                break;
                            else
                            {
                                strblockcmnt = strblockcmnt + Common.arrString[currentTokenIndex];
                                currentTokenIndex = currentTokenIndex + 1;
                            }
                        }
                        currentTokenIndex = currentTokenIndex + 1;
                        if (currentTokenIndex < Common.arrString.Length)
                        {
                            SetCurrentTokenIndex(currentTokenIndex);
                            lookahead = Common.arrString[currentTokenIndex].Split(',')[0].Split('[')[1].Trim();
                            SetToken(Common.arrString[currentTokenIndex]);
                            currentTokenIndex = currentTokenIndex + 1;
                        }
                        else
                        {
                            lookahead = "$";
                            return;
                        }
                    }
                }
                else
                {
                    SetToken(Common.arrString[currentTokenIndex].Trim().ToString());
                    currentTokenIndex = currentTokenIndex + 1;
                }
            }
            else
            {
                lookahead = "$";
                return;
            }
        }

        private void SetCurrentTokenIndex(int currentTokenIndex)
        {
            while (string.IsNullOrEmpty(Common.arrString[currentTokenIndex]))
            {
                currentTokenIndex = currentTokenIndex + 1;
            }
        }

        private bool APARAMS()
        {
            List<string> firstSet = new List<string> { "intnum", "floatnum", "openpar", "not", "id", "plus", "minus" };
            List<string> followSet = new List<string> { "closepar" };
            if (!skipErrors(firstSet, followSet, true)) return false;
            bool error = false;
            if (firstSet.Contains(lookahead))
            {
                if (EXPR() && REPTAPARAMS1())
                writeOutDerivation("APARAMS -> EXPR REPTAPARAMS1");
                else
                    error = true;
            }
            else if (followSet.Contains(lookahead))
            writeOutDerivation("APARAMS -> epsilon");

            else
                error = true;

            return !error;
        }

        private bool REPTAPARAMS1()
        {
            List<string> firstSet = new List<string> { "comma" };
            List<string> followSet = new List<string> { "closepar" };
            if (!skipErrors(firstSet, followSet, true)) return false;
            bool error = false;
            if (firstSet.Contains(lookahead))
            {
                if (APARAMSTAIL() && REPTAPARAMS1())
                writeOutDerivation("REPTAPARAMS1-> APARAMSTAIL REPTAPARAMS1");
                else
                    error = true;
            }
            else if (followSet.Contains(lookahead))
            writeOutDerivation("REPTAPARAMS1 -> epsilon");

            else
                error = true;

            return !error;
        }

        private bool APARAMSTAIL()
        {
            List<string> firstSet = new List<string> { "comma", "closepar" };
            List<string> followSet = new List<string> { "closepar" };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("comma"))
            {
                if (Match("comma") && EXPR())
                writeOutDerivation("APARAMSTAIL -> comma EXPR");
                else
                    error = true;
            }

            else
                error = true;

            return !error;
        }

        private bool AddOP()
        {
            List<string> firstSet = new List<string> { "plus", "minus", "or" };
            List<string> followSet = new List<string> { "id", "intlit", "floatlit", "lpar", "not", "plus", "minus" };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("plus"))
            {
                if (Match("plus"))
                writeOutDerivation("AddOP -> plus");
                else
                    error = true;
            }
            else if (lookahead.Equals("minus"))
            {
                if (Match("minus"))
                writeOutDerivation("AddOP -> minus");
                else
                    error = true;
            }
            else if (lookahead.Equals("or"))
            {
                if (Match("or"))
                writeOutDerivation("AddOP -> or");
                else
                    error = true;
            }

            else
                error = true;

            return !error;
        }

        private bool ARITHEXPR()
        {
            List<string> firstSet = new List<string> { "intnum", "floatnum", "openpar", "not", "id", "plus", "minus" };
            List<string> followSet = new List<string> { "" };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("intnum") || lookahead.Equals("floatnum") || lookahead.Equals("openpar") || lookahead.Equals("not") || lookahead.Equals("id") || lookahead.Equals("plus") || lookahead.Equals("minus"))
            {
                if (TERM() && RIGHTRECARITHEXPR())
                writeOutDerivation("ARITHEXPR -> TERM RIGHTRECARITHEXPR");
                else
                    error = true;
            }

            else
                error = true;

            return !error;
        }

        private bool RIGHTRECARITHEXPR()
        {
            List<string> firstSet = new List<string> { "plus", "minus", "or" };
            List<string> followSet = new List<string> { "closesqbr", "eq", "neq", "lt", "gt", "leq", "geq", "comma", "closepar", "semi" };
            if (!skipErrors(firstSet, followSet, true)) return false;
            bool error = false;
            if (lookahead.Equals("plus") || lookahead.Equals("minus") || lookahead.Equals("or"))
            {
                if (AddOP() && TERM() && PopFromStack("addOp", 2) && RIGHTRECARITHEXPR())
                writeOutDerivation("RIGHTRECARITHEXPR -> AddOP TERM RIGHTRECARITHEXPR");
                else
                    error = true;
            }
            else if (lookahead.Equals("closesqbr") || lookahead.Equals("eq") || lookahead.Equals("neq") || lookahead.Equals("lt") || lookahead.Equals("gt") || lookahead.Equals("leq") || lookahead.Equals("geq") || lookahead.Equals("comma") || lookahead.Equals("closepar") || lookahead.Equals("semi"))
            writeOutDerivation("RIGHTRECARITHEXPR -> epsilon");
            else
                error = true;

            return !error;
        }

        private bool RIGHTRECTERM()
        {
            List<string> firstSet = new List<string> { "mult", "div", "and" };
            List<string> followSet = new List<string> { "closesqbr", "eq", "neq", "lt", "gt", "leq", "geq", "plus", "minus", "or", "comma", "closepar", "semi" };
            if (!skipErrors(firstSet, followSet, true)) return false;
            bool error = false;
            if (lookahead.Equals("mult") || lookahead.Equals("div") || lookahead.Equals("and"))
            {
                if (MULTOP() && FACTOR() && PopFromStack("multOp", 2) && RIGHTRECTERM())
                writeOutDerivation("RIGHTRECTERM -> MULTOP FACTOR RIGHTRECTERM");
                else
                    error = true;
            }
            else if (lookahead.Equals("closesqbr") || lookahead.Equals("eq") || lookahead.Equals("neq") || lookahead.Equals("lt") || lookahead.Equals("gt") || lookahead.Equals("leq") || lookahead.Equals("geq") || lookahead.Equals("plus") || lookahead.Equals("minus") || lookahead.Equals("or") || lookahead.Equals("comma") || lookahead.Equals("closepar") || lookahead.Equals("semi"))
            writeOutDerivation("RIGHTRECTERM -> epsilon");
            else
                error = true;

            return !error;
        }

        private bool REPTSTATBLOCK1()
        {
            List<string> firstSet = new List<string> { "if", "while", "read", "write", "return", "id" };
            List<string> followSet = new List<string> { "closecubr" };
            if (!skipErrors(firstSet, followSet, true)) return false;
            bool error = false;
            if (lookahead.Equals("if") || lookahead.Equals("while") || lookahead.Equals("read") || lookahead.Equals("write") || lookahead.Equals("return") || lookahead.Equals("id"))
            {
                if (STATEMENT() && REPTSTATBLOCK1())
                writeOutDerivation("REPTSTATBLOCK1 -> STATEMENT REPTSTATBLOCK1 ");
                else
                    error = true;
            }
            else if (lookahead.Equals("closecubr"))
            writeOutDerivation("REPTSTATBLOCK1 -> epsilon");
            else
                error = true;

            return !error;
        }

        private bool REPTSTRUCTDECL4()
        {
            List<string> firstSet = new List<string> { "public", "private" };
            List<string> followSet = new List<string> { "closecubr" };
            if (!skipErrors(firstSet, followSet, true)) return false;
            bool error = false;
            if (lookahead.Equals("public") || lookahead.Equals("private"))
            {
                if (VISIBILITY() && PushInStack("visibility") && MEMBERDECL() && PopFromStack("membDecl", 2) && REPTSTRUCTDECL4())
                writeOutDerivation("REPTSTRUCTDECL4 -> VISIBILITY MEMBERDECL REPTSTRUCTDECL4");
                else
                    error = true;
            }
            else if (lookahead.Equals("closecubr"))
            writeOutDerivation("REPTSTRUCTDECL4 -> epsilon");
            else
                error = true;

            return !error;
        }

        private bool REPTVARDECL4()
        {
            List<string> firstSet = new List<string> { "opensqbr" };
            List<string> followSet = new List<string> { "semi" };
            if (!skipErrors(firstSet, followSet, true)) return false;
            bool error = false;
            if (lookahead.Equals("opensqbr"))
            {
                if (ARRAYSIZE() && REPTVARDECL4())
                writeOutDerivation("REPTVARDECL4 -> ARRAYSIZE REPTVARDECL4");
                else
                    error = true;
            }
            else if (lookahead.Equals("semi"))
            writeOutDerivation("REPTVARDECL4 -> epsilon");
            else
                error = true;

            return !error;
        }

        private bool ARRAYSIZE()
        {
            List<string> firstSet = new List<string> { "opensqbr" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("opensqbr"))
            {
                if (Match("opensqbr") && ARRAYDASH())
                writeOutDerivation("ARRAYSIZE -> lsqbr ARRAYDASH");
                else
                    error = true;
            }
            else
                error = true;

            return !error;
        }

        private bool ARRAYDASH()
        {
            List<string> firstSet = new List<string> { "intnum", "closesqbr" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("intnum"))
            {
                if (Match("intnum") && PushInStack("num") && Match("closesqbr"))
                writeOutDerivation("ARRAYDASH -> intnum rsqbr");
                else
                    error = true;
            }
            else if (lookahead.Equals("closesqbr"))
            {
                if (Match("closesqbr"))
                writeOutDerivation("ARRAYDASH -> rsqbr ");
                else
                    error = true;
            }
            else
                error = true;

            return !error;
        }

        private bool ASSIGNOP()
        {
            List<string> firstSet = new List<string> { "assign" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("assign"))
            {
                if (Match("assign"))
                writeOutDerivation("ASSIGNOP -> equal");
                else
                    error = true;
            }
            else
                error = true;

            return !error;
        }

        private bool EXPR()
        {
            List<string> firstSet = new List<string> { "intnum", "floatnum", "openpar", "not", "id", "plus", "minus" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("intnum") || lookahead.Equals("floatnum") || lookahead.Equals("openpar") || lookahead.Equals("not") || lookahead.Equals("id") || lookahead.Equals("plus") || lookahead.Equals("minus"))
            {
                if (ARITHEXPR() && EXPRDASH())
                writeOutDerivation("EXPR -> ARITHEXPR EXPRDASH");
                else
                    error = true;
            }

            else
                error = true;

            return !error;
        }

        private bool EXPRDASH()
        {
            List<string> firstSet = new List<string> { "eq", "neq", "lt", "gt", "leq", "geq" };
            List<string> followSet = new List<string> { "semi", "comma", "closepar" };
            if (!skipErrors(firstSet, followSet, true)) return false;
            bool error = false;
            if (lookahead.Equals("eq") || lookahead.Equals("neq") || lookahead.Equals("lt") || lookahead.Equals("gt") || lookahead.Equals("leq") || lookahead.Equals("geq"))
            {
                if (RELOP() && ARITHEXPR() && PopFromStack("relExp", 3))
                writeOutDerivation("EXPRDASH -> RELOP ARITHEXPR");
                else
                    error = true;
            }
            else if (lookahead.Equals("semi") || lookahead.Equals("comma") || lookahead.Equals("closepar"))
            writeOutDerivation("EXPRDASH ->  epsilon");

            else
                error = true;

            return !error;
        }

        private bool FPARAMS()
        {
            List<string> firstSet = new List<string> { "id" };
            List<string> followSet = new List<string> { "closepar" };
            if (!skipErrors(firstSet, followSet, true)) return false;
            bool error = false;
            if (lookahead.Equals("id"))
            {
                if (Match("id") && PushInStack("id") && Match("colon") && TYPE() && PushInStack("epsilon") && REPTFPARAMS3() && PopFromStackUntilEpsilon("dimList") && PopFromStack("fparam", 3) && REPTFPARAMS4())
                writeOutDerivation("FPARAMS -> id colon TYPE REPTFPARAMS3 REPTFPARAMS4");
                else
                    error = true;
            }
            else if (lookahead.Equals("closepar"))
            writeOutDerivation("FPARAMS ->  epsilon");

            else
                error = true;

            return !error;
        }

        private bool REPTFPARAMS4()
        {
            List<string> firstSet = new List<string> { "comma" };
            List<string> followSet = new List<string> { "closepar" };
            if (!skipErrors(firstSet, followSet, true)) return false;
            bool error = false;
            if (lookahead.Equals("comma"))
            {
                if (FPARAMSTAIL() && PopFromStack("fparam", 3) && REPTFPARAMS4())
                writeOutDerivation("REPTFPARAMS4 -> FPARAMSTAIL REPTFPARAMS4");
                else
                    error = true;
            }
            else if (lookahead.Equals("closepar"))
            writeOutDerivation("REPTFPARAMS4 -> epsilon");

            else
                error = true;

            return !error;
        }

        private bool REPTFPARAMS3()
        {
            List<string> firstSet = new List<string> { "opensqbr" };
            List<string> followSet = new List<string> { "closepar", "comma" };
            if (!skipErrors(firstSet, followSet, true)) return false;
            bool error = false;
            if (lookahead.Equals("opensqbr"))
            {
                if (ARRAYSIZE() && REPTFPARAMS3())
                writeOutDerivation("REPTFPARAMS3 -> ARRAYSIZE REPTFPARAMS3");
                else
                    error = true;
            }
            else if (lookahead.Equals("closepar") || lookahead.Equals("comma"))
            writeOutDerivation("REPTFPARAMS3 -> epsilon");

            else
                error = true;

            return !error;
        }

        private bool FPARAMSTAIL()
        {
            List<string> firstSet = new List<string> { "comma" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("comma"))
            {
                if (Match("comma") && Match("id") && PushInStack("id") && Match("colon") && TYPE() && PushInStack("epsilon") && REPTFPARAMSTAIL4() && PopFromStackUntilEpsilon("dimList"))
                writeOutDerivation("FPARAMSTAIL -> comma id colon TYPE REPTFPARAMSTAIL4");
                else
                    error = true;
            }

            else
                error = true;

            return !error;
        }

        private bool REPTFPARAMSTAIL4()
        {
            List<string> firstSet = new List<string> { "opensqbr" };
            List<string> followSet = new List<string> { "comma", "closepar" };
            if (!skipErrors(firstSet, followSet, true)) return false;
            bool error = false;
            if (lookahead.Equals("opensqbr"))
            {
                if (ARRAYSIZE() && REPTFPARAMSTAIL4())
                writeOutDerivation("REPTFPARAMSTAIL4 -> ARRAYSIZE REPTFPARAMSTAIL4");
                else
                    error = true;
            }
            else if (lookahead.Equals("comma") || lookahead.Equals("closepar"))
            writeOutDerivation("REPTFPARAMSTAIL4 -> epsilon");
            else
                error = true;

            return !error;
        }

        private bool FACTOR()
        {
            List<string> firstSet = new List<string> { "intnum", "floatnum", "id", "openpar", "minus", "plus", "not" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("intnum"))
            {
                if (Match("intnum") && PushInStack("intlit"))
                writeOutDerivation("FACTOR -> intnum");
                else
                    error = true;
            }

            else if (lookahead.Equals("floatnum"))
            {
                if (Match("floatnum") && PushInStack("floatnum"))
                writeOutDerivation("FACTOR -> floatnum");

                else
                    error = true;
            }

            else if (lookahead.Equals("id"))
            {
                if (PushInStack("epsilon") && PushInStack("epsilon") && REPTVARIABLE0() && PushInStack("id") && PopFromStackUntilEpsilon("var0") && FACTORDASH() && PopFromStackUntilEpsilon("functionCallDataMember"))
                writeOutDerivation("FACTOR -> REPTVARIABLE0 id FACTORDASH");
                else
                    error = true;
            }

            else if (lookahead.Equals("openpar"))
            {
                if (Match("openpar") && ARITHEXPR() && Match("closepar"))
                writeOutDerivation("FACTOR -> lpar ARITHEXPR rpar");
                else
                    error = true;
            }

            else if (lookahead.Equals("minus"))
            {
                if (SIGN() && FACTOR() && PopFromStack("sign", 1))
                writeOutDerivation("FACTOR -> SIGN FACTOR");
                else
                    error = true;
            }

            else if (lookahead.Equals("plus"))
            {
                if (SIGN() && FACTOR() && PopFromStack("sign", 1))
                writeOutDerivation("FACTOR -> SIGN FACTOR");
                else
                    error = true;
            }

            else if (lookahead.Equals("not"))
            {
                if (Match("not") && FACTOR() && PopFromStack("not", 1))
                writeOutDerivation("FACTOR -> not FACTOR");
                else
                    error = true;
            }
            else
                error = true;

            return !error;
        }

        private bool REPTVARIABLE0()
        {
            bool error = false;
            if (lookahead.Equals("id"))
            {
                if (!error)
                    Match("id");
                if (lookahead.Equals("dot"))
                {
                    if (IDNEST() && REPTVARIABLE0())
                    {
                        writeOutDerivation("REPTVARIABLE0 -> IDNEST REPTVARIABLE0");
                    }
                    else
                        error = true;
                }
                else
                writeOutDerivation("REPTVARIABLE0 -> epsilon");
            }

            else
                error = true;

            return !error;
        }

        private bool FACTORDASH()
        {
            List<string> firstSet = new List<string> { "openpar", "opensqbr" };
            List<string> followSet = new List<string> { "mult", "div", "and", "closesqbr", "eq", "neq", "lt", "gt", "leq", "geq", "plus", "minus", "or", "comma", "closepar", "semi" };
            if (!skipErrors(firstSet, followSet, true)) return false;
            bool error = false;
            if (lookahead.Equals("openpar"))
            {
                if (Match("openpar") && PushInStack("epsilon") && APARAMS() && PopFromStackUntilEpsilon("aParams") && Match("closepar"))
                writeOutDerivation("FACTORDASH -> lpar APARAMS rpar");
                else
                    error = true;
            }
            else if (lookahead.Equals("opensqbr"))
            {
                if (PushInStack("epsilon") && REPTVARIABLE2() && PopFromStackUntilEpsilon("indiceList"))
                writeOutDerivation("FACTORDASH -> REPTVARIABLE2");
                else
                    error = true;
            }
            else if (lookahead.Equals("mult") || lookahead.Equals("div") || lookahead.Equals("and") || lookahead.Equals("closesqbr") || lookahead.Equals("eq") || lookahead.Equals("neq") || lookahead.Equals("lt") || lookahead.Equals("gt") || lookahead.Equals("leq") || lookahead.Equals("geq") || lookahead.Equals("plus") || lookahead.Equals("minus") || lookahead.Equals("or") || lookahead.Equals("comma") || lookahead.Equals("closepar") || lookahead.Equals("semi"))
            writeOutDerivation("FACTORDASH -> epsilon");
            else
                error = true;

            return !error;
        }

        private bool REPTVARIABLE2()
        {
            List<string> firstSet = new List<string> { "openpar", "opensqbr" };
            List<string> followSet = new List<string> { "mult", "div", "and", "closesqbr", "eq", "neq", "lt", "gt", "leq", "geq", "plus", "minus", "or", "comma", "closepar", "semi", "assign" };
            if (!skipErrors(firstSet, followSet, true)) return false;
            bool error = false;
            if (lookahead.Equals("opensqbr"))
            {
                if (INDICE() && REPTVARIABLE2())
                writeOutDerivation("REPTVARIABLE2 -> INDICE REPTVARIABLE2");
                else
                    error = true;
            }
            else if (lookahead.Equals("mult") || lookahead.Equals("div") || lookahead.Equals("and") || lookahead.Equals("closesqbr") || lookahead.Equals("eq") || lookahead.Equals("neq") || lookahead.Equals("lt") || lookahead.Equals("gt") || lookahead.Equals("leq") || lookahead.Equals("geq") || lookahead.Equals("assign") || lookahead.Equals("plus") || lookahead.Equals("minus") || lookahead.Equals("or") || lookahead.Equals("comma") || lookahead.Equals("closepar") || lookahead.Equals("semi"))
            writeOutDerivation("REPTVARIABLE2 -> epsilon");
            else
                error = true;

            return !error;
        }

        private bool FUNCBODY()
        {
            List<string> firstSet = new List<string> { "opencubr" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("opencubr"))
            {
                if (Match("opencubr") && PushInStack("epsilon") && REPTFUNCBODY1() && PopFromStackUntilEpsilon("statementBlock") && Match("closecubr"))
                writeOutDerivation("FUNCBODY -> lcurbr REPTFUNCBODY1 rcurbr ");
                else
                    error = true;
            }
            else
                error = true;

            return !error;
        }

        private bool REPTFUNCBODY1()
        {
            List<string> firstSet = new List<string> { "let", "if", "while", "read", "write", "return", "id" };
            List<string> followSet = new List<string> { "closecubr" };
            if (!skipErrors(firstSet, followSet, true)) return false;
            bool error = false;
            if (lookahead.Equals("let") || lookahead.Equals("if") || lookahead.Equals("while") || lookahead.Equals("read") || lookahead.Equals("write") || lookahead.Equals("return") || lookahead.Equals("id"))
            {
                if (VARDECLORSTAT() && REPTFUNCBODY1())
                writeOutDerivation("REPTFUNCBODY1 -> VARDECLORSTAT REPTFUNCBODY1");
                else
                    error = true;
            }
            else if (lookahead.Equals("closecubr"))
            writeOutDerivation("REPTFUNCBODY1 -> epsilon");
            else
                error = true;

            return !error;
        }

        private bool FUNCDECL()
        {
            List<string> firstSet = new List<string> { "func" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("func"))
            {
                if (FUNCHEAD() && Match("semi"))
                writeOutDerivation("FUNCDECL -> FUNCHEAD semi");

                else
                    error = true;
            }

            else
                error = true;

            return !error;
        }

        private bool FUNCDEF()
        {
            List<string> firstSet = new List<string> { "func" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("func"))
            {
                if (FUNCHEAD() && FUNCBODY())
                writeOutDerivation("FUNCDEF -> FUNCHEAD FUNCBODY");
                else
                    error = true;
            }
            else
                error = true;

            return !error;
        }

        private bool FUNCHEAD()
        {
            List<string> firstSet = new List<string> { "func" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("func"))
            {
                if (Match("func") && Match("id") && PushInStack("id") && Match("openpar") && PushInStack("epsilon") && FPARAMS() && PopFromStackUntilEpsilon("fparamList") && Match("closepar") && Match("arrow") && RETURNTYPE())
                writeOutDerivation("FUNCHEAD -> func id lpar FPARAMS rpar arrow RETURNTYPE");
                else
                    error = true;
            }
            else
                error = true;

            return !error;
        }

        private bool IDNEST()
        {
            List<string> firstSet = new List<string> { "dot" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("dot"))
            {
                if (PushInStack("id") && IDNESTDASH())
                writeOutDerivation("IDNEST-> id IDNESTDASH");

                else
                    error = true;
            }
            else
                error = true;

            return !error;
        }

        private bool IDNESTDASH()
        {
            List<string> firstSet = new List<string> { "dot", "openpar", "opensqbr" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("openpar"))
            {
                if (Match("openpar") && PushInStack("epsilon") && APARAMS() && PopFromStackUntilEpsilon("aParams") && Match("closepar") && Match("dot") && PopFromStack("dot", 2))
                writeOutDerivation("IDNESTDASH -> lpar APARAMS rpar dot");
                else
                    error = true;
            }
            else if (lookahead.Equals("opensqbr"))
            {
                if (PushInStack("epsilon") && REPTIDNEST1() && PopFromStackUntilEpsilon("indiceList") && Match("dot") && PopFromStack("dot", 2))
                writeOutDerivation("IDNESTDASH -> REPTIDNEST1 dot");
                else
                    error = true;
            }
            else if (lookahead.Equals("dot"))
            {
                if (PushInStack("epsilon") && REPTIDNEST1() && PopFromStackUntilEpsilon("indiceList") && Match("dot") && PopFromStack("dot", 2))
                writeOutDerivation("IDNESTDASH -> REPTIDNEST1 dot");
                else
                    error = true;
            }
            else
                error = true;

            return !error;
        }

        private bool REPTIDNEST1()
        {
            List<string> firstSet = new List<string> { "opensqbr" };
            List<string> followSet = new List<string> { "dot" };
            if (!skipErrors(firstSet, followSet, true)) return false;
            bool error = false;
            if (lookahead.Equals("opensqbr"))
            {
                if (INDICE() && REPTIDNEST1())
                writeOutDerivation("REPTIDNEST1 -> INDICE REPTIDNEST1 ");
                else
                    error = true;
            }
            else if (lookahead.Equals("dot"))
            writeOutDerivation("REPTIDNEST1 -> epsilon");
            else
                error = true;

            return !error;
        }

        private bool IMPLDEF()
        {
            List<string> firstSet = new List<string> { "impl" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("impl"))
            {
                if (Match("impl") && Match("id") && PushInStack("id") && Match("opencubr") && PushInStack("epsilon") && REPTIMPLDEF3() && PopFromStackUntilEpsilon("funcDefList") && Match("closecubr"))
                writeOutDerivation("IMPLDEF -> impl id lcurbr REPTIMPLDEF3 rcurbr");
                else
                    error = true;
            }
            else
                error = true;

            return !error;
        }

        private bool REPTIMPLDEF3()
        {
            List<string> firstSet = new List<string> { "func" };
            List<string> followSet = new List<string> { "closecubr" };
            if (!skipErrors(firstSet, followSet, true)) return false;
            bool error = false;
            if (lookahead.Equals("func"))
            {
                if (FUNCDEF() && PopFromStack("funcDef", 4) && REPTIMPLDEF3())
                writeOutDerivation("REPTIMPLDEF3 -> FUNCDEF REPTIMPLDEF3 ");
                else
                    error = true;
            }
            else if (lookahead.Equals("closecubr"))
            writeOutDerivation("REPTIMPLDEF3 -> epsilon");

            else
                error = true;

            return !error;
        }

        private bool INDICE()
        {
            List<string> firstSet = new List<string> { "opensqbr" };
            List<string> followSet = new List<string> { "" };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("opensqbr"))
            {
                if (Match("opensqbr") && ARITHEXPR() && Match("closesqbr"))
                writeOutDerivation("INDICE -> lsqbr ARITHEXPR rsqbr ");
                else
                    error = true;
            }
            else
                error = true;

            return !error;
        }

        private bool MEMBERDECL()
        {
            List<string> firstSet = new List<string> { "func", "let" };
            List<string> followSet = new List<string> { "" };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("func"))
            {
                if (FUNCDECL() && PopFromStack("funcDecl", 3))
                writeOutDerivation("MEMBERDECL -> FUNCDECL");
                else
                    error = true;
            }
            else if (lookahead.Equals("let"))
            {
                if (VARDECL() && PopFromStack("varDecl", 3))
                writeOutDerivation("MEMBERDECL -> VARDECL");
                else
                    error = true;
            }
            else
                error = true;

            return !error;
        }

        private bool MULTOP()
        {
            List<string> firstSet = new List<string> { "mult", "div", "and" };
            List<string> followSet = new List<string> { "" };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("mult"))
            {
                if (Match("mult"))
                writeOutDerivation("MULTOP -> mult");
                else
                    error = true;
            }
            else if (lookahead.Equals("div"))
            {
                if (Match("div"))
                writeOutDerivation("MULTOP -> div");
                else
                    error = true;
            }
            else if (lookahead.Equals("and"))
            {
                if (Match("and"))
                writeOutDerivation("MULTOP -> and");
                else
                    error = true;
            }
            else
                error = true;

            return !error;
        }

        private bool optstructDecl2()
        {
            List<string> firstSet = new List<string> { "inherits" };
            List<string> followSet = new List<string> { "opencubr" };
            if (!skipErrors(firstSet, followSet, true)) return false;
            bool error = false;
            if (lookahead.Equals("inherits"))
            {
                if (Match("inherits") && Match("id") && PushInStack("id") && REPTOPTSTRUCTDECL22())
                writeOutDerivation("OPTSTRUCTDECL2 -> inherits id REPTOPTSTRUCTDECL22");
                else
                    error = true;
            }
            else if (lookahead.Equals("opencubr"))
            writeOutDerivation("OPTSTRUCTDECL2 -> epsilon");

            else
                error = true;

            return !error;
        }

        private bool REPTOPTSTRUCTDECL22()
        {
            List<string> firstSet = new List<string> { "comma" };
            List<string> followSet = new List<string> { "opencubr" };
            if (!skipErrors(firstSet, followSet, true)) return false;
            bool error = false;
            if (lookahead.Equals("comma"))
            {
                if (Match("comma") && Match("id") && PushInStack("id") && REPTOPTSTRUCTDECL22())
                writeOutDerivation("REPTOPTSTRUCTDECL22 -> comma id REPTOPTSTRUCTDECL22");
                else
                    error = true;
            }
            else if (lookahead.Equals("opencubr"))
            writeOutDerivation("REPTOPTSTRUCTDECL22 -> epsilon");

            else
                error = true;

            return !error;
        }

        private bool PROG()
        {
            List<string> firstSet = new List<string> { "func", "impl", "struct" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("func") || lookahead.Equals("impl") || lookahead.Equals("struct"))
            {
                if (PushInStack("epsilon") && REPTPROG0() && PopFromStackUntilEpsilon("STRUCTORIMPLORFUNCLIST"))
                writeOutDerivation("PROG -> REPTPROG0");
                else
                    error = true;
            }

            else
                error = true;

            return !error;
        }

        private bool REPTPROG0()
        {
            List<string> firstSet = new List<string> { "func", "impl", "struct" };
            List<string> followSet = new List<string> { "$" };
            if (!skipErrors(firstSet, followSet, true)) return false;
            bool error = false;
            if (lookahead.Equals("func") || lookahead.Equals("impl") || lookahead.Equals("struct"))
            {
                if (STRUCTORIMPLORFUNC() && REPTPROG0())
                writeOutDerivation("REPTPROG0 -> STRUCTORIMPLORFUNC REPTPROG0");
                else
                    error = true;
            }
            else if (lookahead.Equals("$"))
            writeOutDerivation("REPTPROG0 -> epsilon");

            else
                error = true;

            return !error;
        }

        private bool RELEXPR()
        {
            List<string> firstSet = new List<string> { "intnum", "floatnum", "openpar", "not", "id", "plus", "minus" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("intnum") || lookahead.Equals("floatnum") || lookahead.Equals("openpar") || lookahead.Equals("not") || lookahead.Equals("id") || lookahead.Equals("plus") || lookahead.Equals("minus"))
            {
                if (ARITHEXPR() && RELOP() && ARITHEXPR() && PopFromStack("relExpr", 3))
                writeOutDerivation("RELEXPR -> ARITHEXPR RELOP ARITHEXPR");
                else
                    error = true;
            }
            else
                error = true;

            return !error;
        }

        private bool RELOP()
        {
            List<string> firstSet = new List<string> { "eq", "neq", "lt", "gt", "leq", "geq" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("eq"))
            {
                if (Match("eq") && PushInStack("relOp"))
                writeOutDerivation("RELOP -> eq");

                else
                    error = true;
            }
            else if (lookahead.Equals("neq"))
            {
                if (Match("neq") && PushInStack("relOp"))
                writeOutDerivation("RELOP -> neq");

                else
                    error = true;
            }
            else if (lookahead.Equals("lt"))
            {
                if (Match("lt") && PushInStack("relOp"))
                writeOutDerivation("RELOP -> lt");

                else
                    error = true;
            }
            else if (lookahead.Equals("gt"))
            {
                if (Match("gt") && PushInStack("relOp"))
                writeOutDerivation("RELOP -> gt");

                else
                    error = true;
            }
            else if (lookahead.Equals("leq"))
            {
                if (Match("leq") && PushInStack("relOp"))
                writeOutDerivation("RELOP -> leq");

                else
                    error = true;
            }
            else if (lookahead.Equals("geq"))
            {
                if (Match("geq") && PushInStack("relOp"))
                writeOutDerivation("RELOP -> geq");
                else
                    error = true;
            }

            else
                error = true;

            return !error;
        }

        private bool RETURNTYPE()
        {
            List<string> firstSet = new List<string> { "void", "integer", "float", "id" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("void"))
            {
                if (Match("void") && PushInStack("type"))
                writeOutDerivation("RETURNTYPE -> void");
                else
                    error = true;
            }
            else if (lookahead.Equals("integer") || lookahead.Equals("float") || lookahead.Equals("id"))
            {
                if (TYPE())
                writeOutDerivation("RETURNTYPE -> TYPE");
                else
                    error = true;
            }
            else
                error = true;

            return !error;
        }

        private bool SIGN()
        {
            List<string> firstSet = new List<string> { "plus", "minus" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("plus"))
            {
                if (Match("plus"))
                writeOutDerivation("SIGN -> plus");
                else
                    error = true;
            }
            else if (lookahead.Equals("minus"))
            {
                if (Match("minus"))
                writeOutDerivation("SIGN -> minus");
                else
                    error = true;
            }
            else
                error = true;

            return !error;
        }

        private bool STATBLOCK()
        {
            List<string> firstSet = new List<string> { "id", "opencubr", "return", "while", "read", "write", "if" };
            List<string> followSet = new List<string> { "else", "semi" };
            if (!skipErrors(firstSet, followSet, true)) return false;
            bool error = false;
            if (lookahead.Equals("id"))
            {
                if (STATEMENT())
                writeOutDerivation("STATBLOCK -> STATEMENT");
                else
                    error = true;
            }
            else if (lookahead.Equals("opencubr"))
            {
                if (Match("opencubr") && REPTSTATBLOCK1() && Match("closecubr"))
                writeOutDerivation("STATBLOCK -> lcurbr REPTSTATBLOCK1 rcurbr");
                else
                    error = true;
            }
            else if (lookahead.Equals("return"))
            {
                if (STATEMENT())
                writeOutDerivation("STATBLOCK -> STATEMENT");
                else
                    error = true;
            }
            else if (lookahead.Equals("while"))
            {
                if (STATEMENT())
                writeOutDerivation("STATBLOCK -> STATEMENT");
                else
                    error = true;
            }
            else if (lookahead.Equals("read"))
            {
                if (STATEMENT())
                writeOutDerivation("STATBLOCK -> STATEMENT");
                else
                    error = true;
            }
            else if (lookahead.Equals("write"))
            {
                if (STATEMENT())
                writeOutDerivation("STATBLOCK -> STATEMENT");
                else
                    error = true;
            }
            else if (lookahead.Equals("if"))
            {
                if (STATEMENT())
                writeOutDerivation("STATBLOCK -> STATEMENT");
                else
                    error = true;
            }
            else if (lookahead.Equals("else") || lookahead.Equals("semi"))
            writeOutDerivation("STATBLOCK -> epsilon");
            else
                error = true;

            return !error;
        }

        private bool STATEMENT()
        {
            List<string> firstSet = new List<string> { "id", "return", "write", "read", "while", "if" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("id"))
            {
                if (PushInStack("epsilon") && REPTVARIABLE0() && PushInStack("id") && PopFromStackUntilEpsilon("var0") && STATEMENTDASH())
                writeOutDerivation("STATEMENT -> REPTVARIABLE0 id STATEMENTDASH");
                else
                    error = true;
            }

            else if (lookahead.Equals("return"))
            {
                if (Match("return") && Match("openpar") && EXPR() && Match("closepar") && Match("semi") && PopFromStack("returnStatement", 1))
                writeOutDerivation("STATEMENT -> return lpar EXPR rpar semi");
                else
                    error = true;
            }

            else if (lookahead.Equals("write"))
            {
                if (Match("write") && Match("openpar") && EXPR() && Match("closepar") && Match("semi") && PopFromStack("writeStatement", 1))
                writeOutDerivation("STATEMENT -> write lpar EXPR rpar semi");
                else
                    error = true;
            }

            else if (lookahead.Equals("read"))
            {
                if (Match("read") && Match("openpar") && VARIABLE() && Match("closepar") && Match("semi") && PopFromStack("readStatement", 1))
                writeOutDerivation("STATEMENT -> read lpar VARIABLE rpar semi");
                else
                    error = true;
            }

            else if (lookahead.Equals("while"))
            {
                if (Match("while") && Match("openpar") && RELEXPR() && Match("closepar") && PushInStack("epsilon") && STATBLOCK() && PopFromStackUntilEpsilon("statementBlock") && Match("semi") && PopFromStack("whileStatement", 2))
                writeOutDerivation("STATEMENT -> while lpar RELEXPR rpar STATBLOCK semi");
                else
                    error = true;
            }

            else if (lookahead.Equals("if"))
            {
                if (Match("if") && Match("openpar") && RELEXPR() && Match("closepar") && Match("then") && PushInStack("epsilon") && STATBLOCK() && PopFromStackUntilEpsilon("statementBlock") && Match("else") && PushInStack("epsilon") && STATBLOCK() && PopFromStackUntilEpsilon("statementBlock") && Match("semi") && PopFromStack("ifStatement", 3))
                writeOutDerivation("STATEMENT -> if lpar RELEXPR rpar then STATBLOCK else STATBLOCK semi");
                else
                    error = true;
            }

            else
                error = true;

            return !error;
        }

        private bool STATEMENTDASH()
        {
            List<string> firstSet = new List<string> { "openpar", "opensqbr", "assign" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("openpar"))
            {
                if (Match("openpar") && PushInStack("epsilon") && APARAMS() && PopFromStackUntilEpsilon("aParams") && Match("closepar") && Match("semi") && PopFromStack("fcallStatement", 2))
                writeOutDerivation("STATEMENTDASH -> lpar APARAMS rpar semi");
                else
                    error = true;
            }
            else if (lookahead.Equals("opensqbr"))
            {
                if (PushInStack("epsilon") && REPTVARIABLE2() && PopFromStackUntilEpsilon("indiceList") && PopFromStack("var", 2) && ASSIGNOP() && EXPR() && Match("semi") && PopFromStack("assignStatement", 2))
                writeOutDerivation("STATEMENTDASH -> REPTVARIABLE2 ASSIGNOP EXPR semi");
                else
                    error = true;
            }
            else if (lookahead.Equals("assign"))
            {
                if (PushInStack("epsilon") && REPTVARIABLE2() && PopFromStackUntilEpsilon("indiceList") && PopFromStack("var", 2) && ASSIGNOP() && EXPR() && Match("semi") && PopFromStack("assignStatement", 2))
                writeOutDerivation("STATEMENTDASH -> REPTVARIABLE2 ASSIGNOP EXPR semi");
                else
                    error = true;
            }
            else
                error = true;

            return !error;
        }

        private bool STRUCTDECL()
        {
            List<string> firstSet = new List<string> { "struct" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("struct"))
            {
                if (Match("struct") && Match("id") && PushInStack("id") && PushInStack("epsilon") && optstructDecl2() && PopFromStackUntilEpsilon("inheritList") && Match("opencubr") && PushInStack("epsilon") && REPTSTRUCTDECL4() && PopFromStackUntilEpsilon("memberList") && Match("closecubr") && Match("semi"))
                writeOutDerivation("STRUCTDECL -> struct id OPTSTRUCTDECL2 lcurbr REPTSTRUCTDECL4 rcurbr semi");

                else
                    error = true;
            }
            else
                error = true;

            return !error;
        }

        private bool STRUCTORIMPLORFUNC()
        {
            List<string> firstSet = new List<string> { "struct", "impl", "func" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("func"))
            {
                if (FUNCDEF() && PopFromStack("funcDef", 4))
                writeOutDerivation("STRUCTORIMPLORFUNC -> FUNCDEF");

                else
                    error = true;
            }
            else if (lookahead.Equals("impl"))
            {
                if (IMPLDEF() && PopFromStack("implDecl", 2))
                {
                    writeOutDerivation("STRUCTORIMPLORFUNC -> IMPLDEF");
                }
                else
                    error = true;
            }
            else if (lookahead.Equals("struct"))
            {
                if (STRUCTDECL() && PopFromStack("structDecl", 3))
                writeOutDerivation("STRUCTORIMPLORFUNC -> STRUCTDECL");
                else
                    error = true;
            }
            else
                error = true;

            return !error;
        }

        private bool TERM()
        {
            List<string> firstSet = new List<string> { "intnum", "floatnum", "openpar", "not", "id", "plus", "minus" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("intnum") || lookahead.Equals("floatnum") || lookahead.Equals("openpar") || lookahead.Equals("not") || lookahead.Equals("id") || lookahead.Equals("plus") || lookahead.Equals("minus"))
            {
                if (FACTOR() && RIGHTRECTERM())
                writeOutDerivation("TERM -> FACTOR RIGHTRECTERM");
                else
                    error = true;
            }

            else
                error = true;

            return !error;
        }

        private bool TYPE()
        {
            List<string> firstSet = new List<string> { "integer", "float", "id" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("integer"))
            {
                if (Match("integer") && PushInStack("type"))
                writeOutDerivation("TYPE -> integer");
                else
                    error = true;
            }
            else if (lookahead.Equals("float"))
            {
                if (Match("float") && PushInStack("type"))
                writeOutDerivation("TYPE -> float");
                else
                    error = true;
            }
            else if (lookahead.Equals("id"))
            {
                if (Match("id") && PushInStack("type"))
                writeOutDerivation("TYPE -> id");
                else
                    error = true;
            }
            else
                error = true;

            return !error;
        }

        private bool VARDECL()
        {
            List<string> firstSet = new List<string> { "let" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("let"))
            {
                if (Match("let") && Match("id") && PushInStack("id") && Match("colon") && TYPE() && PushInStack("epsilon") && REPTVARDECL4() && PopFromStackUntilEpsilon("dimList") && Match("semi"))
                writeOutDerivation("VARDECL -> let id colon TYPE REPTVARDECL4 semi");
                else
                    error = true;
            }
            else
                error = true;

            return !error;
        }

        private bool VARDECLORSTAT()
        {
            List<string> firstSet = new List<string> { "let", "id", "return", "write", "read", "while", "if" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("id"))
            {
                if (STATEMENT())
                writeOutDerivation("VARDECLORSTAT -> STATEMENT");
                else
                    error = true;
            }
            else if (lookahead.Equals("let"))
            {
                if (VARDECL())
                writeOutDerivation("VARDECLORSTAT -> VARDECL");
                else
                    error = true;
            }
            else if (lookahead.Equals("return"))
            {
                if (STATEMENT())
                writeOutDerivation("VARDECLORSTAT -> STATEMENT");
                else
                    error = true;
            }
            else if (lookahead.Equals("write"))
            {
                if (STATEMENT())
                writeOutDerivation("VARDECLORSTAT -> STATEMENT");
                else
                    error = true;
            }
            else if (lookahead.Equals("read"))
            {
                if (STATEMENT())
                writeOutDerivation("VARDECLORSTAT -> STATEMENT");
                else
                    error = true;
            }
            else if (lookahead.Equals("while"))
            {
                if (STATEMENT())
                writeOutDerivation("VARDECLORSTAT -> STATEMENT");
                else
                    error = true;
            }
            else if (lookahead.Equals("if"))
            {
                if (STATEMENT())
                writeOutDerivation("VARDECLORSTAT -> STATEMENT");
                else
                    error = true;
            }

            else
                error = true;

            return !error;
        }

        private bool VARIABLE()
        {
            List<string> firstSet = new List<string> { "id" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("id"))
            {
                if (PushInStack("epsilon") && PushInStack("epsilon") && REPTVARIABLE0() && PushInStack("id") && PopFromStackUntilEpsilon("var0") && PushInStack("epsilon") && REPTVARIABLE2() && PopFromStackUntilEpsilon("indiceList") && PopFromStackUntilEpsilon("variable"))
                writeOutDerivation("VARIABLE -> REPTVARIABLE0 id REPTVARIABLE2");
                else
                    error = true;
            }

            else
                error = true;

            return !error;
        }

        private bool VISIBILITY()
        {
            List<string> firstSet = new List<string> { "public", "private" };
            List<string> followSet = new List<string> { };
            if (!skipErrors(firstSet, followSet, false)) return false;
            bool error = false;
            if (lookahead.Equals("public"))
            {
                if (Match("public"))
                writeOutDerivation("VISIBILITY -> public");
                else
                    error = true;
            }
            else if (lookahead.Equals("private"))
            {
                if (Match("private"))
                writeOutDerivation("VISIBILITY -> private");
                else
                    error = true;
            }
            else
                error = true;

            return !error;
        }

        public void WriteDerivation(string message)
        {
            File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outderivation"), message);
        }

        public void WriteSyntaxErrors(string message)
        {
            File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsyntaxerrors"), message);
        }

        public void SetToken(string content)
        {
            if (content.Split(',')[0].Split('[')[1].Trim() == "comma")
                token = new Token("comma", ",", Convert.ToInt32(content.Split(',')[3].Split(']')[0].Trim()));
            else
                token = new Token(content.Split(',')[0].Split('[')[1].Trim(), content.Split(',')[1].Trim(), Convert.ToInt32(content.Split(',')[2].Split(']')[0].Trim()));
        }
    }
}

