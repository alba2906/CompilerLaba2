namespace Laba1.Core
{
    public sealed class LexicalError
    {
        public string Message { get; set; } = string.Empty;
        public int Line { get; set; }
        public int Column { get; set; }
        public char? InvalidChar { get; set; }

        public string LexemeRepresentation =>
            InvalidChar.HasValue ? InvalidChar.Value.ToString() : Message;

        public string Location => $"строка {Line}, {Column}-{Column}";
    }
}
