using System.Diagnostics.CodeAnalysis;

namespace interactiveCLI.forms;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class DataSourceAttribute : Attribute
{
    public string Name { get; set; } 
    public DataSourceAttribute(string name)
    {
        Name = name;
    }
}