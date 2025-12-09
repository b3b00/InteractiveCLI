namespace interactiveCLI.forms;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class TextAreaAttribute : InputAttribute
{
    public int MaxLines { get; set; } = 0;

    public TextAreaAttribute(string label, int maxLines = 0) : base(label)
    {
        MaxLines = maxLines;
    }
    public TextAreaAttribute(string label, int maxLines = 0, int index = -1) : base(label, null, index)
    {
        MaxLines = maxLines;
    }
}