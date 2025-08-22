using interactiveCLI.forms;
using formGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceGeneratorTests;

public class Program
{
    
    public static void Main(string[] args)
    {
        var source = @"
namespace foo;
[Form]
internal partial class Bar
{
    public void Main()
    {
        Console.WriteLine(""Hello generator!"");
    }

    [Input(""hello"")]
    public bool Hello {get; set;}

    [Input(""good bye !"")]
    public string GoodBye {get; set;}

    [Validator(""Hello"")]
    public bool IsValid(string value) => true;

    [Converter(nameof(Hello))]
    public bool Convert(string value) => value == ""yes"";

    [Input(""select me :"")]
    public string SelectMe {get; set;} 

    [DataSource(nameof(SelectMe))]
    public string[] SelectMeDataSource() => new string[]{""Orange"",""Apple"",""Banana"",""Apricot""}; 

    [Input(""BirthDay"", ""__/__/____"")]
    DateTime BirthDay {get; set;}

    [Validator(nameof(BirthDay))]
    bool ValidateDate(string s) =>DateTime.TryParseExact(s, ""dd/MM/yyyy"", null, DateTimeStyles.None, out var d);

    [Converter(nameof(BirthDay))]
    DateTime ConvertDate(string s) {{
        if (DateTime.TryParseExact(s, ""dd/MM/yyyy"", null, DateTimeStyles.None, out var d)) {{
            return d;
        }} ;
        return DateTime.Now;
    }}
 
};
";

        var generator = new FormGenerator();

        // Source generators should be tested using 'GeneratorDriver'.
        var driver = CSharpGeneratorDriver.Create(new[] { generator });
        
        var compilation = CSharpCompilation.Create("ExpressionGenerated",
            new[] { CSharpSyntaxTree.ParseText(source) },
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