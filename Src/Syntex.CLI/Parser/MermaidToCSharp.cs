using System.CodeDom.Compiler;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace Syntex.Parser;

public enum MermaidModifier
{
    Public,
    Protected,
    Private,
    Internal,
}

public record MermaidMethod(
    string Name,
    MermaidModifier Accessibility,
    string ReturnType,
    string Args,
    bool IsStatic,
    bool IsAbstract);

public record MermaidProperty(
    string Name,
    string Type,
    MermaidModifier Accessibility,
    bool IsStatic);

public class MermaidClass
{
    public string Name { get; set; } = string.Empty;
    public List<MermaidMethod> Methods { get; } = new();
    public List<MermaidProperty> Properties { get; } = new();
    public IEnumerable<string>? Inherits { get; set; }
}

public class MermaidClassToCSharp
{
    private readonly IndentedTextWriter _tw = new(new StringWriter());

    public static IParseTree CreateTree(string text)
    {
        ICharStream stream = CharStreams.fromString(text);
        ITokenSource lexer = new MermaidLexer(stream);
        ITokenStream tokens = new CommonTokenStream(lexer);
        MermaidParser parser = new MermaidParser(tokens, TextWriter.Null, TextWriter.Null);
        IParseTree tree = parser.diagram();
        return tree;
    }

    public string Export(string mermaid)
    {
        var tree = CreateTree(mermaid);
        var v = new Visitor();
        tree.Accept(v);
        foreach (var clazz in v.Classes())
        {
            Write(clazz);
        }

        return ToString();
    }

    public override string ToString() => _tw.InnerWriter.ToString() ?? string.Empty;

    public void Write(MermaidClass mermaid)
    {
        _tw.Write($"public class {mermaid.Name}");
        if (mermaid.Inherits is not null && mermaid.Inherits.Any())
        {
            _tw.Write(" : ");
            _tw.Write(string.Join(", ", mermaid.Inherits));
        }
        _tw.WriteLine();
        _tw.WriteLine("{");
        _tw.Indent++;
        foreach (var prop in mermaid.Properties)
        {
            _tw.Write(prop.Accessibility.ToString().ToLower());
            _tw.Write(' ');
            if (prop.IsStatic)
            {
                _tw.Write("static ");
            }

            _tw.Write(!string.IsNullOrEmpty(prop.Type) ? prop.Type : "TYPE");
            _tw.Write(' ');
            _tw.Write(prop.Name);
            _tw.Write(" { get; set; }");
            _tw.WriteLine();
            _tw.WriteLine();
        }

        foreach (var method in mermaid.Methods)
        {
            _tw.WriteLine();
            _tw.Write(method.Accessibility.ToString().ToLower());
            _tw.Write(' ');
            if (method.IsStatic)
            {
                _tw.Write("static ");
            }

            if (method.IsAbstract)
            {
                _tw.Write("abstract ");
            }

            _tw.Write(string.IsNullOrEmpty(method.ReturnType)
                ? "void"
                : method.ReturnType);
            _tw.Write(' ');

            _tw.Write(method.Name);

            _tw.Write('(');
            _tw.Write(method.Args);
            _tw.Write(')');

            _tw.WriteLine();
            _tw.Write('{');
            _tw.WriteLine();
            _tw.WriteLine();
            _tw.Write('}');
            _tw.WriteLine();
        }
        _tw.Indent--;
        _tw.WriteLine("}");
        _tw.WriteLine();
    }
}

public class Visitor : MermaidBaseVisitor<int>
{
    public Dictionary<string, HashSet<string>> _inheritance = new();
    public Dictionary<string, MermaidClass> _classes = new();

    public IEnumerable<MermaidClass> Classes()
    {
        foreach (var clazz in _inheritance)
        {
            if (_classes.TryGetValue(clazz.Key, out MermaidClass? value))
                value.Inherits = clazz.Value;
        }

        return _classes.Values;
    }

    private MermaidClass EnsureClassExists(string key)
    {
        if (_classes.TryGetValue(key, out var clazz))
        {
            return clazz;
        }
        _classes[key] = new() { Name = key };
        return _classes[key];
    }
    private void AddEntry(Dictionary<string, HashSet<string>> inheritance, string key, string value)
    {
        if (_inheritance.TryGetValue(key, out var set))
        {
            set.Add(value);
        }
        else
        {
            _inheritance[key] = new() { value };
        }
    }

    private IEnumerable<RuleContext> Parents(RuleContext context)
    {
        var c = context;
        while (c.Parent != null)
        {
            yield return c.Parent;
            c = c.Parent;
        }
    }

    private T? FirstParent<T>(RuleContext context) where T : RuleContext
    {
        return Parents(context).OfType<T>().FirstOrDefault();
    }

    public override int VisitField(MermaidParser.FieldContext context)
    {
        MermaidModifier access = context.access_modifier()?.GetText() switch
        {
            "#" => MermaidModifier.Protected,
            "-" => MermaidModifier.Private,
            "~" => MermaidModifier.Internal,
            _ => MermaidModifier.Public
        };
        string type = string.Empty;
        string name = string.Empty;
        var words = context.WORD();
        if (words is not null)
        {
            if (words.Length > 1)
            {
                type = words[0].GetText();
                name = words[1].GetText();
            }
            else
            {
                name = words[0].GetText();
            }
        }
        
        var field = new MermaidProperty(
            name,
            type,
            access,
            IsStatic: context.DOLLAR() is not null
        );
        var className = GetClassContext(context);
        var clazz = EnsureClassExists(className);
        clazz.Properties.Add(field);
        return base.VisitField(context);
    }

    public override int VisitMethod(MermaidParser.MethodContext context)
    {
        MermaidModifier access = context.access_modifier()?.GetText() switch
        {
            "#" => MermaidModifier.Protected,
            "-" => MermaidModifier.Private,
            "~" => MermaidModifier.Internal,
            _ => MermaidModifier.Public
        };
        var word = context.WORD();
        string returnType = string.Empty;
        if (word is not null && word.Length == 2)
        {
            returnType = word[1].GetText();
        }

        var abs = context.ASTERISK() is { Length: > 0 };
        string argStr = string.Empty;
        var args = context.argument_list();
        if (args is not null)
        {
            // first arg
            argStr = args.WORD() is { Length: >0 } ?
                string.Join(" ", args.WORD().Select(w => w.GetText()))
                : string.Empty;
            var terms = args.argument_terminator();
            if (terms is not null && terms.Length > 0)
            {
                var addStr = args.argument_terminator()
                    .Select(term => string.Join(' ', term.WORD().Select(t => t.GetText())));
                argStr = string.Join(", ", new[] { argStr }.Concat(addStr));
            }
        }
        var m = new MermaidMethod(
            context.WORD()[0].GetText(),
            access,
            returnType,
            argStr,
            context.DOLLAR() is not null,
            abs);
        var name = GetClassContext(context);

        var clazz = EnsureClassExists(name);
        clazz.Methods.Add(m);
        return base.VisitMethod(context);
    }

    private string GetClassContext(ParserRuleContext context)
    {
        string name;
        var c = FirstParent<MermaidParser.ClassContext>(context);
        if (c is not null)
        {
            // within class block
            name = c.WORD().GetText();
        }
        else
        {
            var impl = FirstParent<MermaidParser.ImplContext>(context);
            if (impl is null)
                throw new InvalidOperationException("Method must be in a class or impl block");
            name = impl.WORD().GetText();
        }

        return name;
    }

    public override int VisitClass(MermaidParser.ClassContext context)
    {
        EnsureClassExists(context.WORD().GetText());
        return base.VisitClass(context);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    /// <param name="flipped"></param>
    public void AddInheritance(ParserRuleContext context, bool flipped)
    {
        var term = FirstParent<MermaidParser.Relation_terminatorContext>(context);
        var rel = FirstParent<MermaidParser.RelationshipContext>(context);
        if (term is null || rel is null)
            throw new InvalidOperationException();
        var idx = rel.relation_terminator()
            .Select((t, idx) => ReferenceEquals(t, term) ? idx : -1)
            .Where(idx => idx != -1)
            .Single();
        var key = idx > 0 ? rel.relation_terminator(idx - 1).WORD()[0].GetText() : rel.WORD().GetText();
        var value = term.WORD()[0].GetText();
        if (flipped)
            AddEntry(_inheritance, value, key);
        else
            AddEntry(_inheritance, key, value);
    }

    public override int VisitInheritance(MermaidParser.InheritanceContext context)
    {
        AddInheritance(context, true);
        return base.VisitInheritance(context);
    }

    public override int VisitRealization(MermaidParser.RealizationContext context)
    {
        AddInheritance(context, false);
        return base.VisitRealization(context);
    }
}
