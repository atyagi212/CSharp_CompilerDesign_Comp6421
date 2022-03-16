namespace LexDriver
{
    public class STRUCTORIMPLORFUNCLISTNode : Node
    {
        public override void Accept(Visitor visitor)
        {
            visitor.visit(this);
        }
    }
}