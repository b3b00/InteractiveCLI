using interactiveCLI;
using Xunit;

namespace PromptTests;

public class AskPasswordTests
{
    private static Prompt Build(FakeConsole console) => new Prompt(console: console);

    [Fact]
    public void ReturnsTypedPassword_WhenEnterPressed()
    {
        var fake = new FakeConsole();
        fake.EnqueueChar('p');
        fake.EnqueueChar('a');
        fake.EnqueueChar('s');
        fake.EnqueueChar('s');
        fake.EnqueueEnter();
        var prompt = Build(fake);

        var result = prompt.AskPassword("Password: ");

        Assert.True(result.Ok);
        Assert.True(result.IsApplicable);
        Assert.Equal("pass", result.Value);
    }

    [Fact]
    public void HandlesBackspace_RemovesLastCharacter()
    {
        var fake = new FakeConsole();
        fake.EnqueueChar('p');
        fake.EnqueueChar('a');
        fake.EnqueueChar('x');       // typo
        fake.EnqueueBackspace();     // erase typo
        fake.EnqueueChar('s');
        fake.EnqueueEnter();
        var prompt = Build(fake);

        var result = prompt.AskPassword("Password: ");

        Assert.Equal("pas", result.Value);
    }

    [Fact]
    public void HandlesBackspace_WhenPasswordIsEmpty_DoesNotThrow()
    {
        var fake = new FakeConsole();
        fake.EnqueueBackspace();     // backspace on empty — should be ignored
        fake.EnqueueChar('a');
        fake.EnqueueEnter();
        var prompt = Build(fake);

        var result = prompt.AskPassword("Password: ");

        Assert.Equal("a", result.Value);
    }

    [Fact]
    public void ReturnsNotApplicable_WhenConditionIsFalse()
    {
        var fake = new FakeConsole();
        var prompt = Build(fake);

        var result = prompt.AskPassword("Password: ", condition: () => false);

        Assert.False(result.IsApplicable);
    }

    [Fact]
    public void ReturnsApplicable_WhenConditionIsTrue()
    {
        var fake = new FakeConsole();
        fake.EnqueueChar('s');
        fake.EnqueueChar('e');
        fake.EnqueueChar('c');
        fake.EnqueueEnter();
        var prompt = Build(fake);

        var result = prompt.AskPassword("Password: ", condition: () => true);

        Assert.True(result.IsApplicable);
        Assert.Equal("sec", result.Value);
    }

    [Fact]
    public void InvokesCallbacks_AfterPasswordEntered()
    {
        var fake = new FakeConsole();
        fake.EnqueueChar('a');
        fake.EnqueueChar('b');
        fake.EnqueueEnter();
        var prompt = Build(fake);

        string? captured = null;
        prompt.AskPassword("Password: ", callbacks: [s => captured = s]);

        Assert.Equal("ab", captured);
    }

    [Fact]
    public void MasksOutput_WithHiddenChar()
    {
        var fake = new FakeConsole();
        fake.EnqueueChar('s');
        fake.EnqueueChar('e');
        fake.EnqueueChar('c');
        fake.EnqueueEnter();
        var prompt = Build(fake);

        prompt.AskPassword("Password: ", hiddenChar: '#');

        Assert.Contains("###", fake.Output);
        Assert.DoesNotContain("sec", fake.Output);
    }
}
