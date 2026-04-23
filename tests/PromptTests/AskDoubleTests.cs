using interactiveCLI;
using Xunit;

namespace PromptTests;

public class AskDoubleTests
{
    private static Prompt Build(FakeConsole console) => new Prompt(console: console);

    [Fact]
    public void ReturnsParsedDouble_WhenInputIsValid()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("3.14");
        var prompt = Build(fake);

        var result = prompt.AskDouble("label");

        Assert.Equal(3.14, result, precision: 10);
    }

    [Fact]
    public void RetriesUntilValidDouble_WhenNonNumericEntered()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("not-a-number");
        fake.EnqueueLine("2.5");
        var prompt = Build(fake);

        var result = prompt.AskDouble("label");

        Assert.Equal(2.5, result, precision: 10);
    }

    [Fact]
    public void AcceptsWholeNumberAsDouble()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("10");
        var prompt = Build(fake);

        var result = prompt.AskDouble("label");

        Assert.Equal(10.0, result, precision: 10);
    }

    [Fact]
    public void RespectsAdditionalValidator()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("-1.5");  // negative — rejected
        fake.EnqueueLine("0.5");   // positive — accepted
        var prompt = Build(fake);

        var result = prompt.AskDouble(
            "label",
            validator: s => double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v) && v > 0
                ? (true, null)
                : (false, "must be positive")
        );

        Assert.Equal(0.5, result, precision: 10);
    }
}
