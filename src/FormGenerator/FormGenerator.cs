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


        Dictionary<string, SyntaxNode> declarationsByName = formDeclarations.ToDictionary(x => getName(x));
        
        foreach (var declarationSyntax in formDeclarations)
        {
            if (declarationSyntax is ClassDeclarationSyntax classDeclarationSyntax)
            {
                
                GetInputs(classDeclarationSyntax);
                GetValidators(classDeclarationSyntax);
                GetConverters(classDeclarationSyntax);

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
public public class {className} {{
    
    public void heyRodriguez(string s) {{
        Console.WriteLine(""Rodriguez ! père, et fils"");
    }}
}}
";
                context.AddSource($"GeneratedBar.g.cs",dummySource);
                // context.AddSource($"Generated{parserDecl.Identifier.Text}.g.cs", SourceText.From(generatedParser.sourceCode, Encoding.UTF8));
                // context.AddSource($"{className}.g.cs", SourceText.From(generatedGenerator, Encoding.UTF8));

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
                    }
                }
            }

            if (member is MethodDeclarationSyntax methodDeclarationSyntax)
            {
                var validatorAttribute = methodDeclarationSyntax.GetAttribute("Validator");
                if (validatorAttribute != null)
                {
                    var name = validatorAttribute.GetFirstStringArg();
                    if (!string.IsNullOrEmpty(name))
                    {
                        var input = getInputOrCreateNew(name);
                        if (input != null)
                        {
                            input.Validator = methodDeclarationSyntax;
                        }
                    }
                }
                var converterAttribute = methodDeclarationSyntax.GetAttribute("Converter");
                if (converterAttribute != null)
                {
                    var name = converterAttribute.GetFirstStringArg();
                    if (!string.IsNullOrEmpty(name))
                    {
                        var input = getInputOrCreateNew(name);
                        if (input != null)
                        {
                            input.Converter = methodDeclarationSyntax;
                        }
                    }
                }
            }
        }
        ;
        return inputs.Values.ToList();

    }

    private static Dictionary<string,string> GetValidators(ClassDeclarationSyntax classDeclarationSyntax)
    {
        return new Dictionary<string, string>();
    }

    private static Dictionary<string,(string methodName, string type)> GetConverters(ClassDeclarationSyntax classDeclarationSyntax)
    {
        return new Dictionary<string, (string methodName, string type)>();
    }
    
}