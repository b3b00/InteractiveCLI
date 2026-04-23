using System.Diagnostics.CodeAnalysis;

namespace interactiveCLI.forms;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class ConverterAttribute : Attribute
{
    public string Name { get; set; } 
    public ConverterAttribute(string name)
    {
        Name = name;
    }
}