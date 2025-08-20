using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace interactiveCLI.forms;

public class Input
{
    public string Name { get; set; }
    
    public PropertyDeclarationSyntax Field { get; set; }
    
    public MethodDeclarationSyntax Validator { get; set; }
    
    public MethodDeclarationSyntax Converter {get; set;}

    public Input(string name)
    {
        Name = name;
    }
    
    public Input(string name, PropertyDeclarationSyntax field, MethodDeclarationSyntax validator,
        MethodDeclarationSyntax converter) : this(name)
    {
        Field = field;
        Validator = validator;
        Converter = converter;
    } 
}