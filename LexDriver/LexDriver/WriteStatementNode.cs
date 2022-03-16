namespace LexDriver
{
    public class WriteStatementNode : Node
    {
        public override void Accept(Visitor visitor)
        {
            visitor.visit(this);
        }
    }
}