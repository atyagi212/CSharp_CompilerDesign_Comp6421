namespace LexDriver
{
    public class Token
    {
        public string _lexeme { get; set; }
        public string _content { get; set; }
        public int _lineNumber { get; set; }

        public Token(string lexeme, string content, int lineNumber)
        {
            _lexeme = lexeme;
            _content = content;
            _lineNumber = lineNumber;
        }

    }
}