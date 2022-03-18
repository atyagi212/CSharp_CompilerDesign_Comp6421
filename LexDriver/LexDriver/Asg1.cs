using System;
using System.IO;

namespace LexDriver
{
    public class Asg1
    {
        public static void Main()
        {
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/InputFiles");
                string pathOutput = @"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles";
                FileInfo[] arrFiles = directoryInfo.GetFiles();

                foreach (FileInfo file in arrFiles)
                {
                    LexerGenerator.Initializer();
                    string filename = file.Name;
                    string fileExt = Path.GetExtension(file.FullName);
                    if (fileExt == ".src")
                    {
                        Console.WriteLine(filename);
                        StreamReader sr = new StreamReader(file.FullName);
                        Common.fileContent = sr.ReadToEnd();
                        LexerGenerator lexerGenerator = new LexerGenerator();
                        try
                        {
                            while (Common.continueRead)
                            {
                                Token token = lexerGenerator.TokenGenerationbyReadingCharacters();
                                if (token == null) break;
                                if (!File.Exists(Path.Combine(pathOutput, Path.GetFileNameWithoutExtension(filename) + ".outlextokens")))
                                    File.WriteAllText(Path.Combine(pathOutput, Path.GetFileNameWithoutExtension(filename) + ".outlextokens"), "[" + "" + token._lexeme + ", " + token._content + ", " + token._lineNumber + ']' + "\n");
                                else
                                    File.AppendAllText(Path.Combine(pathOutput, Path.GetFileNameWithoutExtension(filename) + ".outlextokens"), "[" + "" + token._lexeme + ", " + token._content + ", " + token._lineNumber + ']' + "\n");

                                if (token._lexeme.Equals("errorcharacter"))
                                {
                                    if (!File.Exists(Path.Combine(pathOutput, Path.GetFileNameWithoutExtension(filename) + ".outlexerrors")))
                                        File.WriteAllText(Path.Combine(pathOutput, Path.GetFileNameWithoutExtension(filename) + ".outlexerrors"), "Lexical error: Invalid character: \"" + token._content + "\": line " + token._lineNumber + ".\n");
                                    else
                                        File.AppendAllText(Path.Combine(pathOutput, Path.GetFileNameWithoutExtension(filename) + ".outlexerrors"), "Lexical error: Invalid character: \"" + token._content + "\": line " + token._lineNumber + ".\n");
                                }
                                if (token._lexeme.Equals("errornumeric"))
                                {
                                    if (!File.Exists(Path.Combine(pathOutput, Path.GetFileNameWithoutExtension(filename) + ".outlexerrors")))
                                        File.WriteAllText(Path.Combine(pathOutput, Path.GetFileNameWithoutExtension(filename) + ".outlexerrors"), "Lexical error: Invalid number: \"" + token._content + "\": line " + token._lineNumber + ".\n");
                                    else
                                        File.AppendAllText(Path.Combine(pathOutput, Path.GetFileNameWithoutExtension(filename) + ".outlexerrors"), "Lexical error: Invalid number: \"" + token._content + "\": line " + token._lineNumber + ".\n");
                                }
                            }

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.StackTrace);
                        }
                    }
                }

                directoryInfo = new DirectoryInfo(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles");
                arrFiles = directoryInfo.GetFiles();

                foreach (FileInfo file in arrFiles)
                {
                    LexerGenerator.Initializer();
                    string fileExt = Path.GetExtension(file.FullName);
                    string filename = Path.GetFileNameWithoutExtension(file.Name);
                    ASTNode node = new ASTNode();
                    ASTNode symTableNodes = new ASTNode();
                    if (fileExt == ".outlextokens")
                    {
                        Console.WriteLine("Processing File: " + filename);
                        StreamReader sr = new StreamReader(file.FullName);
                        Common.arrString = File.ReadAllLines(file.FullName);

                        try
                        {
                            if (!File.Exists(Path.Combine(pathOutput, Path.GetFileNameWithoutExtension(filename) + ".outsyntaxerrors")))
                                using (File.Create(Path.Combine(pathOutput, Path.GetFileNameWithoutExtension(filename) + ".outsyntaxerrors"))) ;
                            if (!File.Exists(Path.Combine(pathOutput, Path.GetFileNameWithoutExtension(filename) + ".outderivation")))
                                using (File.Create(Path.Combine(pathOutput, Path.GetFileNameWithoutExtension(filename) + ".outderivation"))) ;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.StackTrace);
                        }
                        SyntaxAnalysis sa = new SyntaxAnalysis();
                        Console.WriteLine(sa.Parse(filename));
                        Console.WriteLine("=====================\n\n");
                        Console.WriteLine(SyntaxAnalysis.st.Count);
                        try
                        {
                            if (!File.Exists(Path.Combine(pathOutput, Path.GetFileNameWithoutExtension(filename) + ".outast")))
                                using (File.Create(Path.Combine(pathOutput, Path.GetFileNameWithoutExtension(filename) + ".outast"))) ;
                            node = SyntaxAnalysis.st.Peek();
                            SyntaxAnalysis.printAST(node, 0, filename);
                        }
                        catch (IOException e)
                        {
                            Console.WriteLine(e.StackTrace);
                        }
                        Console.WriteLine("=====================\n\n");

                        try
                        {
                            if (!File.Exists(Path.Combine(pathOutput, Path.GetFileNameWithoutExtension(filename) + ".outsymboltables")))
                                using (File.Create(Path.Combine(pathOutput, Path.GetFileNameWithoutExtension(filename) + ".outsymboltables"))) ;
                            SymbolTableGenerator STCVisitor = new SymbolTableGenerator(filename);
                            File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(filename) + ".outsymboltables"), "| table: global\n");
                            File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(filename) + ".outsymboltables"), "===================================================================================\n");
                            symTableNodes = STCVisitor.PrintTable(node);
                        }
                        catch (IOException e)
                        {
                            Console.WriteLine(e.StackTrace);
                        }
                        Console.WriteLine("=====================\n\n");

                        try
                        {
                            if (!File.Exists(Path.Combine(pathOutput, Path.GetFileNameWithoutExtension(filename) + ".outsemanticerrors")))
                                using (File.Create(Path.Combine(pathOutput, Path.GetFileNameWithoutExtension(filename) + ".outsemanticerrors"))) ;
                            SemanticErrorVisitor SEVisitor = new SemanticErrorVisitor(filename);
                            SEVisitor.PrintSemanticErrors(symTableNodes);
                        }
                        catch (IOException e)
                        {
                            Console.WriteLine(e.StackTrace);
                        }
                        Console.WriteLine("=====================\n\n");
                    }
                }

            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }
    }
}
