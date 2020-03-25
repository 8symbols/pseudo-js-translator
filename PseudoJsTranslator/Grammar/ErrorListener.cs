using System;
using System.Collections.Generic;
using Antlr4.Runtime;

namespace PseudoJsTranslator.Grammar
{
    internal class ErrorListener : BaseErrorListener
    {
        private readonly List<string> errors = new List<string>();

        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line,
                                         int charPositionInLine, string msg, RecognitionException e)
        {
            errors.Add($"Error ({line}:{charPositionInLine}): {msg}.");
        }

        public override string ToString()
        {
            return String.Join('\n', errors);
        }
    }
}