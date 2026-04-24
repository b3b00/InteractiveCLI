namespace PromptTests;

public static class TestExtensions
{
    public static string[] GetLines(this string value)
    {
        List<string> lines = new List<string>();
        using (var reader = new StringReader(value))
        {
            string line = reader.ReadLine();
            while (line != null)
            {
                lines.Add(line);
                line = reader.ReadLine();
            }
        }
        return lines.ToArray();
    }
}