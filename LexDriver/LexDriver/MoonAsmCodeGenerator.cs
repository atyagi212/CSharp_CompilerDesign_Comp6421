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
        public Dictionary<string, string> VarOrParam;
        public MoonAsmCodeGenerator(string fileName)
        {
            this.fileName = fileName;
            this.VarOrParam = new Dictionary<string, string>();
            this.filePath = Path.Combine(@"/Users/akshattyagi/Downloads/LexDriver/LexDriver/OutputFiles", Path.GetFileNameWithoutExtension(fileName) + ".moon");
        }

        public void GenerateAssemblyCode()
        {
            foreach (var item in arrFileContent)
            {
                string codeBlockType = item.Split('#')[0].Trim();
                switch (codeBlockType)
                {
                    case "varDeclare":
                        GetVarDeclareAssembly(item);
                        break;

                    case "assignStatement":
                        GetAssignStatementAssembly(item);
                        break;

                    case "writeStatement":
                        GetWriteStatementAssembly(item);
                        break;

                    case "readStatement":
                        GetReadStatementAssembly(item);
                        break;
                }
            }
        }

        private void GetReadStatementAssembly(string item)
        {
            //throw new NotImplementedException();
        }

        private void GetWriteStatementAssembly(string item)
        {
            throw new NotImplementedException();
        }

        private void GetVarDeclareAssembly(string item)
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
            this.VarOrParam.Add(content.Split(' ')[1].Trim(), content.Split(' ')[0].Trim());
            int size = Common.dictMemSize[content.Split(' ')[0].Trim()];
            if (memorySize > 1)
                File.AppendAllText(filePath, content.Split(' ')[1].Trim() + "      res " + (size * memorySize) + "\n");
            else
                File.AppendAllText(filePath, content.Split(' ')[1].Trim() + "      res " + size + "\n");
        }

        private void GetAssignStatementAssembly(string item)
        {
            string content = item.Split('#')[1].Trim();
            string[] arrOperands = content.Split('=');
            int[] arrayIndexes = null;
            int arraysize = 0;
            if (arrOperands[0].Trim().Contains('['))
            {
                arrayIndexes = GetArrayIndexes(arrOperands[0].Trim());
                arraysize = arrayIndexes.Max();
            }
            if (arrOperands[1].Trim().Contains('+'))
            {
                GenerateAirthmeticAssembly(arrOperands[0].Trim(), arrOperands[1].Trim(), arrayIndexes, "+");
            }
            else if (arrOperands[1].Trim().Contains('-'))
            {
                GenerateAirthmeticAssembly(arrOperands[0].Trim(), arrOperands[1].Trim(), arrayIndexes, "-");
            }
            else if (arrOperands[1].Trim().Contains('*'))
            {
                GenerateAirthmeticAssembly(arrOperands[0].Trim(), arrOperands[1].Trim(), arrayIndexes, "*");
            }
            else if (arrOperands[1].Trim().Contains('/'))
            {
                GenerateAirthmeticAssembly(arrOperands[0].Trim(), arrOperands[1].Trim(), arrayIndexes, "/");
            }
            else if (Common.lstAlphabets.Contains(arrOperands[1].Trim()[0].ToString().ToLower().Trim()))
            {
                if (arrayIndexes != null && arrayIndexes.Length > 0)
                {
                    int reg = WriteArrayAssemblyCode(arrayIndexes, arraysize);
                    File.AppendAllText(filePath, "lw r" + reg + "," + arrOperands[1].Trim() + "(r0)" + "\n");
                    File.AppendAllText(filePath, "sw " + arrOperands[0].Split('[')[0].Trim() + "r" + (reg - 1) + ",r" + reg + "\n");
                }
                else
                {
                    File.AppendAllText(filePath, "lw r1," + arrOperands[1].Trim() + "(r0)" + "\n");
                    File.AppendAllText(filePath, "sw " + arrOperands[0].Trim() + "(r0),r1" + "\n");
                }
            }
            else
            {
                if (arrayIndexes != null && arrayIndexes.Length > 0)
                {
                    int reg = WriteArrayAssemblyCode(arrayIndexes, arraysize);
                    File.AppendAllText(filePath, "sw " + arrOperands[0].Split('[')[0].Trim() + "(r" + (reg - 1) + ")," + arrOperands[1].Trim() + "\n");
                }
                else
                {
                    File.AppendAllText(filePath, "sw " + arrOperands[0].Trim() + "(r0)," + arrOperands[1].Trim() + "\n");
                }
            }
        }

        private int[] GetArrayIndexes(string v1)
        {
            string temp = v1.Substring(v1.IndexOf('[') + 1).TrimEnd(']');
            string[] arrTemp = temp.Split("][");
            return arrTemp.Select(int.Parse).ToArray();
        }

        private void GenerateAirthmeticAssembly(string leftOperand, string rightOperands, int[] leftArrayIndexes, string opr)
        {
            string[] arrOperands = rightOperands.Split(opr);
            string oprName = Common.dictOprName[opr];
            int[] firstOperandReg = null;
            int[] secondOperandReg = null;
            int leftArraySize = 0;
            int rightOpr1ArraySize = 0;
            int rightOpr2ArraySize = 0;
            if (leftArrayIndexes != null && leftArrayIndexes.Length > 0)
                leftArraySize = leftArrayIndexes.Max();
            if (arrOperands[0].Trim().Contains('['))
            {
                firstOperandReg = GetArrayIndexes(arrOperands[0].Trim());
                rightOpr1ArraySize = firstOperandReg.Max();
            }
            if (arrOperands[1].Trim().Contains('['))
            {
                secondOperandReg = GetArrayIndexes(arrOperands[1].Trim());
                rightOpr2ArraySize = secondOperandReg.Max();
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
                            File.AppendAllText(filePath, "lw r1," + arrOperands[0].Trim().Split('[')[0] + "(r" + (result - 1) + ")" + "\n");
                        }
                        else
                            File.AppendAllText(filePath, "lw r1," + arrOperands[0].Trim() + "(r0)" + "\n");
                        if (secondOperandReg != null && secondOperandReg.Length > 0)
                        {
                            int result = WriteArrayAssemblyCode(secondOperandReg, rightOpr2ArraySize);
                            File.AppendAllText(filePath, "lw r2," + arrOperands[1].Trim().Split('[')[0] + "(r" + (result - 1) + ")" + "\n");
                        }
                        else
                            File.AppendAllText(filePath, "lw r2," + arrOperands[1].Trim() + "(r0)" + "\n");
                        File.AppendAllText(filePath, oprName + " r3,r1,r2" + "\n");
                        if (leftArrayIndexes != null && leftArrayIndexes.Length > 0)
                        {
                            int result = WriteArrayAssemblyCode(leftArrayIndexes, leftArraySize);
                            File.AppendAllText(filePath, "sw " + leftOperand.Split('[')[0] + "(r" + (result - 1) + "),r3" + "\n");
                        }
                        else
                            File.AppendAllText(filePath, "sw " + leftOperand + "(r0),r3" + "\n");
                    }
                    else
                    {
                        if (firstOperandReg != null && firstOperandReg.Length > 0)
                        {
                            int result = WriteArrayAssemblyCode(firstOperandReg, rightOpr1ArraySize);
                            File.AppendAllText(filePath, "lw r1," + arrOperands[0].Trim().Split('[')[0] + "(r" + (result - 1) + ")" + "\n");
                        }
                        else
                            File.AppendAllText(filePath, "lw r1," + arrOperands[0].Trim() + "(r0)" + "\n");
                        File.AppendAllText(filePath, oprName + "i r2,r1," + arrOperands[1].Trim() + "\n");

                        if (leftArrayIndexes != null && leftArrayIndexes.Length > 0)
                        {
                            int result = WriteArrayAssemblyCode(leftArrayIndexes, leftArraySize);
                            File.AppendAllText(filePath, "sw " + leftOperand.Split('[')[0] + "(r" + (result - 1) + "),r3" + "\n");
                        }
                        else
                            File.AppendAllText(filePath, "sw " + leftOperand + "(r0),r2" + "\n");
                    }
                }
                else if (Common.lstAlphabets.Contains(arrOperands[1].Trim()[0].ToString().ToLower().Trim()))
                {
                    File.AppendAllText(filePath, "lw r1," + arrOperands[1].Trim() + "(r0)" + "\n");
                    if (secondOperandReg != null && secondOperandReg.Length > 0)
                    {
                        int result = WriteArrayAssemblyCode(secondOperandReg, rightOpr2ArraySize);
                        File.AppendAllText(filePath, "lw r2," + arrOperands[1].Trim().Split('[')[0] + "(r" + (result - 1) + ")" + "\n");
                    }
                    else
                        File.AppendAllText(filePath, oprName + "i r2,r1," + arrOperands[0].Trim() + "\n");
                    if (leftArrayIndexes != null && leftArrayIndexes.Length > 0)
                    {
                        int result = WriteArrayAssemblyCode(leftArrayIndexes, leftArraySize);
                        File.AppendAllText(filePath, "sw " + leftOperand.Split('[')[0] + "(r" + (result - 1) + "),r3" + "\n");
                    }
                    else
                        File.AppendAllText(filePath, "sw " + leftOperand + "(r0),r2" + "\n");
                }
                else
                {
                    File.AppendAllText(filePath, "lw r1," + arrOperands[0].Trim() + "(r0)" + "\n");
                    File.AppendAllText(filePath, oprName + "i r2,r1," + arrOperands[1].Trim() + "\n");
                    File.AppendAllText(filePath, "sw " + leftOperand + "(r0),r2" + "\n");
                }
            }
        }

        private int WriteArrayAssemblyCode(int[] arrayIndexes, int arraysize)
        {
            int multiplyRegCount = arrayIndexes.Length + 1;
            for (int i = 1; i <= arrayIndexes.Length; i++)
            {
                File.AppendAllText(filePath, "lw r" + i + "," + arrayIndexes[i - 1] + "(r0)" + "\n");
            }
            for (int i = 1; i <= arrayIndexes.Length; i++)
            {
                File.AppendAllText(filePath, "muli r" + multiplyRegCount + ",r" + i + "," + (arrayIndexes[i - 1] * Common.dictMemSize["integer"] * arraysize) + "\n");
                arraysize = 1;
                multiplyRegCount = multiplyRegCount + 1;
            }
            int addRegCount = multiplyRegCount;
            for (int i = arrayIndexes.Length + 1; i <= multiplyRegCount; i = i * 2)
            {
                File.AppendAllText(filePath, "add r" + addRegCount + ",r" + i + ",r" + (i + 1) + "\n");
                addRegCount = addRegCount + 1;
            }
            return addRegCount;
        }
    }
}
