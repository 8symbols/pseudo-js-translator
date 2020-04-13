#nullable enable

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using PseudoJsTranslator.Ast;
using PseudoJsTranslator.Grammar;
using System;

namespace PseudoJsTranslator
{
    public static class Program
    {
        public static void Main()
        {
            var parser = CreateParser("function foo() { return function(b) { return a + b } }");
            var parseTree = TryToBuildParseTree(parser);
            if (parseTree == null)
            {
                return;
            }
            var ast = TryToBuildAst(parseTree);
            if (ast == null)
            {
                return;
            }
            PrintAsciiTree(ast);
        }

        public static ECMAScriptParser CreateParser(string input)
        {
            var stream = new AntlrInputStream(input);
            var lexer = new ECMAScriptLexer(stream);
            lexer.SetStrictMode(true);
            var tokens = new CommonTokenStream(lexer);
            return new ECMAScriptParser(tokens)
            {
                BuildParseTree = true
            };
        }

        public static IParseTree? TryToBuildParseTree(ECMAScriptParser parser)
        {
            var listener = new ErrorsCollectorListener();
            parser.AddErrorListener(listener);
            var program = parser.program();
            if (parser.NumberOfSyntaxErrors != 0)
            {
                Console.Error.WriteLine(listener.ToString());
            }
            parser.RemoveErrorListener(listener);
            return (parser.NumberOfSyntaxErrors == 0) ? program : null;
        }

        public static Node? TryToBuildAst(IParseTree parseTree)
        {
            try
            {
                return new AstBuilderVisitor().Visit(parseTree);
            }
            catch (Exception e)
            {
                if (e.Data.Contains("Position"))
                {
                    Console.Error.Write(e.Data["Position"]);
                }
                Console.Error.WriteLine(e.Message);
                return null;
            }
        }

        public static void PrintAsciiTree(Node ast)
        {
            var asciiBuilder = new AsciiTreeBuilderVisitor();
            asciiBuilder.Visit(ast);
            Console.Write(asciiBuilder.GetStringTree());
        }
    }
}