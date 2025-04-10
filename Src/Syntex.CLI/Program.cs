using System.Collections.Concurrent;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

namespace Syntex.CLI;

partial class Program
{
    static async Task<int> Main(string[] args)
    {
        var classes = new Option<string[]>(
            name: "--classes",
            description: "The fully qualified names of the classes to generate diagrams for.")
        {
            IsRequired = true,
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = true,
        };

        var solution = new Option<FileInfo[]?>(
            name: "--proj",
        description: "The project(s) or solution(s) containing the files to parse.",
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
                        return [new FileInfo(value)];
                    result.ErrorMessage = $"Invalid solution file: {value}";
                    return null!;
                default:
                    result.ErrorMessage = "Too many files specified";
                    return null!;
            }
        })
        {
            AllowMultipleArgumentsPerToken = true,
            Arity = ArgumentArity.ZeroOrMore
        };

        var allAccessibility = new Option<bool>(
            "--all",
            """
            Include all methods, properties, and fields regardless of accessibility.
            Overrides individual accessibility settings.
            """);

        var pub = new Option<bool>(
            "--public",
            """
            Include only public methods, properties, and fields regardless of accessibility.
            Overrides individual accessibility settings and the 'all' setting. 
            """);
        var methodAccessibility = new Option<Accessibility>(
            name: "--method",
            description: "Minimum accessibility level for methods",
            getDefaultValue: () => Accessibility.Protected);
        var fieldAccessibility = new Option<Accessibility>(
            name: "--field",
            description: "Minimum accessibility level for fields",
            getDefaultValue: () => Accessibility.Protected);
        var propertyAccessibility = new Option<Accessibility>(
            name: "--props",
            description: "Minimum accessibility level for properties",
            getDefaultValue: () => Accessibility.Protected);

        var rc = new RootCommand("C# CLI Application to generate exports from C# source files.");
        rc.AddGlobalOption(solution);
        var mmd = new Command("mmd", "Export to mermaid");
        rc.AddCommand(mmd);
        var mmcd = new Command("cd", "Export to mermaid class diagram");
        mmd.AddCommand(mmcd);
        mmcd.AddOption(classes);
        mmcd.AddOption(allAccessibility);
        mmcd.AddOption(pub);
        mmcd.AddOption(methodAccessibility);
        mmcd.AddOption(fieldAccessibility);
        mmcd.AddOption(propertyAccessibility);
        mmcd.SetHandler(MermaidClassDiagram,
                        solution,
                        classes,
                        allAccessibility,
                        methodAccessibility,
                        fieldAccessibility,
                        propertyAccessibility,
        pub);
        return await rc.InvokeAsync(args);
    }

    private static bool DiscoverProject([NotNullWhen(true)] out FileInfo[]? project)
    {
        var sln = Directory.EnumerateFiles(Directory.GetCurrentDirectory())
            .Where(static f => f.EndsWith(".sln", StringComparison.InvariantCultureIgnoreCase))
            .ToList();
        if (sln.Count > 0)
        {
            project = sln.Select(s => new FileInfo(s)).ToArray();
            return true;
        }
        var proj = Directory.EnumerateFiles(Directory.GetCurrentDirectory())
            .Where(static f => IsProjOrSln(f))
            .ToList();
        if (proj.Count > 0)
        {
            project = proj.Select(s => new FileInfo(s)).ToArray();
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

    private static async Task<int> MermaidClassDiagram(
        FileInfo[]? solution,
        string[] classes,
        bool all,
        Accessibility method,
        Accessibility field,
        Accessibility property,
    bool pub)
    {
        if (solution is null || solution.Length == 0)
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

        if (solution.Any(s => !s.Exists))
        {
            var missing = string.Join(Environment.NewLine, solution.Where(s => !s.Exists).Select(s => s.FullName));
            var errMsg = $"""
                The following project(s) or solution(s) do not exist:
                {missing}
                """;
            throw new ArgumentException(errMsg, nameof(solution));
        }

        MSBuildLocator.RegisterDefaults();
        using var workspace = MSBuildWorkspace.Create();
        var comps = new ConcurrentBag<Compilation?>();
        await Parallel.ForEachAsync(solution, async (s, ct) =>
        {
            if (s.Extension.Equals(".sln", StringComparison.InvariantCultureIgnoreCase))
            {
                var sln = await workspace.OpenSolutionAsync(s.FullName);
                await Parallel.ForEachAsync(sln.Projects,
                    async (project, cancellationToken) => comps.Add(await project.GetCompilationAsync(cancellationToken)));
            }
            else
            {
                var proj = await workspace.OpenProjectAsync(s.FullName);
                comps.Add(await proj.GetCompilationAsync());
            }
        });

        if (all)
        {
            method = field = property = Accessibility.NotApplicable;
        }

        if (pub)
        {
            method = field = property = Accessibility.Public;
        }

        var exporter = new MermaidClassDiagram()
        {
            MinimumMethodAccessibility = method,
            MinimumFieldAccessibility = field,
            MinimumPropertyAccessibility = property
        };
        
        foreach (var cls in FixGenericNames(classes))
        {
            var symbol = comps.Where(c => c is not null)
                .Select(c => c!.GetTypeByMetadataName(cls))
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
