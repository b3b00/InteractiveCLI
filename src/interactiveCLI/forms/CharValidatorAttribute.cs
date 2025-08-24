namespace interactiveCLI.forms;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class CharValidatorAttribute : Attribute
{
    public string Name { get; set; } 
    public CharValidatorAttribute(string name)
    {
        Name = name;
    }
}