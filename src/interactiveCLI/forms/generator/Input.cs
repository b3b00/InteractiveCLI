using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace interactiveCLI.forms;

public class Input
{
    private string _name;

    public string Name { get; set; }

    public string Pattern { get; set; }
    
    public bool IsPasword { get; set; }
    
    public PropertyDeclarationSyntax Field { get; set; }
    
    public string Validator { get; set; }
    
    public string Converter {get; set;}
    
    public string DataSource {get; set;}
    
    public string CharValidator {get; set;}
    
    public string Condition {get; set;}
    
    public AttributeSyntax InputAttribute { get; set; }
    public int Index { get; set; }

    public Input(string name)
    {
        Name = name;
    }
    
    public Input(string name, PropertyDeclarationSyntax field, string validator,
        string converter, string dataSource) : this(name)
    {
        Field = field;
        Validator = validator;
        Converter = converter;
        DataSource = dataSource;
    } 
}