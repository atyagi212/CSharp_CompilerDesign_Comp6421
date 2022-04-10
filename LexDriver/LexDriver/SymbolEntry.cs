using System;
namespace LexDriver
{
    public class SymbolEntry
    {
        public SymbolEntry()
        {
        }
        public string NodeType { get; set; }
        public string Name { get; set; }
        public string ParameterName { get; set; }
        public string ParameterType { get; set; }
        public string ReturnType { get; set; }
        public string VariableName { get; set; }
        public string VariableType { get; set; }
        public string VarVisibility { get; set; }
        public string NodeVisibility { get; set; }
        public string Inherits { get; set; }
        public string ParentClass { get; set; }
        public int Size { get; set; }

        public SymbolEntry(string nodeType, string name, string parameterName, string parameterType, string returnType, string variableName, string variableType, string varVisibility, string nodeVisibility, string inherits, string parentClass)
        {
            NodeType = nodeType;
            Name = name;
            ParameterName = parameterName;
            ParameterType = parameterType;
            ReturnType = returnType;
            VariableName = variableName;
            VariableType = variableType;
            VarVisibility = varVisibility;
            NodeVisibility = nodeVisibility;
            Inherits = inherits;
            ParentClass = parentClass;
        }


        public override string ToString()
        {
            return "SymbolEntry{" +
                    "NodeType='" + NodeType + '\'' +
                    ", Name='" + Name + '\'' +
                    ", ParameterName='" + ParameterName + '\'' +
                    ", ParameterType='" + ParameterType + '\'' +
                    ", ReturnType='" + ReturnType + '\'' +
                    ", VariableName='" + VariableName + '\'' +
                    ", VariableType='" + VariableType + '\'' +
                    ", VarVisibility='" + VarVisibility + '\'' +
                    ", NodeVisibility='" + NodeVisibility + '\'' +
                    ", Inherits='" + Inherits + '\'' +
                    ", ParentClass='" + ParentClass + '\'' +
                    '}';
        }
    }


}
