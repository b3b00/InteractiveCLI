namespace interactiveCLI.forms;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class InputConverterAttribute : Attribute
{
    public string InputName { get; set; }

    public InputConverterAttribute(string inputName)
    {
        InputName = inputName;
    }
}