﻿using Microsoft.CodeAnalysis;
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
        if (arguments.HasValue)
        {
            var expressions = arguments.Value.GetAttributeArgumentExpressions();
            Predicate<ExpressionSyntax> isStringLiteral = expr =>
            {
                return expr is LiteralExpressionSyntax s && s.Kind() == SyntaxKind.StringLiteralExpression;
            };
            Predicate<ExpressionSyntax> isNameOf = expr =>
            {
                if (expr is InvocationExpressionSyntax invocation)
                {
                    if (invocation.Expression is IdentifierNameSyntax identifier && identifier.ToString() == "nameof")
                    {
                        return true;
                    }
                }
                return false;
            };
            

            var stringExpressions = expressions.Where(x => isStringLiteral(x) || isNameOf(x)).ToList();
            if (stringExpressions != null && stringExpressions.Any() && stringExpressions.Count >= nth + 1)
            {
                var nthArg = stringExpressions[nth];
                if (nthArg is LiteralExpressionSyntax literal)
                {
                    return literal.Token.ValueText;
                }

                if (nthArg is InvocationExpressionSyntax invocation)
                {
                    if (invocation.Expression is IdentifierNameSyntax identifier && identifier.ToString() == "nameof")
                    {
                        var e = invocation.Expression;
                        var first = invocation.ArgumentList.Arguments.First();
                        return first.ToString();
                    }
                }
            }
        }

        return null;
    }
    
    public static int GetNthIntArg(this AttributeSyntax attributeSyntax, int nth)
    {
        var arguments = attributeSyntax?.ArgumentList?.Arguments;
        if (arguments.HasValue)
        {
            var expressions = arguments.Value.GetAttributeArgumentExpressions();
            Predicate<ExpressionSyntax> isIntLiteral = expr =>
            {
                return expr is LiteralExpressionSyntax s && s.Kind() == SyntaxKind.NumericLiteralExpression;
            };
            
            

            var intExpressions = expressions.Where(x => isIntLiteral(x)).ToList();
            if (intExpressions != null && intExpressions.Any() && intExpressions.Count >= nth + 1)
            {
                var nthArg = intExpressions[nth];
                if (nthArg is LiteralExpressionSyntax literal)
                {
                    return int.Parse(literal.Token.ValueText);
                }
            }
        }
        return -1;
    }

    public static List<ExpressionSyntax> GetAttributeArgumentExpressions(
        this SeparatedSyntaxList<AttributeArgumentSyntax> attributesArgs)
    {
        List<ExpressionSyntax> result = new List<ExpressionSyntax>();
        if (attributesArgs.Any())
        {
            result = attributesArgs.Select(x => x.Expression).ToList();
        }
        return result;
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