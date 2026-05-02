using System.Text;
using System.Collections.Generic;
using interactiveCLI;

namespace PromptTests;

/// <summary>
/// A fake IConsole implementation for unit testing.
/// Simulates keyboard input and captures all output with a 2D buffer for realistic terminal simulation.
/// </summary>
public class FakeConsole : interactiveCLI.IConsole
{
    private readonly Queue<string?> _lines = new();
    private readonly Queue<ConsoleKeyInfo> _keys = new();
    private readonly char[,] _buffer = new char[1000, 120]; // Rows, Cols
    private readonly StringBuilder _errorOutput = new();
    private readonly StringBuilder _rawOutput = new();

    public FakeConsole()
    {
        for (int r = 0; r < 1000; r++)
            for (int c = 0; c < 120; c++)
                _buffer[r, c] = ' ';
    }

    public Prompt GetPrompt() => new Prompt(console: this);

    // ── Input helpers ──────────────────────────────────────────────────
    public void EnqueueLine(string? line) => _lines.Enqueue(line);
    public void EnqueueLines(params string?[] lines) { foreach (var line in lines) _lines.Enqueue(line); }
    public void EnqueueKey(ConsoleKeyInfo key) => _keys.Enqueue(key);
    public void EnqueueChar(char c) => _keys.Enqueue(new ConsoleKeyInfo(c, CharToConsoleKey(c), false, false, false));
    public void EnqueueChars(string value) { foreach (var c in value) EnqueueChar(c); }
    public void EnqueueSpecialKey(ConsoleKey key) => _keys.Enqueue(new ConsoleKeyInfo('\0', key, false, false, false));
    public void EnqueueLeft() => EnqueueSpecialKey(ConsoleKey.LeftArrow);
    public void EnqueueLeft(int count) { for (int i = 0; i < count; i++) EnqueueLeft(); }
    public void EnqueueRight() => EnqueueSpecialKey(ConsoleKey.RightArrow);
    public void EnqueueRight(int count) { for (int i = 0; i < count; i++) EnqueueRight(); }
    public void EnqueueUp() => EnqueueSpecialKey(ConsoleKey.UpArrow);
    public void EnqueueDown() => EnqueueSpecialKey(ConsoleKey.DownArrow);
    public void EnqueueEnter() => EnqueueSpecialKey(ConsoleKey.Enter);
    public void EnqueueEscape() => EnqueueSpecialKey(ConsoleKey.Escape);
    public void EnqueueSpace() => _keys.Enqueue(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));
    public void EnqueueBackspace() => _keys.Enqueue(new ConsoleKeyInfo('\b', ConsoleKey.Backspace, false, false, false));
    public void EnqueueBackspace(int count) { for (int i = 0; i < count; i++) EnqueueBackspace(); }
    public void EnqueueCtrlKey(ConsoleKey key) => _keys.Enqueue(new ConsoleKeyInfo('\0', key, false, false, true));
    public void EnqueueCtrlKey(char c) => _keys.Enqueue(new ConsoleKeyInfo(c, CharToConsoleKey(c), false, false, true));
    public void EnqueueCtrlEnter() => EnqueueCtrlKey(ConsoleKey.Enter);

    // ── Output inspection ─────────────────────────────────────────────
    public string Output => _rawOutput.ToString();
    public string ScreenOutput => GetOutput();
    public string ErrorOutput => _errorOutput.ToString();
    public string[] OutputLines => GetOutputLines();

    // ── IConsole Implementation ──────────────────────────────────────
    public string? ReadLine() => _lines.Count == 0 ? throw new InvalidOperationException("FakeConsole: no lines") : _lines.Dequeue();
    public ConsoleKeyInfo ReadKey(bool intercept = false) => _keys.Count == 0 ? throw new InvalidOperationException("FakeConsole: no keys") : _keys.Dequeue();
    public void WriteError(string value) => _errorOutput.AppendLine(value);

    public (int Left, int Top) GetCursorPosition() => (_cursorLeft, _cursorTop);
    public void SetCursorPosition(int left, int top) 
    { 
        _cursorLeft = Math.Clamp(left, 0, 119);
        _cursorTop = Math.Clamp(top, 0, 999);
    }
    public int CursorTop => _cursorTop;
    private int _cursorTop = 0;
    private int _cursorLeft = 0;

    public int CursorSize { get; set; } = 25;
    public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.Gray;

    public int WindowHeight { get; set; } = 25;
    public int WindowWidth { get; set; } = 120;
    public int BufferHeight { get; set; } = 1000;
    public int WindowTop { get; set; } = 0;

    public void WriteLine() 
    { 
        _rawOutput.AppendLine();
        _cursorTop++;
        _cursorLeft = 0;
    }

    public void WriteLine(string? value)
    {
        _rawOutput.Append(value);
        WriteToBuffer(value);
        WriteLine();
    }

    public void Write(string? value)
    {
        if (value == null) return;
        _rawOutput.Append(value);
        WriteToBuffer(value);
    }

    public void Write(char value)
    {
        _rawOutput.Append(value);
        WriteToBuffer(value);
    }

    private void WriteToBuffer(string? value)
    {
        if (value == null) return;
        foreach (var c in value) WriteToBuffer(c);
    }

    private void WriteToBuffer(char value)
    {
        if (value == '\r')
        {
            _cursorLeft = 0;
            return;
        }
        if (value == '\n')
        {
            WriteLine();
            return;
        }

        _buffer[_cursorTop, _cursorLeft] = value;
        _cursorLeft++;
        if (_cursorLeft >= 120)
        {
            _cursorLeft = 0;
            _cursorTop++;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────
    private string GetOutput() => string.Join("\n", GetOutputLines());

    private string[] GetOutputLines()
    {
        var lines = new List<string>();
        int maxRow = 0;
        for (int r = 0; r < 1000; r++)
            for (int c = 0; c < 120; c++)
                if (_buffer[r, c] != ' ') maxRow = r;

        for (int r = 0; r <= maxRow; r++)
        {
            var sb = new StringBuilder();
            for (int c = 0; c < 120; c++) sb.Append(_buffer[r, c]);
            lines.Add(sb.ToString().TrimEnd());
        }
        return lines.ToArray();
    }

    private static ConsoleKey CharToConsoleKey(char c)
    {
        if (char.IsAsciiLetterUpper(c)) return (ConsoleKey)c;
        if (char.IsAsciiLetterLower(c)) return (ConsoleKey)(c - 'a' + 'A');
        if (char.IsAsciiDigit(c)) return (ConsoleKey)('D' - 'A' + (int)ConsoleKey.D0 + (c - '0'));
        return ConsoleKey.Oem1; 
    }
}
