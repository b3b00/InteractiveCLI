using System.Diagnostics.CodeAnalysis;

namespace interactiveCLI.forms;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public class CallbackAttribute : Attribute
{
    public string Name { get; set; } 
    public CallbackAttribute(string name)
    {
        Name = name;
    }
}