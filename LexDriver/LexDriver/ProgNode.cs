namespace LexDriver
{
    public class ProgNode:Node
    {
        public SymTab m_symtab { get; set; }


        public override void Accept(Visitor visitor)
        {
            visitor.visit(this);
        }
    }
}