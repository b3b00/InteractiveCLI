using interactiveCLI;
using Xunit;

namespace PromptTests;

public class AskBoolTests
{
    private static readonly string[] TrueValues  = ["yes", "y"];
    private static readonly string[] FalseValues = ["no",  "n"];


    [Theory]
    [InlineData("yes")]
    [InlineData("y")]
    public void ReturnsTrue_WhenTrueValueEntered(string input)
    {
        var fake = new FakeConsole();
        fake.EnqueueLine(input);
        var prompt = fake.GetPrompt();

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
        var prompt = fake.GetPrompt();

        var result = prompt.AskBool("label", TrueValues, FalseValues);

        Assert.False(result);
    }

    [Fact]
    public void RetriesUntilValidBoolValue_WhenInvalidInputEntered()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("maybe");   // neither true nor false
        fake.EnqueueLine("yes");     // valid
        var prompt = fake.GetPrompt();

        var result = prompt.AskBool("label", TrueValues, FalseValues);

        Assert.True(result);
    }

    [Fact]
    public void ErrorIsWritten_WhenInvalidInputEntered()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("blah");
        fake.EnqueueLine("no");
        var prompt = fake.GetPrompt();

        prompt.AskBool("label", TrueValues, FalseValues);

        Assert.False(string.IsNullOrEmpty(fake.ErrorOutput));
    }

    [Fact]
    public void ReturnsFalse_AfterRetryWithFalseValue()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("invalid");
        fake.EnqueueLine("n");
        var prompt = fake.GetPrompt();

        var result = prompt.AskBool("label", TrueValues, FalseValues);

        Assert.False(result);
    }

    [Fact]
    public void CheckToggleWithSpace()
    {
        var fake = new FakeConsole();
        fake.EnqueueSpace();
        fake.EnqueueEnter();
        var prompt = fake.GetPrompt();

        var result = prompt.Ask<bool>("toggle");
        Assert.Contains("toggle❌✔️", fake.Output);
        Assert.True(result.Value);
        ;
    }
    
    [Fact]
    public void CheckDoubleToggleWithSpace()
    {
        var fake = new FakeConsole();
        fake.EnqueueSpace();
        fake.EnqueueSpace();
        fake.EnqueueEnter();
        var prompt = fake.GetPrompt();

        var result = prompt.Ask<bool>("toggle");
        Assert.Contains("toggle❌✔️❌", fake.Output);
        Assert.False(result.Value);
        ;
    }
    
    
    
}
