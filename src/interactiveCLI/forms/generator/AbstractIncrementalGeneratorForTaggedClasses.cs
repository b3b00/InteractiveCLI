using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace interactiveCLI.forms.generator;

public abstract class AbstractIncrementalGeneratorForTaggedClasses<T> : IIncrementalGenerator
{
    private static bool IsTagged(ClassDeclarationSyntax classDeclarationSyntax)
    {
        var fullName = typeof(T).Name; 
        var tag = fullName.EndsWith("Attribute") ? fullName.Substring(0, fullName.Length - "Attribute".Length) : fullName;
        
        foreach (AttributeListSyntax attributeListSyntax in classDeclarationSyntax.AttributeLists)
        {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                string name = attributeSyntax.Name.ToString();
                if (name == tag)
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
                    return node is ClassDeclarationSyntax classDeclarationSyntax && IsTagged(classDeclarationSyntax);
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
    public abstract void Execute(ClassDeclarationSyntax formClass, SourceProductionContext context);
}