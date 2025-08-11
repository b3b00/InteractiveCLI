using System.Text;
using interactiveCLI;


namespace interactiveCLI;



public class Prompt
{
    public string AskText(string label, Predicate<string>? validator = null)
    {
        Console.WriteLine(label+" : ");
        var answer = Console.ReadLine();
        while (true)
        {
            if(validator(answer))
            {
                return answer;
                
            }
            Console.Error.WriteLine("Invalid answer.");
            answer = AskText(label, validator);
        }
    }

    public int AskInt(string label, Predicate<string>? validator = null)
    {
        bool IntValidator(string s)
        {
            return int.TryParse(s, out var x);
        }

        bool CompoundValidator(string s)
        {
            if (validator != null)
            {
                return IntValidator(s) && validator(s);
            }

            return IntValidator(s);
        }

        var answer = AskText(label, CompoundValidator);
        while (true)
        {
            if (int.TryParse(answer, out var value))
            {
                return value;
            }
            answer = AskText(label, CompoundValidator);
        }
    }

    public double AskDouble(string label, Predicate<string>? validator = null)
    {
        bool DoubleValidator(string s)
        {
            return double.TryParse(s, out var x);
        }

        bool CompoundValidator(string s)
        {
            if (validator != null)
            {
                return DoubleValidator(s) && validator(s);
            }

            return DoubleValidator(s);
        }

        var answer = AskText(label, CompoundValidator);
        Console.Error.WriteLine("Invalid answer.");
        answer = AskText(label, CompoundValidator);
        return double.Parse(answer);
    }

    public bool AskBool(string label, string[] trueValues, string[] falseValues, Predicate<string>? validator = null)
    {
        Func<string, (bool ok, bool value)> tryparse = s =>
        {
            if (trueValues.Contains(s))
            {
                return (true, true);
            }

            if (falseValues.Contains(s))
            {
                return (true, false);
            }

            return (false, false);
        };

        bool CompoundValidator(string s)
        {
            var (ok, _tried) = tryparse(s);
            if (validator != null && ok)
            {
                return validator(s);
            }

            return ok;
        }

        StringBuilder prompt = new StringBuilder();
        prompt.Append(label);
        prompt.Append($"[{string.Join(", ", trueValues)}] or [{string.Join(", ", falseValues)}]");
        string promptValue = AskText(prompt.ToString(), CompoundValidator);

        var (isOk, value) = tryparse(promptValue.ToString());
        if (isOk)
        {
            return value;
        }


        return false;
    }

    public string? Password(string label)
    {
        return null;
    }

    public string? Select(string label, params string[] choices)
    {
        interactiveCLI.SelectPrompt select = new interactiveCLI.SelectPrompt(label, choices);
        var choice = select.Select();
        return choice;
    }
}
