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
    }
}
