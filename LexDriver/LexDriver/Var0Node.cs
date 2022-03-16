namespace LexDriver
{
    public class Var0Node : Node
    {
        public override void Accept(Visitor visitor)
        {
            visitor.visit(this);
        }
    }
}