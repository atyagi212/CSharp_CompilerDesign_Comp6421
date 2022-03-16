namespace LexDriver
{
    public class StatementBlockNode : Node
    {
        public override void Accept(Visitor visitor)
        {
            visitor.visit(this);
        }
    }
}