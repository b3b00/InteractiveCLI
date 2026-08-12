using System.Diagnostics.CodeAnalysis;

namespace interactiveCLI.forms;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Property)]
public class AutocompleteAttribute : Attribute 
{
    public AutocompleteAttribute()
    {}
}
