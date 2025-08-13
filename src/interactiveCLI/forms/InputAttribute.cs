namespace interactiveCLI.forms;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class InputAttribute : Attribute
{

    public InputAttribute(string label)
    {
        Label = label;
    }
    
    public InputAttribute(string label, string pattern)
    {
        Label = label;
        Pattern = pattern;
    }
    public string Label { get; }
    
    public string Pattern { get; }
}