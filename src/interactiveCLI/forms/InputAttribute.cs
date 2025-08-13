namespace interactiveCLI.forms;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class InputAttribute : Attribute
{

    public InputAttribute(string label)
    {
        Label = label;
    }
    public string Label { get; }
}