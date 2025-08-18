namespace interactiveCLI.forms;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class InputValidatorAttribute : Attribute
{
    public string InputName { get; set; }

    public InputValidatorAttribute(string inputName)
    {
        InputName = inputName;
    }
}