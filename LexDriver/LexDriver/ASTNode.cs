using System;
using System.Collections;
using System.Collections.Generic;
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
        public SymTab m_symtab { get; set; }

        public ASTNode(string label)
        {
            this.label = label;
            this.value = null;
            Children = new List<ASTNode>();
        }
        public ASTNode(string label, string value)
        {
            this.label = label;
            this.value = value;
            Children = new List<ASTNode>();
        }
        public ASTNode()
        {

        }
    }
}
