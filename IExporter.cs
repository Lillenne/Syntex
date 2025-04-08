using Microsoft.CodeAnalysis;

namespace Syntex;

// Should probably use the existing visitor structure...
public interface IExporter
{
    void Write(string symbolName, INamedTypeSymbol? symbol);
    void Complete();
}
