using System.Runtime.InteropServices;

namespace Syntex;

public abstract class ExpressionRecognizer
{
    public virtual MermaidExpression? Recognize(MermaidContext context)
    {
        foreach (var sequence in LookAhead)
        {
            var span = CollectionsMarshal.AsSpan(context.Tokens);
            var next = span[context.CurrentTokenIndex..(context.CurrentTokenIndex + sequence.Length)];
            if (SequenceEqual(next, sequence))
            {
                throw new NotImplementedException();
                // TODO how to visit recursively?? Left factorization?
                // e.g., class that has methods in it?
                // I need to step into the non-terminal scope
            }
        }

        return null;
    }

    private static bool SequenceEqual(Span<TokenPosition> next, MermaidToken[] sequence)
    {
        for (var i = 0; i < next.Length; i++)
        {
            if (next[i].Token != sequence[i])
            {
                return false;
            }
        }

        return true;
    }

    protected abstract MermaidToken[][] LookAhead { get; }
}

public class MethodExpressionRecognizer : ExpressionRecognizer
{
    protected override MermaidToken[][] LookAhead { get; } =
    {
        new MermaidToken[] { new RawStringToken(), new SemicolonToken(), new RawStringToken(), new OpenBraceToken()},
    };

    protected void Assemble(MermaidContext context, MermaidToken[] matchedSequence)
    {

    }
}