using Antlr4.Runtime;
using System.Collections.Generic;

namespace PseudoJsTranslator.Grammar
{
    public class ErrorsCollectorListener : BaseErrorListener
    {
        public List<SyntaxError> Errors { get; } = new List<SyntaxError>();

        public override void SyntaxError(
            IRecognizer recognizer,
            IToken offendingSymbol,
            int line,
            int charPositionInLine,
            string msg,
            RecognitionException e)
        {
            Errors.Add(new SyntaxError(recognizer, offendingSymbol, line, charPositionInLine, msg, e));
        }

        public override string ToString() => string.Join('\n', Errors);
    }
}