namespace interactiveCLI.forms;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ConverterAttribute : Attribute
{
    public string Name { get; set; } 
    public ConverterAttribute(string name)
    {
        Name = name;
    }
}