using System.Text;

namespace Laba1.Core
{
    public sealed class LexicalAnalyzer
    {
        public AnalysisResult Analyze(string text)
        {
            var result = new AnalysisResult();

            if (text == null)
                text = string.Empty;

            int i = 0;
            int line = 1;
            int column = 1;

            while (i < text.Length)
            {
                char current = text[i];

                // Состояние 1: идентификатор / ключевое слово
                if (IsIdentifierStart(current))
                {
                    ReadIdentifierOrKeyword(text, ref i, ref line, ref column, result);
                    continue;
                }

                // Состояние 2: пробельные символы
                if (IsWhitespace(current))
                {
                    ReadWhitespace(text, ref i, ref line, ref column, result);
                    continue;
                }

                // Состояние 3: оператор =
                if (current == '=')
                {
                    result.Tokens.Add(CreateSingleCharToken(10, TokenType.Assign, "=", line, column));
                    Advance(current, ref i, ref line, ref column);
                    continue;
                }

                // Состояния 4-5: строковый литерал
                if (current == '"')
                {
                    ReadStringLiteral(text, ref i, ref line, ref column, result);
                    continue;
                }

                // Состояние 6: число
                if (char.IsDigit(current))
                {
                    ReadUnsignedInteger(text, ref i, ref line, ref column, result);
                    continue;
                }

                // Состояния 7-12: одиночные символы
                if (TryReadSingleSymbol(current, line, column, out Token? token))
                {
                    result.Tokens.Add(token);
                    Advance(current, ref i, ref line, ref column);
                    continue;
                }

                // ERROR
                result.Errors.Add(new LexicalError
                {
                    Message = $"Недопустимый символ '{current}'.",
                    Line = line,
                    Column = column,
                    InvalidChar = current
                });

                Advance(current, ref i, ref line, ref column);
            }

            return result;
        }

        private static void ReadIdentifierOrKeyword(
            string text,
            ref int i,
            ref int line,
            ref int column,
            AnalysisResult result)
        {
            int startLine = line;
            int startColumn = column;
            var sb = new StringBuilder();

            while (i < text.Length && IsIdentifierPart(text[i]))
            {
                sb.Append(text[i]);
                Advance(text[i], ref i, ref line, ref column);
            }

            string lexeme = sb.ToString();
            int code;
            TokenType type;

            switch (lexeme)
            {
                case "new":
                    code = 3;
                    type = TokenType.KeywordNew;
                    break;

                case "string":
                    code = 4;
                    type = TokenType.KeywordString;
                    break;

                case "int":
                    code = 5;
                    type = TokenType.KeywordInt;
                    break;

                default:
                    code = 2;
                    type = TokenType.Identifier;
                    break;
            }

            result.Tokens.Add(new Token
            {
                Code = code,
                Type = type,
                Lexeme = lexeme,
                Line = startLine,
                StartColumn = startColumn,
                EndColumn = column - 1
            });
        }

        private static void ReadWhitespace(
            string text,
            ref int i,
            ref int line,
            ref int column,
            AnalysisResult result)
        {
            int startLine = line;
            int startColumn = column;
            var sb = new StringBuilder();

            while (i < text.Length && IsWhitespace(text[i]))
            {
                sb.Append(text[i]);
                Advance(text[i], ref i, ref line, ref column);
            }

            int endColumn = line == startLine ? column - 1 : startColumn;

            result.Tokens.Add(new Token
            {
                Code = 11,
                Type = TokenType.Whitespace,
                Lexeme = EscapeWhitespace(sb.ToString()),
                Line = startLine,
                StartColumn = startColumn,
                EndColumn = endColumn
            });
        }

        private static void ReadStringLiteral(
            string text,
            ref int i,
            ref int line,
            ref int column,
            AnalysisResult result)
        {
            int startLine = line;
            int startColumn = column;
            var sb = new StringBuilder();

            sb.Append(text[i]);
            Advance(text[i], ref i, ref line, ref column);

            bool isClosed = false;

            while (i < text.Length)
            {
                char current = text[i];

                if (current == '\r' || current == '\n')
                {
                    result.Errors.Add(new LexicalError
                    {
                        Message = "Незакрытый строковый литерал.",
                        Line = startLine,
                        Column = startColumn
                    });
                    return;
                }

                sb.Append(current);
                Advance(current, ref i, ref line, ref column);

                if (current == '"')
                {
                    isClosed = true;
                    break;
                }
            }

            if (!isClosed)
            {
                result.Errors.Add(new LexicalError
                {
                    Message = "Незакрытый строковый литерал.",
                    Line = startLine,
                    Column = startColumn
                });
                return;
            }

            result.Tokens.Add(new Token
            {
                Code = 6,
                Type = TokenType.StringLiteral,
                Lexeme = sb.ToString(),
                Line = startLine,
                StartColumn = startColumn,
                EndColumn = column - 1
            });
        }

        private static void ReadUnsignedInteger(
            string text,
            ref int i,
            ref int line,
            ref int column,
            AnalysisResult result)
        {
            int startLine = line;
            int startColumn = column;
            var sb = new StringBuilder();

            while (i < text.Length && char.IsDigit(text[i]))
            {
                sb.Append(text[i]);
                Advance(text[i], ref i, ref line, ref column);
            }

            result.Tokens.Add(new Token
            {
                Code = 1,
                Type = TokenType.UnsignedInteger,
                Lexeme = sb.ToString(),
                Line = startLine,
                StartColumn = startColumn,
                EndColumn = column - 1
            });
        }

        private static bool TryReadSingleSymbol(char current, int line, int column, out Token? token)
        {
            token = null;

            switch (current)
            {
                case '<':
                    token = CreateSingleCharToken(7, TokenType.LessThan, "<", line, column);
                    return true;

                case '>':
                    token = CreateSingleCharToken(8, TokenType.GreaterThan, ">", line, column);
                    return true;

                case ',':
                    token = CreateSingleCharToken(9, TokenType.Comma, ",", line, column);
                    return true;

                case '{':
                    token = CreateSingleCharToken(12, TokenType.OpenBrace, "{", line, column);
                    return true;

                case '}':
                    token = CreateSingleCharToken(13, TokenType.CloseBrace, "}", line, column);
                    return true;

                case ';':
                    token = CreateSingleCharToken(16, TokenType.Semicolon, ";", line, column);
                    return true;

                default:
                    return false;
            }
        }

        private static Token CreateSingleCharToken(
            int code,
            TokenType type,
            string lexeme,
            int line,
            int column)
        {
            return new Token
            {
                Code = code,
                Type = type,
                Lexeme = lexeme,
                Line = line,
                StartColumn = column,
                EndColumn = column
            };
        }

        private static bool IsIdentifierStart(char c)
        {
            return char.IsLetter(c) || c == '_';
        }

        private static bool IsIdentifierPart(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }

        private static bool IsWhitespace(char c)
        {
            return c == ' ' || c == '\t' || c == '\r' || c == '\n';
        }

        private static void Advance(char c, ref int i, ref int line, ref int column)
        {
            i++;

            if (c == '\n')
            {
                line++;
                column = 1;
            }
            else if (c != '\r')
            {
                column++;
            }
        }

        private static string EscapeWhitespace(string value)
        {
            return value
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t")
                .Replace(" ", "␠");
        }
    }
}
