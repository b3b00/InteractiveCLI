namespace interactiveCLI;

public interface IConsole
{
    string? ReadLine();
    ConsoleKeyInfo ReadKey(bool intercept = false);
    void Write(string? value);
    void Write(char value);
    void WriteLine(string? value);
    void WriteLine();
    void WriteError(string value);
    (int Left, int Top) GetCursorPosition();
    void SetCursorPosition(int left, int top);
    int CursorTop { get; }
    int CursorSize { get; set; }
    ConsoleColor ForegroundColor { get; set; }
    int WindowHeight { get; }
    int WindowWidth { get; }
    int BufferHeight { get; }
    int WindowTop { get; set; }
}
