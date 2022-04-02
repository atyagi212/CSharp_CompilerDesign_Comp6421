using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LexDriver
{
    public class CodeGenerator
    {
        private string fileName;
        private StringBuilder moonContent;
        private string funcNameRoot;
        private int regCounter = 0;
        public CodeGenerator(string fileName)
        {
            this.fileName = fileName;
            this.moonContent = new StringBuilder();
        }

        public StringBuilder generate(ASTNode root)
        {
            ASTNode structOrImplOrFuncListNode = root.Children[0];
            traverseTree(structOrImplOrFuncListNode);
            return moonContent;
        }

        private void traverseTree(ASTNode node)
        {
            foreach (ASTNode child in node.Children)
            {
                if (child.label.Equals("funcDef"))
                {
                    funcNameRoot = child.Children.Find(x => x.label == "id").value;
                    generateFuncDefCode(child);
                }
            }
        }

        private void generateFuncDefCode(ASTNode node)
        {
            string funcName = null;
            string type = null;
            List<string> fparamList = null;
            node.Children.Reverse();
            foreach (ASTNode child in node.Children)
            {
                if (child.label.Equals("statementBlock"))
                {
                    moonContent.Append("\n" + "start functionDef# " + funcName + " " + String.Join(",", fparamList) + ":" + type + "\n");
                    generateStatementBlockCode(child);
                }
                else if (child.label.Equals("fparamList"))
                {
                    fparamList = getFparamList(child);
                }
                else if (child.label.Equals("id"))
                {
                    funcName = child.value;
                }
                else if (child.label.Equals("type"))
                {
                    type = child.value;
                }
            }
            moonContent.Append("end functionDef# " + funcName + " " + String.Join(",", fparamList) + "\n");
        }

        private List<string> getFparamList(ASTNode node)
        {
            List<string> fparamList = new List<string>();
            foreach (ASTNode child in node.Children)
            {
                fparamList.Add(getFparam(child));
            }
            return fparamList;
        }

        private string getFparam(ASTNode node)
        {
            string id = null;
            string type = null;
            string dimList = null;
            foreach (ASTNode child in node.Children)
            {
                if (child.label.Equals("dimList"))
                {
                    dimList = getDimList(child);
                }
                else if (child.label.Equals("type"))
                {
                    type = child.value;
                }
                else if (child.label.Equals("id"))
                {
                    id = child.value;
                }
            }
            return type + " " + id + dimList;
        }

        private void generateIfStatementCode(ASTNode node)
        {
            moonContent.Append("start generateIfStatementCode#" + "\n");
            node.Children.Reverse();
            int block = 1;
            foreach (ASTNode child in node.Children)
            {
                if (child.label.Equals("statementBlock"))
                {
                    moonContent.Append("start block " + block + "\n");
                    generateStatementBlockCode(child);
                    moonContent.Append("end block " + block + "\n");
                    block++;
                }
                else if (child.label.Equals("relExpr"))
                {
                    generateRelExprCode(child);
                }
            }
            moonContent.Append("end generateIfStatementCode#" + "\n");
        }

        private void generateRelExprCode(ASTNode node)
        {
            moonContent.Append("start generateRelExprCode" + "\n");
            string var1 = null;
            string var2 = null;
            string relOp = null;
            foreach (ASTNode child in node.Children)
            {
                if (child.label.Equals("intlit"))
                {
                    if (var1 == null)
                    {
                        var1 = child.value;
                    }
                    else
                    {
                        var2 = child.value;
                    }
                }
                else if (child.label.Equals("functionCallDataMember"))
                {
                    if (var1 == null)
                    {
                        var1 = getDataMember(child);
                    }
                    else
                    {
                        var2 = getDataMember(child);
                    }
                }
                else if (child.label.Equals("relOp"))
                {
                    relOp = child.value;
                }
                else if (child.label.Equals("multOp") || child.label.Equals("addOp"))
                {
                    if (var1 == null)
                    {
                        var1 = generateMultAddOp(child);
                    }
                    else
                    {
                        var2 = generateMultAddOp(child);
                    }
                }
            }
            moonContent.Append("condition# " + var2 + " " + relOp + " " + var1 + "\n");
            moonContent.Append("end generateRelExprCode" + "\n");
        }

        private void generateStatementBlockCode(ASTNode pnode)
        {
            moonContent.Append("start generateStatementBlockCode" + "\n");
            pnode.Children.Reverse();
            foreach (ASTNode node in pnode.Children)
            {
                if (node.label.Equals("writeStatement"))
                {
                    generateWriteStatementCode(node);
                }
                else if (node.label.Equals("readStatement"))
                {
                    generateReadStatementCode(node);
                }
                else if (node.label.Equals("varDecl"))
                {
                    generateVarDeclCode(node);
                }
                else if (node.label.Equals("assignStatement"))
                {
                    generateAssignStatementCode(node);
                }
                else if (node.label.Equals("ifStatement"))
                {
                    generateIfStatementCode(node);
                }
                else if (node.label.Equals("whileStatement"))
                {
                    generateWhileStatementCode(node);
                }
                else if (node.label.Equals("fcallStatement"))
                {
                    generateFcallStatementCode(node);
                }
                else if (node.label.Equals("returnStatement"))
                {
                    generateReturnStatement(node);
                }
            }
            moonContent.Append("end generateStatementBlockCode" + "\n");
        }

        private void generateReturnStatement(ASTNode node)
        {
            string var = null;
            foreach (ASTNode child in node.Children)
            {
                if (child.label.Equals("intlit"))
                {
                    var = child.value;
                }
                else if (child.label.Equals("functionCallDataMember"))
                {
                    var = getDataMember(child);
                }
            }
            moonContent.Append("return " + var + "\n");
        }

        private void generateFcallStatementCode(ASTNode node)
        {
            string fcallName = null;
            List<string> aParams = null;
            foreach (ASTNode child in node.Children)
            {
                if (child.label.Equals("aParams"))
                {
                    aParams = getAParams(child);
                }
                else if (child.label.Equals("var0"))
                {
                    fcallName = getVar0(child);
                }
            }
            moonContent.Append("funcCall# " + fcallName + " " + String.Join(",", aParams) + "\n");
        }

        private List<string> getAParams(ASTNode node)
        {
            List<string> aparams = new List<string>();
            foreach (ASTNode child in node.Children)
            {
                if (child.label.Equals("intlit"))
                {
                    aparams.Add(child.value);
                }
                else if (child.label.Equals("functionCallDataMember"))
                {
                    aparams.Add(getDataMember(child));
                }
            }
            return aparams;
        }

        private void generateWhileStatementCode(ASTNode node)
        {
            moonContent.Append("start generateWhileStatementCode" + "\n");
            node.Children.Reverse();
            foreach (ASTNode child in node.Children)
            {
                if (child.label.Equals("relExpr"))
                {
                    generateRelExprCode(child);
                }
                else if (child.label.Equals("statementBlock"))
                {
                    generateStatementBlockCode(child);
                }

            }
            moonContent.Append("end generateWhileStatementCode" + "\n");
        }

        private void generateAssignStatementCode(ASTNode node)
        {
            string var = null;
            string tempVar = null;
            foreach (ASTNode child in node.Children)
            {
                if (child.label.Equals("intlit"))
                {
                    tempVar = child.value;
                }
                else if (child.label.Equals("var"))
                {
                    var = getVariable(child);
                }
                else if (child.label.Equals("functionCallDataMember"))
                {
                    tempVar = getDataMember(child);
                }
                else if (child.label.Equals("multOp") || child.label.Equals("addOp"))
                {
                    tempVar = tempVar = generateMultAddOp(child);
                }
            }
            moonContent.Append("assignStatement# " + var + " = " + tempVar + "\n");
        }

        private string generateMultAddOp(ASTNode node)
        {
            string var1 = null;
            string var2 = null;
            string opr = null;
            foreach (ASTNode child in node.Children)
            {
                if (child.label.Equals("functionCallDataMember"))
                {
                    if (var1 == null)
                    {
                        var1 = getDataMember(child);
                    }
                    else
                    {
                        var2 = getDataMember(child);
                    }
                }
                else if (child.label.Equals("intlit"))
                {
                    if (var1 == null)
                    {
                        var1 = child.value;
                    }
                    else
                    {
                        var2 = child.value;
                    }
                }
                else if (child.label.Equals("opr"))
                {
                    opr = child.value;
                }
                else if (child.label.Equals("multOp") || child.label.Equals("addOp"))
                {
                    if (var1 == null)
                    {
                        var1 = generateMultAddOp(child);
                    }
                    else
                    {
                        var2 = generateMultAddOp(child);
                    }
                }
            }
            string tempvar = generateTempVar();
            if (node.label.Equals("multOp"))
            {
                moonContent.Append("assignStatement# " + tempvar + " = " + var2 + " " + opr + " " + var1 + "\n");
            }
            else if (node.label.Equals("addOp"))
            {

                moonContent.Append("assignStatement# " + tempvar + " = " + var2 + " " + opr + " " + var1 + "\n");
            }
            return tempvar;
        }
        private static long idCounter = 0;
        private string generateTempVar()
        {
            string tempVar = "temp" + idCounter++;
            moonContent.Append("varDeclare# integer " + tempVar + "\n");
            return tempVar;
        }

        private void generateVarDeclCode(ASTNode node)
        {
            string type = "";
            string id = "";
            string dimList = "";
            foreach (ASTNode child in node.Children)
            {
                if (child.label.Equals("dimList"))
                {
                    dimList = getDimList(child);
                }
                else if (child.label.Equals("type"))
                {
                    type = child.value;
                }
                else if (child.label.Equals("id"))
                {
                    id = child.value;
                }
            }

            moonContent.Append("varDeclare# " + type + " " + id + dimList + "\n");

        }

        private string getDimList(ASTNode node)
        {
            //memorySize = 1;
            StringBuilder sb = new StringBuilder();
            foreach (ASTNode child in node.Children)
            {
                //memorySize = memorySize * Convert.ToInt32(child.value.Trim());
                sb.Insert(0, "[" + child.value + "]");
            }
            return sb.ToString();
        }

        private void generateReadStatementCode(ASTNode node)
        {
            moonContent.Append("readStatement# " + "read " + getVariable(node.Children[0]) + "\n");
        }

        private void generateWriteStatementCode(ASTNode node)
        {
            //todo expr
            moonContent.Append("writeStatement# " + "write " + getDataMember(node.Children[0]) + "\n");
        }

        private string getDataMember(ASTNode node)
        {
            // todo
            return getVariable(node);
        }

        private string getVariable(ASTNode node)
        {
            string var0 = "";
            string indiceList = "";
            foreach (ASTNode child in node.Children)
            {
                if (child.label.Equals("var0"))
                {
                    var0 = getVar0(child);
                }
                else if (child.label.Equals("indiceList"))
                {
                    indiceList = getIndiceList(child);
                }
            }
            return var0 + indiceList;
        }

        private string getVar0(ASTNode node)
        {
            // todo
            return node.Children[0].value;
        }

        private string getIndiceList(ASTNode node)
        {
            StringBuilder sb = new StringBuilder();
            foreach (ASTNode child in node.Children)
            {
                if (child.label.Equals("intlit"))
                {
                    sb.Insert(0, "[" + child.value + "]");
                }
                else if (child.label.Equals("functionCallDataMember"))
                {
                    sb.Insert(0, "[" + getDataMember(child) + "]");
                }
            }
            return sb.ToString();
        }

    }
}
