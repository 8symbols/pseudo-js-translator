#nullable enable
#pragma warning disable RCS1079  // Throwing of new NotImplementedException.

using Antlr4.Runtime;
using PseudoJsTranslator.Ast;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Antlr4.Runtime.Tree;

namespace PseudoJsTranslator.Grammar
{
    // Part for [partially] released features.
    public partial class AstBuilderVisitor : ECMAScriptBaseVisitor<Node>
    {
        /// <summary>
        /// Returns all context's children in a list of nodes.
        /// </summary>
        /// <typeparam name="T">Some subtype of <see cref="Node"/>.</typeparam>
        /// <param name="context">Context. May be null.</param>
        /// <returns>List of context's children or empty list if <paramref name="context"/> is null.</returns>
        private List<T> ListChildren<T>(ParserRuleContext? context) where T : Node
        {
            var nodes = new List<T>();
            if (context != null)
            {
                nodes.AddRange(context.children.Select(Visit).Cast<T>());
            }
            return nodes;
        }

        public override Node VisitIdentifierName(ECMAScriptParser.IdentifierNameContext context)
        {
            if (context.reservedWord() != null)
            {
                return VisitReservedWord(context.reservedWord());
            }
            return new Identifier(context, context.Identifier().Symbol.Text);
        }

        public override Node VisitReservedWord(ECMAScriptParser.ReservedWordContext context)
        {
            if (context.keyword() != null)
            {
                return Visit(context.keyword());
            }
            if (context.futureReservedWord() != null)
            {
                return Visit(context.futureReservedWord());
            }
            if (context.NullLiteral() != null)
            {
                return new NullLiteral(context);
            }
            return new BooleanLiteral(context, context.BooleanLiteral().Symbol.Text == "true");
        }

        public override Node VisitKeyword(ECMAScriptParser.KeywordContext context)
        {
            return new Identifier(context, ((ITerminalNode)context.GetChild(0)).Symbol.Text);
        }

        public override Node VisitFutureReservedWord(ECMAScriptParser.FutureReservedWordContext context)
        {
            return new Identifier(context, ((ITerminalNode)context.GetChild(0)).Symbol.Text);
        }

        public override Node VisitIdentifierExpression(ECMAScriptParser.IdentifierExpressionContext context)
        {
            return new Identifier(context, context.Identifier().Symbol.Text);
        }

        public override Node VisitLiteral(ECMAScriptParser.LiteralContext context)
        {
            if (context.RegularExpressionLiteral() != null)
            {
                throw new NotImplementedException(
                    GenerateErrorMessage(context, "regular expressions aren't supported."));
            }
            if (context.StringLiteral() != null)
            {
                return new StringLiteral(context, context.StringLiteral().Symbol.Text);
            }
            if (context.BooleanLiteral() != null)
            {
                return new BooleanLiteral(context, context.BooleanLiteral().Symbol.Text == "true");
            }
            if (context.NullLiteral() != null)
            {
                return new NullLiteral(context);
            }
            return Visit(context.numericLiteral());
        }

        public override Node VisitLiteralExpression(ECMAScriptParser.LiteralExpressionContext context)
        {
            return Visit(context.literal());
        }

        public override Node VisitNumericLiteral(ECMAScriptParser.NumericLiteralContext context)
        {
            if (context.DecimalLiteral() == null)
            {
                throw new NotImplementedException(
                    GenerateErrorMessage(context, "only decimal numbers are supported."));
            }

            double v;
            try
            {
                v = double.Parse(context.DecimalLiteral().Symbol.Text, CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                e.Data.Add("Position", GenerateErrorLocationMessage(context));
                throw;
            }

            return new NumericLiteral(context, v);
        }

        public override Node VisitProgram(ECMAScriptParser.ProgramContext context)
        {
            return new Ast.Program(context)
            {
                Body = ListChildren<Statement>(context.sourceElements())
            };
        }

        public override Node VisitExpressionStatement(ECMAScriptParser.ExpressionStatementContext context)
        {
            return new ExpressionStatement(context, (Expression)Visit(context.expressionSequence()));
        }

        public override Node VisitBlock(ECMAScriptParser.BlockContext context)
        {
            return new BlockStatement(context)
            {
                Body = ListChildren<Statement>(context.statementList())
            };
        }

        public override Node VisitFunctionBody(ECMAScriptParser.FunctionBodyContext context)
        {
            return new FunctionBody(context)
            {
                Body = ListChildren<Statement>(context.sourceElements())
            };
        }

        public override Node VisitEmptyStatement(ECMAScriptParser.EmptyStatementContext context)
        {
            return new EmptyStatement(context);
        }

        public override Node VisitReturnStatement(ECMAScriptParser.ReturnStatementContext context)
        {
            var node = new ReturnStatement(context);
            if (context.expressionSequence() != null)
            {
                node.Argument = (Expression)Visit(context.expressionSequence());
            }
            return node;
        }

        public override Node VisitBreakStatement(ECMAScriptParser.BreakStatementContext context)
        {
            return new BreakStatement(context);
        }

        public override Node VisitContinueStatement(ECMAScriptParser.ContinueStatementContext context)
        {
            return new ContinueStatement(context);
        }

        public override Node VisitIfStatement(ECMAScriptParser.IfStatementContext context)
        {
            var node = new IfStatement(
                context,
                (Expression)Visit(context.expressionSequence()),
                (Statement)Visit(context.statement()[0]));
            if (context.Else() != null)
            {
                node.Alternate = (Statement)Visit(context.statement()[1]);
            }
            return node;
        }

        public override Node VisitWhileStatement(ECMAScriptParser.WhileStatementContext context)
        {
            return new WhileStatement(
                context,
                (Expression)Visit(context.expressionSequence()),
                (Statement)Visit(context.statement()));
        }

        public override Node VisitFunctionDeclaration(ECMAScriptParser.FunctionDeclarationContext context)
        {
            var node = new FunctionDeclaration(
                context,
                (FunctionBody)Visit(context.functionBody()),
                new Identifier(context, context.Identifier().Symbol.Text));
            if (context.formalParameterList() != null)
            {
                foreach (var identifier in context.formalParameterList().Identifier())
                {
                    node.Params.Add(new Identifier(context, identifier.Symbol.Text));
                }
            }
            return node;
        }

        public override Node VisitVariableDeclaration(ECMAScriptParser.VariableDeclarationContext context)
        {
            var node = new VariableDeclarator(context, new Identifier(context, context.Identifier().Symbol.Text));
            if (context.initialiser() != null)
            {
                node.Init = (Expression)Visit(context.initialiser().singleExpression());
            }
            return node;
        }

        public override Node VisitVariableDeclarationList(ECMAScriptParser.VariableDeclarationListContext context)
        {
            var node = new VariableDeclaration(context);
            node.Declarations.AddRange(context.variableDeclaration().Select(Visit).Cast<VariableDeclarator>());
            return node;
        }

        public override Node VisitVariableStatement(ECMAScriptParser.VariableStatementContext context)
        {
            return Visit(context.variableDeclarationList());
        }

        public override Node VisitArrayLiteral(ECMAScriptParser.ArrayLiteralContext context)
        {
            var node = new ArrayExpression(context);

            if (context.elementList() != null)
            {
                foreach (var element in context.elementList().children)
                {
                    switch (element)
                    {
                        case ECMAScriptParser.SingleExpressionContext _:
                            node.Elements.Add((Expression)Visit(element));
                            break;

                        case ECMAScriptParser.ElisionContext _:
                            node.Elements.AddRange(Enumerable.Repeat((Expression?)null, element.ChildCount));
                            break;
                    }
                }
            }

            if (context.elision() != null)
            {
                node.Elements.AddRange(Enumerable.Repeat((Expression?)null, context.elision().ChildCount));
            }

            return node;
        }

        public override Node VisitArrayLiteralExpression(ECMAScriptParser.ArrayLiteralExpressionContext context)
        {
            return Visit(context.arrayLiteral());
        }

        public override Node VisitObjectLiteral(ECMAScriptParser.ObjectLiteralContext context)
        {
            var node = new ObjectExpression(context);
            if (context.propertyNameAndValueList() != null)
            {
                var propertyAssignments = context.propertyNameAndValueList().propertyAssignment();
                node.Elements.AddRange(propertyAssignments.Select(Visit).Cast<Property>());
            }
            return node;
        }

        public override Node VisitObjectLiteralExpression(ECMAScriptParser.ObjectLiteralExpressionContext context)
        {
            return Visit(context.objectLiteral());
        }

        public override Node VisitPropertyExpressionAssignment(ECMAScriptParser.PropertyExpressionAssignmentContext context)
        {
            return new Property(
                context,
                (Expression)Visit(context.propertyName()),
                (Expression)Visit(context.singleExpression()));
        }

        public override Node VisitPropertyName(ECMAScriptParser.PropertyNameContext context)
        {
            if (context.identifierName() != null)
            {
                return Visit(context.identifierName());
            }
            if (context.numericLiteral() != null)
            {
                return Visit(context.numericLiteral());
            }
            return new StringLiteral(context, context.StringLiteral().Symbol.Text);
        }

        public override Node VisitFunctionExpression(ECMAScriptParser.FunctionExpressionContext context)
        {
            var node = new FunctionExpression(context, (FunctionBody)Visit(context.functionBody()));
            if (context.Identifier() != null)
            {
                node.Id = new Identifier(context, context.Identifier().Symbol.Text);
            }
            if (context.formalParameterList() != null)
            {
                foreach (var identifier in context.formalParameterList().Identifier())
                {
                    node.Params.Add(new Identifier(context, identifier.Symbol.Text));
                }
            }
            return node;
        }

        public override Node VisitUnaryMinusExpression(ECMAScriptParser.UnaryMinusExpressionContext context)
        {
            return new UnaryExpression(
                context,
                UnaryOperator.Minus,
                (Expression)Visit(context.singleExpression()));
        }

        public override Node VisitUnaryPlusExpression(ECMAScriptParser.UnaryPlusExpressionContext context)
        {
            return new UnaryExpression(
                context,
                UnaryOperator.Plus,
                (Expression)Visit(context.singleExpression()));
        }

        public override Node VisitNotExpression(ECMAScriptParser.NotExpressionContext context)
        {
            return new UnaryExpression(
                context,
                UnaryOperator.LogicalNot,
                (Expression)Visit(context.singleExpression()));
        }

        public override Node VisitBitNotExpression(ECMAScriptParser.BitNotExpressionContext context)
        {
            return new UnaryExpression(
                context,
                UnaryOperator.BitwiseNot,
                (Expression)Visit(context.singleExpression()));
        }

        public override Node VisitDeleteExpression(ECMAScriptParser.DeleteExpressionContext context)
        {
            return new UnaryExpression(
                context,
                UnaryOperator.Delete,
                (Expression)Visit(context.singleExpression()));
        }

        public override Node VisitEqualityExpression(ECMAScriptParser.EqualityExpressionContext context)
        {
            var anOperator = context.children[1].GetText();
            if (anOperator == "==" || anOperator == "!=")
            {
                throw new NotImplementedException(
                    GenerateErrorMessage(context, "only '===' and '!==' equality operators are supported."));
            }
            return new BinaryExpression(
                context,
                OperatorsUtils.BinaryOperatorFromString(anOperator),
                (Expression)Visit(context.singleExpression()[0]),
                (Expression)Visit(context.singleExpression()[1])
            );
        }

        public override Node VisitRelationalExpression(ECMAScriptParser.RelationalExpressionContext context)
        {
            return new BinaryExpression(
                context,
                OperatorsUtils.BinaryOperatorFromString(context.children[1].GetText()),
                (Expression)Visit(context.singleExpression()[0]),
                (Expression)Visit(context.singleExpression()[1])
            );
        }

        public override Node VisitBitShiftExpression(ECMAScriptParser.BitShiftExpressionContext context)
        {
            return new BinaryExpression(
                context,
                OperatorsUtils.BinaryOperatorFromString(context.children[1].GetText()),
                (Expression)Visit(context.singleExpression()[0]),
                (Expression)Visit(context.singleExpression()[1])
            );
        }

        public override Node VisitAdditiveExpression(ECMAScriptParser.AdditiveExpressionContext context)
        {
            return new BinaryExpression(
                context,
                OperatorsUtils.BinaryOperatorFromString(context.children[1].GetText()),
                (Expression)Visit(context.singleExpression()[0]),
                (Expression)Visit(context.singleExpression()[1])
            );
        }

        public override Node VisitMultiplicativeExpression(ECMAScriptParser.MultiplicativeExpressionContext context)
        {
            return new BinaryExpression(
                context,
                OperatorsUtils.BinaryOperatorFromString(context.children[1].GetText()),
                (Expression)Visit(context.singleExpression()[0]),
                (Expression)Visit(context.singleExpression()[1])
            );
        }

        public override Node VisitBitOrExpression(ECMAScriptParser.BitOrExpressionContext context)
        {
            return new BinaryExpression(
                context,
                BinaryOperator.BitwiseOr,
                (Expression)Visit(context.singleExpression()[0]),
                (Expression)Visit(context.singleExpression()[1])
            );
        }

        public override Node VisitBitXOrExpression(ECMAScriptParser.BitXOrExpressionContext context)
        {
            return new BinaryExpression(
                context,
                BinaryOperator.BitwiseXor,
                (Expression)Visit(context.singleExpression()[0]),
                (Expression)Visit(context.singleExpression()[1])
            );
        }

        public override Node VisitBitAndExpression(ECMAScriptParser.BitAndExpressionContext context)
        {
            return new BinaryExpression(
                context,
                BinaryOperator.BitwiseAnd,
                (Expression)Visit(context.singleExpression()[0]),
                (Expression)Visit(context.singleExpression()[1])
            );
        }

        public override Node VisitAssignmentExpression(ECMAScriptParser.AssignmentExpressionContext context)
        {
            return new AssignmentExpression(
                context,
                AssignmentOperator.Assign,
                (Expression)Visit(context.singleExpression()[0]),
                (Expression)Visit(context.singleExpression()[1])
            );
        }

        public override Node VisitLogicalOrExpression(ECMAScriptParser.LogicalOrExpressionContext context)
        {
            return new LogicalExpression(
                context,
                LogicalOperator.LogicalOr,
                (Expression)Visit(context.singleExpression()[0]),
                (Expression)Visit(context.singleExpression()[1])
            );
        }

        public override Node VisitLogicalAndExpression(ECMAScriptParser.LogicalAndExpressionContext context)
        {
            return new LogicalExpression(
                context,
                LogicalOperator.LogicalAnd,
                (Expression)Visit(context.singleExpression()[0]),
                (Expression)Visit(context.singleExpression()[1])
            );
        }

        public override Node VisitMemberIndexExpression(ECMAScriptParser.MemberIndexExpressionContext context)
        {
            return new MemberExpression(
                context,
                (Expression)Visit(context.singleExpression()),
                (Expression)Visit(context.expressionSequence()),
                true);
        }

        public override Node VisitMemberDotExpression(ECMAScriptParser.MemberDotExpressionContext context)
        {
            return new MemberExpression(
                context,
                (Expression)Visit(context.singleExpression()),
                (Identifier)Visit(context.identifierName()),
                false);
        }

        public override Node VisitArgumentsExpression(ECMAScriptParser.ArgumentsExpressionContext context)
        {
            var node = new CallExpression(context, (Expression)Visit(context.singleExpression()));
            if (context.arguments().argumentList() != null)
            {
                var expressions = context.arguments().argumentList().singleExpression();
                node.Arguments.AddRange(expressions.Select(Visit).Cast<Expression>());
            }
            return node;
        }

        public override Node VisitExpressionSequence(ECMAScriptParser.ExpressionSequenceContext context)
        {
            var node = new SequenceExpression(context);
            node.Expressions.AddRange(context.singleExpression().Select(Visit).Cast<Expression>());
            return node;
        }

        public override Node VisitParenthesizedExpression(ECMAScriptParser.ParenthesizedExpressionContext context)
        {
            return Visit(context.expressionSequence());
        }
    }

    // Part for rules that are handled by other rules.
    public partial class AstBuilderVisitor
    {
        public override Node VisitElementList(ECMAScriptParser.ElementListContext context)
        {
            throw new InvalidOperationException(GenerateErrorMessage(context, MethodBase.GetCurrentMethod()?.Name));
        }

        public override Node VisitSourceElements(ECMAScriptParser.SourceElementsContext context)
        {
            throw new InvalidOperationException(GenerateErrorMessage(context, MethodBase.GetCurrentMethod()?.Name));
        }

        public override Node VisitElision(ECMAScriptParser.ElisionContext context)
        {
            throw new InvalidOperationException(GenerateErrorMessage(context, MethodBase.GetCurrentMethod()?.Name));
        }

        public override Node VisitPropertyNameAndValueList(ECMAScriptParser.PropertyNameAndValueListContext context)
        {
            throw new InvalidOperationException(GenerateErrorMessage(context, MethodBase.GetCurrentMethod()?.Name));
        }

        public override Node VisitArguments(ECMAScriptParser.ArgumentsContext context)
        {
            throw new InvalidOperationException(GenerateErrorMessage(context, MethodBase.GetCurrentMethod()?.Name));
        }

        public override Node VisitArgumentList(ECMAScriptParser.ArgumentListContext context)
        {
            throw new InvalidOperationException(GenerateErrorMessage(context, MethodBase.GetCurrentMethod()?.Name));
        }

        public override Node VisitFormalParameterList(ECMAScriptParser.FormalParameterListContext context)
        {
            throw new InvalidOperationException(GenerateErrorMessage(context, MethodBase.GetCurrentMethod()?.Name));
        }

        public override Node VisitInitialiser(ECMAScriptParser.InitialiserContext context)
        {
            throw new InvalidOperationException(GenerateErrorMessage(context, MethodBase.GetCurrentMethod()?.Name));
        }

        public override Node VisitStatementList(ECMAScriptParser.StatementListContext context)
        {
            throw new InvalidOperationException(GenerateErrorMessage(context, MethodBase.GetCurrentMethod()?.Name));
        }
    }

    // Part for features that aren't going to be implemented.
    public partial class AstBuilderVisitor
    {
        private static string GenerateErrorLocationMessage(ParserRuleContext context) =>
            $"Error ({context.Start.Line}:{context.Stop.Column}): ";

        private static string GenerateErrorMessage(ParserRuleContext context, string? message) =>
            GenerateErrorLocationMessage(context) + message;

        private static string GenerateErrorMessage(ParserRuleContext context) =>
            GenerateErrorMessage(
                context,
                $" '{context.GetType().Name}' isn't going to be released. Consider using some normal software.");

        public override Node VisitDoStatement(ECMAScriptParser.DoStatementContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitForStatement(ECMAScriptParser.ForStatementContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitForVarStatement(ECMAScriptParser.ForVarStatementContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitForInStatement(ECMAScriptParser.ForInStatementContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitForVarInStatement(ECMAScriptParser.ForVarInStatementContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitWithStatement(ECMAScriptParser.WithStatementContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitLabeledStatement(ECMAScriptParser.LabeledStatementContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitSwitchStatement(ECMAScriptParser.SwitchStatementContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitThrowStatement(ECMAScriptParser.ThrowStatementContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitTryStatement(ECMAScriptParser.TryStatementContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitDebuggerStatement(ECMAScriptParser.DebuggerStatementContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitGetter(ECMAScriptParser.GetterContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitPropertyGetter(ECMAScriptParser.PropertyGetterContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitSetter(ECMAScriptParser.SetterContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitPropertySetter(ECMAScriptParser.PropertySetterContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitPropertySetParameterList(ECMAScriptParser.PropertySetParameterListContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitAssignmentOperator(ECMAScriptParser.AssignmentOperatorContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitAssignmentOperatorExpression(
            ECMAScriptParser.AssignmentOperatorExpressionContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitCaseBlock(ECMAScriptParser.CaseBlockContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitCaseClause(ECMAScriptParser.CaseClauseContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitCaseClauses(ECMAScriptParser.CaseClausesContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitCatchProduction(ECMAScriptParser.CatchProductionContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitDefaultClause(ECMAScriptParser.DefaultClauseContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitFinallyProduction(ECMAScriptParser.FinallyProductionContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitInExpression(ECMAScriptParser.InExpressionContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitInstanceofExpression(ECMAScriptParser.InstanceofExpressionContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitNewExpression(ECMAScriptParser.NewExpressionContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitPostDecreaseExpression(ECMAScriptParser.PostDecreaseExpressionContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitPostIncrementExpression(ECMAScriptParser.PostIncrementExpressionContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitPreDecreaseExpression(ECMAScriptParser.PreDecreaseExpressionContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitPreIncrementExpression(ECMAScriptParser.PreIncrementExpressionContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitTernaryExpression(ECMAScriptParser.TernaryExpressionContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitThisExpression(ECMAScriptParser.ThisExpressionContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitTypeofExpression(ECMAScriptParser.TypeofExpressionContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }

        public override Node VisitVoidExpression(ECMAScriptParser.VoidExpressionContext context)
        {
            throw new NotImplementedException(GenerateErrorMessage(context));
        }
    }
}

#pragma warning restore RCS1079  // Throwing of new NotImplementedException.