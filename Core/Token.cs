namespace Laba1.Core
{
    public sealed class Token
    {
        public int Code { get; set; }
        public TokenType Type { get; set; }
        public string Lexeme { get; set; } = string.Empty;
        public int Line { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }

        public string Location => $"строка {Line}, {StartColumn}-{EndColumn}";
    }
}

