using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace LexDriver
{
    public class SemanticErrorVisitor : Visitor
    {
        private string fileName;
        public SemanticErrorVisitor(string outputfilename)
        {
            this.fileName = outputfilename;
        }

        public ASTNode PrintSemanticErrors(ASTNode p_Node)
        {
            var listImplementationNode = p_Node.Children.Find(x => x.label == "STRUCTORIMPLORFUNCLIST").Children;

            foreach (var node in listImplementationNode)
            {
                switch (node.label)
                {
                    case "structDecl":
                        {
                            FindClassErrors(node, listImplementationNode, node);
                            break;
                        }
                    case "funcDef":
                        {
                            FindFunctionErrors(node, listImplementationNode, node);
                            break;
                        }
                }
            }
            return p_Node;

        }

        private void CheckForAssignmentErrors(ASTNode p_Node, List<ASTNode> allNodes, ASTNode rootNode)
        {
            DataTable dt = new DataTable();
            string rightSideOutputType = string.Empty;
            string leftSideOutputType = string.Empty;
            var isComplexVar = p_Node.Children.Find(x => x.label == "var").Children.Find(y => y.label == "var0").Children.Find(z => z.label == "dot");
            var isArrayVar = p_Node.Children.Find(x => x.label == "var").Children.Find(y => y.label == "indiceList").Children;
            var variableName = p_Node.Children.Find(x => x.label == "var").Children.Find(y => y.label == "var0").Children.Find(z => z.label == "id");
            dt = rootNode.m_symtab;
            var parentClass = dt.AsEnumerable().Where(x => x.Field<string>("ParentClass") != string.Empty && x.Field<string>("ParentClass") != null).ToList();
            var rightSideNode = p_Node.Children[0].label;
            switch (rightSideNode)
            {
                case "intlit":
                    rightSideOutputType = "integer";
                    break;
                case "functionCallDataMember":
                    rightSideOutputType = GetOutputFCallDMemberType(p_Node, parentClass, allNodes, rootNode);
                    break;

                case "floatnum":
                    rightSideOutputType = "float";
                    break;

            }
            //if (isComplexVar != null)
            //    leftSideOutputType = GetComplexVarType(p_Node, parentClass, allNodes, rootNode);

            //if (isArrayVar.Children.Count>0)
            //    leftSideOutputType = GetArrayVarType(p_Node, parentClass, allNodes, rootNode);

            if (variableName != null && isComplexVar == null && isArrayVar.Count == 0)
                leftSideOutputType = GetVarType(variableName, parentClass, allNodes, rootNode);
            if (leftSideOutputType.Contains("[]"))
                leftSideOutputType = leftSideOutputType.Split('[')[0];


            if (parentClass == null)
            {
                var localorGlobal = dt.AsEnumerable().Where(x => x.Field<string>("ParameterName") == variableName.value || x.Field<string>("VariableName") == variableName.value);
                if (localorGlobal == null)
                {
                    File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsemanticerrors"), "Undeclared local variable: at line " + variableName.line + "\n");
                }
            }

            if (!string.IsNullOrEmpty(leftSideOutputType) && !string.IsNullOrEmpty(rightSideOutputType) && leftSideOutputType != rightSideOutputType)
            {
                File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsemanticerrors"), "Undeclared local variable: at line " + variableName.line + "\n");
            }
        }

        private string GetVarType(ASTNode variableName, List<DataRow> parentClass, List<ASTNode> allNodes, ASTNode rootNode)
        {
            if (parentClass.Count == 0)
            {
                var paramCount = rootNode.m_symtab.AsEnumerable().Where(x => x.Field<string>("ParameterName") == variableName.value).Select(y => y.Field<string>("ParameterType")).ToList();
                var varCount = rootNode.m_symtab.AsEnumerable().Where(x => x.Field<string>("VariableName") == variableName.value).Select(y => y.Field<string>("VariableType")).ToList();

                if (paramCount.Count > 0 && varCount.Count > 0)
                {
                    File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsemanticerrors"), "Multiple declaration of parameter: at line " + rootNode.line + "\n");
                    return string.Empty;
                }
                else if (paramCount.Count == 0 && varCount.Count > 0)
                {
                    return varCount[0].ToString();
                }
                else if (paramCount.Count > 0 && varCount.Count == 0)
                {
                    return paramCount[0].ToString();
                }
                else if (paramCount.Count == 0 && varCount.Count == 0)
                {
                    File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsemanticerrors"), "Undeclared local variable: at line " + variableName.line + "\n");
                    return string.Empty;
                }
                else
                    return string.Empty;

            }
            else
            {
                return string.Empty;
            }
        }

        private string GetArrayVarType(ASTNode p_Node, EnumerableRowCollection<DataRow> parentClass, List<ASTNode> allNodes, ASTNode rootNode)
        {
            throw new NotImplementedException();
        }

        private string GetComplexVarType(ASTNode p_Node, EnumerableRowCollection<DataRow> parentClass, List<ASTNode> allNodes, ASTNode rootNode)
        {
            throw new NotImplementedException();
        }

        private string GetOutputFCallDMemberType(ASTNode p_Node, List<DataRow> parentClass, List<ASTNode> allNodes, ASTNode rootNode)
        {
            var funcdatamemberNode = p_Node.Children[0];
            if (funcdatamemberNode.Children.Find(x => x.label == "var0").Children.Find(y => y.label == "dot") == null)
            {
                var variable = funcdatamemberNode.Children.Find(x => x.label == "var0").Children.Find(y => y.label == "id").value;
                if (parentClass.Count == 0)
                {
                    var row = rootNode.m_symtab.AsEnumerable().Where(x => x.Field<string>("VariableName") == variable || x.Field<string>("ParameterName") == variable).ToList();
                    if (row.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(row[0]["VariableType"].ToString()))
                            return row[0]["VariableType"].ToString();
                        else
                            return row[0]["ParameterType"].ToString();
                    }
                }
            }
            return string.Empty;
        }

        private string GetOutputExprType(ASTNode p_Node, EnumerableRowCollection<DataRow> parentClass, List<ASTNode> allNodes, ASTNode rootNode)
        {
            throw new NotImplementedException();
        }

        private void CheckForUndefinedErrors(ASTNode p_Node, List<ASTNode> allNodes, ASTNode rootNode)
        {
            //var listImplementationNode = p_Node.Children.Find(x => x.label == "STRUCTORIMPLORFUNCLIST").Children;
            var functionCallName = p_Node.Children.Find(x => x.label == "var0").Children.Find(y => y.label == "id");
            if (rootNode.Children.Find(x => x.label == "id").value != functionCallName.value)
            {
                bool nameMatch = false;
                var allFuncNodes = allNodes.FindAll(x => x.label == "funcDef");
                foreach (var node in allNodes)
                {
                    if (node.Children.Find(x => x.label == "id").value == functionCallName.value)
                        nameMatch = true;
                }
                if (!nameMatch)
                    File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsemanticerrors"), "Undeclared free function: at line " + functionCallName.line + " \n");
            }


        }

        private void FindFunctionErrors(ASTNode node, List<ASTNode> allNodes, ASTNode rootNode)
        {
            var StatementBlockNode = node.Children.Find(x => x.label == "statementBlock");
            CheckFunctionGlobalErrors(node);
            foreach (ASTNode child in StatementBlockNode.Children)
            {

                if (child.label == "assignStatement")
                {
                    CheckForAssignmentErrors(child, allNodes, rootNode);
                }
                if (child.label == "fcallStatement")
                {
                    CheckForUndefinedErrors(child, allNodes, rootNode);
                }
            }
        }

        private void CheckFunctionGlobalErrors(ASTNode node)
        {
            var GroupByParam = node.m_symtab.AsEnumerable().GroupBy(e => e.Field<string>("ParameterName")).Select(d => new { d.Key, Count = d.Count() }).ToList();
            var GroupByVar = node.m_symtab.AsEnumerable().GroupBy(e => e.Field<string>("VariableName")).Select(d => new { d.Key, Count = d.Count() }).ToList();

            if (GroupByParam.Count > 0)
            {
                var result = GroupByParam.Where(x => x.Count > 1 && x.Key != string.Empty && x.Key != null).Select(y => y.Key).ToList();
                if (result.Count > 0)
                    File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsemanticerrors"), "Multiple same name parameters: at line " + node.line + "\n");
            }

            if (GroupByVar.Count > 0)
            {
                var result = GroupByVar.Where(x => x.Count > 1 && x.Key != string.Empty && x.Key != null).Select(y => y.Key).ToList();
                foreach (var item in result)
                {
                    var lineNumber = node.Children.Find(x => x.label == "statementBlock").Children.FindAll(y => y.label == "id").Find(z => z.value == item.ToString()).line;
                    File.AppendAllText(Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".outsemanticerrors"), "Multiple same name variables: at line " + lineNumber + "\n");
                }
            }
        }

        private void FindClassErrors(ASTNode node, List<ASTNode> allNodes, ASTNode rootNode)
        {
            var implNodes = allNodes.FindAll(x => x.label == "implDecl");
            foreach (var childImplNode in implNodes)
            {
                if (childImplNode.Children.Find(y => y.label == "id").value == node.Children.Find(y => y.label == "id").value)
                {
                    var functionDefList = childImplNode.Children.Find(x => x.label == "funcDefList").Children.FindAll(y => y.label == "funcDef");
                    foreach (var funcNode in functionDefList)
                    {
                        var StatementBlockNode = funcNode.Children.Find(x => x.label == "statementBlock");
                        foreach (ASTNode child in StatementBlockNode.Children)
                        {

                            if (child.label == "assignStatement")
                            {
                                CheckForAssignmentErrors(child, allNodes, node);
                            }
                            if (child.label == "fcallStatement")
                            {
                                CheckForUndefinedErrors(child, allNodes, rootNode);
                            }
                        }

                    }
                }
            }
            //var memberList = node.Children.Find(x => x.label == "memberList");
            //foreach (ASTNode child in memberList.Children)
            //{
            //    if (child.label == "")
            //    {
            //        CheckForArrayIndexErrors(child);
            //        CheckForReturnTypeErrors(child);
            //        CheckForAssignmentErrors(child, allNodes, rootNode);
            //        CheckForUndefinedErrors(child, allNodes, rootNode);
            //    }
            //    FindClassErrors(child, allNodes, rootNode);
            //}
        }

        private void CheckForReturnTypeErrors(ASTNode p_Node)
        {
            throw new NotImplementedException();
        }

        private void CheckForArrayIndexErrors(ASTNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(ProgNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(TypeNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(IdNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(EpsilonNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(VisibilityNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(NumNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(IntlitNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(FloatnumNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(RelOpNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(AddOpNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(MultOpNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(MembDeclNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(RelExpNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(FparamNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(SignNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(NotNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(DotNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(FuncDefNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(FuncDeclNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(VarDeclNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(RelExprNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(ReturnStatementNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(WriteStatementNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(ReadStatementNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(WhileStatementNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(IfStatementNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(FcallStatementNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(VarNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(AssignStatementNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(ImplDeclNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(StructDeclNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(DimListNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(Var0Node p_Node)
        {
            throw new NotImplementedException();
        }


        public override void visit(AParamsNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(IndiceListNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(StatementBlockNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(FparamListNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(FuncDefListNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(STRUCTORIMPLORFUNCLISTNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(InheritListNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(MemberListNode p_Node)
        {
            throw new NotImplementedException();
        }

        public override void visit(VariableNode p_Node)
        {
            throw new NotImplementedException();
        }
    }
}
