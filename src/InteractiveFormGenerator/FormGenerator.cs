using System.Text;
using generatorLogging;
using interactiveCLI.forms;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace interactiveFormGenerator;

[Generator]
public class FormGenerator : IIncrementalGenerator
{

    private static bool IsForm(ClassDeclarationSyntax classDeclarationSyntax)
    {
        foreach (AttributeListSyntax attributeListSyntax in classDeclarationSyntax.AttributeLists)
        {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                string name = attributeSyntax.Name.ToString();
                if (name == "Form")
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ClassDeclarationSyntax> calculatorClassesProvider =
            context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (SyntaxNode node, CancellationToken cancelToken) =>
                {
                    return node is ClassDeclarationSyntax classDeclarationSyntax && IsForm(classDeclarationSyntax);
                },
                transform: (GeneratorSyntaxContext ctx, CancellationToken cancelToken) =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)ctx.Node;
                    return classDeclaration;
                }
            );


        context.RegisterSourceOutput(calculatorClassesProvider,
            (sourceProductionContext, calculatorClass) => Execute(calculatorClass, sourceProductionContext));
    }

    /// <summary>
    /// This method is where the real work of the generator is done
    /// This ensures optimal performance by only executing the generator when needed
    /// The method can be named whatever you want but Execute seems to be the standard 
    /// </summary>
    /// <param name="formClass"></param>
    /// <param name="context"></param>
    public void Execute(ClassDeclarationSyntax formClass, SourceProductionContext context)
    {
        GeneratorLogging.SetLogFilePath("C:/tmp/FormGenLog.txt");
        Func<SyntaxNode, string> getName = (node) =>
        {
            if (node is ClassDeclarationSyntax classDeclarationSyntax)
            {
                return classDeclarationSyntax.Identifier.ToString();
            }

            if (node is EnumDeclarationSyntax enumDeclarationSyntax)
            {
                return enumDeclarationSyntax.Identifier.ToString();
            }

            return "";
        };
        //
        // var formDeclarations = declarations.Where(x => IsForm(x as ClassDeclarationSyntax)).ToList();
        // context.Log($" found {formDeclarations.Count} forms");
        //
        // Dictionary<string, SyntaxNode> declarationsByName = formDeclarations.ToDictionary(x => getName(x));
        //
        // foreach (var declarationSyntax in formDeclarations)
        // {

        // if (declarationSyntax is ClassDeclarationSyntax classDeclarationSyntax)
        // {
        context.Log($"processing form class {formClass.Identifier.Text.ToString()}");
        var inputs = GetInputs(formClass);


        var className = formClass.Identifier.Text;

        var isPartial =
            formClass.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword));
        if (!isPartial)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "FormGeneratorErrors.NOT_PARTIAL",
                    "Form is not partial",
                    "Form {0} is not partial",
                    "form",
                    DiagnosticSeverity.Error,
                    true), formClass.GetLocation(), formClass.Identifier.Text));
        }

        var dummySource = $@"
namespace foo; 
public partial class {className} {{
    
   public void Ask() {{
   
        Prompt prompt = new Prompt();
        ";
        foreach (Input input in inputs)
        {
            var inputSourceCode = InputGenerator.Generate(input);
            dummySource += @$"
//
// field {input.Name}
//
{inputSourceCode}";
        }

        dummySource += @"
   }
}
";
        context.AddSource($"Generated{className}.g.cs", dummySource);

    }


    private static List<Input> GetInputs(ClassDeclarationSyntax classDeclarationSyntax)
    {
        Dictionary<string, Input> inputs = new Dictionary<string, Input>();

        Func<string, Input> getInputOrCreateNew = (name) =>
        {
            if (!string.IsNullOrEmpty(name))
            {
                if (inputs.TryGetValue(name, out var input))
                {
                    return input;
                }

                var newInput = new Input(name);
                inputs[name] = newInput;
                return newInput;
            }

            return null;
        };

        var members = classDeclarationSyntax.Members;
        foreach (var member in members)
        {
            if (member is PropertyDeclarationSyntax propertyDeclarationSyntax)
            {
                var name = propertyDeclarationSyntax.Identifier.ToString();
                var inputAttribute = propertyDeclarationSyntax.GetAttribute("Input");
                if (inputAttribute != null)
                {
                    var input = getInputOrCreateNew(name);
                    if (input != null)
                    {
                        input.Field = propertyDeclarationSyntax;
                        input.InputAttribute = inputAttribute;
                    }
                }
            }

            if (member is MethodDeclarationSyntax methodDeclarationSyntax)
            {
                SetMethod("Validator", methodDeclarationSyntax, getInputOrCreateNew,
                    (input, method) => input.Validator = methodDeclarationSyntax);

                SetMethod("Converter", methodDeclarationSyntax, getInputOrCreateNew,
                    (input, method) => input.Converter = methodDeclarationSyntax);

                SetMethod("DataSource", methodDeclarationSyntax, getInputOrCreateNew,
                    (input, method) => input.DataSource = methodDeclarationSyntax);
            }
        }

        ;
        return inputs.Values.ToList();

    }

    private static void SetMethod(string attributeName, MethodDeclarationSyntax methodDeclarationSyntax,
        Func<string, Input> getInputOrCreateNew, Action<Input, MethodDeclarationSyntax> setter)
    {
        var dataSourceAttribute = methodDeclarationSyntax.GetAttribute(attributeName);
        if (dataSourceAttribute != null)
        {
            var name = dataSourceAttribute.GetNthStringArg(0);
            if (!string.IsNullOrEmpty(name))
            {
                var input = getInputOrCreateNew(name);
                if (input != null)
                {
                    setter(input, methodDeclarationSyntax);
                }
            }
        }
    }
}


    