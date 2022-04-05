using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LexDriver
{
    public class MoonAsmCodeGenerator
    {
        public static string[] arrFileContent;
        private string fileName;
        private string filePath;
        public Dictionary<string, int> VarOrParam;
        private bool isread;
        private string functionName;

        public MoonAsmCodeGenerator(string fileName)
        {
            this.fileName = fileName;
            this.VarOrParam = new Dictionary<string, int>();
            this.filePath = Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".moon");
        }

        public void GenerateAssemblyCode()
        {
            int nextContentToRead = 0;
            var funcDefCount = Array.FindAll(arrFileContent, x => x.Contains("start functionDef#")).Count();
            if (funcDefCount > 1)
            {
                File.AppendAllText(filePath, "j main" + "\n");
            }
            foreach (var item in arrFileContent.Select((value, index) => new { index, value }))
            {
                nextContentToRead = ProcessingTags(item.value, item.index, nextContentToRead, arrFileContent.ToList(), ref VarOrParam);
            }
            //File.AppendAllText(filePath, "hlt" + "\n");
            foreach (var item in VarOrParam)
            {
                int size = 0;
                int memorySize = 1;
                if (item.Key.Contains(','))
                {
                    size = Common.dictMemSize[item.Key.Split(',')[1].Trim()];
                    memorySize = Convert.ToInt32(item.Key.Split(',')[2].Trim());
                }
                else
                    size = item.Value;
                if (memorySize > 1)
                    File.AppendAllText(filePath, item.Key.Split(',')[0].Trim() + "      res " + (size * memorySize) + "\n");
                else
                    File.AppendAllText(filePath, item.Key + "      res " + size + "\n");
            }


        }

        private int GetIfStatementAssembly(int index, List<string> rootBlockCode)
        {
            Dictionary<string, int> temp = new Dictionary<string, int>();
            List<string> blockCode = new List<string>();
            int nextContentToRead = 0;
            int nextIndex = 0;
            for (int i = index + 1; i < rootBlockCode.Count; i++)
            {
                blockCode.Add(rootBlockCode[i]);
                if (rootBlockCode[i] == "end generateIfStatementCode#")
                {
                    index = i + 1;
                    break;
                }
            }

            foreach (var content in blockCode.Select((value, index) => new { index, value }))
            {
                nextContentToRead = ProcessingTags(content.value, content.index, nextContentToRead, blockCode, ref temp);
            }
            return index;
        }

        private int GetConditionAssembly(string value, int index, List<string> rootBlockCode)
        {
            string content = value.Split('#')[1].Trim();
            string compOpr = string.Empty;
            string[] operands = null;
            foreach (var item in Common.dictCompareOpr)
            {
                if (content.Contains(item.Key))
                {
                    compOpr = item.Value;
                    operands = content.Split(item.Key);
                    break;
                }

            }
            if (operands != null && operands.Length > 0 && !string.IsNullOrEmpty(compOpr))
            {
                if (compOpr != "not")
                {
                    if (Common.lstAlphabets.Contains(operands[0].Trim()[0].ToString().ToLower()) && Common.lstAlphabets.Contains(operands[1].Trim()[0].ToString().ToLower()))
                    {

                        if (rootBlockCode.Last() == "end generateWhileStatementCode")
                        {
                            File.AppendAllText(filePath, "gowhile1" + "\n");
                            File.AppendAllText(filePath, "lw r1," + (VarOrParam.ContainsKey(functionName + operands[0].Trim()) ? functionName + operands[0].Trim() : operands[0].Trim()) + "(r0)" + "\n");
                            File.AppendAllText(filePath, "lw r2," + (VarOrParam.ContainsKey(functionName + operands[1].Trim()) ? functionName + operands[1].Trim() : operands[1].Trim()) + "(r0)" + "\n");
                            File.AppendAllText(filePath, compOpr + " r3,r1,r2" + "\n");
                            File.AppendAllText(filePath, "bz r3,endwhile1" + "\n");
                            index = WhileStatementBlock1(index, rootBlockCode);
                        }
                        if (rootBlockCode.Last() == "end generateIfStatementCode#")
                        {
                            File.AppendAllText(filePath, "lw r1," + (functionName + operands[0].Trim()) + "(r0)" + "\n");
                            File.AppendAllText(filePath, "lw r2," + (functionName + operands[1].Trim()) + "(r0)" + "\n");
                            File.AppendAllText(filePath, compOpr + " r3,r1,r2" + "\n");
                            File.AppendAllText(filePath, "bz r3,block2" + "\n");
                            index = StatementBlock1(index, rootBlockCode);
                            index = StatementBlock2(index, rootBlockCode);
                        }

                    }
                    else
                    {

                        if (rootBlockCode.Last() == "end generateWhileStatementCode")
                        {
                            File.AppendAllText(filePath, "gowhile1" + "\n");
                            File.AppendAllText(filePath, "lw r1," + (VarOrParam.ContainsKey(functionName + operands[0].Trim()) ? functionName + operands[0].Trim() : operands[0].Trim()) + "(r0)" + "\n");
                            File.AppendAllText(filePath, compOpr + "i r2,r1," + operands[1].Trim() + "\n");
                            File.AppendAllText(filePath, "bz r2,endwhile1" + "\n");
                            index = WhileStatementBlock1(index, rootBlockCode);
                        }
                        if (rootBlockCode.Last() == "end generateIfStatementCode#")
                        {
                            File.AppendAllText(filePath, "lw r1," + (functionName + operands[0].Trim()) + "(r0)" + "\n");
                            File.AppendAllText(filePath, compOpr + "i r2,r1," + operands[1].Trim() + "\n");
                            File.AppendAllText(filePath, "bz r2,block2" + "\n");
                            index = StatementBlock1(index, rootBlockCode);
                            index = StatementBlock2(index, rootBlockCode);
                        }
                    }
                }
                else
                {

                }
                return index + 1;
            }
            return 0;
        }

        private int WhileStatementBlock1(int index, List<string> rootBlockCode)
        {
            if (!VarOrParam.ContainsKey("buf"))
                VarOrParam.Add("buf", 40);
            Dictionary<string, int> temp = new Dictionary<string, int>();
            List<string> blockCode = new List<string>();
            int nextContentToRead = 0;
            for (int i = index + 1; i < rootBlockCode.Count; i++)
            {
                if (rootBlockCode[i].Trim() == "start generateStatementBlockCode")
                    break;
                else
                    index = i;
            }
            for (int i = index + 1; i < rootBlockCode.Count; i++)
            {
                blockCode.Add(rootBlockCode[i]);
                if (rootBlockCode[rootBlockCode.Count - 1] == "end generateStatementBlockCode")
                    break;
                else
                    index = i;
            }
            foreach (var lstItem in blockCode.Select((value, index) => new { index, value }))
            {
                nextContentToRead = ProcessingTags(lstItem.value, lstItem.index, nextContentToRead, blockCode, ref temp);
            }
            File.AppendAllText(filePath, "j gowhile1" + "\n");
            File.AppendAllText(filePath, "endwhile1" + "\n");
            return index;
        }

        private int StatementBlock2(int index, List<string> rootBlockCode)
        {
            if (!VarOrParam.ContainsKey("buf"))
                VarOrParam.Add("buf", 40);
            Dictionary<string, int> temp = new Dictionary<string, int>();
            List<string> blockCode = new List<string>();
            int nextContentToRead = 0;
            for (int i = index + 1; i < rootBlockCode.Count; i++)
            {
                if (rootBlockCode[i].Trim() == "start block 2")
                    break;
                else
                    index = i;
            }
            for (int i = index + 1; i < rootBlockCode.Count; i++)
            {
                blockCode.Add(rootBlockCode[i]);
                if (rootBlockCode[i] == "end block 2")
                    break;
                else
                    index = i;
            }
            foreach (var lstItem in blockCode.Select((value, index) => new { index, value }))
            {
                nextContentToRead = ProcessingTags(lstItem.value, lstItem.index, nextContentToRead, blockCode, ref temp);
            }
            File.AppendAllText(filePath, "endif1" + "\n");
            return index;
        }

        private int StatementBlock1(int index, List<string> rootBlockCode)
        {
            Dictionary<string, int> temp = new Dictionary<string, int>();
            List<string> blockCode = new List<string>();
            int nextContentToRead = 0;
            for (int i = index + 1; i < rootBlockCode.Count; i++)
            {
                if (rootBlockCode[i].Trim() == "start block 1")
                    break;
                else
                    index = i;
            }
            for (int i = index + 1; i < rootBlockCode.Count; i++)
            {
                blockCode.Add(rootBlockCode[i]);
                if (rootBlockCode[i] == "end block 1")
                    break;
                else
                    index = i;
            }
            foreach (var lstItem in blockCode.Select((value, index) => new { index, value }))
            {
                nextContentToRead = ProcessingTags(lstItem.value, lstItem.index, nextContentToRead, blockCode, ref temp);
            }
            File.AppendAllText(filePath, "j endif1" + "\n");
            File.AppendAllText(filePath, "block2" + "\n");
            return index;
        }

        private void GetReadStatementAssembly(string item, Dictionary<string, int> dictVarParam)
        {
            isread = true;
            string content = item.Split('#')[1].Trim();
            if (!VarOrParam.ContainsKey("buf"))
            {
                File.AppendAllText(filePath, "getc r1" + "\n");
                if (content.Split(' ')[1].Contains('['))
                {
                    var keyExist = false;
                    int arrSize = 0;
                    int[] indexes = GetArrayIndexes(content.Split(' ')[1].Trim());
                    foreach (var keypair in dictVarParam)
                    {
                        if (keypair.Key.Split(',')[0].Trim() == content.Split(' ')[1].Split('[')[0].Trim())
                        {
                            keyExist = true;
                            arrSize = dictVarParam[keypair.Key];
                            break;
                        }
                    }
                    if (!keyExist)
                    {
                        dictVarParam = new Dictionary<string, int>();
                        dictVarParam = this.VarOrParam;
                        foreach (var keypair in dictVarParam)
                        {
                            if (keypair.Key.Split(',')[0].Trim() == content.Split(' ')[1].Split('[')[0].Trim())
                            {
                                keyExist = true;
                                arrSize = dictVarParam[keypair.Key];
                                break;
                            }
                        }
                    }
                    int reg = WriteArrayAssemblyCode(indexes, arrSize);
                    File.AppendAllText(filePath, "sw " + content.Split(' ')[1].Trim().Split('[')[0].Trim() + "(r" + (reg - 1) + "),r1" + "\n");
                }
                else
                {
                    File.AppendAllText(filePath, "sw " + content.Split(' ')[1].Trim() + "(r0),r1" + "\n");
                }
            }

        }

        private void GetWriteStatementAssembly(string item, Dictionary<string, int> dictVarParam)
        {
            string content = item.Split('#')[1].Trim();
            if (!VarOrParam.ContainsKey("buf"))
            {
                if (content.Split(' ')[1].Contains('['))
                {
                    var keyExist = false;
                    int arrSize = 0;
                    int[] indexes = GetArrayIndexes(content.Split(' ')[1].Trim());
                    foreach (var keypair in dictVarParam)
                    {
                        if (keypair.Key.Split(',')[0].Trim() == content.Split(' ')[1].Split('[')[0].Trim())
                        {
                            keyExist = true;
                            arrSize = dictVarParam[keypair.Key];
                            break;
                        }
                    }
                    if (!keyExist)
                    {
                        dictVarParam = new Dictionary<string, int>();
                        dictVarParam = this.VarOrParam;
                        foreach (var keypair in dictVarParam)
                        {
                            if (keypair.Key.Split(',')[0].Trim() == content.Split(' ')[1].Split('[')[0].Trim())
                            {
                                keyExist = true;
                                arrSize = dictVarParam[keypair.Key];
                                break;
                            }
                        }
                    }
                    int reg = WriteArrayAssemblyCode(indexes, arrSize);
                    File.AppendAllText(filePath, "lw r1," + content.Split(' ')[1].Trim().Split('[')[0].Trim() + "(r" + (reg - 1) + ")" + "\n");
                }
                else
                {
                    File.AppendAllText(filePath, "lw r1," + content.Split(' ')[1].Trim() + "(r0)" + "\n");
                }

                File.AppendAllText(filePath, "putc r1" + "\n");
            }
            else
            {
                File.AppendAllText(filePath, "sw -8(r14),r1" + "\n");
                File.AppendAllText(filePath, "addi r1, r0, buf" + "\n");
                File.AppendAllText(filePath, "sw -12(r14),r1" + "\n");
                File.AppendAllText(filePath, "jl     r15,intstr" + "\n");
                File.AppendAllText(filePath, "sw -8(r14),r13" + "\n");
                File.AppendAllText(filePath, "jl     r15,putstr" + "\n");
            }
        }

        private Dictionary<string, int> GetVarDeclareAssembly(string item, Dictionary<string, int> temp)
        {
            int memorySize = 1;
            string content = item.Split('#')[1].Trim();
            if (content.Contains('['))
            {
                int[] arrIndex = GetArrayIndexes(content.Split(' ')[1].Trim());
                foreach (var index in arrIndex)
                {
                    memorySize = memorySize * Convert.ToInt32(index);
                }
            }
            if (content.Split(' ')[1].Contains('['))
            {
                int[] indexes = GetArrayIndexes(content.Split(' ')[1].Trim());
                VarOrParam.Add(functionName + content.Split(' ')[1].Split('[')[0].Trim() + "," + content.Split(' ')[0].Trim() + "," + memorySize, indexes.Max());
            }
            else
            {
                VarOrParam.Add(functionName + content.Split(' ')[1].Trim(), Common.dictMemSize[content.Split(' ')[0].Trim()]);
            }

            return temp;
        }

        private void GetAssignStatementAssembly(string item, Dictionary<string, int> dictVarParam)
        {
            string content = item.Split('#')[1].Trim();
            string[] arrOperands = content.Split('=');
            int[] arrayIndexes = null;
            int arraysize = 0;
            if (arrOperands[0].Trim().Contains('['))
            {
                arrayIndexes = GetArrayIndexes(arrOperands[0].Trim());
                foreach (var keypair in dictVarParam)
                {
                    if (keypair.Key.Split(',')[0].Trim() == arrOperands[0].Split('[')[0].Trim())
                    {
                        arraysize = dictVarParam[keypair.Key];
                        break;
                    }
                }
            }
            if (arrOperands[1].Trim().Contains('+'))
            {
                GenerateAirthmeticAssembly(arrOperands[0].Trim(), arrOperands[1].Trim(), arrayIndexes, "+", dictVarParam);
            }
            else if (arrOperands[1].Trim().Contains('-'))
            {
                GenerateAirthmeticAssembly(arrOperands[0].Trim(), arrOperands[1].Trim(), arrayIndexes, "-", dictVarParam);
            }
            else if (arrOperands[1].Trim().Contains('*'))
            {
                GenerateAirthmeticAssembly(arrOperands[0].Trim(), arrOperands[1].Trim(), arrayIndexes, "*", dictVarParam);
            }
            else if (arrOperands[1].Trim().Contains('/'))
            {
                GenerateAirthmeticAssembly(arrOperands[0].Trim(), arrOperands[1].Trim(), arrayIndexes, "/", dictVarParam);
            }
            else if (Common.lstAlphabets.Contains(arrOperands[1].Trim()[0].ToString().ToLower().Trim()))
            {
                if (arrayIndexes != null && arrayIndexes.Length > 0)
                {
                    int reg = WriteArrayAssemblyCode(arrayIndexes, arraysize);
                    File.AppendAllText(filePath, "lw r" + reg + "," + (functionName + arrOperands[1].Trim()) + "(r0)" + "\n");
                    File.AppendAllText(filePath, "sw " + (functionName + arrOperands[0].Split('[')[0].Trim()) + "r" + (reg - 1) + ",r" + reg + "\n");
                }
                else
                {
                    File.AppendAllText(filePath, "lw r1," + (functionName + arrOperands[1].Trim()) + "(r0)" + "\n");
                    File.AppendAllText(filePath, "sw " + (functionName + arrOperands[0].Trim()) + "(r0),r1" + "\n");
                }
            }
            else
            {
                if (arrayIndexes != null && arrayIndexes.Length > 0)
                {
                    int reg = WriteArrayAssemblyCode(arrayIndexes, arraysize);
                    File.AppendAllText(filePath, "addi r1,r0," + arrOperands[1].Trim() + "\n");
                    File.AppendAllText(filePath, "sw " + (functionName + arrOperands[0].Split('[')[0].Trim()) + "(r" + (reg - 1) + "),r1" + "\n");
                }
                else
                {
                    File.AppendAllText(filePath, "addi r1,r0," + arrOperands[1].Trim() + "\n");
                    File.AppendAllText(filePath, "sw " + (functionName + arrOperands[0].Trim()) + "(r0),r1" + "\n");
                }
            }
        }

        private int[] GetArrayIndexes(string v1)
        {
            string temp = v1.Substring(v1.IndexOf('[') + 1).TrimEnd(']');
            string[] arrTemp = temp.Split("][");
            List<string> result = new List<string>();
            foreach (var item in arrTemp)
            {
                if (Common.lstAlphabets.Contains(item[0].ToString().ToLower()))
                    result.Add("0");
                else
                    result.Add(item);
            }
            return result.Select(int.Parse).ToArray();
        }

        private void GenerateAirthmeticAssembly(string leftOperand, string rightOperands, int[] leftArrayIndexes, string opr, Dictionary<string, int> dictVarParam)
        {
            string[] arrOperands = rightOperands.Split(opr);
            string oprName = Common.dictOprName[opr];
            int[] firstOperandReg = null;
            int[] secondOperandReg = null;
            int leftArraySize = 0;
            int rightOpr1ArraySize = 0;
            int rightOpr2ArraySize = 0;
            if (leftArrayIndexes != null && leftArrayIndexes.Length > 0)
            {
                foreach (var keypair in dictVarParam)
                {
                    if (keypair.Key.Split(',')[0].Trim() == leftOperand.Split('[')[0].Trim())
                    {
                        leftArraySize = dictVarParam[keypair.Key];
                        break;
                    }
                }
            }
            if (arrOperands[0].Trim().Contains('['))
            {
                firstOperandReg = GetArrayIndexes(arrOperands[0].Trim());
                foreach (var keypair in dictVarParam)
                {
                    if (keypair.Key.Split(',')[0].Trim() == arrOperands[0].Split('[')[0].Trim())
                    {
                        rightOpr1ArraySize = dictVarParam[keypair.Key];
                        break;
                    }
                }
            }
            if (arrOperands[1].Trim().Contains('['))
            {
                secondOperandReg = GetArrayIndexes(arrOperands[1].Trim());
                foreach (var keypair in dictVarParam)
                {
                    if (keypair.Key.Split(',')[0].Trim() == arrOperands[1].Split('[')[0].Trim())
                    {
                        rightOpr2ArraySize = dictVarParam[keypair.Key];
                        break;
                    }
                }
            }
            if (arrOperands.Length == 2)
            {
                if (Common.lstAlphabets.Contains(arrOperands[0].Trim()[0].ToString().ToLower().Trim()))
                {
                    if (Common.lstAlphabets.Contains(arrOperands[1].Trim()[0].ToString().ToLower().Trim()))
                    {
                        if (firstOperandReg != null && firstOperandReg.Length > 0)
                        {
                            int result = WriteArrayAssemblyCode(firstOperandReg, rightOpr1ArraySize);
                            File.AppendAllText(filePath, "lw r1," + (functionName + arrOperands[0].Trim().Split('[')[0]) + "(r" + (result - 1) + ")" + "\n");
                        }
                        else
                            File.AppendAllText(filePath, "lw r1," + (functionName + arrOperands[0].Trim()) + "(r0)" + "\n");
                        if (secondOperandReg != null && secondOperandReg.Length > 0)
                        {
                            int result = WriteArrayAssemblyCode(secondOperandReg, rightOpr2ArraySize);
                            File.AppendAllText(filePath, "lw r2," + (functionName + arrOperands[1].Trim().Split('[')[0]) + "(r" + (result - 1) + ")" + "\n");
                        }
                        else
                            File.AppendAllText(filePath, "lw r2," + (functionName + arrOperands[1].Trim()) + "(r0)" + "\n");
                        File.AppendAllText(filePath, oprName + " r3,r1,r2" + "\n");
                        if (leftArrayIndexes != null && leftArrayIndexes.Length > 0)
                        {
                            int result = WriteArrayAssemblyCode(leftArrayIndexes, leftArraySize);
                            File.AppendAllText(filePath, "sw " + (functionName + leftOperand.Split('[')[0]) + "(r" + (result - 1) + "),r3" + "\n");
                        }
                        else
                            File.AppendAllText(filePath, "sw " + (functionName + leftOperand) + "(r0),r3" + "\n");
                    }
                    else
                    {
                        if (firstOperandReg != null && firstOperandReg.Length > 0)
                        {
                            int result = WriteArrayAssemblyCode(firstOperandReg, rightOpr1ArraySize);
                            File.AppendAllText(filePath, "lw r1," + (functionName + arrOperands[0].Trim().Split('[')[0]) + "(r" + (result - 1) + ")" + "\n");
                        }
                        else
                            File.AppendAllText(filePath, "lw r1," + (functionName + arrOperands[0].Trim()) + "(r0)" + "\n");
                        File.AppendAllText(filePath, oprName + "i r2,r1," + arrOperands[1].Trim() + "\n");

                        if (leftArrayIndexes != null && leftArrayIndexes.Length > 0)
                        {
                            int result = WriteArrayAssemblyCode(leftArrayIndexes, leftArraySize);
                            File.AppendAllText(filePath, "sw " + (functionName + leftOperand.Split('[')[0]) + "(r" + (result - 1) + "),r3" + "\n");
                        }
                        else
                            File.AppendAllText(filePath, "sw " + (functionName + leftOperand) + "(r0),r2" + "\n");
                    }
                }
                else if (Common.lstAlphabets.Contains(arrOperands[1].Trim()[0].ToString().ToLower().Trim()))
                {
                    File.AppendAllText(filePath, "lw r1," + (functionName + arrOperands[1].Trim()) + "(r0)" + "\n");
                    if (secondOperandReg != null && secondOperandReg.Length > 0)
                    {
                        int result = WriteArrayAssemblyCode(secondOperandReg, rightOpr2ArraySize);
                        File.AppendAllText(filePath, "lw r2," + (functionName + arrOperands[1].Trim().Split('[')[0]) + "(r" + (result - 1) + ")" + "\n");
                    }
                    else
                        File.AppendAllText(filePath, oprName + "i r2,r1," + arrOperands[0].Trim() + "\n");
                    if (leftArrayIndexes != null && leftArrayIndexes.Length > 0)
                    {
                        int result = WriteArrayAssemblyCode(leftArrayIndexes, leftArraySize);
                        File.AppendAllText(filePath, "sw " + (functionName + leftOperand.Split('[')[0]) + "(r" + (result - 1) + "),r3" + "\n");
                    }
                    else
                        File.AppendAllText(filePath, "sw " + (functionName + leftOperand) + "(r0),r2" + "\n");
                }
                else
                {
                    File.AppendAllText(filePath, "addi r1,r0," + arrOperands[0].Trim() + "\n");
                    File.AppendAllText(filePath, oprName + "i r2,r1," + arrOperands[1].Trim() + "\n");
                    File.AppendAllText(filePath, "sw " + (functionName + leftOperand) + "(r0),r2" + "\n");
                }
            }
        }

        private int WriteArrayAssemblyCode(int[] arrayIndexes, int arraysize)
        {
            int tmp = 0;
            if (isread)
                tmp = 1;

            int multiplyRegCount = arrayIndexes.Length + 1 + tmp;
            List<int> multiplyIndexes = new List<int>();
            for (int i = 1; i <= arrayIndexes.Length; i++)
            {
                File.AppendAllText(filePath, "addi r" + (i + tmp) + ",r0," + arrayIndexes[i - 1] + "\n");
            }
            for (int i = 1; i <= arrayIndexes.Length; i++)
            {
                File.AppendAllText(filePath, "muli r" + multiplyRegCount + ",r" + i + "," + (arrayIndexes[i - 1] * Common.dictMemSize["integer"] * arraysize) + "\n");
                arraysize = 1;
                multiplyIndexes.Add(multiplyRegCount);
                multiplyRegCount = multiplyRegCount + 1;
            }
            int addRegCount = multiplyRegCount;
            for (int i = 1; i < multiplyIndexes.Count(); i = i + 1)
            {
                if (i == 1)
                {
                    File.AppendAllText(filePath, "add r" + addRegCount + ",r" + multiplyIndexes[i - 1] + ",r" + multiplyIndexes[i] + "\n");
                }
                else
                {
                    File.AppendAllText(filePath, "add r" + addRegCount + ",r" + multiplyIndexes[i] + ",r" + (addRegCount - 1) + "\n");
                }
                addRegCount = addRegCount + 1;
            }
            return addRegCount;
        }

        private int ProcessingTags(string value, int index, int nextContentToRead, List<string> blockCode, ref Dictionary<string, int> temp)
        {
            string codeBlockType = value.Split('#')[0].Trim();
            if (nextContentToRead == 0 || nextContentToRead == index)
            {
                nextContentToRead = 0;
                switch (codeBlockType)
                {
                    case "varDeclare":
                        {
                            temp = GetVarDeclareAssembly(value, temp);
                            break;
                        }

                    case "assignStatement":
                        GetAssignStatementAssembly(value, temp);
                        break;

                    case "writeStatement":
                        GetWriteStatementAssembly(value, temp);
                        break;

                    case "readStatement":
                        GetReadStatementAssembly(value, temp);
                        break;

                    case "condition":
                        {
                            nextContentToRead = GetConditionAssembly(value, index, blockCode);
                            break;
                        }

                    case "start generateIfStatementCode":
                        {
                            nextContentToRead = GetIfStatementAssembly(index, blockCode);
                            break;
                        }
                    case "start generateWhileStatementCode":
                        {
                            nextContentToRead = GetWhileStatementAssembly(index, blockCode);
                            break;
                        }

                    case "start functionDef":
                        {
                            nextContentToRead = GetFunctionDeclAssembly(index, blockCode);
                            break;
                        }

                    case "funcCall":
                        {
                            nextContentToRead = GetFunctionCallAssembly(index, blockCode, temp);
                            break;
                        }
                }
            }
            return nextContentToRead;
        }

        private int GetFunctionCallAssembly(int index, List<string> blockCode, Dictionary<string, int> temp)
        {
            string functionSignature = blockCode[index].Split('#')[1].Trim();
            if (blockCode[index].Split('#')[0].Trim() == "funcCall")
            {
                string functionName = functionSignature.Split(' ')[0].Trim();
                string[] parameters = functionSignature.Split(functionName)[1].Trim().Split(',');
                foreach (var item in parameters.Select((value, index) => new { index, value }))
                {
                    if (!string.IsNullOrEmpty(item.value))
                    {
                        if (Common.lstAlphabets.Contains(item.value[0].ToString().ToLower()))
                        {
                            if (item.value.Contains('['))
                            {
                                int[] arrIndexes = GetArrayIndexes(item.value);
                                int reg = WriteArrayAssemblyCode(arrIndexes, temp[item.value.Split('[')[0].Trim()]);
                                File.AppendAllText(filePath, "lw r" + (item.index + 1) + "," + item.value.Split('[')[0].Trim() + "(r" + (reg - 1) + ")" + "\n");
                                File.AppendAllText(filePath, "sw " + functionName + "p" + (item.index + 1) + "(r0),r" + (item.index + 1) + "\n");
                            }
                            else
                            {
                                File.AppendAllText(filePath, "lw r" + (item.index + 1) + "," + (this.functionName + item.value) + "(r0)" + "\n");
                                string key = VarOrParam.FirstOrDefault(kvp => kvp.Key.Contains(functionName)).Key;
                                File.AppendAllText(filePath, "sw " + key + "(r0),r" + (item.index + 1) + "\n");
                            }
                        }
                        else
                        {
                            File.AppendAllText(filePath, "lw r" + (item.index + 1) + "," + item.value + "(r0)" + "\n");
                            string key = VarOrParam.FirstOrDefault(kvp => kvp.Key.Contains(functionName)).Key;
                            File.AppendAllText(filePath, "sw " + key + "(r0),r" + (item.index + 1) + "\n");
                        }
                    }
                }
                File.AppendAllText(filePath, "jl r15," + functionName + "\n");


            }
            return index;
        }

        private int GetWhileStatementAssembly(int index, List<string> rootBlockCode)
        {
            Dictionary<string, int> temp = new Dictionary<string, int>();
            List<string> blockCode = new List<string>();
            int nextContentToRead = 0;
            int nextIndex = 0;
            for (int i = index + 1; i < rootBlockCode.Count; i++)
            {
                blockCode.Add(rootBlockCode[i]);
                if (rootBlockCode[i] == "end generateWhileStatementCode")
                {
                    index = i + 1;
                    break;
                }
            }

            foreach (var content in blockCode.Select((value, index) => new { index, value }))
            {
                nextContentToRead = ProcessingTags(content.value, content.index, nextContentToRead, blockCode, ref temp);
            }
            return index;
        }

        private int GetFunctionDeclAssembly(int index, List<string> rootBlockCode)
        {
            string functionSignature = rootBlockCode[index].Split('#')[1].Trim();
            functionName = functionSignature.Split(' ')[0].Trim();
            string[] parameters = functionSignature.Split(functionName)[1].Trim().Split(':')[0].Trim().Split(',');
            string returnType = functionSignature.Split(functionName)[1].Trim().Split(':')[1].Trim();
            File.AppendAllText(filePath, functionName + "\n");

            if (returnType.ToLower() != "void")
                File.AppendAllText(filePath, functionName + "res res " + Common.dictMemSize[returnType.ToLower()] + "\n");

            foreach (var item in parameters)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    if (item.Split(' ')[1].Trim().Contains('['))
                    {
                        VarOrParam.Add(functionName + item.Split(' ')[1].Trim().Split('[')[0], Common.dictMemSize[item.Split(' ')[0].Trim().ToLower()]);
                        //File.AppendAllText(filePath, functionName + item.Split(' ')[1].Trim().Split('[')[0] + " res " + Common.dictMemSize[item.Split(' ')[0].Trim().ToLower()] + "\n");
                    }
                    else
                    {
                        VarOrParam.Add(functionName + item.Split(' ')[1].Trim(), Common.dictMemSize[item.Split(' ')[0].Trim().ToLower()]);
                        //File.AppendAllText(filePath, functionName + item.Split(' ')[1].Trim() + " res " + Common.dictMemSize[item.Split(' ')[0].Trim().ToLower()] + "\n");
                    }
                }
            }
            List<string> blockCode = new List<string>();
            int nextContentToRead = 0;
            int nextIndex = 0;
            for (int i = index + 1; i < rootBlockCode.Count; i++)
            {
                blockCode.Add(rootBlockCode[i]);
                if (rootBlockCode[i].Contains("end functionDef#"))
                {
                    index = i + 1;
                    break;
                }
            }

            foreach (var content in blockCode.Select((value, index) => new { index, value }))
            {
                if (content.value.Contains("return "))
                {
                    File.AppendAllText(filePath, "lw r1," + content.value.Split("return ")[1].Trim() + "\n");
                    File.AppendAllText(filePath, "sw " + functionName + "res(r0),r1" + "\n");
                    File.AppendAllText(filePath, "jr r15" + "\n");
                }
                nextContentToRead = ProcessingTags(content.value, content.index, nextContentToRead, blockCode, ref this.VarOrParam);
            }
            //File.AppendAllText(filePath, "jr r15" + "\n");
            //File.AppendAllText(filePath, functionName + "\n");
            File.AppendAllText(filePath, "hlt" + "\n");

            return index;
        }
    }
}
