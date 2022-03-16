namespace LexDriver
{
    public class WhileStatementNode : Node
    {
        public override void Accept(Visitor visitor)
        {
            visitor.visit(this);
        }
    }
}