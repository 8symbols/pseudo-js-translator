namespace PseudoJsTranslator.Ast
{
    /// <summary>
    /// Represents the source location information of the <see cref="Node"/>.
    /// </summary>
    public struct SourceLocation
    {
        public Position Start { get; set; }
        public Position End { get; set; }

        public SourceLocation(Position start, Position end)
        {
            Start = start;
            End = end;
        }

        public override string ToString() => $"{Start}-{End}";
    }

    public struct Position
    {
        public uint Line { get; set; }
        public uint Column { get; set; }

        public Position(uint line, uint column)
        {
            Line = line;
            Column = column;
        }

        public override string ToString() => $"{Line}:{Column}";
    }
}