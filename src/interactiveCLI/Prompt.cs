using System.Text;
using interactiveCLI;
using interactiveCLI.forms;


namespace interactiveCLI;

public class Result<T>
{
    public T Value { get; set; }

    public bool Ok { get; set; }

    public bool IsApplicable { get; set; } = true;
}

public class Prompt
{
    public string InvalidInputMessage { get; set; }

    public Prompt(string invalidInputMessage = null)
    {
        InvalidInputMessage = invalidInputMessage;
    }

    public string AskText(string label, Func<string,(bool ok, string errorMessage)> validator = null, string pattern = null,
        Predicate<(int, char)>? charValidator = null)
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
            var errorMessage = InvalidInputMessage ?? "Invalid answer.";
            if (validator != null)
            {
                var avalidation = validator(answer);
                if (avalidation.ok)
                {
                    return answer;
                }
                else
                {
                    errorMessage ??= avalidation.errorMessage;
                }
            }

            Console.Error.WriteLine(errorMessage);
            answer = AskText(label, validator, pattern);
        }
    }

    private void Log(string message)
    {
        File.AppendAllLines("c:/tmp/debug.txt", [message]);
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
            if (pattern[i] == '_')
                editableIndexes[idx++] = i;

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
                        int index = editableIndexes[current - 1];
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


    public int AskInt(string label, Func<string,(bool ok, string errorMessage)>? validator = null)
    {
        bool IntValidator(string s)
        {
            return int.TryParse(s, out var x);
        }

        (bool ok, string errorMessage) CompoundValidator(string s)
        {
            if (validator != null)
            {
                var validation = validator(s);
                if (IntValidator(s) && validation.ok)
                {
                    return (true, null);
                }
                return (false,validation.errorMessage);
            }

            return (IntValidator(s),null);
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

    public Result<T> Ask<T>(string label, string pattern = null,string[] possibleValues = null, Func<string,(bool ok, string errorMessage)>? validator = null,
        Func<string, T>? converter = null, Func<string[]> dataSource = null, Predicate<(int, char)>? charValidator = null,
        Func<bool> condition=null, params Action<T>[] callbacks)
    {
        if (condition != null && !condition())
        {
            return new  Result<T> { IsApplicable = false };
        } 
        
        while (true)
        {
            Console.Write(label);
            string input = null;
            if ((possibleValues != null && possibleValues.Length >= 2) || (dataSource !=null))
            {
                if ((possibleValues == null || possibleValues.Length < 2) && dataSource != null)
                {
                    possibleValues = dataSource();
                }
                input = Select(label, choices: possibleValues);
            }
            else if (!string.IsNullOrEmpty(pattern))
            {
                input = AskText(label,validator,pattern,charValidator:charValidator);
            }
            else if ((typeof(T) == typeof(bool) || typeof(T) == typeof(Boolean)) 
                     && (charValidator == null && validator == null && converter == null))
            {
                input = Check<T>(label, validator);
            }
            else
            {
                input = Console.ReadLine();
            }

            var typeConverter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
            bool isValid = false;
            string errorMessage = null;
            try
            {
                if (validator != null)
                {
                    var validation = validator(input);
                    isValid = validation.ok;
                    errorMessage = validation.errorMessage;
                }
                else
                {
                    if (typeConverter != null)
                    {
                        isValid = typeConverter.CanConvertFrom(typeof(string));
                    }
                }
            }
            catch (ArgumentException e)
            {
                Console.Error.WriteLine(e.Message);
                isValid = false;
            }

            if (isValid)
            {
                if (converter != null)
                {
                    return new Result<T>()
                        { Ok = true, Value = converter(input) };
                }
                else
                {
                    try
                    {
                        if (typeConverter != null)
                        {
                            T convertedValue = (T)typeConverter.ConvertFrom(input);
                            if (callbacks != null && callbacks.Length > 0)
                            {
                                foreach (var callback in callbacks)
                                {
                                    callback(convertedValue);
                                }
                            }
                            
                            return new Result<T>()
                                { Ok = true, Value = convertedValue };
                        }
                    }
                    catch
                    {
                        isValid = false;
                    }
                }
            }

            Console.Error.WriteLine(errorMessage ?? (InvalidInputMessage ?? "Invalid answer."));
            //return new Result<T>() { Ok = false };
        }
    }

    private string Check<T>(string label, Func<string, (bool ok, string errorMessage)>? validator)
    {
        bool isChecked = false;
        var position = Console.GetCursorPosition();
        Console.Write("❌");
        var key = Console.ReadKey(true);
        while (key.Key !=  ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Spacebar)
            {
                isChecked = !isChecked;
                Console.SetCursorPosition(position.Left, position.Top);
                //Console.Write(" ");
                Console.Write(isChecked ? "✔️" : "❌");
                key = Console.ReadKey(true);
            }
        }
        Console.WriteLine();
        return isChecked.ToString();
    }

    public double AskDouble(string label, Func<string,(bool ok, string errorMessage)>? validator = null)
    {
        bool DoubleValidator(string s)
        {
            return double.TryParse(s, out var x);
        }

        (bool ok, string errorMessage) CompoundValidator(string s)
        {
            if (validator != null)
            {
                var validation = validator(s);
                if (DoubleValidator(s) && validation.ok)
                {
                    return (true,null);
                }

                return (false, validation.errorMessage);
            }

            return (DoubleValidator(s),null);
        }

        var answer = AskText(label, CompoundValidator);
        return double.Parse(answer);
    }

    public bool AskBool(string label, string[] trueValues, string[] falseValues, Func<string,(bool ok, string errorMessage)>? validator = null)
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

        (bool ok, string errorMessage) CompoundValidator(string s)
        {
            var (ok, _tried) = tryparse(s);
            if (validator != null && ok)
            {
                var validation = validator(s);
                if (validation.ok && ok)
                {
                    return (true, null);
                }

                return (false, validation.errorMessage);
            }

            return (ok,null);
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

    public Result<string> AskPassword(string label, char hiddenChar = '*', Func<string,(bool ok, string errorMessage)>? validator = null, Func<bool> condition = null, params Action<string>[] callbacks)
    {
        if (condition != null && !condition())
        {
            return new Result<string>()
            {
                IsApplicable = false
            };
        }

        while (true)
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
                    Console.Write(hiddenChar);
                }
            }

            if (callbacks != null && callbacks.Length > 0)
            {
                foreach (var callback in callbacks)
                {
                    callback(password.ToString());
                }
            }


            if (validator != null)
            {
                var errorMessage = InvalidInputMessage ?? "Invalid answer.";
                if (validator != null)
                {
                    var validation = validator(password.ToString());
                    if (validation.ok)
                    {
                        return new Result<string>()
                        {
                            Ok = true,
                            Value = password.ToString(),
                            IsApplicable = true
                        };
                    }
                    else
                    {
                        errorMessage = validation.errorMessage ?? errorMessage; 
                        Console.Error.WriteLine(errorMessage);
                    }
                }
            }
            else
            {
                return new Result<string>()
                {
                    Ok = true,
                    Value = password.ToString(),
                    IsApplicable = true
                };
            }
        }

    }

    public string? Select(string label, Func<string, bool, string> formatter = null, string[] choices = null)
    {
        interactiveCLI.SelectPrompt select = new interactiveCLI.SelectPrompt(label, choices, formatter);
        var choice = select.Select();
        return choice;
    }

    public T AskForm<T>()
    {
        FormBuilder<T> formBuilder = new FormBuilder<T>();
        T formBackingData = (T)Activator.CreateInstance(typeof(T));
        var form = formBuilder.Build(formBackingData, this);
        formBackingData = form.Ask();
        return formBackingData;
    }
}