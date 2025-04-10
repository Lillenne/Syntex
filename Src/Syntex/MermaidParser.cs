using System.Buffers;

namespace Syntex;

public class MermaidParser
{
    private readonly MermaidToken[] _tokens;
    private readonly ExpressionRecognizer[] _recognizers;

    public MermaidParser(
        MermaidContext context,
        IEnumerable<ExpressionRecognizer> recognizers,
        IEnumerable<MermaidToken> tokens)
    {
        _recognizers = recognizers.ToArray();
        _tokens = tokens.ToArray();
        Context = context;
    }

    public MermaidContext Context { get; }
    /// <summary>
    /// 1. Full tokenization pass
    /// 2. Token recognition
    /// </summary>
    /// <returns></returns>
    public MermaidExpression Parse()
    {
        while (Context.CurrentPosition < Context.Text.Length)
        {
            MatchToken();
        }

        Context.CurrentPosition = 0;
        while (Context.CurrentPosition < Context.Text.Length)
        {
            foreach (var recognizer in _recognizers)
            {
                // recognizer.Recognize()
            }
        }
    }
    //
    // private int SegmentLength(char open, char close)
    // {
    //     var rem = Context.Remainder;
    //     if (rem.Length < 1)
    //     {
    //
    //     }
    //     if (rem[0] != open)
    //     {
    //         throw new InvalidOperationException("Context.Remainder[0] must be the open character.");
    //     }
    //     var c = 1;
    //     var pos = 1;
    //     while (c > 0)
    //     {
    //
    //     }
    // }

    private void MatchToken()
    {
        MermaidToken? t = null;
        int matchLen = 0;
        var rem = Context.Remainder;
        DoMatchTokens(ref t, rem);

        if (t is null)
        {
            while (t is null && matchLen != rem.Length)
            {
                matchLen++;
                DoMatchTokens(ref t, rem[matchLen..]);
            }
            // matchLen = Context.Remainder.IndexOfAny(' ', '\n'); // TODO line endings per os?
            // t = new RawStringToken(Context.Remainder[..matchLen].ToString());
        }

        if (t is not null)
            Context.Tokens.Add(new(Context.CurrentPosition, t));
        Context.Advance(matchLen);
    }

    private void DoMatchTokens(ref MermaidToken? t, ReadOnlySpan<char> rem)
    {
        int matchLen;
        foreach (var token in _tokens)
        {
            // TODO if multiple matches, default to longest match. Currently need tokens
            // from most to least specific
            matchLen = token.Match(rem);
            if (matchLen <= 0)
                continue;
            t = token;
        }
    }
}