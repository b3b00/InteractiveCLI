using System.IO;

namespace interactiveCLI;

public class SelectPrompt
{
    private string[] _choices;
    private string[] _allChoices;
    private string _searchString = "";
    private int _lastPrintedCount = 0;
    private string _label;
    Func<string, bool, int, string> _formatter;
    bool _isIndexed = false;
    bool _autocomplete = false;
    private readonly IConsole _console;

    public string DefaultFormatter(string value, bool selected, int index)
    {
        string lead = selected ? "\x1b[1;32m >" : "  ";
        string tail = selected ? "<\x1b[0m" : " ";
        var item = $"{lead}{value}{tail}";
        if (_isIndexed && _choices.Length < 10)
        {
            item  = $"{lead}{index+1}. {value}{tail}";
        }
        return item;
    }

    public SelectPrompt(string label, string[] choices, Func<string, bool, int, string> formatter = null, bool isIndexed = false, IConsole console = null, bool autocomplete = false)
    {
        _isIndexed = isIndexed;
        _autocomplete = autocomplete;
        _label = label;
        _choices = choices;
        _allChoices = choices;
        _formatter = formatter ?? DefaultFormatter;
        _console = console ?? new SystemConsole();
    }

    private void Print((int left, int top) startPosition, int position)
    {
        _console.SetCursorPosition(startPosition.left, startPosition.top);
        for (int i = 0; i < _choices.Length; i++)
        {
            var item = _formatter(_choices[i], i == position, i);
            _console.SetCursorPosition(startPosition.left, startPosition.top + i);
            _console.Write($"{item}\x1b[K");
        }
        for (int i = _choices.Length; i < _lastPrintedCount; i++)
        {
            _console.SetCursorPosition(startPosition.left, startPosition.top + i);
            _console.Write("\x1b[K");
        }
        _lastPrintedCount = _choices.Length;
    }

    public string? Select()
    {
        _console.WriteLine();
        var (left, top) = _console.GetCursorPosition();
        
        if (_autocomplete)
        {
            _console.WriteLine($"? {_label} {_searchString}");
        }

        int position = 0;
        _lastPrintedCount = _choices.Length;

        for (int i = 0; i < _choices.Length; i++)
        {
            var item = _formatter(_choices[i], i == position, i);
            _console.WriteLine($"{item}\x1b[K");
        }
        var (l, t) = _console.GetCursorPosition();
        top = t - _choices.Length - (_autocomplete ? 1 : 0);

        ConsoleKeyInfo key = _console.ReadKey(true);

        bool indexSelected = false;
        
        while (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Escape && !indexSelected)
        {
            bool redrawSearch = false;
            bool redrawList = false;

            if (key.Key == ConsoleKey.DownArrow)
            {
                if (position < _choices.Length - 1)
                {
                    position++;
                    redrawList = true;
                }
            }
            else if (key.Key == ConsoleKey.UpArrow)
            {
                if (position > 0)
                {
                    position--;
                    redrawList = true;
                }
            }
            else if (key.Key == ConsoleKey.Backspace)
            {
                if (_autocomplete && _searchString.Length > 0)
                {
                    _searchString = _searchString.Substring(0, _searchString.Length - 1);
                    redrawSearch = true;
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                if (_isIndexed && _choices.Length < 10)
                {
                    bool isIndexKey = false;
                    for (int i = 1; i <= _choices.Length; i++)
                    {
                        if (key.KeyChar == i.ToString()[0])
                        {
                            position = i - 1;
                            indexSelected = true;
                            isIndexKey = true;
                            break;
                        }
                    }
                    if (isIndexKey) break;
                }
                
                if (_autocomplete)
                {
                    _searchString += key.KeyChar;
                    redrawSearch = true;
                }
            }

            if (redrawSearch)
            {
                _console.SetCursorPosition(left, top);
                _console.Write($"? {_label} {_searchString}\x1b[K");
                
                _choices = _allChoices.Where(c => c.Contains(_searchString, StringComparison.OrdinalIgnoreCase)).ToArray();
                if (position >= _choices.Length) 
                {
                    position = Math.Max(0, _choices.Length - 1);
                }
                redrawList = true;
            }

            if (redrawList)
            {
                Print((left, top + (_autocomplete ? 1 : 0)), position);
            }

            if (!indexSelected)
            {
                key = _console.ReadKey(true);
            }
        }
        
        if (key.Key == ConsoleKey.Escape)
        {
            _console.SetCursorPosition(left, top + (_autocomplete ? 1 : 0) + _lastPrintedCount);
            return null;
        }

        _console.SetCursorPosition(left, top + (_autocomplete ? 1 : 0) + _lastPrintedCount);
        return _choices.Length > 0 ? _choices[position] : null;
    }
}
