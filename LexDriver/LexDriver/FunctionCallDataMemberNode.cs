namespace LexDriver
{
    public class FunctionCallDataMemberNode : Node
    {
        public override void Accept(Visitor visitor)
        {
            visitor.visit(this);
        }
    }
}