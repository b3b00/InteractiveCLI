using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace formGenerator;

public static class Extensions
{
    public static AttributeSyntax GetAttribute(this MemberDeclarationSyntax memberDeclarationSyntax, string attributeName)
    {
        foreach (AttributeListSyntax attributeListSyntax in memberDeclarationSyntax.AttributeLists)
        {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                string name = attributeSyntax.Name.ToString();
                if (name == attributeName)
                {
                    return attributeSyntax;
                }
            }
        }
        return null;
    }
    
    public static string GetNthStringArg(this AttributeSyntax attributeSyntax, int nth)
    {
        var arguments = attributeSyntax?.ArgumentList?.Arguments;
        if (arguments != null && arguments.HasValue && arguments.Value.Any() &&  arguments.Value.Count >= nth + 1 )
        {
            var nameArg = arguments.Value[nth];
            var expr = nameArg.Expression;
            if (expr != null)
            {
                if (expr is LiteralExpressionSyntax literal )
                {
                    if (literal.Kind() == SyntaxKind.StringLiteralExpression)
                    {
                        return literal.Token.ValueText;
                    }
                    ;
                }
                else if (expr is InvocationExpressionSyntax invocation)
                {
                    if (invocation.Expression is IdentifierNameSyntax identifier && identifier.ToString() == "nameof")
                    {
                        var e = invocation.Expression;
                        var first = invocation.ArgumentList.Arguments.First();
                        return first.ToString();
                    }
                }
                ;
                return null;
            }
        }

        return null;
    }

    public static void Log(this SourceProductionContext context, string message, Location location = null)
    {
        context.ReportDiagnostic(Diagnostic.Create(
            new DiagnosticDescriptor(
                "FormGeneratorErrors.INFO",
                message,
                message,
                "form",
                DiagnosticSeverity.Info,
                true), location ?? Location.None));
    }
}