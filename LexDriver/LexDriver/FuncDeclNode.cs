namespace LexDriver
{
    public class FuncDeclNode : Node
    {
        public override void Accept(Visitor visitor)
        {
            visitor.visit(this);
        }
    }
}