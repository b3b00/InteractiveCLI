namespace interactiveCLI.forms;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class ConditionAttribute : Attribute
{
    public string Name { get; set; } 
    public ConditionAttribute(string name)
    {
        Name = name;
    }
}