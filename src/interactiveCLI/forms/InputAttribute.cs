namespace interactiveCLI.forms;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class InputAttribute<T> : Attribute
{

    public InputAttribute(string label)
    {
        Label = label;
    }
    
    public InputAttribute(string label, string? pattern = null, string[]? possibleValues = null, Predicate<string>? validator = null, Func<string,T>? converter = null)
    {
        Label = label;
        Pattern = pattern;
        PossibleValues = possibleValues;
        Validator = validator;
        Converter = converter;
    }
    public string Label { get; }
    
    public string? Pattern { get; }
    
    public string[]? PossibleValues { get; }
    
    public Predicate<string>? Validator { get; }
    
    public Func<string,T>? Converter { get; }
    
}