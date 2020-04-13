using Antlr4.Runtime;
using System.Collections.Generic;

namespace PseudoJsTranslator.Grammar
{
    internal class ErrorListener : BaseErrorListener
    {
        private readonly List<SyntaxError> _errors = new List<SyntaxError>();

        public override void SyntaxError(
            IRecognizer recognizer,
            IToken offendingSymbol,
            int line,
            int charPositionInLine,
            string msg,
            RecognitionException e)
        {
            _errors.Add(new SyntaxError(recognizer, offendingSymbol, line, charPositionInLine, msg, e));
        }

        public override string ToString() => string.Join('\n', _errors);
    }
}