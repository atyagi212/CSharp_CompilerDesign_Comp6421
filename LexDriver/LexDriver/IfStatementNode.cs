namespace LexDriver
{
    public class IfStatementNode : Node
    {
        public override void Accept(Visitor visitor)
        {
            visitor.visit(this);
        }
    }
}