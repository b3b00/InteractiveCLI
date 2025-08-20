using System.Text;

namespace interactiveCLI.forms;

public class InputGenerator
{
    public static string Generate(Input input)

    {
        StringBuilder builder = new StringBuilder();
        var attribute = input.Field.GetAttribute("Input");
        
        
    }
}