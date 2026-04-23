using System.Text;

namespace PromptTests;

/// <summary>
/// A fake IConsole implementation for unit testing.
/// Simulates keyboard input and captures all output.
/// </summary>
public class FakeConsole : interactiveCLI.IConsole
{
    private readonly Queue<string?> _lines = new();
    private readonly Queue<ConsoleKeyInfo> _keys = new();
    private readonly StringBuilder _output = new();
    private readonly StringBuilder _errorOutput = new();

    // ── Input helpers ──────────────────────────────────────────────────

    /// <summary>Enqueue a line that will be returned by the next ReadLine() call.</summary>
    public void EnqueueLine(string? line) => _lines.Enqueue(line);

    /// <summary>Enqueue multiple lines at once.</summary>
    public void EnqueueLines(params string?[] lines)
    {
        foreach (var line in lines) _lines.Enqueue(line);
    }

    /// <summary>Enqueue a ConsoleKeyInfo that will be returned by the next ReadKey() call.</summary>
    public void EnqueueKey(ConsoleKeyInfo key) => _keys.Enqueue(key);

    /// <summary>Enqueue a character keystroke.</summary>
    public void EnqueueChar(char c)
        => _keys.Enqueue(new ConsoleKeyInfo(c, CharToConsoleKey(c), false, false, false));

    /// <summary>Enqueue a special key (arrow, Enter, Escape, etc.).</summary>
    public void EnqueueSpecialKey(ConsoleKey key)
        => _keys.Enqueue(new ConsoleKeyInfo('\0', key, false, false, false));

    /// <summary>Enqueue Enter key.</summary>
    public void EnqueueEnter() => EnqueueSpecialKey(ConsoleKey.Enter);

    /// <summary>Enqueue Escape key.</summary>
    public void EnqueueEscape() => EnqueueSpecialKey(ConsoleKey.Escape);

    /// <summary>Enqueue Spacebar key.</summary>
    public void EnqueueSpace() => _keys.Enqueue(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));

    /// <summary>Enqueue Backspace key.</summary>
    public void EnqueueBackspace() => _keys.Enqueue(new ConsoleKeyInfo('\b', ConsoleKey.Backspace, false, false, false));

    /// <summary>Enqueue a Ctrl+key combination.</summary>
    public void EnqueueCtrlKey(ConsoleKey key)
        => _keys.Enqueue(new ConsoleKeyInfo('\0', key, false, false, true));

    // ── Output inspection ─────────────────────────────────────────────

    public string Output => _output.ToString();
    public string ErrorOutput => _errorOutput.ToString();

    // ── IConsole implementation ───────────────────────────────────────

    public string? ReadLine()
    {
        if (_lines.Count == 0)
            throw new InvalidOperationException("FakeConsole: no more lines in the queue.");
        return _lines.Dequeue();
    }

    public ConsoleKeyInfo ReadKey(bool intercept = false)
    {
        if (_keys.Count == 0)
            throw new InvalidOperationException("FakeConsole: no more keys in the queue.");
        return _keys.Dequeue();
    }

    public void Write(string? value) => _output.Append(value);
    public void Write(char value) => _output.Append(value);
    public void WriteLine(string? value) => _output.AppendLine(value);
    public void WriteLine() => _output.AppendLine();
    public void WriteError(string value) => _errorOutput.AppendLine(value);

    public (int Left, int Top) GetCursorPosition() => (0, 0);
    public void SetCursorPosition(int left, int top) { /* no-op in tests */ }
    public int CursorTop => 0;
    public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.Gray;

    // ── Helpers ───────────────────────────────────────────────────────

    private static ConsoleKey CharToConsoleKey(char c)
    {
        if (char.IsAsciiLetterUpper(c))
            return (ConsoleKey)c;
        if (char.IsAsciiLetterLower(c))
            return (ConsoleKey)(c - 'a' + 'A');
        if (char.IsAsciiDigit(c))
            return (ConsoleKey)('D' - 'A' + (int)ConsoleKey.D0 + (c - '0'));
        return ConsoleKey.Oem1; // fallback for symbols
    }
}
