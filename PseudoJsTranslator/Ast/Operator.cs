using System.Collections.Generic;

namespace PseudoJsTranslator.Ast
{
    public static class OperatorsUtils
    {
        private static readonly Dictionary<string, UnaryOperator> UnaryOperators =
            new Dictionary<string, UnaryOperator>()
        {
            { "-", UnaryOperator.Minus },
            { "+", UnaryOperator.Plus },
            { "!", UnaryOperator.LogicalNot },
            { "~", UnaryOperator.BitwiseNot },
            { "delete", UnaryOperator.Delete },
        };

        private static readonly Dictionary<string, BinaryOperator> BinaryOperators =
            new Dictionary<string, BinaryOperator>()
        {
            { "===", BinaryOperator.Equal },
            { "!==", BinaryOperator.Unequal },
            { "<", BinaryOperator.Less },
            { "<=", BinaryOperator.LessOrEqual },
            { ">", BinaryOperator.Greater },
            { ">=", BinaryOperator.GreaterOrEqual },
            { "<<", BinaryOperator.LeftShift },
            { ">>>", BinaryOperator.RightShift },
            { ">>", BinaryOperator.ArithmeticRightShift },
            { "+", BinaryOperator.Addition },
            { "-", BinaryOperator.Subtraction },
            { "*", BinaryOperator.Multiplication },
            { "/", BinaryOperator.Division },
            { "%", BinaryOperator.Remainder },
            { "|", BinaryOperator.BitwiseOr },
            { "^", BinaryOperator.BitwiseXor },
            { "&", BinaryOperator.BitwiseAnd },
        };

        private static readonly Dictionary<string, AssignmentOperator> AssignmentOperators =
            new Dictionary<string, AssignmentOperator>()
        {
            { "=", AssignmentOperator.Assign },
        };

        private static readonly Dictionary<string, LogicalOperator> LogicalOperators =
            new Dictionary<string, LogicalOperator>()
        {
            { "||", LogicalOperator.LogicalOr },
            { "&&", LogicalOperator.LogicalAnd },
        };

        public static UnaryOperator UnaryOperatorFromString(string unaryOperator)
        {
            return UnaryOperators[unaryOperator];
        }

        public static BinaryOperator BinaryOperatorFromString(string binaryOperator)
        {
            return BinaryOperators[binaryOperator];
        }

        public static AssignmentOperator AssignmentOperatorFromString(string assignmentOperator)
        {
            return AssignmentOperators[assignmentOperator];
        }

        public static LogicalOperator LogicalOperatorFromString(string logicalOperator)
        {
            return LogicalOperators[logicalOperator];
        }
    }

    public enum UnaryOperator : byte
    {
        Minus,
        Plus,
        LogicalNot,
        BitwiseNot,
        Delete,
    }

    public enum BinaryOperator : byte
    {
        Equal,
        Unequal,
        Less,
        LessOrEqual,
        Greater,
        GreaterOrEqual,
        LeftShift,
        RightShift,
        ArithmeticRightShift,
        Addition,
        Subtraction,
        Multiplication,
        Division,
        Remainder,
        BitwiseOr,
        BitwiseXor,
        BitwiseAnd,
    }

    public enum AssignmentOperator : byte
    {
        Assign,
    }

    public enum LogicalOperator : byte
    {
        LogicalOr,
        LogicalAnd,
    }
}