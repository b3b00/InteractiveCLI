using System.Text;
using interactiveCLI;
using interactiveCLI.forms;


namespace interactiveCLI;


public class Result<T>
{
    public T Value {get;set;}
    
    public bool Ok {get;set;}
}


public class Prompt
{
    public string AskText(string label, Predicate<string>? validator = null , string pattern = null, Predicate<(int,char)> ? charValidator = null)
    {
        Console.WriteLine(label);
        string answer = null;
        if (!string.IsNullOrWhiteSpace(pattern) && pattern.Contains("_"))
        {
            answer = ReadPatternCopilot(pattern, charValidator);
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
    
    
    
/// <summary>
/// This method has been generated with Github Copilot.
/// It displays a pattern that he user must fill. (ex __/__/____ for a date following the format dd/MM/yyyy)
/// </summary>
/// <param name="pattern">A pattern where `_` are free space and other chars are constant.</param>
/// <returns>the entered string</returns>

public string ReadPatternCopilot(string pattern, Predicate<(int position, char c)>? isAllowed = null)
{
    char[] buffer = pattern.ToCharArray();
    int[] editableIndexes = new int[pattern.Count(c => c == '_')];
    int idx = 0;
    for (int i = 0; i < pattern.Length; i++)
        if (pattern[i] == '_') editableIndexes[idx++] = i;

    int current = 0;
    Console.Write(pattern);
    Console.SetCursorPosition(editableIndexes[0], Console.CursorTop);

    while (true)
    {
        var key = Console.ReadKey(true);

        if (key.Key == ConsoleKey.Escape)
        {
            Console.WriteLine("ESC");
            return null;
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (current > 0)
            {
                if (current != editableIndexes.Length)
                {
                    current--;
                    //current--;
                    buffer[editableIndexes[current]] = '_';
                    Console.SetCursorPosition(editableIndexes[current], Console.CursorTop);
                    Console.Write('_');
                    Console.SetCursorPosition(editableIndexes[current], Console.CursorTop);
                }
                else
                {
                    int index = editableIndexes[current-1]; 
                    buffer[index] = '_';
                    Console.SetCursorPosition(index, Console.CursorTop);
                    Console.Write('_');
                    Console.SetCursorPosition(index, Console.CursorTop);
                    current--;
                }

                
            }
        }
        else if (key.Key == ConsoleKey.LeftArrow && current > 0)
        {
            current--;
            Console.SetCursorPosition(editableIndexes[current], Console.CursorTop);
        }
        else if (key.Key == ConsoleKey.RightArrow && current < editableIndexes.Length)
        {
            current++;
            if (current < editableIndexes.Length)
                Console.SetCursorPosition(editableIndexes[current], Console.CursorTop);
        }
        else if (key.Key == ConsoleKey.Enter)
        {
            break;
        }
        else if (!char.IsControl(key.KeyChar) && current < editableIndexes.Length)
        {
            int pos = editableIndexes[current];
            if (isAllowed == null || isAllowed((pos, key.KeyChar)))
            {
                buffer[pos] = key.KeyChar;
                Console.SetCursorPosition(pos, Console.CursorTop);
                Console.Write(key.KeyChar);
                current++;
                if (current < editableIndexes.Length)
                    Console.SetCursorPosition(editableIndexes[current], Console.CursorTop);
            }
        }
    }
    Console.WriteLine();
    return new string(buffer);
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
    
    public Result<T> Ask<T>(string label)
    {
        Console.Write(label);
        string input = Console.ReadLine();
        var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
        if (converter != null && converter.IsValid(input))
        {
            return new Result<T>() { Ok = false };
        }
        throw new InvalidCastException($"Impossible de convertir '{input}' en {typeof(T).Name}");
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

    public string? AskPassword(string label)
    {
        Console.Write(label);
        var password = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }
            else if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Length--;
                    Console.Write("\b \b");
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password.Append(key.KeyChar);
                Console.Write("*");
            }
        }
        return password.ToString();
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
