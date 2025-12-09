using System.Text;
using formGenerator;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace interactiveCLI.forms;

public class InputGenerator
{
    public static string Generate(Input input)

    {
        StringBuilder builder = new StringBuilder();
        var attribute = input.Field.GetAttribute("Input");
        string type = input.Field.Type.ToString();
        string possibleValues = "null";
        var inputAttribute = input.Field.GetAttribute("Input");
        string validator = GenerateMethod(input.Validator);
        string converter = GenerateMethod(input.Converter);
        string dataSource = GenerateMethod(input.DataSource,withArgument:false);
        string charValidator = GenerateMethod(input.CharValidator,argument:"(int position, char c)");
        string condition = GenerateMethod(input.Condition,withArgument:false);
        string callbacks = null;
        if (input.Callbacks != null && input.Callbacks.Length > 0)
        {
            callbacks = string.Join(", ", input.Callbacks.Select(x => $@"({type} s) => {x}(s)"));
        }
        else
        {
            callbacks = "null";
        }
        
        
        if (type == "bool" | type == "Boolean" 
            && (string.IsNullOrEmpty(validator) && string.IsNullOrEmpty(converter) && string.IsNullOrEmpty(charValidator)))
        {
            converter = @"(s) => s == true.ToString()";
        }
        string label = input.InputAttribute.GetNthStringArg(0,"label");
        string pattern = input.InputAttribute.GetNthStringArg(1, "pattern");
        pattern = !string.IsNullOrEmpty(pattern) ? $"\"{pattern}\"" : "null";

        string ask = "";
        if (input.IsPasword)
        {
            var hide = input.InputAttribute.GetNthCharArg(0, "hiddenChar");
            ask = $@"

    var {input.Name}Result = prompt.AskPassword(""{label}"",hiddenChar:'{(hide.HasValue ? hide.Value : "*")}', validator:{validator}, condition:{condition}, callbacks:{callbacks});
    if ({input.Name}Result.Ok && {input.Name}Result.IsApplicable) {{
        {input.Name} = {input.Name}Result.Value;
    }}
";
        }
        else if (input.IsTextArea)
        {
            ask = $@"
    var {input.Name}Result = prompt.AskMultiLineText(""{label}"", maxLines:{input.MaxLines});
    if ({input.Name}Result.Ok && {input.Name}Result.IsApplicable) {{
        {input.Name} = {input.Name}Result.Value;
    }}
";
        }
        else
        {
            ask = $@"
    var {input.Name}Result = prompt.Ask<{type}>(""{label}"",
        pattern:{pattern},
        possibleValues:{possibleValues},
        validator:{validator},
        converter:{converter},
        dataSource:{dataSource},
        charValidator:{charValidator},
        condition:{condition},
        callbacks:{callbacks});
    if ({input.Name}Result.Ok && {input.Name}Result.IsApplicable) {{
        {input.Name} = {input.Name}Result.Value;
    }}
";
        }

        return ask;
    }

    public static string GenerateMethod(string methodName, string argument = "string",bool withArgument = true)
    {
        if (string.IsNullOrEmpty(methodName))
        {
            return "null";
        }

        if (withArgument)
        {
            return $"({argument} s) => {methodName}(s)";
        }
        return $"() => {methodName}()";
    }
}