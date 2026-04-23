namespace interactiveCLI;

public class SystemConsole : IConsole
{
    public string? ReadLine() => Console.ReadLine();
    public ConsoleKeyInfo ReadKey(bool intercept = false) => Console.ReadKey(intercept);
    public void Write(string? value) => Console.Write(value);
    public void Write(char value) => Console.Write(value);
    public void WriteLine(string? value) => Console.WriteLine(value);
    public void WriteLine() => Console.WriteLine();
    public void WriteError(string value) => Console.Error.WriteLine(value);
    public (int Left, int Top) GetCursorPosition() => Console.GetCursorPosition();
    public void SetCursorPosition(int left, int top) => Console.SetCursorPosition(left, top);
    public int CursorTop => Console.CursorTop;
    public ConsoleColor ForegroundColor
    {
        get => Console.ForegroundColor;
        set => Console.ForegroundColor = value;
    }
}
