using formGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using SharpFileSystem.FileSystems;
using Xunit;

namespace PromptTests;

public class GeneratorTests
{
    private string GenerateAndCheckExpectations(string form, string[] expectations)
    {
        var runResult = Generate(form);

        var errors = runResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
        Assert.Empty(errors);
        var trees = runResult.GeneratedTrees;
        Assert.NotNull(trees);
        Assert.Single(trees);
        

        var first = trees.First()?.ToString();
        Assert.NotNull(first);
        Assert.NotEmpty(first);
        foreach (var expectation in expectations)
        {
            Assert.Contains(expectation, first);
        }
        
        return first;
    }

    private void GenerateAndCheckError(string form, FormGeneratorErrors error)
    {
        var runResult = Generate(form);
        //var errors = runResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
        Assert.NotNull(runResult);
        Assert.NotNull(runResult.Diagnostics);
        Assert.Single(runResult.Diagnostics);
        var diagnostic = runResult.Diagnostics[0];
        Assert.NotNull(diagnostic);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal(GetErrorId(error), diagnostic.Id);
        
        
    }

    private GeneratorDriverRunResult Generate(string form)
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(GetType().Assembly);
        var testform = fs.ReadAllText($"/generatorData/{form}");

        var generator = new FormGenerator();

        // Source generators should be tested using 'GeneratorDriver'.
        var driver = CSharpGeneratorDriver.Create(new[] { generator });
        
        var compilation = CSharpCompilation.Create("GeneratorTests",
            new[] { CSharpSyntaxTree.ParseText(testform) },
            new[]
            {
                // To support 'System.Attribute' inheritance, add reference to 'System.Private.CoreLib'.
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            });

        // Run generators. Don't forget to use the new compilation rather than the previous one.
        var runResult = driver.RunGenerators(compilation).GetRunResult();
        return runResult;
    }


    [Fact]
    public void TestTextInputAndGlobalErrorMessage()
    {
        string[] expectations =
        [
            "public partial class TestText",
            "Prompt(\"[1;31mInvalid input.[0m\");",
            "var TestResult = prompt.Ask<string>(\"code : \","
        ];
        var test = GenerateAndCheckExpectations("testText.txt", expectations);
    }
    
    [Fact]
    public void TestSelect()
    {
        string[] expectations =
        [
            "public partial class TestSelect",
            "prompt.Ask<string>(\"select a fruit :\",",
            "dataSource:() => SelectMeDataSource(),"
        ];
        var test = GenerateAndCheckExpectations("testSelect.txt", expectations);
    }
    
    [Fact]
    public void TestPassword()
    {
        string[] expectations =
        [
            "public partial class TestPassword",
            "prompt.AskPassword(\"password : \",hiddenChar:'*', validator:null, condition:null, callbacks:null);"
        ];
        var test = GenerateAndCheckExpectations("testPassword.txt", expectations);
    }
    
    [Fact]
    public void TestTextArea()
    {
        string[] expectations =
        [
            "public partial class TestTextArea",
            "prompt.AskMultiLineText(\"content : \", maxLines:5, finishKey:ConsoleKey.Enter, validator:null, condition:null, null);"
        ];
        var test = GenerateAndCheckExpectations("testTextArea.txt", expectations);
    }
    
    [Fact]
    public void TestTextAreaWithFinishKey()
    {
        string[] expectations =
        [
            "public partial class TestTextAreaWithFinishKey",
            "prompt.AskMultiLineText(\"content : \", maxLines:5, finishKey:ConsoleKey.D, validator:null, condition:null, null);"
        ];
        var test = GenerateAndCheckExpectations("testTextAreaWithFinishKey.txt", expectations);
    }

    [Fact]
    public void TestCallbacks()
    {
        string[] expectations =
        [
            "public partial class TestTextWithCallbacks {",
            "prompt.Ask<string>(",
            "callbacks:(string s) => CallBack1(s), (string s) => CallBack2(s)"
        ];
        var test = GenerateAndCheckExpectations("testTextWithCallBacks.txt", expectations);
    }

    
    private string GetErrorId(FormGeneratorErrors error)
    {
        return $"FORM_ERROR_{error}";
    }

    [Fact]
    public void TestMissingPartial()
    {
        GenerateAndCheckError("errors/testNotPartial.txt",FormGeneratorErrors.NOT_PARTIAL);
    }
    
    [Fact]
    public void TestMissingNamespace()
    {
        GenerateAndCheckError("errors/testNoNamespace.txt", FormGeneratorErrors.NS_NOT_FOUND);
    }
}