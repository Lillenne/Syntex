namespace Syntex;

public abstract class MermaidExpression
{
    public MermaidContext? Context { get; private set; }
    public int StartPosition { get; protected set; }
    public int EndPosition { get; protected set; }
    public ReadOnlySpan<char> Span => Context is null ? default : Context.TextSpan[StartPosition..EndPosition];

    public void Interpret(MermaidContext context)
    {
        Context = context;
        DoInterpret();
    }
    protected abstract void DoInterpret();
    public abstract void Accept(MermaidExpressionVisitor visitor);
}

public class MermaidDiagramTypeExpression : MermaidExpression
{
    public string DiagramType { get; set; }
    protected override void DoInterpret()
    {
        throw new NotImplementedException();
    }

    public override void Accept(MermaidExpressionVisitor visitor) => visitor.Visit(this);
}

public class MermaidDirectionExpression : MermaidExpression
{
    public enum MermaidDirection
    {
        TopToBottom = 0,
        BottomToTop = 1,
        LeftToRight = 2,
        RightToLeft = 3
    }
    public MermaidDirection Direction { get; set; }

    protected override void DoInterpret()
    {
        throw new NotImplementedException();
    }

    public override void Accept(MermaidExpressionVisitor visitor) => visitor.Visit(this);
}

public class MermaidFieldExpression : MermaidExpression
{
    public MermaidClassExpression? Parent { get; set; }
    public MermaidAccessibility Accesibility { get; set; }
    public string Name { get; set; }
    // public ReadOnlySpan<char> NameSpan { get; set; }

    public override void Accept(MermaidExpressionVisitor visitor) => visitor.Visit(this);
}

public class MermaidMethodExpression : MermaidExpression
{
    public MermaidClassExpression Parent { get; set; }
    public MermaidAccessibility Accesibility { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsStatic { get; set; }
    public IList<MermaidArgumentExpression> Arguments { get; } = [];
    public string MethodName { get; set; }
    // public ReadOnlySpan<char> MethodNameSpan => throw new NotImplementedException();
    public string ReturnType { get; set; }
    // public ReadOnlySpan<char> ReturnTypeSpan => throw new NotImplementedException();

    public override void Accept(MermaidExpressionVisitor visitor) => visitor.Visit(this);
}

/// <summary>
/// 1. <<interface>> Shape
/// 2. class Shape{
/// <<interface>>
/// }
/// </summary>
public class MermaidNoteExpression : MermaidExpression
{
    public string NoteText { get; set; }
    // public string NoteTextSpan { get; set; }

    public override void Accept(MermaidExpressionVisitor visitor) => visitor.Visit(this);
}

public class MermaidRelationExpression : MermaidExpression
{
    public MermaidRelationToken Relation { get; set; }
    public MermaidLinkToken Link { get; set; }
}

public class MermaidAttributesExpression : MermaidExpression
{

}

public class MermaidCardinalityExpression : MermaidExpression
{

}

public class MermaidCommentExpression : MermaidExpression
{

}

public class MermaidRelationshipExpression : MermaidExpression
{
    public string Description { get; set; }
    public MermaidClassExpression Left { get; set; }
    public MermaidClassExpression Right { get; set; }
    // public ReadOnlySpan<char> DescriptionSpan { get; }

    public override void Accept(MermaidExpressionVisitor visitor) => visitor.Visit(this);
}

public class MermaidArgumentExpression : MermaidExpression
{
    public string Type { get; set; }
    // public ReadOnlySpan<char> TypeSpan => throw new NotImplementedException();
    public string Name { get; set; }
    // public ReadOnlySpan<char> NameSpan => throw new NotImplementedException();

    public override void Accept(MermaidExpressionVisitor visitor) => visitor.Visit(this);
}

public class MermaidAnnotationExpression : MermaidExpression
{
    public string Annotation { get; set; }
    // public ReadOnlySpan<char> AnnotationSpan => throw new NotImplementedException();

    public override void Accept(MermaidExpressionVisitor visitor) => visitor.Visit(this);
}

public class MermaidClassExpression : MermaidExpression
{
    public IList<MermaidAnnotationExpression> Annotations { get; } = [];
    public IList<MermaidFieldExpression> Fields { get; } = [];
    public IList<MermaidMethodExpression> Methods { get; } = [];

    public override void Accept(MermaidExpressionVisitor visitor) => visitor.Visit(this);
}
