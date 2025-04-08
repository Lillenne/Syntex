using System.CodeDom.Compiler;
using Microsoft.CodeAnalysis;

namespace Syntex;

public class MermaidClassDiagram : IExporter
{
    private readonly List<string> _notFound;
    private string? _className;
    private Dictionary<INamedTypeSymbol, List<INamedTypeSymbol?>> _classes = new(SymbolEqualityComparer.Default);
    private IndentedTextWriter _writer;

    public MermaidClassDiagram()
    {
        _notFound = [];
        _writer = new IndentedTextWriter(new StringWriter());
        Append("classDiagram");
        AppendLine();
        _writer.Indent++;
    }

    public void Write(string clazz, INamedTypeSymbol? symbol)
    {
        if (symbol is null)
        {
            _notFound.Add(clazz);
            return;
        }

        var parents = new List<INamedTypeSymbol?>();
        _classes[symbol] = parents;
        if (symbol.BaseType is not null)
            parents.Add(symbol.BaseType);

        parents.AddRange(symbol.Interfaces);
    }

    public void Complete()
    {
        foreach (var clazz in _classes)
        {
            FinalWrite(clazz.Key);
            foreach (var parent in clazz.Value
                         .Where(c => c is not null && _classes.Keys.Any(p => p.Name == c.Name)))
            {
                // TODO if base class implements interface, don't directly add realization relation
                var text = parent!.IsAbstract
                    ? $"{FormatTypeText(clazz.Key)} ..|> {FormatTypeText(parent)}"
                    : $"{FormatTypeText(parent)} <|.. {FormatTypeText(clazz.Key)}";
                Append(text);
                AppendLine();
            }

            AppendLine();
        }

        if (_notFound.Count != 0)
        {
            AppendLine();
            Append($"note \"{string.Join(", ", _notFound)} not found\"");
        }

        var s = _writer.InnerWriter.ToString();
        Console.WriteLine(s);
    }

    private void WriteName()
    {
        ArgumentException.ThrowIfNullOrEmpty(_className);
        Append($"class {_className}");
        AppendLine();
    }

    private void Write(IMethodSymbol method)
    {
        WriteClassPrefix();
        Append('+');
        Append(method.Name);
        Append('(');
        Append(string.Join(", ", method.Parameters.Select(p => $"{p.Type.Name} {p.Name}")));
        Append(')');
        if (!method.ReturnsVoid)
        {
            Append(' ');
            Append(method.ReturnType.Name);
        }
        AppendLine();
    }

    private void Write(IPropertySymbol property)
    {
        WriteClassPrefix();
        Append('+');
        Append(property.Type.Name);
        Append(' ');
        Append(property.Name);
        AppendLine();
    }

    private void Write(IFieldSymbol field)
    {
        WriteClassPrefix();
        Append('+');
        Append(field.Type.Name);
        Append(' ');
        Append(field.Name);
        AppendLine();
    }

    private void WriteClassPrefix() => _writer.Write($"{_className} : ");

    private string FormatTypeText(INamedTypeSymbol symbol)
    {
        var name = symbol.Name;
        if (!symbol.IsGenericType)
        {
            return name;
        }

        var count = _classes.Keys.Count(k => k.Name == symbol.Name);
        char start = 'T';
        var replacement = string.Join(", ",
            Enumerable.Range(0, symbol.TypeArguments.Length)
                .Select(i => (char)(start + i)));
        return $"{symbol.Name}{(count > 1 ? symbol.TypeArguments.Length : "")}~{replacement}~";
    }

    private void FinalWrite(INamedTypeSymbol symbol)
    {
        _className = FormatTypeText(symbol);
        WriteName();
        var methods = symbol.GetMembers()
            .Where(s => s is { Kind: SymbolKind.Method, DeclaredAccessibility: Accessibility.Public })
            .Cast<IMethodSymbol>()
            .Where(s => s.MethodKind == MethodKind.Ordinary);
        foreach (var method in methods)
            Write(method);

        var fields = symbol.GetMembers()
            .Where(s => s is { Kind: SymbolKind.Field, DeclaredAccessibility: Accessibility.Public })
            .Cast<IFieldSymbol>();
        foreach (var field in fields)
            Write(field);

        var properties = symbol.GetMembers()
            .Where(s => s is { Kind: SymbolKind.Property, DeclaredAccessibility: Accessibility.Public })
            .Cast<IPropertySymbol>();
        foreach (var prop in properties)
            Write(prop);
    }

    private void Append(char text) => _writer.Write(text);
    private void Append(string text) => _writer.Write(text);
    private void AppendLine() => _writer.WriteLine();
}
