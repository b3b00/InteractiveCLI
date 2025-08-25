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
        
        string possibleValues = "null";
        var inputAttribute = input.Field.GetAttribute("Input");
        string validator = GenerateMethod(input.Validator);
        string converter = GenerateMethod(input.Converter);
        string dataSource = GenerateMethod(input.DataSource,withArgument:false);
        string charValidator = GenerateMethod(input.CharValidator,argument:"(int position, char c)");
        string type = input.Field.Type.ToString();
        if (type == "bool" | type == "Boolean")
        {
            converter = @"(s) => s == true.ToString()";
        }
        string label = input.InputAttribute.GetNthStringArg(0);
        string pattern = input.InputAttribute.GetNthStringArg(1);
        pattern = !string.IsNullOrEmpty(pattern) ? $"\"{pattern}\"" : "null";
        
        var ask = $@"
    var {input.Name}Result = prompt.Ask<{type}>(""{label}"",pattern:{pattern},possibleValues:{possibleValues}, validator:{validator},converter:{converter},dataSource:{dataSource}, charValidator:{charValidator});
    if ({input.Name}Result.Ok) {{
        {input.Name} = {input.Name}Result.Value;
    }}
";
        
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