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

    private readonly IConsole _console;

    public Prompt(string invalidInputMessage = null, IConsole console = null)
    {
        InvalidInputMessage = invalidInputMessage;
        _console = console ?? new SystemConsole();
    }

    public string AskText(string label, Func<string,(bool ok, string errorMessage)> validator = null, string pattern = null,
        Predicate<(int, char)>? charValidator = null)
    {
        _console.WriteLine();
        string answer = null;
        if (!string.IsNullOrWhiteSpace(pattern) && pattern.Contains("_"))
        {
            answer = ReadPatternCopilot(pattern, charValidator);
        }
        else
        {
            answer = _console.ReadLine();
        }

        while (true)
        {
            var errorMessage = InvalidInputMessage ?? "Invalid answer.";
            if (validator != null)
            {
                var validation = validator(answer);
                if (validation.ok)
                {
                    return answer;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(validation.errorMessage))
                    {
                        errorMessage = validation.errorMessage;
                    }
                    errorMessage ??= validation.errorMessage;
                }
            }
            else
            {
                return answer;
            }

            _console.WriteError(errorMessage);
            answer = AskText(label, validator, pattern);
        }
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
        _console.Write(pattern);
        _console.SetCursorPosition(editableIndexes[0], _console.CursorTop);

        while (true)
        {
            var key = _console.ReadKey(true);

            if (key.Key == ConsoleKey.Escape)
            {
                _console.WriteLine("ESC");
                return null;
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (current > 0)
                {
                    if (current != editableIndexes.Length)
                    {
                        current--;
                        buffer[editableIndexes[current]] = '_';
                        _console.SetCursorPosition(editableIndexes[current], _console.CursorTop);
                        _console.Write('_');
                        _console.SetCursorPosition(editableIndexes[current], _console.CursorTop);
                    }
                    else
                    {
                        int index = editableIndexes[current - 1];
                        buffer[index] = '_';
                        _console.SetCursorPosition(index, _console.CursorTop);
                        _console.Write('_');
                        _console.SetCursorPosition(index, _console.CursorTop);
                        current--;
                    }
                }
            }
            else if (key.Key == ConsoleKey.LeftArrow && current > 0)
            {
                current--;
                _console.SetCursorPosition(editableIndexes[current], _console.CursorTop);
            }
            else if (key.Key == ConsoleKey.RightArrow && current < editableIndexes.Length)
            {
                current++;
                if (current < editableIndexes.Length)
                    _console.SetCursorPosition(editableIndexes[current], _console.CursorTop);
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
                    _console.SetCursorPosition(pos, _console.CursorTop);
                    _console.Write(key.KeyChar);
                    current++;
                    if (current < editableIndexes.Length)
                        _console.SetCursorPosition(editableIndexes[current], _console.CursorTop);
                }
            }
        }

        _console.WriteLine();
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
        int.TryParse(answer, out var value);
        return value;
    }

    public Result<T> Ask<T>(string label, string pattern = null,string[] possibleValues = null, Func<string,(bool ok, string errorMessage)>? validator = null,
        Func<string, T>? converter = null, Func<string[]> dataSource = null, Predicate<(int, char)>? charValidator = null,
        Func<bool> condition=null, bool isIndexed=false, params Action<T>[] callbacks)
    {
        if (condition != null && !condition())
        {
            return new  Result<T> { IsApplicable = false };
        } 
        
        while (true)
        {
            _console.Write(label);
            string input = null;
            if ((possibleValues != null && possibleValues.Length >= 2) || (dataSource !=null))
            {
                if ((possibleValues == null || possibleValues.Length < 2) && dataSource != null)
                {
                    possibleValues = dataSource();
                }
                input = Select(label, choices: possibleValues, isIndexed:isIndexed);
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
                input = _console.ReadLine();
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
                _console.WriteError(e.Message);
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

            _console.WriteError(errorMessage ?? (InvalidInputMessage ?? "Invalid answer."));
            //return new Result<T>() { Ok = false };
        }
    }

    private string Check<T>(string label, Func<string, (bool ok, string errorMessage)>? validator)
    {
        bool isChecked = false;
        var position = _console.GetCursorPosition();
        _console.Write("❌");
        var key = _console.ReadKey(true);
        while (key.Key !=  ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Spacebar)
            {
                isChecked = !isChecked;
                _console.SetCursorPosition(position.Left, position.Top);
                _console.Write(isChecked ? "✔️" : "❌");
                key = _console.ReadKey(true);
            }
        }
        _console.WriteLine();
        return isChecked.ToString();
    }

    public double AskDouble(string label, Func<string,(bool ok, string errorMessage)>? validator = null)
    {
        bool DoubleValidator(string s)
        {
            return double.TryParse(s, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out _);
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
        return double.Parse(answer, System.Globalization.CultureInfo.InvariantCulture);
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
            _console.Write(label);
            var password = new StringBuilder();
            while (true)
            {
                var key = _console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    _console.WriteLine();
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password.Length--;
                        _console.Write("\b \b");
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    password.Append(key.KeyChar);
                    _console.Write(hiddenChar);
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
                        _console.WriteError(errorMessage);
                        return new Result<string>()
                        {
                            Ok = false,
                        };
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

    public string? Select(string label, Func<string, bool, int, string> formatter = null, string[] choices = null, bool isIndexed = false)
    {
        interactiveCLI.SelectPrompt select = new interactiveCLI.SelectPrompt(label, choices, formatter, isIndexed, _console);
        var choice = select.Select();
        return choice;
    }


    /// <summary>
    /// Prompts the user for multi-line text input, similar to an HTML textarea.
    /// Press Enter to create new lines, and Ctrl+Enter or Escape to finish input.
    /// </summary>
    /// <param name="label">The prompt label to display</param>
    /// <param name="maxLines">Maximum number of lines allowed (0 for unlimited)</param>
    /// <param name="finishKey">Key combination to finish input (default: Ctrl+Enter)</param>
    /// <param name="validator">Optional validator function</param>
    /// <param name="condition"></param>
    /// <param name="callbacks"></param>
    /// <returns>The multi-line text input</returns>
    ///
    public Result<string> AskMultiLineText(string label,
        int maxLines = 0,
        ConsoleKey finishKey = ConsoleKey.Enter,
        Func<string, (bool ok, string errorMessage)>? validator = null,
        Func<bool>? condition=null,
        params Action<string>[] callbacks)
    {
        if (condition != null && !condition())
        {
            return new  Result<string> { IsApplicable = false };
        } 
        
        while (true)
        {
            _console.WriteLine(label);
            //Console.WriteLine("(Press Ctrl+Enter to finish, Escape to cancel)");

            var lines = new List<StringBuilder>();
            lines.Add(new StringBuilder());
            int cursorX = 0;
            int cursorY = 0;
            int startTop = _console.CursorTop;
            bool isInsertMode = true;
            int originalCursorSize = _console.CursorSize;
            _console.CursorSize = 25;

            void SetCursor(int x, int y)
            {
                try
                {
                    int absoluteY = startTop + y;
                    
                    // 1. Ensure it's within BufferHeight (scroll buffer if needed)
                    while (absoluteY >= _console.BufferHeight)
                    {
                        _console.SetCursorPosition(0, _console.BufferHeight - 1);
                        _console.WriteLine();
                        startTop--;
                        absoluteY--;
                    }

                    if (absoluteY < 0) absoluteY = 0;
                    if (absoluteY >= _console.BufferHeight) absoluteY = _console.BufferHeight - 1;

                    // 2. Ensure it's within Window view (scroll window if needed)
                    try
                    {
                        int winTop = _console.WindowTop;
                        int winHeight = _console.WindowHeight;
                        
                        if (y == 0)
                        {
                            // When at the very first line, show the label if it fits
                            int targetTop = absoluteY > 0 ? absoluteY - 1 : absoluteY;
                            if (winTop != targetTop)
                            {
                                _console.WindowTop = targetTop;
                            }
                        }
                        else if (absoluteY >= winTop + winHeight - 1 && absoluteY < _console.BufferHeight - 1)
                        {
                            // Maintain 1-line margin at the bottom when navigating down
                            _console.WindowTop = absoluteY - winHeight + 2;
                        }
                        else if (absoluteY <= winTop && absoluteY > 0)
                        {
                            // Maintain 1-line margin at the top when navigating up
                            _console.WindowTop = absoluteY - 1;
                        }
                    }
                    catch { /* WindowTop might not be supported on all platforms */ }

                    _console.SetCursorPosition(x, absoluteY);
                }
                catch { }
            }

            void SetCursorPositionInternal(int x, int y)
            {
                try
                {
                    int absoluteY = startTop + y;
                    
                    // Ensure it's within BufferHeight (scroll buffer if needed)
                    while (absoluteY >= _console.BufferHeight)
                    {
                        _console.SetCursorPosition(0, _console.BufferHeight - 1);
                        _console.WriteLine();
                        startTop--;
                        absoluteY--;
                    }

                    if (absoluteY < 0) absoluteY = 0;
                    if (absoluteY >= _console.BufferHeight) absoluteY = _console.BufferHeight - 1;
                    _console.SetCursorPosition(x, absoluteY);
                }
                catch { }
            }

            void UpdateCursorPosition()
            {
                SetCursor(cursorX, cursorY);
            }

            void RedrawLine(int y)
            {
                try 
                {
                    SetCursorPositionInternal(0, y);
                    _console.Write(lines[y].ToString() + "   ");
                    UpdateCursorPosition();
                } 
                catch { }
            }

            void RedrawFrom(int y)
            {
                try
                {
                    // Pre-scroll buffer if needed to ensure startTop is stable for the whole redraw
                    SetCursor(0, lines.Count);

                    for (int i = y; i <= lines.Count; i++)
                    {
                        SetCursorPositionInternal(0, i);
                        if (i < lines.Count)
                        {
                            _console.Write(lines[i].ToString() + "   ");
                        }
                        else
                        {
                            // Clear one line below to handle deletions
                            _console.Write(new string(' ', Math.Max(0, _console.WindowWidth - 1)));
                        }
                    }
                    UpdateCursorPosition();
                }
                catch { }
            }

            while (true)
            {
                var key = _console.ReadKey(true);
                
                // Finish input with Ctrl+Enter
                if (key.Key == finishKey && key.Modifiers == ConsoleModifiers.Control)
                {
                    _console.CursorSize = originalCursorSize;
                    SetCursorPositionInternal(0, lines.Count);
                    _console.WriteLine();
                    break;
                }
                // Cancel with Escape
                else if (key.Key == ConsoleKey.Escape)
                {
                    _console.CursorSize = originalCursorSize;
                    SetCursorPositionInternal(0, lines.Count);
                    _console.WriteLine();
                    return new Result<string>()
                    {
                        IsApplicable = false,
                        Ok = false
                    };
                }
                // Toggle Insert/Overwrite mode
                else if (key.Key == ConsoleKey.Insert)
                {
                    isInsertMode = !isInsertMode;
                    _console.CursorSize = isInsertMode ? 25 : 100;
                }
                // Arrow keys
                else if (key.Key == ConsoleKey.LeftArrow)
                {
                    if (cursorX > 0)
                    {
                        cursorX--;
                        UpdateCursorPosition();
                    }
                    else if (cursorY > 0)
                    {
                        cursorY--;
                        cursorX = lines[cursorY].Length;
                        UpdateCursorPosition();
                    }
                }
                else if (key.Key == ConsoleKey.RightArrow)
                {
                    if (cursorX < lines[cursorY].Length)
                    {
                        cursorX++;
                        UpdateCursorPosition();
                    }
                    else if (cursorY < lines.Count - 1)
                    {
                        cursorY++;
                        cursorX = 0;
                        UpdateCursorPosition();
                    }
                }
                else if (key.Key == ConsoleKey.UpArrow)
                {
                    if (cursorY > 0)
                    {
                        cursorY--;
                        if (cursorX > lines[cursorY].Length) cursorX = lines[cursorY].Length;
                    }
                    UpdateCursorPosition();
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    if (cursorY < lines.Count - 1)
                    {
                        cursorY++;
                        if (cursorX > lines[cursorY].Length) cursorX = lines[cursorY].Length;
                    }
                    UpdateCursorPosition();
                }
                // Home and End keys
                else if (key.Key == ConsoleKey.Home)
                {
                    if (key.Modifiers == ConsoleModifiers.Control)
                    {
                        cursorY = 0;
                        cursorX = 0;
                    }
                    else
                    {
                        cursorX = 0;
                    }
                    UpdateCursorPosition();
                }
                else if (key.Key == ConsoleKey.End)
                {
                    if (key.Modifiers == ConsoleModifiers.Control)
                    {
                        cursorY = lines.Count - 1;
                        cursorX = lines[cursorY].Length;
                    }
                    else
                    {
                        cursorX = lines[cursorY].Length;
                    }
                    UpdateCursorPosition();
                }
                // New line with Enter
                else if (key.Key == ConsoleKey.Enter)
                {
                    if (maxLines > 0 && lines.Count >= maxLines)
                    {
                        continue;
                    }

                    string remainder = lines[cursorY].ToString().Substring(cursorX);
                    lines[cursorY].Length = cursorX;
                    lines.Insert(cursorY + 1, new StringBuilder(remainder));
                    
                    cursorY++;
                    cursorX = 0;
                    RedrawFrom(cursorY - 1);
                }
                // Backspace
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (cursorX > 0)
                    {
                        cursorX--;
                        lines[cursorY].Remove(cursorX, 1);
                        RedrawLine(cursorY);
                    }
                    else if (cursorY > 0)
                    {
                        string currentLineText = lines[cursorY].ToString();
                        lines.RemoveAt(cursorY);
                        cursorY--;
                        cursorX = lines[cursorY].Length;
                        lines[cursorY].Append(currentLineText);
                        RedrawFrom(cursorY);
                    }
                }
                // Delete
                else if (key.Key == ConsoleKey.Delete)
                {
                    if (cursorX < lines[cursorY].Length)
                    {
                        lines[cursorY].Remove(cursorX, 1);
                        RedrawLine(cursorY);
                    }
                    else if (cursorY < lines.Count - 1)
                    {
                        string nextLineText = lines[cursorY + 1].ToString();
                        lines.RemoveAt(cursorY + 1);
                        lines[cursorY].Append(nextLineText);
                        RedrawFrom(cursorY);
                    }
                }
                // Regular character input
                else if (!char.IsControl(key.KeyChar))
                {
                    if (isInsertMode)
                    {
                        lines[cursorY].Insert(cursorX, key.KeyChar);
                        cursorX++;
                        RedrawLine(cursorY);
                    }
                    else
                    {
                        if (cursorX < lines[cursorY].Length)
                        {
                            lines[cursorY][cursorX] = key.KeyChar;
                            cursorX++;
                            RedrawLine(cursorY);
                        }
                        else
                        {
                            lines[cursorY].Append(key.KeyChar);
                            cursorX++;
                            RedrawLine(cursorY);
                        }
                    }
                }
            }

            var result = string.Join("\n", lines.Select(l => l.ToString()));

            // Validation
            if (validator != null)
            {
                var validation = validator(result);
                if (validation.ok)
                {
                    if (callbacks != null && callbacks.Length > 0)
                    {
                        foreach (var callback in callbacks)
                        {
                            callback(result);
                        }
                    }
                    
                    return new Result<string>()
                    {
                        Value = result,
                        Ok = true,
                        IsApplicable = true
                    };
                }
                else
                {
                    var errorMessage = validation.errorMessage ?? InvalidInputMessage ?? "Invalid input.";
                    _console.WriteError(errorMessage);
                    continue;
                }
            }
            else
            {
                if (callbacks != null && callbacks.Length > 0)
                {
                    foreach (var callback in callbacks)
                    {
                        callback(result);
                    }
                }
                
                return new Result<string>()
                {
                    Value = result,
                    Ok = true,
                    IsApplicable = true
                };
            }
        }
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