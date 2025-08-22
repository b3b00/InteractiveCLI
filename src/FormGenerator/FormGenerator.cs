using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;


namespace interactiveCLI.forms;

[Generator]
public class FormGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        
        Dictionary<ClassDeclarationSyntax,string> _lexerAndParserTypes = new();
        
        // Filter classes annotated with the [Report] attribute. Only filtered Syntax Nodes can trigger code generation.
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                (s, _) => s is ClassDeclarationSyntax,
                (ctx, _) => GetClassDeclarationForSourceGen(ctx))
            .Where(t => t.formAttributeFound)
            .Select((t, _) =>
            {
                _lexerAndParserTypes[t.classDeclarationSyntax] = t.formType;
                return t.classDeclarationSyntax;
            });

        var provider2 = context.SyntaxProvider.CreateSyntaxProvider((s, _) => s is ClassDeclarationSyntax || s is EnumDeclarationSyntax,
            ((ctx,_) =>  ctx.Node ));

        
        // Generate the source code.
        context.RegisterSourceOutput(context.CompilationProvider.Combine(provider2.Collect()),
            ((ctx, t) => GenerateCode(ctx, t.Left, t.Right)));
    }
    
    private void GenerateCode(SourceProductionContext context, Compilation compilation, ImmutableArray<SyntaxNode> declarations)
    {
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
        
        var formDeclarations = declarations.Where(x => IsForm(x as ClassDeclarationSyntax)).ToList();
context.Log($" found {formDeclarations.Count} forms");

        Dictionary<string, SyntaxNode> declarationsByName = formDeclarations.ToDictionary(x => getName(x));
        
        foreach (var declarationSyntax in formDeclarations)
        {
            
            if (declarationSyntax is ClassDeclarationSyntax classDeclarationSyntax)
            {
                context.Log($"processing form class {classDeclarationSyntax.Identifier.Text.ToString()}");
                var inputs = GetInputs(classDeclarationSyntax);
                

                var className = classDeclarationSyntax.Identifier.Text;
                
                var isPartial =
                    classDeclarationSyntax.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword));
                if (!isPartial)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "FormGeneratorErrors.NOT_PARTIAL",
                            "Form is not partial",
                            "Form {0} is not partial",
                            "form",
                            DiagnosticSeverity.Error,
                            true), classDeclarationSyntax.GetLocation(), classDeclarationSyntax.Identifier.Text));
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
                
dummySource +=@"
   }
}
";
                context.AddSource($"Generated{className}.g.cs",dummySource);
            }
            else
            {
                context.Log($"FormGenerator : {declarationSyntax.ToString()} is not a class declaration");
            }
        }
    }

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
    
    private static (ClassDeclarationSyntax classDeclarationSyntax, string formType, bool
        formAttributeFound) GetClassDeclarationForSourceGen(
            GeneratorSyntaxContext context)
    {

        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        // Go through all attributes of the class.
        foreach (AttributeListSyntax attributeListSyntax in classDeclarationSyntax.AttributeLists)
        {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                string name = attributeSyntax.Name.ToString();
                if (name == "Form")
                {
                    return (classDeclarationSyntax, name, true);            
                }
            }
        }
        return (null, null, false);
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
                SetMethod("Validator",methodDeclarationSyntax, getInputOrCreateNew,
                    (input,method) => input.Validator = methodDeclarationSyntax);
                
                SetMethod("Converter",methodDeclarationSyntax, getInputOrCreateNew,
                    (input,method) => input.Converter = methodDeclarationSyntax);
                
                SetMethod("DataSource",methodDeclarationSyntax, getInputOrCreateNew,
                    (input,method) => input.DataSource = methodDeclarationSyntax);
            }
        }
        ;
        return inputs.Values.ToList();

    }

    private static void SetMethod(string attributeName, MethodDeclarationSyntax methodDeclarationSyntax, Func<string, Input> getInputOrCreateNew, Action<Input,MethodDeclarationSyntax> setter  )
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
                    setter(input,methodDeclarationSyntax);
                }
            }
        }
    }

    
    
}