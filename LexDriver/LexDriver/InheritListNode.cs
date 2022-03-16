namespace LexDriver
{
    public class InheritListNode : Node
    {
        public override void Accept(Visitor visitor)
        {
            visitor.visit(this);
        }
    }
}