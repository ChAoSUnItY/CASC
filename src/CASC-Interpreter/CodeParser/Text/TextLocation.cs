namespace CASC.CodeParser.Text
{
    public struct TextLocation {
        public TextLocation(SourceText source, TextSpan span)
        {
            Source = source;
            Span = span;
        }

        public SourceText Source { get; }
        public TextSpan Span { get; }
        public int StartLine => Source.GetLineIndex(Span.Start);
        public int StartCharacter => Span.Start - Source.Lines[StartLine].Start;
        public int EndLine => Source.GetLineIndex(Span.End);
        public int EndCharacter => Span.End - Source.Lines[StartLine].Start;
    }
}