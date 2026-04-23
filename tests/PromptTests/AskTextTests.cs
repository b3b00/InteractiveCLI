using interactiveCLI;
using Xunit;

namespace PromptTests;

public class AskTextTests
{
    private static Prompt Build(FakeConsole console) => new Prompt(console: console);

    [Fact]
    public void ReturnsEnteredText_WhenValidatorAccepts()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("hello");
        var prompt = Build(fake);

        var result = prompt.AskText("label", validator: s => (true, null));

        Assert.Equal("hello", result);
    }

    [Fact]
    public void RetriesUntilValidInput()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("bad");    // first attempt — rejected
        fake.EnqueueLine("good");   // second attempt — accepted
        var prompt = Build(fake);

        var result = prompt.AskText(
            "label",
            validator: s => (s == "good", "invalid")
        );

        Assert.Equal("good", result);
    }

    [Fact]
    public void WritesErrorMessage_OnInvalidInput()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("bad");
        fake.EnqueueLine("ok");
        var prompt = Build(fake);

        prompt.AskText("label", validator: s => (s == "ok", "try again"));

        Assert.Contains("try again", fake.ErrorOutput);
    }

    [Fact]
    public void UsesCustomInvalidInputMessage_WhenSet()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("wrong");
        fake.EnqueueLine("right");
        var prompt = new Prompt(invalidInputMessage: "custom error", console: fake);

        prompt.AskText("label", validator: s => (s == "right", null));

        Assert.Contains("custom error", fake.ErrorOutput);
    }

    [Fact]
    public void ReturnsNullLine_WhenInputIsEmpty()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine(string.Empty);
        var prompt = Build(fake);

        var result = prompt.AskText("label", validator: _ => (true, null));

        Assert.Equal(string.Empty, result);
    }
}
