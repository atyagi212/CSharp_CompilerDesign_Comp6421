namespace LexDriver
{
    public class SymTab
    {
        private int v1;
        private string v2;
        private object p;

        public SymTab(int v1, string v2, object p)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.p = p;
        }
    }
}