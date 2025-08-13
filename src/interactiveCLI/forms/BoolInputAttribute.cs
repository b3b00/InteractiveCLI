namespace interactiveCLI.forms;

public class BoolInputAttribute : InputAttribute
{
    public BoolInputAttribute(string label, string[] trueValues,  string[] falseValues) :  base(label)
    {
        TrueValues = trueValues;
        FalseValues = falseValues;
    }
    
    public string[] TrueValues {get; set;}
    
    public string[] FalseValues {get; set;}
    
}