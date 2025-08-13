namespace interactiveCLI.forms;

public class SelectAttribute : InputAttribute
{
    public string[] Values { get; set; }

    public SelectAttribute(string label, string[] values) : base(label)
    {
        Values = values;
    }
}