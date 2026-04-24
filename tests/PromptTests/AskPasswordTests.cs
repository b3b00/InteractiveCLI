using interactiveCLI;
using Xunit;

namespace PromptTests;

public class AskPasswordTests
{

    [Fact]
    public void ReturnsTypedPassword_WhenEnterPressed()
    {
        var fake = new FakeConsole();
        fake.EnqueueChars(@"pass");
        fake.EnqueueEnter();
        var prompt = fake.GetPrompt();

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
        var prompt = fake.GetPrompt();

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
        var prompt = fake.GetPrompt();

        var result = prompt.AskPassword("Password: ");

        Assert.Equal("a", result.Value);
    }

    [Fact]
    public void ReturnsNotApplicable_WhenConditionIsFalse()
    {
        var fake = new FakeConsole();
        var prompt = fake.GetPrompt();

        var result = prompt.AskPassword("Password: ", condition: () => false);

        Assert.False(result.IsApplicable);
    }

    [Fact]
    public void ReturnsApplicable_WhenConditionIsTrue()
    {
        var fake = new FakeConsole();
        fake.EnqueueChars("secret");
        fake.EnqueueEnter();
        var prompt = fake.GetPrompt();;

        var result = prompt.AskPassword("Password: ", condition: () => true);

        Assert.True(result.IsApplicable);
        Assert.Equal("secret", result.Value);
    }

    [Fact]
    public void InvokesCallbacks_AfterPasswordEntered()
    {
        var fake = new FakeConsole();
        fake.EnqueueChars(@"ab");
        fake.EnqueueEnter();
        var prompt = fake.GetPrompt();

        string? captured = null;
        prompt.AskPassword("Password: ", callbacks: [s => captured = s]);

        Assert.Equal("ab", captured);
    }

    [Fact]
    public void MasksOutput_WithHiddenChar()
    {
        var fake = new FakeConsole();
        fake.EnqueueChars("secret");
        fake.EnqueueEnter();
        var prompt = fake.GetPrompt();

        prompt.AskPassword("Password: ", hiddenChar: '#');

        Assert.Contains("######", fake.Output);
        Assert.DoesNotContain("secret", fake.Output);
    }
    
    [Fact]
    public void MasksOutput_ValidationFails()
    {
        var fake = new FakeConsole();
        fake.EnqueueChars("12345");
        fake.EnqueueEnter();
        var prompt = fake.GetPrompt();

        var errorMessage = "password is damn simple !";
        var result = prompt.AskPassword("Password: ", validator: (string v) =>
        {
            bool ok = v != "12345";
            return (ok, ok ? null : errorMessage);
        });
        
        Assert.DoesNotContain("secret", fake.Output);
        Assert.False(result.Ok);
        Assert.Contains(errorMessage, fake.ErrorOutput);
        
    }
}
