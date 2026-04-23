using interactiveCLI;
using Xunit;

namespace PromptTests;

public class AskBoolTests
{
    private static readonly string[] TrueValues  = ["yes", "y"];
    private static readonly string[] FalseValues = ["no",  "n"];

    private static Prompt Build(FakeConsole console) => new Prompt(console: console);

    [Theory]
    [InlineData("yes")]
    [InlineData("y")]
    public void ReturnsTrue_WhenTrueValueEntered(string input)
    {
        var fake = new FakeConsole();
        fake.EnqueueLine(input);
        var prompt = Build(fake);

        var result = prompt.AskBool("label", TrueValues, FalseValues);

        Assert.True(result);
    }

    [Theory]
    [InlineData("no")]
    [InlineData("n")]
    public void ReturnsFalse_WhenFalseValueEntered(string input)
    {
        var fake = new FakeConsole();
        fake.EnqueueLine(input);
        var prompt = Build(fake);

        var result = prompt.AskBool("label", TrueValues, FalseValues);

        Assert.False(result);
    }

    [Fact]
    public void RetriesUntilValidBoolValue_WhenInvalidInputEntered()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("maybe");   // neither true nor false
        fake.EnqueueLine("yes");     // valid
        var prompt = Build(fake);

        var result = prompt.AskBool("label", TrueValues, FalseValues);

        Assert.True(result);
    }

    [Fact]
    public void ErrorIsWritten_WhenInvalidInputEntered()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("blah");
        fake.EnqueueLine("no");
        var prompt = Build(fake);

        prompt.AskBool("label", TrueValues, FalseValues);

        Assert.False(string.IsNullOrEmpty(fake.ErrorOutput));
    }

    [Fact]
    public void ReturnsFalse_AfterRetryWithFalseValue()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("invalid");
        fake.EnqueueLine("n");
        var prompt = Build(fake);

        var result = prompt.AskBool("label", TrueValues, FalseValues);

        Assert.False(result);
    }
}
