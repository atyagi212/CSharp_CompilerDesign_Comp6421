namespace LexDriver
{
    public class StructDeclNode : Node
    {
        public override void Accept(Visitor visitor)
        {
            visitor.visit(this);
        }
    }
}