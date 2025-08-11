using System.IO;

namespace interactiveCLI;

public class SelectPrompt
{



    private string[] _choices;
    private string _label;
    Func<string, bool, string> _formatter;

    public string DefaultFormatter(string value, bool selected)
    {
        string lead = selected ? "\x1b[1;32m >" : "  ";
        string tail = selected ? "<\x1b[0m" : " ";
        var item = $"{lead}{value}{tail}";
        return item;
    }


    public SelectPrompt(string label, string[] choices, Func<string, bool, string> formatter = null)
    {
        _label = label;
        _choices = choices;
        _formatter = formatter ?? DefaultFormatter;
    }

    private void Print((int left, int top) startPosition, int position)
    {
        Console.SetCursorPosition(startPosition.left, startPosition.top);
        for (int i = 0; i < _choices.Length; i++)
        {
            var item = _formatter(_choices[i], i == position);
            Console.SetCursorPosition(startPosition.left, startPosition.top + i);
            Console.Write(item);
        }
    }

    public string? Select()
    {

        Console.WriteLine(_label);
        var (left, top) = Console.GetCursorPosition();


        int position = 0;


        for (int i = 0; i < _choices.Length; i++)
        {
            var item = _formatter(_choices[i], i == position);
            var bak = Console.ForegroundColor;
            Console.WriteLine(item);
        }
        var (l, t) = Console.GetCursorPosition();
        // compute start position from last item position (to correctly handle bottom of the console)
        top = t - _choices.Length;


        ConsoleKeyInfo key = Console.ReadKey();

        while (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Escape)
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
            key = Console.ReadKey();
        }
        if (key.Key == ConsoleKey.Escape)
        {

            Console.SetCursorPosition(left, top + _choices.Length);
            return null;
        }

        return _choices[position];
    }
}