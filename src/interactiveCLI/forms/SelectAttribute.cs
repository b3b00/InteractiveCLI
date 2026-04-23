using System.Diagnostics.CodeAnalysis;

namespace interactiveCLI.forms;

[ExcludeFromCodeCoverage]
public class SelectAttribute : InputAttribute
{
    public string[] Values { get; set; }

    public SelectAttribute(string label, string[] values) : base(label)
    {
        Values = values;
    }
}