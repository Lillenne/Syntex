using System.Runtime.InteropServices;

namespace Syntex.Test;


public class MermaidTests
{
    [Fact]
    public void Test1()
    {
        Assert.True(true);
    }

    [Fact]
    public void AccessibilityTests()
    {
    }

    private static string ExpectedOutput = """
                                            classDiagram
                                                class ITest
                                                ITest : +Method() Int32
                                                ITest : +Int32 Property

                                                class Test
                                                Test : +Method() Int32
                                                Test : +Int32 Property
                                                Test ..|> ITest

                                                class Test1~T~
                                                Test1~T~ : +Method() Int32
                                                Test1~T~ : +String SomeProp
                                                Test1~T~ : +Int32 Property
                                                Test <|.. Test1~T~

                                                class Test2~T, U~
                                                Test2~T, U~ : +String SomeProp2
                                                Test1~T~ <|.. Test2~T, U~
                                            """;
}

//
// public record MermaidToken(
//     LineType type,
//     string ClassName,
//     MermaidAccessibility Accessibility,
//     string? Name,
//     string? ReturnType);
//
// public interface IMermaidLineParser
// {
//     bool Parse(string line, out MermaidToken token);
// }
//
// public class MermaidClassDefinitionParser : IMermaidLineParser
// {
//
// }

// public enum LineType
// {
//     Blank = 0,
//     ChartType = 1,
//     ClassDefinition = 2,
//     Method = 3,
//     FieldOrProperty = 4,
//     Relation = 5,
// }
//

//     public static MermaidToken Parse(string str)
//     {
//         var classDefn = Regex.Match(str, "class (.+)");
//         if (classDefn.Success)
//             {
//                 return new MermaidToken(
//                     LineType.ClassDefinition,
//                     classDefn.Groups[1].Value,
//                     MermaidAccessibility.NotApplicable,
//                     null,
//                     null);
//             }
//         // var methodFieldProp = Regex.Match(str, "(\\S+) : (\\S+) (\\+#-\\*)(\\S+)(\\(\\)) (\\S*)");
//         // string pattern = @"\s*(\S+)(?:~(\S+)~)?\s*(:\s*(\+|\#|\-|\*)?(\S+)(\(\))?\s*(\S+))$";
//         string pattern = @"\s*(\S+)(?:~(\S+)~)?\s*:\s*(\+|\#|\-|~)?(\S+)(\((.*)\))?(\*)?\s*(\S+)?(\*)?(\$)?";
//         // \s+: Captures any initial whitepaces
//         // (\S+): Captures the class name (the first part before ~T~).
//         // (?:~(\S+)~): Optional non-capture group for generics
//         //     \S+: Matches one or more non-whitespace characters.
//         // \s*: Matches any leading whitespace (if any) after the ~T~.
//         // : : Matches the colon and the optional space after it.
//         // \s*: Matches any leading whitespace (if any) after the :
//         // (\+|\#|\-|\*)?: Captures the access modifier, which can be +, #, -, or ~.
//         // (\S+): Captures the method/field name.
//         // (\(\))?: Captures the optional parentheses () if they are present (methods only).
//         //     (.*) : Captures the method arguments
//         // (\*)? : Optionally captures the abstract modifier if after the method name
//         // \s*(\S+): Captures the return type (like Int32) that comes after any whitespace.
//         // \s* : Any whitespace before return type
//         // (\S+)? : Return type name
//         // (\*)? : Optionally captures the abstract modifier if after the return type name
//         // (\$)? : Captures the static modifier
//         var methodFieldProp = Regex.Match(str, pattern);
//         if (methodFieldProp.Success)
//         {
//             var clazz = methodFieldProp.Captures[1].Value;
//             var genericCapture = methodFieldProp.Captures[2].Value;
//             var isGeneric = string.IsNullOrWhiteSpace(genericCapture);
//             var nGeneric = isGeneric
//                 ? genericCapture.Count(@char => @char == ',') + 1
//                 : 0;
//             var accessibility = methodFieldProp.Captures[3].ValueSpan[0] switch
//             {
//                 '+' => MermaidAccessibility.Public,
//                 '-' => MermaidAccessibility.Private,
//                 '#' => MermaidAccessibility.Protected,
//                 _ => MermaidAccessibility.NotApplicable,
//             };
//             var name = methodFieldProp.Captures[4].Value;
//             var isMethod = !string.IsNullOrEmpty(methodFieldProp.Captures[5].Value);
//             // var args =
//             return new MermaidToken(
//                 isMethod ? LineType.Method : LineType.FieldOrProperty,
// methodFieldProp.Captures[1].Value,
//  Accessibility
//
//             );
//
//         }
//
//         var relation = Regex.Match(str, )
//
//     }

// [Flags]
// public enum MethodProperties
// {
//     Virtual = 1,
//     Abstract = 2,
//     Static = 4
// }

