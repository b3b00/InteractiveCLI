using interactiveCLI.forms;
using formGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SharpFileSystem.FileSystems;

namespace SourceGeneratorTests;

public class Program
{
    
    public static void Main(string[] args)
    {
       
        
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(typeof(Program).Assembly);
        var testform = fs.ReadAllText("/testform.txt");

        var generator = new FormGenerator();

        // Source generators should be tested using 'GeneratorDriver'.
        var driver = CSharpGeneratorDriver.Create(new[] { generator });
        
        var compilation = CSharpCompilation.Create("ExpressionGenerated",
            new[] { CSharpSyntaxTree.ParseText(testform) },
            new[]
            {
                // To support 'System.Attribute' inheritance, add reference to 'System.Private.CoreLib'.
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            });

        // Run generators. Don't forget to use the new compilation rather than the previous one.
        var runResult = driver.RunGenerators(compilation).GetRunResult();

        var trees = runResult.GeneratedTrees;
        if (trees.Any())
        {
            foreach (var tree in trees)
            {
                Console.WriteLine(tree.ToString());
            }
        }
        else
        {
            Console.Error.WriteLine("no generated trees");
        }

        if (runResult.Diagnostics.Any())
        {
            foreach (var diagnostic in runResult.Diagnostics)
            {
                Console.Error.WriteLine(diagnostic.ToString());
            }
        }
        
        Console.WriteLine(runResult.ToString());

        
        
        
    }
}