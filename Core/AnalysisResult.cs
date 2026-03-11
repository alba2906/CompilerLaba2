using System.Collections.Generic;

namespace Laba1.Core
{
    public sealed class AnalysisResult
    {
        public List<Token> Tokens { get; } = new();
        public List<LexicalError> Errors { get; } = new();
        public bool HasErrors => Errors.Count > 0;
    }
}
