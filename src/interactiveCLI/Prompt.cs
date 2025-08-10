namespace interactiveCLI;

public class Prompt
{
    public T Text<T>(string label)
    {
        Console.WriteLine(label+" : ");
        var answer = Console.ReadLine();
        return (T)Convert.ChangeType(answer, typeof(T));
    }

    public string Password(string label)
    {
        return null;
    }

    public string Select(string label, params string[] options)
    {
        return null;
    }
}
