#nullable enable

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PseudoJsTranslator.Ast
{
    public class AsciiTreeBuilderVisitor : AstBaseVisitor<object?>
    {
        public uint VerticalIdent { get; set; } = 1;
        public uint HorizontalIdent { get; set; } = 3;

        public bool IsPositionsAdditionEnabled { get; set; } = true;

        private uint Depth { get; set; }
        private HashSet<uint> DepthsWithUnvisitedChildren { get; } = new HashSet<uint>();

        private StringBuilder Sb { get; } = new StringBuilder();

        /// <summary>
        /// Returns built tree.
        /// </summary>
        public string GetStringTree()
        {
            return Sb.ToString();
        }

        /// <summary>
        /// Clears built tree.
        /// </summary>
        public void ClearTree()
        {
            Sb.Clear();
        }

        /// <summary>
        /// Adds ASCII characters that connect nodes to <see cref="Sb"/>.
        /// </summary>
        private void AddIdents()
        {
            if (Depth == 0)
            {
                return;
            }

            for (var i = 0; i < VerticalIdent; ++i)
            {
                for (var j = 1; j < Depth; ++j)
                {
                    Sb.Append(DepthsWithUnvisitedChildren.Contains((uint)j) ? '|' : ' ');
                    Sb.Append(' ', (int)HorizontalIdent);
                }
                Sb.AppendLine("|");
            }

            for (var i = 1; i < Depth; ++i)
            {
                Sb.Append(DepthsWithUnvisitedChildren.Contains((uint)i) ? '|' : ' ');
                Sb.Append(' ', (int)HorizontalIdent);
            }
            Sb.Append('+');
            Sb.Append('-', (int)HorizontalIdent);
        }

        /// <summary>
        /// Adds parenthesized node position to <see cref="Sb"/> if <see cref="IsPositionsAdditionEnabled"/> is true
        /// and <see cref="Node.Loc"/> isn't null. Otherwise does nothing.
        /// </summary>
        private void AddNodePosition(Node node)
        {
            if (IsPositionsAdditionEnabled && node.Loc.HasValue)
            {
                Sb.Append('(').Append(node.Loc.Value.Start).Append(")");
            }
        }

        /// <summary>
        /// Adds node type name to <see cref="Sb"/>.
        /// </summary>
        private void AddNodeMetaInfo(Node node)
        {
            Sb.Append(node.GetType().Name);
        }

        /// <summary>
        /// Adds extra information wrapped in square brackets and separated by commas to <see cref="Sb"/>.
        /// </summary>
        /// <param name="info">Variable number of arguments.</param>
        /// <remarks>
        /// It's expected that this method will be called in the start of every visit.
        /// If no information has been provided, square brackets are not added.
        /// Adds a line terminator to the end.
        /// </remarks>
        private void AddNodeInfo(params string[] info)
        {
            if (info.Length != 0)
            {
                Sb.Append('[').AppendJoin(", ", info).Append(']');
            }
            Sb.AppendLine();
        }

        /// <summary>
        /// Visits all provided children with <see cref="Depth"/> and <see cref="DepthsWithUnvisitedChildren"/> handling.
        /// </summary>
        /// <param name="children">Variable number of arguments.</param>
        /// <remarks>
        /// It must be called no more than once on the parent node.
        /// If a child is null, it's treated as an undefined node.
        /// </remarks>
        private void VisitChildren(params Node[] children)
        {
            ++Depth;
            DepthsWithUnvisitedChildren.Add(Depth);

            foreach (var child in children)
            {
                if (child == children[^1])
                {
                    DepthsWithUnvisitedChildren.Remove(Depth);
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

            DepthsWithUnvisitedChildren.Remove(Depth);
            --Depth;
        }

        private void VisitUndefinedNode()
        {
            AddIdents();
            Sb.AppendLine("undefined");
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