using System.Text;
//using generatorLogging;
using interactiveCLI.forms;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace formGenerator;

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
                transform: (ctx, cancelToken) =>
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
        string invalidInputMessage = null;
        var formAttribute = formClass.GetAttribute("Form");
        if (formAttribute != null)
        {
            invalidInputMessage = formAttribute.GetNthStringArg(0);
        }
        
        var inputs = GetInputs(formClass);

        var className = formClass.Identifier.Text;
        //The previous Descendent Node check has been removed as it was only intended to help produce the error seen in logging
        BaseNamespaceDeclarationSyntax? formNamespace = formClass.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
        formNamespace ??= formClass.Ancestors().OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault();
            
        if(formNamespace is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "FormGeneratorErrors.NS_NOT_FOUND",
                    $"Could not find namespace for {className}",
                    "Form {0} has no namespace",
                    "form",
                    DiagnosticSeverity.Error,
                    true), formClass.GetLocation(), formClass.Identifier.Text));
        }
        
        var isPartial =
            formClass.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword));
        
        // GeneratorLogging.LogMessage($"{className} {(isPartial ? "is" : "is not")} partial");
        
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

        string invalidError = invalidInputMessage == null ? "null" : $@"""{invalidInputMessage}""";

        var dummySource = $@"
using interactiveCLI;
using interactiveCLI.forms;

namespace {formNamespace?.Name};
 


public partial class {className} {{
    
   public void Ask() {{
   
        Prompt prompt = new Prompt({invalidError});
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
                        int index = inputAttribute.GetNthIntArg(0);
                        input.Index = index;
                    }
                

           
                SetMethod("Validator", propertyDeclarationSyntax, getInputOrCreateNew,
                    (method) => input.Validator = method);

                SetMethod("Converter", propertyDeclarationSyntax, getInputOrCreateNew,
                    (method) => input.Converter = method);

                SetMethod("DataSource", propertyDeclarationSyntax, getInputOrCreateNew,
                    (method) => input.DataSource = method);
                
                SetMethod("CharValidator", propertyDeclarationSyntax, getInputOrCreateNew,
                    ( method) => input.CharValidator = method);
                }
            }
        }

        var ordered = inputs.Values.Where(x => x.Index >= 0).OrderBy(x => x.Index).ToList();
        ordered.AddRange(inputs.Values.Where(x => x.Index < 0));
        return ordered;

    }

    private static void SetMethod(string attributeName, PropertyDeclarationSyntax propertyDeclarationSyntax,
        Func<string, Input> getInputOrCreateNew, Action<string> setter)
    {
        var callbackAttribute = propertyDeclarationSyntax.GetAttribute(attributeName);
        if (callbackAttribute != null)
        {
            var name = callbackAttribute.GetNthStringArg(0);
            
            if (!string.IsNullOrEmpty(name))
            {
                    setter(name);
            }
        }
    }
}


    