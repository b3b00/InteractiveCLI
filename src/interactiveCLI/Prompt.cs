using System.Text;
using interactiveCLI;
using interactiveCLI.forms;


namespace interactiveCLI;



public class Prompt
{
    public string AskText(string label, Predicate<string>? validator = null , string pattern = null)
    {
        Console.WriteLine(label);
        string answer = null;
        if (!string.IsNullOrWhiteSpace(pattern) && pattern.Contains("_"))
        {
            answer = ReadPattern(pattern);
        }
        else
        {
            answer = Console.ReadLine();
        }

        while (true)
        {
            if (validator == null || validator(answer))
            {
                return answer;

            }
            Console.Error.WriteLine("Invalid answer.");
            answer = AskText(label, validator, pattern);
        }
    }

    private void Log(string message)
    {
        File.AppendAllLines("c:/tmp/debug.txt",[message]);
    }

    private string ReadPattern(string pattern)
    {
        if (!string.IsNullOrEmpty(pattern) && pattern.Contains("_"))
        {
            var start = Console.GetCursorPosition();
            Console.Write(pattern);
            Console.SetCursorPosition(start.Left,start.Top);
            var key = Console.ReadKey();
            string value = "";
            int i = 1;
            while (key.Key != ConsoleKey.Enter)
            {
                
                Console.SetCursorPosition(start.Left + i, start.Top);
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (value.Length > 0)
                    {
                        i--;
                    }
                }

                else if (key.Key == ConsoleKey.Escape)
                {
                    return null;
                }
                else
                {
                    if (i-1 < pattern.Length)
                    {
                        var currentPatternChar = pattern[i-1];
                        if (currentPatternChar == '_')
                        {
                            value += key.KeyChar;
                            while (i < pattern.Length && pattern[i] != '_')
                            {
                                value += pattern[i];
                                i++;
                                Console.SetCursorPosition(start.Left + i, start.Top);
                            }
                        }
                        i++;
                    }
                    key = Console.ReadKey();
                }
            }
            return value;
            
        }

        return null;
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
        return AskText(label);
    }

    public string? Select(string label, Func<string,bool,string> formatter = null, params string[] choices)
    {
        interactiveCLI.SelectPrompt select = new interactiveCLI.SelectPrompt(label, choices, formatter);
        var choice = select.Select();
        return choice;
    }

    public T AskForm<T>()
    {
        FormBuilder<T> formBuilder = new FormBuilder<T>();
        T formBackingData = (T)Activator.CreateInstance(typeof(T));
        var form = formBuilder.Build(formBackingData,this);
        formBackingData = form.Ask();
        return formBackingData;
    }
}
