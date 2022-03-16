namespace LexDriver
{
    public class AddOpNode : Node
    {
        public override void Accept(Visitor visitor)
        {
            visitor.visit(this);
        }
    }
}