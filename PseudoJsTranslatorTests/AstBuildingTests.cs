using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PseudoJsTranslator.Ast;
using PseudoJsTranslator.Grammar;
using Program = PseudoJsTranslator.Program;

namespace PseudoJsTranslatorTests
{
    [TestClass]
    public class AstBuildingTests
    {
        [TestMethod]
        public void AstBuilding_Valid()
        {
            var path = Path.Combine("tests", "ast-building", "valid");
            var astBuilder = new AstBuilderVisitor();
            var asciiPrinter = new AsciiTreeBuilderVisitor();

            foreach (var testFile in Directory.GetFiles(path, "*.js", SearchOption.AllDirectories))
            {
                var parser = Program.CreateParser(File.ReadAllText(testFile));
                asciiPrinter.Visit(astBuilder.Visit(Program.TryToBuildParseTree(parser)));

                var expected = File.ReadAllText(Path.ChangeExtension(testFile, ".tree.ascii"));
                Assert.AreEqual(asciiPrinter.GetStringTree(), expected);
                asciiPrinter.ClearTree();
            }
        }

        [TestMethod]
        public void AstBuilding_Invalid_NotImplemented()
        {
            var path = Path.Combine("tests", "ast-building", "invalid", "not-implemented");
            var astBuilder = new AstBuilderVisitor();

            foreach (var testFile in Directory.GetFiles(path, "*.js", SearchOption.AllDirectories))
            {
                var parser = Program.CreateParser(File.ReadAllText(testFile));
                var parseTree = Program.TryToBuildParseTree(parser);
                Assert.ThrowsException<NotImplementedException>(() => astBuilder.Visit(parseTree));
            }
        }
    }
}