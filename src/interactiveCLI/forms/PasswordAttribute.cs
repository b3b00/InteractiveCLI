namespace interactiveCLI.forms;

public class PasswordAttribute : InputAttribute
{
    private char HiddenChar { get; set; }
    
    public PasswordAttribute(string label, char hiddenChar = '*', int index = -1) : base(label, index:index)
    {
        HiddenChar = hiddenChar;
    }
}