namespace interactiveCLI.forms;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class ValidatorAttribute : Attribute
{
    public string Name { get; set; } 
    public ValidatorAttribute(string name)
    {
        Name = name;
    }
}