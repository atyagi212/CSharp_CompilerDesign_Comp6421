using System;
using System.Collections.Generic;

namespace LexDriver
{
    public class Table
    {
        List<SymbolEntry> content;
        public Table()
        {
            this.content = new List<SymbolEntry>();
        }

        public void AddNewRow(SymbolEntry entryFields)
        {
            content.Add(new SymbolEntry(entryFields.NodeType, entryFields.Name, entryFields.ParameterName, entryFields.ParameterType, entryFields.ReturnType, entryFields.VariableName, entryFields.VariableType, entryFields.VarVisibility, entryFields.NodeVisibility, entryFields.Inherits, entryFields.ParentClass));
        }

        public override string ToString()
        {
            return "Table{" +
                    "content=" + content +
                    '}';
        }

        public List<string> getDataParentClass()
        {
            List<string> ll = new List<string>();
            foreach (SymbolEntry se in this.content)
            {
                if (se.ParentClass != null)
                    ll.Add(se.ParentClass);
            }
            return ll;
        }

        public List<string> getDataParameterNameVariableName(string variableName)
        {
            List<string> ll = new List<string>();
            foreach (SymbolEntry se in this.content)
            {
                if (se.ParameterName.Equals(variableName) || se.VariableName.Equals(variableName))
                    ll.Add(se.ParentClass);
            }
            return ll;
        }

        public List<string> getDataParameterName(string value)
        {
            List<string> ll = new List<string>();
            foreach (SymbolEntry se in this.content)
            {
                if (se.ParameterName == value)
                    ll.Add(se.ParameterName);
            }
            return ll;
        }

        public List<string> getDataVariableName(string value)
        {
            List<string> ll = new List<string>();
            foreach (SymbolEntry se in this.content)
            {
                if (se.VariableName == value)
                    ll.Add(se.VariableName);
            }
            return ll;
        }

        public Dictionary<string, int> getGroupByParam()
        {
            Dictionary<string, int> map = new Dictionary<string, int>();
            foreach (SymbolEntry se in this.content)
            {
                map.Add(se.ParameterName, getOrDefault(se.ParameterName) + 1);
            }
            return map;
        }

        private int getOrDefault(string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName.Trim()))
                return 0;
            else
                return Convert.ToInt32(parameterName.Trim());

        }

        public Dictionary<string, int> getGroupByVar()
        {
            Dictionary<string, int> map = new Dictionary<string, int>();
            foreach (SymbolEntry se in this.content)
            {
                map.Add(se.VariableName, getOrDefault(se.VariableName) + 1);
            }
            return map;
        }
    }
}
