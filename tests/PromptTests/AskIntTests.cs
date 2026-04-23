using interactiveCLI;
using Xunit;

namespace PromptTests;

public class AskIntTests
{
    private static Prompt Build(FakeConsole console) => new Prompt(console: console);

    [Fact]
    public void ReturnsParsedInteger_WhenInputIsValid()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("42");
        var prompt = Build(fake);

        var result = prompt.AskInt("label");

        Assert.Equal(42, result);
    }

    [Fact]
    public void RetriesUntilValidInteger_WhenNonIntegerEntered()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("abc");   // invalid
        fake.EnqueueLine("7");     // valid
        var prompt = Build(fake);

        var result = prompt.AskInt("label");

        Assert.Equal(7, result);
    }

    [Fact]
    public void RetriesUntilValidInteger_WhenDecimalEntered()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("3.14");
        fake.EnqueueLine("3");
        var prompt = Build(fake);

        var result = prompt.AskInt("label");

        Assert.Equal(3, result);
    }

    [Fact]
    public void RespectsAdditionalValidator()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("3");    // valid int, but fails range check
        fake.EnqueueLine("10");   // valid int, passes range check
        var prompt = Build(fake);

        var result = prompt.AskInt(
            "label",
            validator: s => int.TryParse(s, out var v) && v >= 5
                ? (true, null)
                : (false, "must be >= 5")
        );

        Assert.Equal(10, result);
    }

    [Fact]
    public void ReturnsNegativeInteger()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("-99");
        var prompt = Build(fake);

        var result = prompt.AskInt("label");

        Assert.Equal(-99, result);
    }
}
