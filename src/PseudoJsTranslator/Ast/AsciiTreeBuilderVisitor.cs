#nullable enable

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PseudoJsTranslator.Ast
{
    /// <summary>Represents <see cref="Node"/> and all its children as an ASCII tree string.</summary>
    /// <remarks>
    /// Every node type name are optionally followed by its parenthesized start position and/or some extra info
    /// (such as operator type, identifier name, literal value, function parameters names) in square brackets.
    /// In sparse arrays, the word <i>undefined</i> is added in place of missing element.
    /// </remarks>
    public class AsciiTreeBuilderVisitor : AstBaseVisitor<object?>
    {
        public uint VerticalIdent { get; set; } = 1;
        public uint HorizontalIdent { get; set; } = 3;
        public bool IsPositionsAdditionEnabled { get; set; } = true;

        private uint _depth;
        private readonly HashSet<uint> _depthsWithUnvisitedChildren = new HashSet<uint>();
        private readonly StringBuilder _sb = new StringBuilder();

        /// <summary>
        /// Returns built tree.
        /// </summary>
        public string GetStringTree()
        {
            return _sb.ToString();
        }

        /// <summary>
        /// Clears built tree.
        /// </summary>
        public void ClearTree()
        {
            _sb.Clear();
        }

        /// <summary>
        /// Adds ASCII characters that connect nodes to <see cref="_sb"/>.
        /// </summary>
        private void AddIdents()
        {
            if (_depth == 0)
            {
                return;
            }

            for (var i = 0; i < VerticalIdent; ++i)
            {
                for (var j = 1; j < _depth; ++j)
                {
                    _sb.Append(_depthsWithUnvisitedChildren.Contains((uint)j) ? '|' : ' ');
                    _sb.Append(' ', (int)HorizontalIdent);
                }
                _sb.AppendLine("|");
            }

            for (var i = 1; i < _depth; ++i)
            {
                _sb.Append(_depthsWithUnvisitedChildren.Contains((uint)i) ? '|' : ' ');
                _sb.Append(' ', (int)HorizontalIdent);
            }
            _sb.Append('+');
            _sb.Append('-', (int)HorizontalIdent);
        }

        /// <summary>
        /// Adds parenthesized node position to <see cref="_sb"/> if <see cref="IsPositionsAdditionEnabled"/> is true
        /// and <see cref="Node.Loc"/> isn't null. Otherwise does nothing.
        /// </summary>
        private void AddNodePosition(Node node)
        {
            if (IsPositionsAdditionEnabled && node.Loc.HasValue)
            {
                _sb.Append('(').Append(node.Loc.Value.Start).Append(")");
            }
        }

        /// <summary>
        /// Adds node type name to <see cref="_sb"/>.
        /// </summary>
        private void AddNodeMetaInfo(Node node)
        {
            _sb.Append(node.GetType().Name);
        }

        /// <summary>
        /// Adds node info wrapped in square brackets and separated by commas to <see cref="_sb"/>.
        /// </summary>
        /// <param name="info">Variable number of arguments.</param>
        /// <remarks>
        /// This method is expected to be called at the beginning of each visit.
        /// If no information is provided, square brackets aren't added.
        /// Adds a line terminator at the end.
        /// </remarks>
        private void AddNodeInfo(params string[] info)
        {
            if (info.Length != 0)
            {
                _sb.Append('[').AppendJoin(", ", info).Append(']');
            }
            _sb.AppendLine();
        }

        /// <summary>Visits all child nodes.</summary>
        /// <param name="children">Variable number of arguments.</param>
        /// <remarks>
        /// It must be called no more than once on the parent node.
        /// If the child is null, it's treated as an undefined node.
        /// </remarks>
        private void VisitChildren(params Node[] children)
        {
            ++_depth;
            _depthsWithUnvisitedChildren.Add(_depth);

            foreach (var child in children)
            {
                if (child == children[^1])
                {
                    _depthsWithUnvisitedChildren.Remove(_depth);
                }
                if (child != null)
                {
                    Visit(child);
                }
                else
                {
                    VisitUndefinedNode();
                }
            }

            _depthsWithUnvisitedChildren.Remove(_depth);
            --_depth;
        }

        private void VisitUndefinedNode()
        {
            AddIdents();
            _sb.AppendLine("undefined");
        }

        public override object? Visit(Node node)
        {
            AddIdents();
            AddNodeMetaInfo(node);
            AddNodePosition(node);
            return base.Visit(node);
        }

        private void VisitFunction(IFunction node)
        {
            AddNodeInfo(node.Params.Select(identifier => identifier.Name).ToArray());
            if (node.Id != null)
            {
                VisitChildren(node.Id, node.Body);
            }
            else
            {
                VisitChildren(node.Body);
            }
        }

        public override object? Visit(Identifier node)
        {
            AddNodeInfo(node.Name);
            return null;
        }

        public override object? Visit(StringLiteral node)
        {
            AddNodeInfo(node.Value);
            return null;
        }

        public override object? Visit(BooleanLiteral node)
        {
            AddNodeInfo(node.Value.ToString());
            return null;
        }

        public override object? Visit(NullLiteral node)
        {
            AddNodeInfo();
            return null;
        }

        public override object? Visit(NumericLiteral node)
        {
            AddNodeInfo(node.Value.ToString(CultureInfo.InvariantCulture));
            return null;
        }

        public override object? Visit(Program node)
        {
            AddNodeInfo();
            VisitChildren(node.Body.Cast<Node>().ToArray());
            return null;
        }

        public override object? Visit(ExpressionStatement node)
        {
            AddNodeInfo();
            VisitChildren(node.Expression);
            return null;
        }

        public override object? Visit(BlockStatement node)
        {
            AddNodeInfo();
            VisitChildren(node.Body.Cast<Node>().ToArray());
            return null;
        }

        public override object? Visit(FunctionBody node)
        {
            AddNodeInfo();
            VisitChildren(node.Body.Cast<Node>().ToArray());
            return null;
        }

        public override object? Visit(EmptyStatement node)
        {
            AddNodeInfo();
            return null;
        }

        public override object? Visit(ReturnStatement node)
        {
            AddNodeInfo();
            if (node.Argument != null)
            {
                VisitChildren(node.Argument);
            }
            return null;
        }

        public override object? Visit(BreakStatement node)
        {
            AddNodeInfo();
            return null;
        }

        public override object? Visit(ContinueStatement node)
        {
            AddNodeInfo();
            return null;
        }

        public override object? Visit(IfStatement node)
        {
            AddNodeInfo();
            if (node.Alternate != null)
            {
                VisitChildren(node.Test, node.Consequent, node.Alternate);
            }
            else
            {
                VisitChildren(node.Test, node.Consequent);
            }
            return null;
        }

        public override object? Visit(WhileStatement node)
        {
            AddNodeInfo();
            VisitChildren(node.Test, node.Body);
            return null;
        }

        public override object? Visit(FunctionDeclaration node)
        {
            VisitFunction(node);
            return null;
        }

        public override object? Visit(VariableDeclaration node)
        {
            AddNodeInfo();
            VisitChildren(node.Declarations.Cast<Node>().ToArray());
            return null;
        }

        public override object? Visit(VariableDeclarator node)
        {
            AddNodeInfo();
            if (node.Init != null)
            {
                VisitChildren(node.Id, node.Init);
            }
            else
            {
                VisitChildren(node.Id);
            }
            return null;
        }

        public override object? Visit(ArrayExpression node)
        {
            AddNodeInfo();
            VisitChildren(node.Elements.Cast<Node>().ToArray());
            return null;
        }

        public override object? Visit(ObjectExpression node)
        {
            AddNodeInfo();
            VisitChildren(node.Elements.Cast<Node>().ToArray());
            return null;
        }

        public override object? Visit(Property node)
        {
            AddNodeInfo();
            VisitChildren(node.Key, node.Value);
            return null;
        }

        public override object? Visit(FunctionExpression node)
        {
            VisitFunction(node);
            return null;
        }

        public override object? Visit(UnaryExpression node)
        {
            AddNodeInfo(node.Operator.ToString());
            VisitChildren(node.Argument);
            return null;
        }

        public override object? Visit(BinaryExpression node)
        {
            AddNodeInfo(node.Operator.ToString());
            VisitChildren(node.Left, node.Right);
            return null;
        }

        public override object? Visit(AssignmentExpression node)
        {
            AddNodeInfo(node.Operator.ToString());
            VisitChildren(node.Left, node.Right);
            return null;
        }

        public override object? Visit(LogicalExpression node)
        {
            AddNodeInfo(node.Operator.ToString());
            VisitChildren(node.Left, node.Right);
            return null;
        }

        public override object? Visit(MemberExpression node)
        {
            AddNodeInfo(node.Computed ? "a[b]" : "a.b");
            VisitChildren(node.Object, node.Property);
            return null;
        }

        public override object? Visit(CallExpression node)
        {
            AddNodeInfo();
            var children = new List<Node> { node.Callee };
            children.AddRange(node.Arguments);
            VisitChildren(children.ToArray());
            return null;
        }

        public override object? Visit(SequenceExpression node)
        {
            AddNodeInfo();
            VisitChildren(node.Expressions.Cast<Node>().ToArray());
            return null;
        }
    }
}