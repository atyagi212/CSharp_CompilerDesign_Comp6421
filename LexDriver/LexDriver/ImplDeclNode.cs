namespace LexDriver
{
    public class ImplDeclNode : Node
    {
        public override void Accept(Visitor visitor)
        {
            visitor.visit(this);
        }
    }
}