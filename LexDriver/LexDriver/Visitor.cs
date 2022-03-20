namespace LexDriver
{
    public abstract class Visitor
    {
        public abstract void visit(ProgNode p_Node);
        public abstract void visit(TypeNode p_Node);
        public abstract void visit(IdNode p_Node);
        public abstract void visit(EpsilonNode p_Node);
        public abstract void visit(VisibilityNode p_Node);
        public abstract void visit(NumNode p_Node);
        public abstract void visit(IntlitNode p_Node);
        public abstract void visit(FloatnumNode p_Node);
        public abstract void visit(RelOpNode p_Node);
        public abstract void visit(AddOpNode p_Node);
        public abstract void visit(MultOpNode p_Node);
        public abstract void visit(MembDeclNode p_Node);
        public abstract void visit(RelExpNode p_Node);
        public abstract void visit(FparamNode p_Node);
        public abstract void visit(SignNode p_Node);
        public abstract void visit(NotNode p_Node);
        public abstract void visit(DotNode p_Node);
        public abstract void visit(FuncDefNode p_Node);
        public abstract void visit(FuncDeclNode p_Node);
        public abstract void visit(VarDeclNode p_Node);
        public abstract void visit(RelExprNode p_Node);
        public abstract void visit(ReturnStatementNode p_Node);
        public abstract void visit(WriteStatementNode p_Node);
        public abstract void visit(ReadStatementNode p_Node);
        public abstract void visit(WhileStatementNode p_Node);
        public abstract void visit(IfStatementNode p_Node);
        public abstract void visit(FcallStatementNode p_Node);
        public abstract void visit(VarNode p_Node);
        public abstract void visit(AssignStatementNode p_Node);
        public abstract void visit(ImplDeclNode p_Node);
        public abstract void visit(StructDeclNode p_Node);
        public abstract void visit(DimListNode p_Node);
        public abstract void visit(Var0Node p_Node);
        public abstract void visit(AParamsNode p_Node);
        public abstract void visit(IndiceListNode p_Node);
        public abstract void visit(StatementBlockNode p_Node);
        public abstract void visit(FparamListNode p_Node);
        public abstract void visit(FuncDefListNode p_Node);
        public abstract void visit(STRUCTORIMPLORFUNCLISTNode p_Node);
        public abstract void visit(InheritListNode p_Node);
        public abstract void visit(MemberListNode p_Node);
        public abstract void visit(VariableNode p_Node);
    }
}