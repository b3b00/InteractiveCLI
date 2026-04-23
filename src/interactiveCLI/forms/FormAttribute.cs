using System.Diagnostics.CodeAnalysis;

namespace interactiveCLI.forms;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Class,AllowMultiple = false)]
public class FormAttribute : Attribute
{
    public string InvalidInputMessage {get; set;}

    public FormAttribute(string invalidInputMessage = null)
    {
        InvalidInputMessage = invalidInputMessage;
    }
    
}