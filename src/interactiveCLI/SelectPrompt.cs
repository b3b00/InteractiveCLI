using System.IO;

namespace interactiveCLI;

public class SelectPrompt
{
    private string[] _choices;
    private string _label;
    Func<string, bool, int, string> _formatter;
    bool _isIndexed = false;
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


    public SelectPrompt(string label, string[] choices, Func<string, bool, int, string> formatter = null, bool isIndexed = false, IConsole console = null)
    {
        _isIndexed = isIndexed;
        _label = label;
        _choices = choices;
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
            _console.Write(item);
        }
        _console.WriteLine();
        
    }

    public string? Select()
    {

        _console.WriteLine();
        var (left, top) = _console.GetCursorPosition();


        int position = 0;


        for (int i = 0; i < _choices.Length; i++)
        {
            var item = _formatter(_choices[i], i == position, i);
            var bak = _console.ForegroundColor;
            _console.WriteLine(item);
        }
        var (l, t) = _console.GetCursorPosition();
        // compute start position from last item position (to correctly handle bottom of the console)
        top = t - _choices.Length;


        ConsoleKeyInfo key = _console.ReadKey();

        bool indexSelected = false;
        
        while (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Escape && !indexSelected)
        {
            if (key.Key == ConsoleKey.DownArrow)
            {
                if (position < _choices.Length - 1)
                {
                    position++;
                    Print((left, top), position);
                }
            }
            if (key.Key == ConsoleKey.UpArrow)
            {
                if (position > 0)
                {
                    position--;
                    Print((left, top), position);
                }
            }
            if (_isIndexed &&  _choices.Length < 10)
            {
                for (int i = 1; i <= _choices.Length; i++)
                {
                    if (key.KeyChar == i.ToString()[0])
                    {
                        position = i-1;
                        indexSelected = true;
                        break;
                    }
                }
            }

            if (!indexSelected)
            {
                key = _console.ReadKey();
            }
        }
        if (key.Key == ConsoleKey.Escape)
        {

            _console.SetCursorPosition(left, top + _choices.Length);
            return null;
        }

        
            
        return _choices[position];
    }
}
