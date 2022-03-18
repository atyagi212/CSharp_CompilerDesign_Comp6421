using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexDriver
{
    public class ASTNode
    {
        public string label { get; set; }
        public string value { get; set; }
        public List<ASTNode> Children { get; set; }
        public DataTable m_symtab { get; set; }
        public SymbolEntry entry { get; set; }
        public int line = -1;

        public ASTNode(string label)
        {
            this.label = label;
            this.value = null;
            Children = new List<ASTNode>();
        }
        public ASTNode(string label, string value,int line)
        {
            this.line = line;
            this.label = label;
            this.value = value;
            Children = new List<ASTNode>();
        }
        public ASTNode()
        {

        }

    }
}
