namespace Syntex;

public record struct TokenPosition(int Position, MermaidToken Token);

public class MermaidContext
{
    public MermaidContext(string text)
    {
        Text = text;
    }

    public int CurrentTokenIndex { get; private set; }
    public TokenPosition CurrentToken => Tokens.Count > CurrentTokenIndex ? Tokens[CurrentTokenIndex] : default;
    public List<TokenPosition> Tokens { get; } = new();
    public string Text { get; }
    public ReadOnlySpan<char> TextSpan => Text;
    public ReadOnlySpan<char> Remainder => CurrentPosition <= 0 ? TextSpan : Text.AsSpan()[CurrentPosition..];
    public int CurrentPosition { get; set; }
    public void Advance(int tokenLength) => CurrentPosition += tokenLength;

    public TokenPosition NextToken()
    {
        CurrentTokenIndex++;
        return CurrentToken;
    }

    public ReadOnlySpan<char> CurrentLine()
    {
        var start = Math.Max(0, TextSpan[..CurrentPosition].LastIndexOf('\n'));
        var end = Math.Max(Text.Length, TextSpan[CurrentPosition..].IndexOf('\n'));
        return TextSpan[start..end];
    }

    public IEnumerable<TokenPosition> TakeNextNTokens(int nTokens)
        => Tokens.Skip(CurrentTokenIndex).Take(nTokens);
}