using Microsoft.CodeAnalysis;

namespace Syntex.CLI;

public class ConsoleExporter : IExporter
{
    public void Write(string symbolName, INamedTypeSymbol? symbol)
    {
        if (string.IsNullOrWhiteSpace(symbolName))
        {
            Console.WriteLine("No symbol name specified");
            return;
        }

        Console.WriteLine(symbol?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) ?? $"{symbolName} not found");
    }

    public void Complete() { }
}
