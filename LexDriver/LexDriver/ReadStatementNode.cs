namespace LexDriver
{
    public class ReadStatementNode : Node
    {
        public override void Accept(Visitor visitor)
        {
            visitor.visit(this);
        }
    }
}