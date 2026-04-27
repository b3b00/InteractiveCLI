using System.ComponentModel.DataAnnotations.Schema;
using interactiveCLI;
using Xunit;

namespace PromptTests;

public class AskTextTests
{

    [Fact]
    public void ReturnsEnteredText_WhenValidatorAccepts()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("hello");
        var prompt = fake.GetPrompt();

        var result = prompt.AskText("label", validator: s => (true, null));

        Assert.Equal("hello", result);
    }

    [Fact]
    public void RetriesUntilValidInput()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("bad");    // first attempt — rejected
        fake.EnqueueLine("good");   // second attempt — accepted
        var prompt = fake.GetPrompt();

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
        var prompt = fake.GetPrompt();

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
        var prompt = fake.GetPrompt();

        var result = prompt.AskText("label", validator: _ => (true, null));

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ReturnsManyLines_WhenInputIsManyLine()
    {
        var fake = new FakeConsole();
        fake.EnqueueChars("first");
        fake.EnqueueEnter();
        fake.EnqueueChars("second");
        fake.EnqueueEnter();
        fake.EnqueueChars("third");
        fake.EnqueueCtrlKey(ConsoleKey.Enter);
        var prompt = fake.GetPrompt();
        var result = prompt.AskMultiLineText("content");
        Assert.True(result.Ok);
        var content = result.Value;
        Assert.Equal("first\nsecond\nthird", content);
    }
    
    [Fact]
    public void ReturnsManyLines_WhenInputManyLineAndEditBackspaceThroughLines()
    {
        var fake = new FakeConsole();
        fake.EnqueueChars("first");
        fake.EnqueueEnter();
        fake.EnqueueChars("second");
        fake.EnqueueBackspace(9);
        fake.EnqueueChars("update");
        fake.EnqueueEnter();
        fake.EnqueueChars("second");
        fake.EnqueueEnter();
        fake.EnqueueChars("third");
        fake.EnqueueCtrlKey(ConsoleKey.Enter);
        var prompt = fake.GetPrompt();
        var result = prompt.AskMultiLineText("content");
        Assert.True(result.Ok);
        var content = result.Value;
        Assert.Equal("firupdate\nsecond\nthird", content);
    }
    
    [Fact]
    public void ReturnsManyLines_WhenInputManyLineAndEditLeftThroughLines()
    {
        var fake = new FakeConsole();
        fake.EnqueueChars("first");
        fake.EnqueueEnter();
        fake.EnqueueChars("second");
        fake.EnqueueLeft(9);
        fake.EnqueueChars("sty");
        fake.EnqueueRight(7);
        fake.EnqueueEnter();
        fake.EnqueueChars("third");
        fake.EnqueueCtrlKey(ConsoleKey.Enter);
        var prompt = fake.GetPrompt();
        var result = prompt.AskMultiLineText("content");
        Assert.True(result.Ok);
        var content = result.Value;
        // Since arrow keys ARE supported:
        // first -> (Enter) -> second -> (Left 9) -> moves to 'fir|st' -> (sty) -> 'firstyst' -> (Right 7) -> moves to 'seco|nd' -> (Enter) -> (third) -> 'thirdnd'
        Assert.Equal("firstyst\nseco\nthirdnd", content);
    }
}
