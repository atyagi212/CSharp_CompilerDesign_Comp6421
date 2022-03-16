using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexDriver
{
    public abstract class Node
    {
        public string label { get; set; }
        public List<Node> Children { get; set; }
        public SymTab m_symtab { get; set; }

        public Node(string label)
        {
            this.label = label;
            Children = new List<Node>();
        }
        public Node()
        {

        }
        public abstract void Accept(Visitor visitor);
    }
}
