#nullable enable

using System.Collections.Generic;
using Antlr4.Runtime;

namespace PseudoJsTranslator.Ast
{
    public struct Position
    {
        public uint Line { get; set; }
        public uint Column { get; set; }

        public Position(uint line, uint column)
        {
            Line = line;
            Column = column;
        }

        public override string ToString() => $"({Line}:{Column})";
    }

    public abstract class Node
    {
        public Position Start { get; set; }
        public Position End { get; set; }

        protected Node(ParserRuleContext context)
        {
            Start = new Position((uint)context.Start.Line, (uint)context.Start.Column);
            End = new Position((uint)context.Stop.Line, (uint)context.Stop.Column);
        }
    }

    public abstract class Statement : Node
    {
        protected Statement(ParserRuleContext context) : base(context)
        {
        }
    }

    public abstract class Declaration : Statement
    {
        protected Declaration(ParserRuleContext context) : base(context)
        {
        }
    }

    public abstract class Expression : Node
    {
        protected Expression(ParserRuleContext context) : base(context)
        {
        }
    }

    public interface IFunction
    {
        public Identifier? Id { get; set; }
        public List<Identifier> Params { get; set; }
        public FunctionBody Body { get; set; }
    }

    public class Identifier : Expression
    {
        public string Name { get; set; }

        public Identifier(ParserRuleContext context, string name) : base(context)
        {
            Name = name;
        }
    }

    public abstract class Literal<T> : Expression
    {
        public T Value { get; set; }

        protected Literal(ParserRuleContext context, T value) : base(context)
        {
            Value = value;
        }
    }

    public class StringLiteral : Literal<string>
    {
        public StringLiteral(ParserRuleContext context, string value) : base(context, value)
        {
        }
    }

    public class BooleanLiteral : Literal<bool>
    {
        public BooleanLiteral(ParserRuleContext context, bool value) : base(context, value)
        {
        }
    }

    public class NullLiteral : Literal<object>
    {
        public NullLiteral(ParserRuleContext context) : base(context, "null literal value")
        {
        }
    }

    public class NumericLiteral : Literal<double>
    {
        public NumericLiteral(ParserRuleContext context, double value) : base(context, value)
        {
        }
    }

    public class Program : Node
    {
        public List<Statement> Body { get; set; } = new List<Statement>();

        public Program(ParserRuleContext context) : base(context)
        {
        }
    }

    public class ExpressionStatement : Statement
    {
        public Expression Expression { get; set; }

        public ExpressionStatement(ParserRuleContext context, Expression expression) : base(context)
        {
            Expression = expression;
        }
    }

    public class BlockStatement : Statement
    {
        public List<Statement> Body { get; set; } = new List<Statement>();

        public BlockStatement(ParserRuleContext context) : base(context)
        {
        }
    }

    public class FunctionBody : BlockStatement
    {
        public FunctionBody(ParserRuleContext context) : base(context)
        {
        }
    }

    public class EmptyStatement : Statement
    {
        public EmptyStatement(ParserRuleContext context) : base(context)
        {
        }
    }

    public class ReturnStatement : Statement
    {
        public Expression? Argument { get; set; }

        public ReturnStatement(ParserRuleContext context) : base(context)
        {
        }
    }

    public class BreakStatement : Statement
    {
        public BreakStatement(ParserRuleContext context) : base(context)
        {
        }
    }

    public class ContinueStatement : Statement
    {
        public ContinueStatement(ParserRuleContext context) : base(context)
        {
        }
    }

    public class IfStatement : Statement
    {
        public Expression Test { get; set; }
        public Statement Consequent { get; set; }
        public Statement? Alternate { get; set; }

        public IfStatement(ParserRuleContext context, Expression test, Statement consequent) : base(context)
        {
            Test = test;
            Consequent = consequent;
        }
    }

    public class WhileStatement : Statement
    {
        public Expression Test { get; set; }
        public Statement Body { get; set; }

        public WhileStatement(ParserRuleContext context, Expression test, Statement body) : base(context)
        {
            Test = test;
            Body = body;
        }
    }

    public class FunctionDeclaration : Declaration, IFunction
    {
        public Identifier? Id { get; set; }
        public List<Identifier> Params { get; set; } = new List<Identifier>();
        public FunctionBody Body { get; set; }

        public FunctionDeclaration(ParserRuleContext context, FunctionBody body, Identifier id) : base(context)
        {
            Body = body;
            Id = id;
        }
    }

    public class VariableDeclaration : Declaration
    {
        public List<VariableDeclarator> Declarations { get; set; } = new List<VariableDeclarator>();

        public VariableDeclaration(ParserRuleContext context) : base(context)
        {
        }
    }

    public class VariableDeclarator : Node
    {
        public Identifier Id { get; set; }
        public Expression? Init { get; set; }

        public VariableDeclarator(ParserRuleContext context, Identifier id) : base(context)
        {
            Id = id;
        }
    }

    public class ArrayExpression : Expression
    {
        public List<Expression?> Elements { get; set; } = new List<Expression?>();

        public ArrayExpression(ParserRuleContext context) : base(context)
        {
        }
    }

    public class ObjectExpression : Expression
    {
        public List<Property> Elements { get; set; } = new List<Property>();

        public ObjectExpression(ParserRuleContext context) : base(context)
        {
        }
    }

    public class Property : Node
    {
        /// <summary>
        /// Must be a <see cref="Literal{T}"/> or <see cref="Identifier"/>.
        /// </summary>
        public Expression Key { get; set; }

        public Expression Value { get; set; }

        public Property(ParserRuleContext context, Expression key, Expression value) : base(context)
        {
            Key = key;
            Value = value;
        }
    }

    public class FunctionExpression : Expression, IFunction
    {
        public Identifier? Id { get; set; }
        public List<Identifier> Params { get; set; } = new List<Identifier>();
        public FunctionBody Body { get; set; }

        public FunctionExpression(ParserRuleContext context, FunctionBody body) : base(context)
        {
            Body = body;
        }
    }

    public class UnaryExpression : Expression
    {
        public UnaryOperator Operator { get; set; }
        public Expression Argument { get; set; }

        public UnaryExpression(
            ParserRuleContext context,
            UnaryOperator anOperator,
            Expression argument) : base(context)
        {
            Operator = anOperator;
            Argument = argument;
        }
    }

    public abstract class BinaryOperatorExpression<T> : Expression
    {
        public T Operator { get; set; }
        public Expression Left { get; set; }
        public Expression Right { get; set; }

        protected BinaryOperatorExpression(
            ParserRuleContext context,
            T anOperator,
            Expression left,
            Expression right) : base(context)
        {
            Operator = anOperator;
            Left = left;
            Right = right;
        }
    }

    public class BinaryExpression : BinaryOperatorExpression<BinaryOperator>
    {
        public BinaryExpression(
            ParserRuleContext context,
            BinaryOperator anOperator,
            Expression left,
            Expression right) : base(context, anOperator, left, right)
        {
        }
    }

    public class AssignmentExpression : BinaryOperatorExpression<AssignmentOperator>
    {
        public AssignmentExpression(
            ParserRuleContext context,
            AssignmentOperator anOperator,
            Expression left,
            Expression right) : base(context, anOperator, left, right)
        {
        }
    }

    public class LogicalExpression : BinaryOperatorExpression<LogicalOperator>
    {
        public LogicalExpression(
            ParserRuleContext context,
            LogicalOperator anOperator,
            Expression left,
            Expression right) : base(context, anOperator, left, right)
        {
        }
    }

    /// <summary>A member expression.</summary>
    /// <remarks>
    /// If computed is true, the node corresponds to a computed <b>a[b]</b> member expression
    /// and property is an <see cref="Expression"/>. If computed is false, the node corresponds
    /// to a static <b>a.b</b> member expression and property is an <see cref="Identifier"/>.
    /// </remarks>
    public class MemberExpression : Expression
    {
        public Expression Object { get; set; }
        public Expression Property { get; set; }
        public bool Computed { get; set; }

        public MemberExpression(
            ParserRuleContext context,
            Expression anObject,
            Expression property,
            bool computed) : base(context)
        {
            Object = anObject;
            Property = property;
            Computed = computed;
        }
    }

    public class CallExpression : Expression
    {
        public Expression Callee { get; set; }
        public List<Expression> Arguments { get; set; } = new List<Expression>();

        public CallExpression(ParserRuleContext context, Expression callee) : base(context)
        {
            Callee = callee;
        }
    }

    public class SequenceExpression : Expression
    {
        public List<Expression> Expressions { get; set; } = new List<Expression>();

        public SequenceExpression(ParserRuleContext context) : base(context)
        {
        }
    }
}