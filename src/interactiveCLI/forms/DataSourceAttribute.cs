namespace interactiveCLI.forms;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class DataSourceAttribute : Attribute
{
    public string Name { get; set; } 
    public DataSourceAttribute(string name)
    {
        Name = name;
    }
}