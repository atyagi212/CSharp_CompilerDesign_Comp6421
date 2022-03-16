using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace LexDriver
{
    public class SymbolTableGenerator : Visitor
    {
        private string fileName;

        public SymbolTableGenerator(string outputfilename)
        {
            this.fileName = outputfilename;
        }

        public void PrintTable(ASTNode p_Node)
        {
            string nodeName = string.Empty;


            //for (int i = 0; i < p_Node.Children.Count; i++)
            //{
            //    switch (p_Node.Children[i].label)
            //    {
            //        case "structDecl":
            //            WriteClassTable(p_Node.Children[i], true);
            //            break;
            //        case "funcDef":
            //            WriteFunctionTable(p_Node.Children[i], true);
            //            break;
            //    }
            //    PrintTable(p_Node.Children[i], i == p_Node.Children.Count - 1);
            //}
            var listImplementationNode = p_Node.Children.Find(x => x.label == "STRUCTORIMPLORFUNCLIST").Children;
            foreach (var node in listImplementationNode)
            {
                switch (node.label)
                {
                    case "structDecl":
                        WriteClassTable(node, listImplementationNode);
                        break;
                    case "funcDef":
                        WriteFunctionTable(node);
                        break;
                }
            }

            //File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsymboltables"), p_Node.label);
            //indent += last ? "   " : "|  ";


        }

        public void WriteClassTable(ASTNode node, List<ASTNode> rootNodes)
        {
            var defnitionNode = node.Children.Find(x => x.label == "id");
            File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsymboltables"), "| class     | " + defnitionNode.value + "\n");
            File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsymboltables"), "|    ==============================================================================\n");
            File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsymboltables"), "|     | table: " + defnitionNode.value + "\n");
            File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsymboltables"), "|    ==============================================================================\n");

            var inheritNode = node.Children.Find(x => x.label == "inheritList");
            if (inheritNode.Children != null && inheritNode.Children.Count > 0)
            {
                string inheritText = "|     | inherit     | ";
                foreach (var inheritChildNode in inheritNode.Children)
                {
                    inheritText = inheritText + inheritChildNode.value;
                }
                File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsymboltables"), inheritText + "\n");
            }
            else
                File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsymboltables"), "|     | inherit     | none \n");
            WriteClassvariable(node);
            WriteClassMethods(node, rootNodes, defnitionNode.value);
        }

        private void WriteClassvariable(ASTNode node)
        {
            foreach (var childNode in node.Children)
            {
                if (childNode.label == "varDecl")
                {
                    WriteVariableTable(childNode);
                }
                WriteClassvariable(childNode);
            }
        }

        private void WriteVariableTable(ASTNode childNode)
        {
            var defnitionNode = childNode.Children.Find(x => x.label == "id");
            var defnitionType = childNode.Children.Find(x => x.label == "type");
            if (defnitionNode != null && defnitionType != null)
            {
                File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsymboltables"), "|     | data      | " + defnitionNode.value + "      | " + defnitionType.value + "\n");
            }
        }

        private void WriteClassMethods(ASTNode node, List<ASTNode> rootNodes, string className)
        {
            foreach (var childNode in node.Children)
            {
                if (childNode.label == "funcDecl")
                {
                    WriteFunctionTable(childNode, rootNodes, className);
                }
                WriteClassMethods(childNode, rootNodes, className);
            }
        }

        public void WriteImplementationTable(ASTNode node)
        {
            var defnitionNode = node.Children.Find(x => x.label == "id");
            File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsymboltables"), "class\n");
            foreach (var childNode in node.Children)
            {
                if (childNode.label == "funcDef")
                {
                    WriteFunctionTable(childNode);
                }
            }
        }

        public void WriteFunctionTable(ASTNode node, List<ASTNode> rootNodes = null, string className = "")
        {
            if (className == string.Empty)
            {
                MethodWriting(node);
            }
            else
            {
                if (rootNodes != null)
                {
                    var implNodes = rootNodes.FindAll(x => x.label == "implDecl");
                    foreach (var childImplNode in implNodes)
                    {
                        if (childImplNode.Children.Find(y => y.label == "id").value == className)
                        {
                            var functionDefList = childImplNode.Children.Find(x => x.label == "funcDefList").Children.FindAll(y => y.label == "funcDef");
                            foreach (var funcNode in functionDefList)
                            {
                                MethodWriting(funcNode);
                            }
                        }
                    }
                }
            }
        }

        private void MethodWriting(ASTNode node)
        {
            var defnitionNode = node.Children.Find(x => x.label == "id");
            var defnitionType = node.Children.Find(x => x.label == "type");
            var defnitionParam = node.Children.Find(x => x.label == "fparamList");
            if (defnitionNode != null)
            {
                File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsymboltables"), "| function   | " + defnitionNode.value);
                if (defnitionParam != null && defnitionParam.Children.Count > 0)
                {
                    string fparamString = "     | (";
                    foreach (var paramNode in defnitionParam.Children)
                    {
                        if (paramNode.Children != null && paramNode.Children.Count > 0)
                        {
                            fparamString = fparamString + paramNode.Children.Find(x => x.label == "type").value + ", ";
                        }
                    }
                    fparamString = fparamString.Trim().TrimEnd(',');
                    fparamString = fparamString + ")";
                    File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsymboltables"), fparamString);
                }
                else
                    File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsymboltables"), "     | ()");
                if (defnitionType != null)
                    File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsymboltables"), ": " + defnitionType.value + "\n");
                File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsymboltables"), "|    ==============================================================================\n");
                File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsymboltables"), "|     | table: " + defnitionNode.value + "\n");
                File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsymboltables"), "|    ==============================================================================\n");
            }
            foreach (var childNode in node.Children)
            {
                if (childNode.label == "statementBlock")
                {
                    var prevNode = new ASTNode();
                    foreach (var variableNode in childNode.Children)
                    {

                        if (variableNode.label == "id")
                        {
                            File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsymboltables"), "|     | local      | " + variableNode.value + "      | " + prevNode.value + "\n");
                        }
                        prevNode = variableNode;
                    }
                }
            }
            File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsymboltables"), "===================================================================================\n");
        }

        public override void visit(TypeNode p_Node)
        {
            foreach (Node node in p_Node.Children)
            {
                node.Accept(this);
            }
        }

        public override void visit(IdNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(EpsilonNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(VisibilityNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(NumNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(IntlitNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(FloatnumNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(RelOpNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(AddOpNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(MultOpNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(MembDeclNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(RelExpNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(FparamNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(SignNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(NotNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(DotNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(FuncDefNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(FuncDeclNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(VarDeclNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(RelExprNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(ReturnStatementNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(WriteStatementNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(ReadStatementNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(WhileStatementNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(IfStatementNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(FcallStatementNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(VarNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(AssignStatementNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(ImplDeclNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(StructDeclNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(DimListNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(Var0Node p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(FunctionCallDataMemberNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(AParamsNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(IndiceListNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(StatementBlockNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(FparamListNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(FuncDefListNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(STRUCTORIMPLORFUNCLISTNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(InheritListNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(MemberListNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(VariableNode p_Node)
        {
            foreach (Node node in p_Node.Children) { node.Accept(this); }
        }

        public override void visit(ProgNode p_Node)
        {
            throw new NotImplementedException();
        }
    }
}
