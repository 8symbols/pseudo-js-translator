namespace PseudoJsTranslator.Ast
{
    public abstract class AstBaseVisitor<T>
    {
        public virtual T Visit(Node node)
        {
            return Visit((dynamic)node);
        }

        public abstract T Visit(Identifier node);

        public abstract T Visit(StringLiteral node);

        public abstract T Visit(BooleanLiteral node);

        public abstract T Visit(NullLiteral node);

        public abstract T Visit(NumericLiteral node);

        public abstract T Visit(Program node);

        public abstract T Visit(ExpressionStatement node);

        public abstract T Visit(BlockStatement node);

        public abstract T Visit(FunctionBody node);

        public abstract T Visit(EmptyStatement node);

        public abstract T Visit(ReturnStatement node);

        public abstract T Visit(BreakStatement node);

        public abstract T Visit(ContinueStatement node);

        public abstract T Visit(IfStatement node);

        public abstract T Visit(WhileStatement node);

        public abstract T Visit(FunctionDeclaration node);

        public abstract T Visit(VariableDeclaration node);

        public abstract T Visit(VariableDeclarator node);

        public abstract T Visit(ArrayExpression node);

        public abstract T Visit(ObjectExpression node);

        public abstract T Visit(Property node);

        public abstract T Visit(FunctionExpression node);

        public abstract T Visit(UnaryExpression node);

        public abstract T Visit(BinaryExpression node);

        public abstract T Visit(AssignmentExpression node);

        public abstract T Visit(LogicalExpression node);

        public abstract T Visit(MemberExpression node);

        public abstract T Visit(CallExpression node);

        public abstract T Visit(SequenceExpression node);
    }
}