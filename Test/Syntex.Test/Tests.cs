using Syntex.Parser;

namespace Syntex.Test;

public class Tests
{
    [Fact]
    public void TestGeneration()
    {
        var text = """
classDiagram
    A <|-- B
    B ..|> C
    
    class A
    class B {
      -TestField
      +int AnotherTestField
      +TestMethod() int*
      +VoidMethod()$
    }
""";

        var output = """
                     public class A
                     {
                     }

                     public class B : A, C
                     {
                         private TYPE TestField { get; set; }
                         
                         public int AnotherTestField { get; set; }
                         
                         
                         public abstract int TestMethod()
                         {
                         
                         }
                         
                         public static void VoidMethod()
                         {
                         
                         }
                     }
                     
                     
                     """;
        var export = new MermaidClassToCSharp().Export(text);
        Assert.Equal(output.ReplaceLineEndings(), export.ReplaceLineEndings());
    }

    [Fact]
    public void TestFields()
    {
        var text = """
                   classDiagram
                       MyClass : #DoStuff$
                       class MyClass {
                           #DoStuffA
                           ~DoStuffB$
                           +int DoStuffC
                           -string DoStuffD
                       }
                   """;

        var tree = MermaidClassToCSharp.CreateTree(text);
        var v = new Visitor();
        tree.Accept(v);
        var fields = v._classes["MyClass"].Properties;
        Assert.Equal(5, fields.Count);
        Assert.Equal(new MermaidProperty(
            "DoStuff",
            string.Empty,
            MermaidModifier.Protected,
            true), fields[0]);
        Assert.Equal(new MermaidProperty(
            "DoStuffA",
            string.Empty,
            MermaidModifier.Protected,
            false), fields[1]);
        Assert.Equal(new MermaidProperty(
            "DoStuffB",
            string.Empty,
            MermaidModifier.Internal,
            true), fields[2]);
        Assert.Equal(new MermaidProperty(
            "DoStuffC",
            "int",
            MermaidModifier.Public,
            false), fields[3]);
        Assert.Equal(new MermaidProperty(
            "DoStuffD",
            "string",
            MermaidModifier.Private,
            false), fields[4]);
    }

    [Fact]
    public void TestInheritance()
    {
        var text = """
classDiagram
    A <|-- B <|-- C : Label
    D <|-- E
    F ..|> G
""";
          var tree = MermaidClassToCSharp.CreateTree(text);
          var v = new Visitor();
          tree.Accept(v);
          AssertInheritanceContains(v._inheritance["C"], new []{"A", "B"});
          AssertInheritanceContains(v._inheritance["E"], new []{ "D" });
          AssertInheritanceContains(v._inheritance["F"], new []{ "G" });
    }

    [Fact]
    public void TestClasses()
    {
        var text = """
                   classDiagram
                       MyClass : #DoStuff()
                       class MyClass {
                           #DoStuffA(string hi, bye) type*
                           ~DoStuffB() int$
                           +DoStuffC()* int
                           -DoStuffD() string
                       }
                       class empty
                   """;

        var tree = MermaidClassToCSharp.CreateTree(text);
        var v = new Visitor();
        tree.Accept(v);
        Assert.Equal(2, v._classes.Count);
    }

    [Fact]
    public void TestMethods()
    {
        var text = """
classDiagram
    MyClass : #DoStuff()
    class MyClass {
        #DoStuffA(string hi, bye) type*
        ~DoStuffB() int$
        +DoStuffC()* int
        -DoStuffD() string
    }
""";

          var tree = MermaidClassToCSharp.CreateTree(text);
          var v = new Visitor();
          tree.Accept(v);
          var m = v._classes["MyClass"].Methods;
          Assert.Equal(new MermaidMethod(
                                         "DoStuff",
              MermaidModifier.Protected,
              string.Empty,
              string.Empty,
              false,
              false),
              m[0]);
          Assert.Equal(new MermaidMethod(
                                         "DoStuffA",
              MermaidModifier.Protected,
              "type",
              "string hi, bye",
              false,
              true),
              m[1]);
          Assert.Equal(new MermaidMethod(
                                         "DoStuffB",
              MermaidModifier.Internal,
              "int",
              string.Empty,
              true,
              false),
              m[2]);
          Assert.Equal(new MermaidMethod(
                                         "DoStuffC",
              MermaidModifier.Public,
              "int",
              string.Empty,
              false,
              true),
              m[3]);
          Assert.Equal(new MermaidMethod(
                                         "DoStuffD",
              MermaidModifier.Private,
              "string",
              string.Empty,
              false,
              false),
              m[4]);
          Assert.True(m.Count == 5);
    }

    private void AssertInheritanceContains(HashSet<string> collection, IEnumerable<string> values)
    {
        foreach (var parent in collection)
            Assert.Contains(parent, collection);
    }
}