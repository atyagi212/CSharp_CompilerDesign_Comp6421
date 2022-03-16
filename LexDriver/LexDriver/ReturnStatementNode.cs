namespace LexDriver
{
    public class ReturnStatementNode : Node
    {
        public override void Accept(Visitor visitor)
        {
            visitor.visit(this);
        }
    }
}