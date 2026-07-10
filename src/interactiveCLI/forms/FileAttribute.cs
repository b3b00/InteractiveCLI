using System.Diagnostics.CodeAnalysis;

namespace interactiveCLI.forms;

[ExcludeFromCodeCoverage]
public class FileAttribute : InputAttribute
{
    public FileAttribute(string label) : base(label)
    {
    }
}
