namespace Syntex;

/// <summary>
/// Represents syntax token in the mermaid class diagram.
/// </summary>
public abstract class MermaidToken
{
    protected abstract string Value { get; }
    protected virtual ReadOnlySpan<char> ValueSpan => Value.AsSpan();

    /// <summary>
    /// Query if the beginning of the source matches the token. Return the length of the match.
    /// </summary>
    /// <param name="source">The text source.</param>
    /// <returns>Return the length of the match.</returns>
    public virtual int Match(ReadOnlySpan<char> source) => source.StartsWith(Value) ? Value.Length : 0;

    public override bool Equals(object? obj) => obj is MermaidToken m && m.GetType() == GetType();
    public override int GetHashCode()
    {
        int hc = int.MinValue;
        foreach (var c in ValueSpan)
        {
            unchecked
            {
                hc += c;
            }
        }

        return hc;
    }
}

public class InlineCssToken : MermaidToken
{
    protected override string Value { get; } = ":::";
}

public class SemicolonToken : MermaidToken
{
    protected override string Value { get; } = ":";
}

public class CssClassDefinitionToken : MermaidToken
{
    protected override string Value { get; } = "classDef";
}

// public class LabelStartToken : MermaidToken
// {
//     public override string Value { get; } = "[\"";
// }
//
// public class LabelEndToken : MermaidToken
// {
//     public override string Value { get; } = "\"]";
// }

// public class MermaidAttributesToken : MermaidToken
// {
//     protected override string Value { get; } = "---";
// }

public class RawStringToken : MermaidToken
{
    public RawStringToken() : this(string.Empty) { }

    public RawStringToken(string value)
    {
        Value = value;
    }

    protected override string Value { get; } = string.Empty;
}

public class ClassDiagramToken : MermaidToken
{
    protected override string Value { get; } = "classDiagram";
}

public class NewLineToken : MermaidToken
{
    protected override string Value => "\n"; // Environment.Newline?
}

public class CommentToken : MermaidToken
{
    protected override string Value => "%%";
}

public abstract class MermaidRelationToken;
public abstract class MermaidLinkToken;

public class MermaidAnnotationStartToken : MermaidToken
{
    protected override string Value => "<<";
}

public class MermaidAnnotationEndToken : MermaidToken
{
    protected override string Value => ">>";
}

public class PlusToken : MermaidToken
{
    protected override string Value => "+";
}

public class MinusToken : MermaidToken
{
    protected override string Value => "-";
}

public class HashToken : MermaidToken
{
    protected override string Value => "#";
}

public class TildeToken : MermaidToken
{
    protected override string Value => "~";
}

public class BackTickToken : MermaidToken
{
    protected override string Value => "`";
}

public class CommaToken : MermaidToken
{
    protected override string Value => ",";
}

public class OpenBracketToken : MermaidToken
{
    protected override string Value => "[";
}

public class CloseBracketToken : MermaidToken
{
    protected override string Value => "]";
}

public class OpenBraceToken : MermaidToken
{
    protected override string Value => "{";
}

public class CloseBraceToken : MermaidToken
{
    protected override string Value => "}";
}

public class OpenParenToken : MermaidToken
{
    protected override string Value => "(";
}

public class CloseParenToken : MermaidToken
{
    protected override string Value => ")";
}