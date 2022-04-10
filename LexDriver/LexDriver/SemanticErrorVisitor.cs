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
            Common.semanticErrors = new List<string>();
            GetRootCodeErrors(listImplementationNode);
            foreach (var node in listImplementationNode.Select((value, index) => new { index, value }))
            {
                switch (node.value.label)
                {
                    case "structDecl":
                        {
                            FindClassErrors(node.value, listImplementationNode, node.value);
                            break;
                        }
                    case "funcDef":
                        {
                            FindFunctionErrors(node.value, listImplementationNode, node.value);
                            break;
                        }
                }
            }
            return p_Node;

        }

        private void GetRootCodeErrors(List<ASTNode> listImplementationNode)
        {
            var classNodes = listImplementationNode.FindAll(x => x.label == "structDecl");
            string rootClass = string.Empty;
            string rootInheritClass = string.Empty;
            foreach (var node in classNodes)
            {
                var inheritNode = node;
                bool isInhetitsFlag = true;
                rootClass = node.Children.Find(x => x.label == "id").value;
                while (isInhetitsFlag)
                {
                    rootInheritClass = GetInheritClass(inheritNode, classNodes);
                    if (string.IsNullOrEmpty(rootInheritClass) || rootInheritClass == rootClass)
                        isInhetitsFlag = false;
                    else
                    {
                        inheritNode = classNodes.Where(x => x.Children.Find(y => y.label == "id").value == rootInheritClass).FirstOrDefault();
                    }
                }
                if (string.IsNullOrEmpty(rootInheritClass))
                    continue;
                else if (rootClass == rootInheritClass)
                    Common.semanticErrors.Add("[error] Circular class dependency (inheritance cycles): at line " + node.Children.Find(x => x.label == "id").line);
            }

        }

        private string GetInheritClass(ASTNode node, List<ASTNode> classNodes)
        {
            var inheritsNode = node.m_symtab.AsEnumerable().Where(x => x.Field<string>("Inherits") != string.Empty && x.Field<string>("Inherits") != null).Select(y => y.Field<string>("Inherits")).ToList();
            if (inheritsNode.Count > 0)
                return inheritsNode[0];
            else
                return string.Empty;
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
            if (isComplexVar != null)
            {
                var varName = isComplexVar.Children.Find(x => x.label == "id");
                var varNameType = rootNode.m_symtab.AsEnumerable().Where(x => x.Field<string>("VariableName") == varName.value).Select(y => y.Field<string>("VariableType")).FirstOrDefault();
                var paramType = rootNode.m_symtab.AsEnumerable().Where(x => x.Field<string>("ParameterName") == varName.value).Select(y => y.Field<string>("ParameterType")).FirstOrDefault();
                if (!string.IsNullOrEmpty(varNameType) && Common.dictMemSize.ContainsKey(varNameType))
                    Common.semanticErrors.Add("[error] '.' operator used on non-class type: at line " + varName.line);
                else if (!string.IsNullOrEmpty(paramType) && Common.dictMemSize.ContainsKey(paramType))
                    Common.semanticErrors.Add("[error] '.' operator used on non-class type: at line " + varName.line);

                //leftSideOutputType = GetComplexVarType(p_Node, parentClass, allNodes, rootNode);
            }

            if (isArrayVar.Count > 0)
            {
                var nonIntIndexes = isArrayVar.Find(x => x.label != "intlit");
                if (nonIntIndexes != null)
                    Common.semanticErrors.Add("[error] Array index is not an integer: at line " + nonIntIndexes.line);
                var varNameType = rootNode.m_symtab.AsEnumerable().Where(x => x.Field<string>("VariableName") == variableName.value).Select(y => y.Field<string>("VariableType")).SingleOrDefault();
                var paramType = rootNode.m_symtab.AsEnumerable().Where(x => x.Field<string>("ParameterName") == variableName.value).Select(y => y.Field<string>("ParameterType")).SingleOrDefault();
                if (!string.IsNullOrEmpty(varNameType))
                {
                    nonIntIndexes = isArrayVar.Find(x => x.label == "intlit");
                    varNameType = varNameType.Trim();
                    if ((varNameType.Split('[').Count() - 1) != isArrayVar.Count)
                    {
                        Common.semanticErrors.Add("[error] Use of array with wrong number of dimensions: at line " + nonIntIndexes.line);
                    }
                }
                else if (!string.IsNullOrEmpty(paramType))
                {
                    nonIntIndexes = isArrayVar.Find(x => x.label == "intlit");
                    paramType = paramType.Trim();
                    if ((paramType.Split('[').Count() - 1) != isArrayVar.Count)
                    {
                        Common.semanticErrors.Add("[error] Use of array with wrong number of dimensions: at line " + nonIntIndexes.line);
                    }
                }

                //leftSideOutputType = GetArrayVarType(p_Node, parentClass, allNodes, rootNode);
            }

            if (variableName != null && isComplexVar == null && isArrayVar.Count == 0)
                leftSideOutputType = GetVarType(variableName, parentClass, allNodes, rootNode);
            if (leftSideOutputType.Contains("[]"))
                leftSideOutputType = leftSideOutputType.Split('[')[0];


            if (parentClass == null)
            {
                var localorGlobal = dt.AsEnumerable().Where(x => x.Field<string>("ParameterName") == variableName.value || x.Field<string>("VariableName") == variableName.value);
                if (localorGlobal == null)
                {
                    Common.semanticErrors.Add("[error] Undeclared variable (check for existence of local variable): at line " + variableName.line);
                }
            }

            if (!string.IsNullOrEmpty(leftSideOutputType) && !string.IsNullOrEmpty(rightSideOutputType) && leftSideOutputType != rightSideOutputType)
            {
                Common.semanticErrors.Add("[error] Undeclared variable (check for existence of local variable): at line " + variableName.line);
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
                    Common.semanticErrors.Add("[error] multiply declaration of parameter: at line " + rootNode.line);
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
                    Common.semanticErrors.Add("[error] Undeclared variable (check for existence of local variable): at line " + variableName.line);
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
                var funcCallParam = p_Node.Children.Find(x => x.label == "aParams").Children;
                List<string> lstFuncCallTypes = GetFunctionCallTypes(funcCallParam, rootNode);
                List<string> lstFuncDefTypes = new List<string>();
                bool nameMatch = false;
                bool paramCountMatch = false;
                var funcDefParamCount = 0;
                var allFuncNodes = allNodes.FindAll(x => x.label == "funcDef");
                foreach (var node in allFuncNodes)
                {
                    if (node.Children.Find(x => x.label == "id").value == functionCallName.value)
                    {
                        nameMatch = true;
                        funcDefParamCount = node.m_symtab.AsEnumerable().Where(x => x.Field<string>("ParameterName") != string.Empty).ToList().Count;
                        lstFuncDefTypes = node.m_symtab.AsEnumerable().Where(x => x.Field<string>("ParameterName") != string.Empty).Select(y => y.Field<string>("ParameterType")).ToList();

                        if (funcCallParam.Count != funcDefParamCount && !paramCountMatch)
                        {
                            paramCountMatch = false;
                        }
                        else if (funcCallParam.Count == funcDefParamCount)
                        {
                            paramCountMatch = true;
                        }

                        if (lstFuncCallTypes.Count == lstFuncDefTypes.Count)
                        {
                            foreach (var item in lstFuncCallTypes.Select((val, index) => new { index, val }))
                            {
                                if (lstFuncDefTypes[item.index].Contains('['))
                                {
                                    if (item.val.Trim().Split('[')[0] != lstFuncDefTypes[item.index].Trim().Split('[')[0])
                                    {
                                        Common.semanticErrors.Add("[error] Function call with wrong type of parameters: at line " + functionCallName.line);
                                        break;
                                    }
                                    else if ((item.val.Trim().Split('[').Count() - 1) != (lstFuncDefTypes[item.index].Trim().Split('[').Count() - 1))
                                    {
                                        Common.semanticErrors.Add("[error] Array parameter using wrong number of dimensions: at line " + functionCallName.line);
                                        break;
                                    }
                                }
                                else
                                {
                                    if (item.val.Trim() != lstFuncDefTypes[item.index].Trim())
                                    {
                                        Common.semanticErrors.Add("[error] Function call with wrong type of parameters: at line " + functionCallName.line);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                if (!nameMatch)
                    Common.semanticErrors.Add("[error] Undeclared free function: at line " + functionCallName.line);
                if (!paramCountMatch && nameMatch)
                    Common.semanticErrors.Add("[error] Function call with wrong number of parameters: at line " + functionCallName.line);
            }


        }

        private List<string> GetFunctionCallTypes(List<ASTNode> funcCallParam, ASTNode rootNode)
        {
            List<string> lstFuncCallTypes = new List<string>();
            if (funcCallParam.Count > 0)
            {
                foreach (var node in funcCallParam)
                {
                    if (node.label == "functionCallDataMember")
                    {
                        var paramName = node.Children.Find(x => x.label == "var0").Children.Find(y => y.label == "id").value;
                        if (!string.IsNullOrEmpty(paramName))
                        {
                            var paramType = rootNode.m_symtab.AsEnumerable().Where(x => x.Field<string>("ParameterName") == paramName).Select(y => y.Field<string>("ParameterType")).ToList();
                            var varType = rootNode.m_symtab.AsEnumerable().Where(x => x.Field<string>("VariableName") == paramName).Select(y => y.Field<string>("VariableType")).ToList();
                            if (paramType.Count > 0)
                            {
                                lstFuncCallTypes.Add(paramType[0]);
                            }
                            else if (varType.Count > 0)
                            {
                                lstFuncCallTypes.Add(varType[0]);
                            }
                        }
                    }
                    else
                        lstFuncCallTypes.Add(Common.dictTypes[node.label]);
                }
            }
            return lstFuncCallTypes;
        }

        private void FindFunctionErrors(ASTNode node, List<ASTNode> allNodes, ASTNode rootNode)
        {
            var StatementBlockNode = node.Children.Find(x => x.label == "statementBlock");
            CheckFunctionGlobalErrors(node, allNodes);
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
                if (child.label == "varDecl")
                {
                    if (!Common.dictMemSize.ContainsKey(child.Children.Find(x => x.label == "type").value))
                    {
                        var getClassName = allNodes.Where(x => x.label == "structDecl").Where(y => y.Children.Find(z => z.label == "id").value == child.Children.Find(x => x.label == "type").value).ToList();
                        if (getClassName == null || getClassName.Count == 0)
                            Common.semanticErrors.Add("[error] undeclared class: at line " + child.Children.Find(x => x.label == "id").line);
                    }
                }
            }
            var paramsNode = node.Children.Find(x => x.label == "fparamList");
            if (paramsNode.Children.Count > 0)
            {
                foreach (var child in paramsNode.Children)
                {
                    if (!Common.dictMemSize.ContainsKey(child.Children.Find(x => x.label == "type").value))
                    {
                        var getClassName = allNodes.Where(x => x.label == "structDecl").Where(y => y.Children.Find(z => z.label == "id").value == child.Children.Find(x => x.label == "type").value);
                        if (getClassName == null)
                            Common.semanticErrors.Add("[error] undeclared class: at line " + child.Children.Find(x => x.label == "id").line);
                    }
                }
            }
        }

        private void CheckFunctionGlobalErrors(ASTNode node, List<ASTNode> allNodes)
        {
            var GroupByParam = node.m_symtab.AsEnumerable().GroupBy(e => e.Field<string>("ParameterName")).Select(d => new { d.Key, Count = d.Count() }).ToList();
            var GroupByVar = node.m_symtab.AsEnumerable().GroupBy(e => e.Field<string>("VariableName")).Select(d => new { d.Key, Count = d.Count() }).ToList();

            if (GroupByParam.Count > 0)
            {
                var result = GroupByParam.Where(x => x.Count > 1 && x.Key != string.Empty && x.Key != null).Select(y => y.Key).ToList();
                if (result.Count > 0)
                    Common.semanticErrors.Add("[error] multiply same name parameters: at line " + node.line);
            }

            if (GroupByVar.Count > 0)
            {
                var result = GroupByVar.Where(x => x.Count > 1 && x.Key != string.Empty && x.Key != null).Select(y => y.Key).ToList();
                foreach (var item in result)
                {
                    int lineNumber = 0;
                    var varNodes = node.Children.Find(x => x.label == "statementBlock").Children.FindAll(y => y.label == "varDecl");
                    foreach (var varNode in varNodes)
                    {
                        if (varNode.Children.Find(p => p.label == "id").value == item.ToString())
                            lineNumber = varNode.Children.Find(p => p.label == "id").line;
                    }
                    Common.semanticErrors.Add("[error] multiply declared identifier in function : at line " + lineNumber);
                }
            }

            var funcDecl = allNodes.Where(x => x.label == "funcDef" && x.Children.Find(y => y.label == "id").value == node.Children.Find(z => z.label == "id").value).ToList();

            if (funcDecl.Count() > 1)
            {
                int i = 1;
                while (i < funcDecl.Count())
                {
                    if ((funcDecl[i].Children.Find(x => x.label == "fparamList").Children.Count() == funcDecl[0].Children.Find(x => x.label == "fparamList").Children.Count())
                        && (funcDecl[i].Children.Find(x => x.label == "type").value == funcDecl[0].Children.Find(x => x.label == "type").value))
                    {
                        Common.semanticErrors.Add("[error] multiply declared free function: at line " + funcDecl[i].Children.Find(y => y.label == "id").line);
                    }
                    else
                        Common.semanticErrors.Add("[warning] overloaded free function: at line " + funcDecl[i].Children.Find(y => y.label == "id").line);
                    i++;
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
                    CheckforGlobalClassErrors(node, functionDefList, allNodes);
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

        private void CheckforGlobalClassErrors(ASTNode node, List<ASTNode> functionDefList, List<ASTNode> allNodes)
        {
            List<string> implementedFunction = new List<string>();
            List<string> declaredFunction = new List<string>();
            List<string> varDecl = new List<string>();
            ASTNode classNameNode = node.Children.Find(x => x.label == "id");
            string inheritsClass = node.m_symtab.AsEnumerable().Where(e => e.Field<string>("NodeType") == "Class" && e.Field<string>("Name") == classNameNode.value && e.Field<string>("Inherits") != "").Select(d => d.Field<string>("Inherits")).ToList().Distinct().SingleOrDefault();

            foreach (var implNode in functionDefList)
            {
                implementedFunction.Add(implNode.Children.Find(q => q.label == "id").value);
            }
            var memDeclNodes = node.Children.Find(x => x.label == "memberList").Children.FindAll(y => y.label == "membDecl");
            if (memDeclNodes != null)
            {
                foreach (var memNode in memDeclNodes)
                {
                    var funcNode = memNode.Children.Find(x => x.label == "funcDecl");
                    var varNode = memNode.Children.Find(x => x.label == "varDecl");
                    if (varNode != null)
                    {
                        if (varDecl.Count() > 0 && varDecl.Contains(varNode.Children.Find(y => y.label == "id").value))
                            Common.semanticErrors.Add("[error] multiply declared data member in class: at line " + varNode.Children.Find(y => y.label == "id").line);
                        else
                            varDecl.Add(varNode.Children.Find(y => y.label == "id").value);
                    }
                    if (funcNode != null)
                    {
                        if (declaredFunction.Count > 0 & declaredFunction.Contains(funcNode.Children.Find(y => y.label == "id").value))
                            Common.semanticErrors.Add("[warning] overloaded member function: at line " + funcNode.Children.Find(y => y.label == "id").line);
                        else
                            declaredFunction.Add(funcNode.Children.Find(y => y.label == "id").value);
                        if (!implementedFunction.Contains(funcNode.Children.Find(y => y.label == "id").value))
                            Common.semanticErrors.Add("[error] undefined member function declaration: at line " + funcNode.Children.Find(y => y.label == "id").line);

                        if (!string.IsNullOrEmpty(inheritsClass))
                        {
                            var inheritClassNode = allNodes.Where(p => p.label == "structDecl");
                            foreach (var inheritClass in inheritClassNode)
                            {
                                var inheritNode = inheritClass.Children.Find(z => z.label == "id").value;
                                if (inheritNode == inheritsClass)
                                {
                                    var inheritFunctNames = inheritClass.m_symtab.AsEnumerable().Where(x => x.Field<string>("NodeType") == "Function").Select(y => y.Field<string>("Name")).ToList().Distinct().ToList();
                                    if (inheritFunctNames != null && inheritFunctNames.Count > 0)
                                    {
                                        if (inheritFunctNames.Contains(funcNode.Children.Find(y => y.label == "id").value))
                                            Common.semanticErrors.Add("[warning] Overridden member function: at line " + funcNode.Children.Find(y => y.label == "id").line);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (var item in implementedFunction)
            {
                if (!declaredFunction.Contains(item))
                {
                    var listUndeclFunc = functionDefList.Where(x => x.Children.Find(y => y.label == "id").value == item);
                    foreach (var undeclFunc in listUndeclFunc)
                    {
                        Common.semanticErrors.Add("[error] undeclared member function definition: at line " + undeclFunc.Children.Find(y => y.label == "id").line);
                    }
                }
            }

            var classDecl = allNodes.Where(x => x.label == "structDecl" && x.Children.Find(y => y.label == "id").value == node.Children.Find(z => z.label == "id").value).ToList();
            if (classDecl.Count() > 1)
            {
                int i = 1;
                while (i < classDecl.Count())
                {
                    Common.semanticErrors.Add("[error] multiply declared class: at line " + classDecl[i].Children.Find(y => y.label == "id").line);
                    i++;
                }

            }

            var varNames = node.m_symtab.AsEnumerable().Where(e => e.Field<string>("NodeType") == "Class" && e.Field<string>("Name") == classNameNode.value).Select(d => d.Field<string>("VariableName")).ToList();

            var inheritVarNames = allNodes.Where(p => p.label == "structDecl").Where(y => y.Children.Find(z => z.label == "id").value == inheritsClass);
            foreach (var classNode in inheritVarNames)
            {
                var variableInherits = classNode.m_symtab.AsEnumerable().Where(x => x.Field<string>("NodeType") == "Class").Select(y => y.Field<string>("VariableName")).ToList().Distinct().ToList();
                var CommonVar = variableInherits.Intersect(varNames);
                if (CommonVar != null && CommonVar.Count() > 0)
                {
                    foreach (var varName in CommonVar)
                    {
                        if (memDeclNodes != null)
                        {
                            foreach (var memNode in memDeclNodes)
                            {
                                var varNode = memNode.Children.Find(x => x.label == "varDecl");
                                if (varNode != null)
                                {
                                    if (varNode.Children.Find(y => y.label == "id").value == varName)
                                        Common.semanticErrors.Add("[warning] shadowed inherited data member: at line " + varNode.Children.Find(y => y.label == "id").line);

                                }
                            }
                        }
                    }
                }
            }
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
