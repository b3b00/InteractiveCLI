using interactiveCLI.forms;

namespace PromptTests;

[Form]
public partial class IntValidation
{
    public (bool ok, string message) LowerThan5(string value)
    {
        if (int.TryParse(value, out var v) && v < 5)
        {
            return (true, null);
        }
        return (false, $"value must be < 5");
    }
    [Input("int lower than 5")]
    [Validator(nameof(LowerThan5))]
    public int Integer { get; set; }
}