using interactiveCLI;
using Xunit;

namespace PromptTests;

public class AskFileTests
{
    [Fact]
    public void AskFile_TabAutocomplete_And_Enter()
    {
        var fake = new FakeConsole();
        var prompt = fake.GetPrompt();

        string[] MockProvider(string path)
        {
            if (path == "s" || path == "src")
                return new[] { "src/file1.cs", "src/file2.cs" };
            return System.Array.Empty<string>();
        }

        fake.EnqueueChars("s");
        fake.EnqueueSpecialKey(System.ConsoleKey.Tab);
        fake.EnqueueEnter();

        var result = prompt.AskFile("Path: ", fileProvider: MockProvider);

        Assert.True(result.Ok);
        Assert.Equal("src/file", result.Value);
    }
    
    [Fact]
    public void AskFile_Navigate_And_Select()
    {
        var fake = new FakeConsole();
        var prompt = fake.GetPrompt();

        string[] MockProvider(string path)
        {
            if (path == "test")
                return new[] { "test1.txt", "test2.txt", "test3.txt" };
            return System.Array.Empty<string>();
        }

        fake.EnqueueChars("test");
        fake.EnqueueDown(); // select test1.txt
        fake.EnqueueDown(); // select test2.txt
        fake.EnqueueEnter(); // fill test2.txt and submit

        var result = prompt.AskFile("Path: ", fileProvider: MockProvider);

        Assert.True(result.Ok);
        Assert.Equal("test2.txt", result.Value);
    }
    
    [Fact]
    public void AskFile_Navigate_Then_Type_Cancels_Selection()
    {
        var fake = new FakeConsole();
        var prompt = fake.GetPrompt();

        string[] MockProvider(string path)
        {
            if (path == "t")
                return new[] { "t1.txt", "t2.txt" };
            if (path == "t3")
                return new[] { "t3.txt" };
            return System.Array.Empty<string>();
        }

        fake.EnqueueChars("t");
        fake.EnqueueDown(); // select t1.txt
        fake.EnqueueChars("3"); // selection canceled, now it's "t3"
        fake.EnqueueEnter(); // submit

        var result = prompt.AskFile("Path: ", fileProvider: MockProvider);

        Assert.True(result.Ok);
        Assert.Equal("t3", result.Value);
    }
}
