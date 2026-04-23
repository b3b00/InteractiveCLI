using System.IO;

namespace interactiveCLI;

public class SelectPrompt
{



    private string[] _choices;
    private string _label;
    Func<string, bool, int, string> _formatter;
    bool _isIndexed = false;

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


    public SelectPrompt(string label, string[] choices, Func<string, bool, int, string> formatter = null, bool isIndexed = false)
    {
        _isIndexed = isIndexed;
        _label = label;
        _choices = choices;
        _formatter = formatter ?? DefaultFormatter;
    }

    private void Print((int left, int top) startPosition, int position)
    {
        Console.SetCursorPosition(startPosition.left, startPosition.top);
        for (int i = 0; i < _choices.Length; i++)
        {
            var item = _formatter(_choices[i], i == position, i);
            Console.SetCursorPosition(startPosition.left, startPosition.top + i);
            Console.Write(item);
        }
        Console.WriteLine();
        
    }

    public string? Select()
    {

        Console.WriteLine();
        var (left, top) = Console.GetCursorPosition();


        int position = 0;


        for (int i = 0; i < _choices.Length; i++)
        {
            var item = _formatter(_choices[i], i == position, i);
            var bak = Console.ForegroundColor;
            Console.WriteLine(item);
        }
        var (l, t) = Console.GetCursorPosition();
        // compute start position from last item position (to correctly handle bottom of the console)
        top = t - _choices.Length;


        ConsoleKeyInfo key = Console.ReadKey();

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
                key = Console.ReadKey();
            }
        }
        if (key.Key == ConsoleKey.Escape)
        {

            Console.SetCursorPosition(left, top + _choices.Length);
            return null;
        }

        
            
        return _choices[position];
    }
}