namespace LexDriver
{
    public class FuncDefListNode : Node
    {
        public override void Accept(Visitor visitor)
        {
            visitor.visit(this);
        }
    }
}