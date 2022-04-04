using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LexDriver
{
    public class SymbolTableGeneratorVisitor : Visitor
    {
        private string fileName;
        private string filePath;
        private string functionName;

        public SymbolTableGeneratorVisitor(string outputfilename)
        {
            this.fileName = outputfilename;
            this.filePath = Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsymboltables");
        }

        public ASTNode PrintTable(ASTNode p_Node)
        {
            var listImplementationNode = p_Node.Children.Find(x => x.label == "STRUCTORIMPLORFUNCLIST").Children;
            listImplementationNode.Reverse();
            foreach (var node in listImplementationNode)
            {
                node.m_symtab = InitializeDataTable();
                switch (node.label)
                {
                    case "structDecl":
                        {
                            ASTNode result = WriteClassTable(node, listImplementationNode);
                            break;
                        }
                    case "funcDef":
                        {
                            ASTNode result = WriteFunctionTable(node, listImplementationNode);
                            break;
                        }
                }
            }
            return p_Node;

        }

        public ASTNode WriteClassTable(ASTNode node, List<ASTNode> rootNodes)
        {
            node.entry = new SymbolEntry();
            var defnitionNode = node.Children.Find(x => x.label == "id");
            var classNodes = node.Children.Where(x => x.label == "memberList").SingleOrDefault().Children;
            int classMemory = 0;
            foreach (var memNode in classNodes)
            {
                foreach (var classvar in memNode.Children)
                {
                    if (classvar.label == "varDecl")
                        classMemory = classMemory + Common.dictMemSize[classvar.Children.Find(x => x.label == "type").value];
                }
            }
            File.AppendAllText(filePath, "| class     | " + defnitionNode.value + "|     | " + classMemory + "\n");
            File.AppendAllText(filePath, "|    ==============================================================================\n");
            File.AppendAllText(filePath, "|     | table: " + defnitionNode.value + "\n");
            File.AppendAllText(filePath, "|    ==============================================================================\n");

            var inheritNode = node.Children.Find(x => x.label == "inheritList");
            if (inheritNode.Children != null && inheritNode.Children.Count > 0)
            {
                string inheritText = "|     | inherit     | ";
                foreach (var inheritChildNode in inheritNode.Children)
                {
                    inheritText = inheritText + inheritChildNode.value;
                    node.entry.NodeType = "Class";
                    node.entry.Name = defnitionNode.value;
                    node.entry.Inherits = inheritChildNode.value;
                    node.entry.Size = classMemory;
                }
                File.AppendAllText(filePath, inheritText + "\n");
            }
            else
                File.AppendAllText(filePath, "|     | inherit     | none \n");

            node = WriteClassvariable(node, node);
            node = WriteClassMethods(node, rootNodes, node, defnitionNode.value);

            return node;
        }

        private ASTNode WriteClassvariable(ASTNode node, ASTNode rootClassNode)
        {
            var memListNode = node.Children.Find(x => x.label == "memberList");
            foreach (var childNode in memListNode.Children)
            {
                foreach (var child in childNode.Children)
                {
                    if (child.label == "varDecl")
                    {
                        var visibility = childNode.Children.Find(x => x.label == "visibility").value;
                        rootClassNode = WriteVariableTable(child, visibility, rootClassNode);
                    }
                }
            }
            return rootClassNode;
        }

        private ASTNode WriteVariableTable(ASTNode childNode, string visibility, ASTNode rootClassNode)
        {
            var defnitionNode = childNode.Children.Find(x => x.label == "id");
            var defnitionType = childNode.Children.Find(x => x.label == "type");
            if (defnitionNode != null && defnitionType != null)
            {
                rootClassNode.entry.VariableName = defnitionNode.value;
                rootClassNode.entry.VariableType = defnitionType.value;
                rootClassNode.entry.VarVisibility = visibility;
                rootClassNode.m_symtab = AddNewRow(rootClassNode.m_symtab, rootClassNode.entry);
                int arraySize = 1;
                //for (int i = 0; i < dimListNodeCount; i++)
                //{
                //    arraySize = arraySize * Convert.ToInt32(variableNode.Children[0].Children[i].value);
                //    dimListNode = dimListNode + "[" + variableNode.Children[0].Children[i].value + "]";
                //}
                File.AppendAllText(filePath, "|     | data      | " + defnitionNode.value + "      | " + defnitionType.value + "  | " + visibility + "      | " + (Common.dictMemSize[defnitionType.value.ToLower().Trim()] * arraySize) + "\n");
            }
            return rootClassNode;
        }

        private ASTNode WriteClassMethods(ASTNode node, List<ASTNode> rootNodes, ASTNode rootClassNode, string className)
        {
            var memListNode = node.Children.Find(x => x.label == "memberList");
            foreach (var childNode in memListNode.Children)
            {
                foreach (var child in childNode.Children)
                {
                    if (child.label == "funcDecl")
                    {
                        var visibility = childNode.Children.Find(x => x.label == "visibility").value;
                        var funcName = child.Children.Find(x => x.label == "id").value;
                        rootClassNode = WriteFunctionTable(child, rootNodes, rootClassNode, className, visibility, funcName);
                    }
                }
            }
            return rootClassNode;
        }


        public ASTNode WriteFunctionTable(ASTNode node, List<ASTNode> rootNodes = null, ASTNode rootClassNode = null, string className = "", string visibility = "", string funcName = "")
        {
            functionName = string.Empty;
            if (className == string.Empty)
            {
                string[] functionTable = File.ReadAllLines(filePath);
                return MethodWriting(node, null, "", "", rootNodes);
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
                                if (funcNode.Children.Find(x => x.label == "id").value == funcName)
                                    rootClassNode = MethodWriting(funcNode, rootClassNode, className, visibility, rootNodes);
                            }
                        }
                    }
                }
                return rootClassNode;
            }
        }

        private ASTNode MethodWriting(ASTNode node, ASTNode rootClassNode = null, string ClassName = "", string visibility = "", List<ASTNode> allNodes = null)
        {
            var defnitionNode = node.Children.Find(x => x.label == "id");
            var defnitionType = node.Children.Find(x => x.label == "type");
            var defnitionParam = node.Children.Find(x => x.label == "fparamList");
            functionName = defnitionNode.value;
            ASTNode tempNode = new ASTNode();
            if (defnitionNode != null)
            {
                tempNode.entry = new SymbolEntry() { NodeType = "Function", Name = defnitionNode.value, NodeVisibility = visibility, ReturnType = defnitionType.value };
                File.AppendAllText(filePath, "| function   | " + defnitionNode.value);
                if (defnitionParam != null && defnitionParam.Children.Count > 0)
                {
                    string fparamString = "     | (";
                    foreach (var paramNode in defnitionParam.Children)
                    {
                        if (paramNode.Children != null && paramNode.Children.Count > 0)
                        {
                            fparamString = fparamString + paramNode.Children.Find(x => x.label == "type").value;
                            var DimListNodeCount = paramNode.Children.Find(x => x.label == "dimList").Children.Count;
                            if (DimListNodeCount > 0)
                            {
                                for (int i = 0; i < DimListNodeCount; i++)
                                {
                                    fparamString = fparamString + "[]";
                                }
                                fparamString = fparamString + " ,";
                            }
                            else
                                fparamString = fparamString + ", ";
                            tempNode.entry.ParameterName = paramNode.Children.Find(x => x.label == "id").value;
                            tempNode.entry.ParameterType = paramNode.Children.Find(x => x.label == "type").value;
                            for (int i = 0; i < DimListNodeCount; i++)
                            {
                                tempNode.entry.ParameterType = tempNode.entry.ParameterType + "[]";
                            }


                            if (rootClassNode == null)
                            {
                                node.entry = tempNode.entry;
                                node.m_symtab = AddNewRow(node.m_symtab, node.entry);
                            }
                            else
                            {
                                rootClassNode.entry = tempNode.entry;
                                rootClassNode.entry.ParentClass = ClassName;
                                rootClassNode.m_symtab = AddNewRow(rootClassNode.m_symtab, rootClassNode.entry);
                            }


                        }
                    }
                    fparamString = fparamString.Trim().TrimEnd(',');
                    fparamString = fparamString + ")";
                    File.AppendAllText(filePath, fparamString);
                }
                else
                    File.AppendAllText(filePath, "     | ()");
                if (defnitionType != null)
                    File.AppendAllText(filePath, ": " + defnitionType.value);

                if (!string.IsNullOrEmpty(visibility))
                    File.AppendAllText(filePath, "  | " + visibility);
                //else
                //    File.AppendAllText(filePath, "\n");

                var nodesForMemory = node.Children.Where(x => x.label == "statementBlock").SingleOrDefault().Children.Where(y => y.label == "varDecl");
                int memorySize = 0;
                foreach (var nodeMem in nodesForMemory)
                {
                    if (nodeMem.Children[0].Children.Count > 0)
                    {
                        int arraySize = 1;
                        foreach (var nodeArr in nodeMem.Children[0].Children)
                        {
                            arraySize = arraySize * Convert.ToInt32(nodeArr.value);
                        }
                        memorySize = memorySize + (arraySize * Common.dictMemSize[nodeMem.Children[1].value]);
                    }
                    else
                    {
                        if (Common.dictMemSize.ContainsKey(nodeMem.Children[1].value))
                            memorySize = memorySize + Common.dictMemSize[nodeMem.Children[1].value];
                        else
                        {
                            var classNodes = allNodes.Where(x => x.label == "structDecl");
                            foreach (var classNode in classNodes)
                            {
                                if (classNode.Children.Where(x => x.label == "id").SingleOrDefault().value == nodeMem.Children[1].value)
                                {
                                    foreach (DataRow row in classNode.m_symtab.Rows)
                                    {
                                        if (row["NodeType"].ToString() == "Class" && row["Name"].ToString() == nodeMem.Children[1].value)
                                        {
                                            memorySize = memorySize + Convert.ToInt32(row["Size"].ToString());
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                File.AppendAllText(filePath, "      | " + memorySize + "\n");

                File.AppendAllText(filePath, "|    ==============================================================================\n");
                File.AppendAllText(filePath, "|     | table: " + defnitionNode.value + "\n");
                File.AppendAllText(filePath, "|    ==============================================================================\n");

            }
            foreach (var childNode in node.Children)
            {
                if (childNode.label == "statementBlock")
                {
                    foreach (var variableNode in childNode.Children)
                    {
                        if (variableNode.label == "varDecl")
                        {
                            var variableNodeIndex = childNode.Children.IndexOf(variableNode);
                            //var type = childNode.Children[variableNodeIndex - 1].value;
                            //var dimListNodeCount = childNode.Children[variableNodeIndex - 2].Children.Count;
                            string idVal = variableNode.Children[2].value;
                            string type = variableNode.Children[1].value;
                            int dimListNodeCount = variableNode.Children[0].Children.Count;
                            string dimListNode = string.Empty;
                            int arraySize = 1;
                            for (int i = 0; i < dimListNodeCount; i++)
                            {
                                arraySize = arraySize * Convert.ToInt32(variableNode.Children[0].Children[i].value);
                                dimListNode = dimListNode + "[" + variableNode.Children[0].Children[i].value + "]";
                            }
                            int memorySize = 0;
                            if (Common.dictMemSize.ContainsKey(type.ToLower().Trim()))
                            {
                                memorySize = Common.dictMemSize[type.ToLower().Trim()];
                            }
                            else
                            {
                                var classNodes = allNodes.Where(x => x.label == "structDecl");
                                foreach (var classNode in classNodes)
                                {
                                    if (classNode.Children.Where(x => x.label == "id").SingleOrDefault().value == type)
                                    {
                                        foreach (DataRow row in classNode.m_symtab.Rows)
                                        {
                                            if (row["NodeType"].ToString() == "Class" && row["Name"].ToString() == type)
                                            {
                                                memorySize = memorySize + Convert.ToInt32(row["Size"].ToString());
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            File.AppendAllText(filePath, "|     | local      | " + idVal + "      | " + type + dimListNode + "      | " + (memorySize * arraySize) + "\n");

                            if (rootClassNode == null)
                            {
                                if (node.entry == null)
                                    node.entry = tempNode.entry;
                                node.entry.ParameterType = string.Empty;
                                node.entry.ParameterName = string.Empty;
                                node.entry.VariableName = idVal;
                                node.entry.VariableType = type + dimListNode;
                                node.m_symtab = AddNewRow(node.m_symtab, node.entry);
                            }
                            else
                            {
                                if (rootClassNode.entry == null)
                                    rootClassNode.entry = tempNode.entry;
                                rootClassNode.entry.ParentClass = ClassName;
                                rootClassNode.entry.ParameterType = string.Empty;
                                rootClassNode.entry.ParameterName = string.Empty;
                                rootClassNode.entry.VariableName = variableNode.value;
                                rootClassNode.entry.VariableType = type + dimListNode;
                                rootClassNode.m_symtab = AddNewRow(rootClassNode.m_symtab, rootClassNode.entry);
                            }
                        }
                    }
                }
            }
            File.AppendAllText(filePath, "===================================================================================\n");
            if (rootClassNode != null)
                return rootClassNode;
            else
                return node;
        }

        public override void visit(TypeNode p_Node)
        {
            foreach (Node node in p_Node.Children)
            {
                node.Accept(this);
            }
        }

        public DataTable InitializeDataTable()
        {
            DataTable table = new DataTable();
            PropertyInfo[] propertyInfo = typeof(LexDriver.SymbolEntry).GetProperties();
            foreach (PropertyInfo prop in propertyInfo)
            {
                table.Columns.Add(prop.Name);
            }
            return table;
        }

        public DataTable AddNewRow(DataTable table, SymbolEntry entryFields)
        {
            PropertyInfo[] propertyInfo = typeof(LexDriver.SymbolEntry).GetProperties();
            DataRow dr = table.NewRow();
            foreach (PropertyInfo prop in propertyInfo)
            {
                dr[prop.Name] = entryFields.GetType().GetProperty(prop.Name).GetValue(entryFields);
            }
            table.Rows.Add(dr);
            return table;
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
