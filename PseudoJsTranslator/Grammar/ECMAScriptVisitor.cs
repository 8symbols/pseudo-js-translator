using System;
using Antlr4.Runtime;

namespace PseudoJsTranslator.Grammar
{
    partial class ECMAScriptVisitor : ECMAScriptBaseVisitor<object>
    {
    }

    /// <summary>
    /// Part for features that aren't going to be implemented.
    /// </summary>
    partial class ECMAScriptVisitor : ECMAScriptBaseVisitor<object>
    {
        private string GenerateErrorMessage(ParserRuleContext context, string featureName) =>
            $"Error ({context.Start.Line}:{context.Start.Column}): feature '{featureName}' isn't going to be released." +
            " Consider using some normal software.";

        public override object VisitWithStatement(ECMAScriptParser.WithStatementContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context, "with statement"));
        }

        public override object VisitLabeledStatement(ECMAScriptParser.LabeledStatementContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context, "labeled statement"));
        }

        public override object VisitSwitchStatement(ECMAScriptParser.SwitchStatementContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context, "switch statement"));
        }

        public override object VisitThrowStatement(ECMAScriptParser.ThrowStatementContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context, "throw statement"));
        }

        public override object VisitTryStatement(ECMAScriptParser.TryStatementContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context, "try statement"));
        }

        public override object VisitDebuggerStatement(ECMAScriptParser.DebuggerStatementContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context, "debugger statement"));
        }

        public override object VisitLiteral(ECMAScriptParser.LiteralContext context)
        {
            if (context.RegularExpressionLiteral() != null)
            {
                throw new NotImplementedException(GenerateErrorMessage(context, "regular expression literal"));
            }
            return base.VisitLiteral(context);
        }
    }
}