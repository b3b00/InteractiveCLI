namespace interactiveCLI.forms;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class InputAttribute : Attribute
{

    public InputAttribute(string label)
    {
        Label = label;
    }
    
    public InputAttribute(string label, string pattern = null, int index = -1)
    {
        Label = label;
        Pattern = pattern;
        Index = index;
    }
    public string Label { get; }
    
    public string Pattern { get; }
    
    public int? Index { get; set; }
}

