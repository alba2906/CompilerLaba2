namespace Laba1.Core
{
    public enum TokenType
    {
        UnsignedInteger,   // 1
        Identifier,        // 2
        KeywordNew,        // 3
        KeywordString,     // 4
        KeywordInt,        // 5
        StringLiteral,     // 6
        LessThan,          // 7
        GreaterThan,       // 8
        Comma,             // 9
        Assign,            // 10
        Whitespace,        // 11
        OpenBrace,         // 12
        CloseBrace,        // 13
        Semicolon,         // 14
        Error              // 99
    }
}
