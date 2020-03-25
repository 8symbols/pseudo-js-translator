using Antlr4.Runtime;
using PseudoJsTranslator.Grammar;
using System;
using System.IO;

namespace PseudoJsTranslator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var stream = new AntlrInputStream(File.ReadAllText("script.js"));
            var lexer = new ECMAScriptLexer(stream);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            tokens.Fill();
            foreach (var token in tokens.GetTokens())
            {
                Console.WriteLine(token.ToString());
            }

            var parser = new ECMAScriptParser(tokens)
            {
                BuildParseTree = true
            };
            var listener = new ErrorListener();
            parser.AddErrorListener(listener);
            var context = parser.program();
            if (parser.NumberOfSyntaxErrors != 0)
            {
                Console.Error.WriteLine(listener.ToString());
                return;
            }

            try
            {
                var visitor = new ECMAScriptVisitor();
                visitor.Visit(context);
                Console.WriteLine(context.ToStringTree());
            }
            catch (NotImplementedException e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }
    }
}