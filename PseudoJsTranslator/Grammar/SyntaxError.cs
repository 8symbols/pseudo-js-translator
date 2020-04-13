using Antlr4.Runtime;

namespace PseudoJsTranslator.Grammar
{
    internal class SyntaxError
    {
        public IRecognizer Recognizer { get; }
        public IToken OffendingSymbol { get; }
        public int Line { get; }
        public int CharPositionInLine { get; }
        public string Msg { get; }
        public RecognitionException E { get; }

        public SyntaxError(
            IRecognizer recognizer,
            IToken offendingSymbol,
            int line,
            int charPositionInLine,
            string msg,
            RecognitionException e)
        {
            Recognizer = recognizer;
            OffendingSymbol = offendingSymbol;
            Line = line;
            CharPositionInLine = charPositionInLine;
            Msg = msg;
            E = e;
        }

        public override string ToString() => $"Error ({Line}:{CharPositionInLine}): {Msg}.";
    }
}