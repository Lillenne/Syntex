using System.Collections.Concurrent;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Syntex;

partial class Program
{
    static async Task<int> Main(string[] args)
    {
        var rc = new RootCommand("C# CLI Application to generate mermaid class diagrams from C# source files.");
        var classes = new Option<string[]>(
            name: "--classes",
            description: "The fully qualified names of the classes to generate diagrams for.")
        {
            IsRequired = true,
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = true,
        };

        var solution = new Option<FileInfo?>(
            name: "--sln",
        description: "The project or solution containing the files to parse.",
        parseArgument: result =>
        {
            Console.WriteLine($"Tokens {result.Tokens.Count}, {result.Tokens}");
            switch (result.Tokens.Count)
            {
                case 0:
                    if (DiscoverProject(out var proj))
                        return proj;
                    result.ErrorMessage = "No solution specified or found in cwd";
                    return null!;
                case 1:
                    var value = Path.GetFullPath(result.Tokens.Single().Value);
                    if (File.Exists(value) && IsProjOrSln(value))
                        return new FileInfo(value);
                    result.ErrorMessage = $"Invalid solution file: {value}";
                    return null!;
                default:
                    result.ErrorMessage = "Too many files specified";
                    return null!;
            }
        });

        var outputOption = new Option<FileInfo?>(
            name: "--output",
            description: "The file to save the mermaid class diagram to.");

        rc.AddOption(solution);
        rc.AddOption(outputOption);
        rc.AddOption(classes);
        rc.SetHandler(Handler, solution, outputOption, classes);
        return await rc.InvokeAsync(args);
    }

    private static bool DiscoverProject([NotNullWhen(true)] out FileInfo? project)
    {
        var sln = Directory.EnumerateFiles(Directory.GetCurrentDirectory())
            .FirstOrDefault(static f => f.EndsWith(".sln", StringComparison.InvariantCultureIgnoreCase));
        if (sln is not null)
        {
            project = new FileInfo(sln);
            return true;
        }
        var proj = Directory.EnumerateFiles(Directory.GetCurrentDirectory())
            .FirstOrDefault(static f => IsProjOrSln(f));
        if (File.Exists(proj))
        {
            project = new FileInfo(proj);
            return true;
        }

        project = null;
        return false;
    }

    private static bool IsProjOrSln(string f)
    {
        return f.EndsWith(".sln", StringComparison.InvariantCultureIgnoreCase)
            || f.EndsWith(".csproj", StringComparison.InvariantCultureIgnoreCase);;
    }

    private static async Task<int> Handler(FileInfo? solution, FileInfo? output, string[] classes)
    {
        if (solution is null)
        {
            if (!DiscoverProject(out solution))
            {
                throw new ArgumentException($"""
                                             No solution specified and none in the current working directory. 
                                             cwd: {Directory.GetCurrentDirectory()}""
                                             Aborting...
                                             """);
            }
        }

        if (!solution.Exists)
        {
            throw new ArgumentException("The project or solution does not exist.", nameof(solution));
        }

        using var workspace = MSBuildWorkspace.Create();
        var comps = new ConcurrentBag<Compilation?>();
        if (solution.Extension.Equals(".sln", StringComparison.InvariantCultureIgnoreCase))
        {
            var sln = await workspace.OpenSolutionAsync(solution.FullName);
            await Parallel.ForEachAsync(sln.Projects,
                async (project, cancellationToken) => comps.Add(await project.GetCompilationAsync(cancellationToken)));
        }
        else
        {
            var proj = await workspace.OpenProjectAsync(solution.FullName);
            comps.Add(await proj.GetCompilationAsync());
        }

        var exporter = new MermaidClassDiagram(output?.FullName);
        foreach (var cls in FixGenericNames(classes))
        {
            var symbol = comps.Select(c => c.GetTypeByMetadataName(cls))
                .FirstOrDefault(t => t is not null);
            exporter.Write(cls, symbol);
        }

        exporter.Complete();
        return 0;
    }

    private static IEnumerable<string> FixGenericNames(string[] classes)
    {
        return classes.Select(c => 
            GenericRegex().Replace(c, match =>
            {
                var c = 1;
                foreach (var character in match.Value)
                    if (character == ',')
                        c++;
                return $"`{c}";
            })).Distinct();
    }

    [GeneratedRegex("<.*>")]
    public static partial Regex GenericRegex();
}
