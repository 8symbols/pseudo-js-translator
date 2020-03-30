using System;
using System.Collections.Generic;
using Antlr4.Runtime;

namespace PseudoJsTranslator.Grammar
{
    internal class ErrorListener : BaseErrorListener
    {
        private readonly List<SyntaxError> errors = new List<SyntaxError>();

        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line,
                                         int charPositionInLine, string msg, RecognitionException e)
        {
            errors.Add(new SyntaxError(recognizer, offendingSymbol, line, charPositionInLine, msg, e));
        }

        public override string ToString()
        {
            return string.Join('\n', errors);
        }
    }
}