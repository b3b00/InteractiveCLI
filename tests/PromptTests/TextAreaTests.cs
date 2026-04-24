using interactiveCLI;
using interactiveCLI.forms;
using Xunit;

namespace PromptTests;

[Form]
public partial class TextAreaTester
{
    [TextArea("content:",maxLines:5)]
    [Validator(nameof(NoSWord))]
    [Callback(nameof(CallBack1))]
    [Callback(nameof(CallBack2))]
    public string Content  { get; set; }

    public string C1 { get; set; }
    public string C2 { get; set; }
    
    public void CallBack1(string result)
    {
        C1 = result + " - callback1";
    }

    public void CallBack2(string result)
    {
        C2 = result + " - callback2";
    }

    
    public (bool ok, string errorMessage) NoSWord(string value)
    {
        bool ok = !value.Contains("suck", StringComparison.OrdinalIgnoreCase);
        if (ok)
        {
            return (true, null);
        }
        else
        {
            return (false, "be polite please.");
        }
    }
    
    
    
}

public class TextAreaTests
{
    [Fact]
    public void TextArea_EscCancelsInput()
    {
        var fake = new FakeConsole();
        fake.EnqueueChars("content");
        fake.EnqueueEscape();
        Prompt prompt = new Prompt(console:fake);
        
        TextAreaTester tester = new TextAreaTester();
        tester.Ask(prompt);
        Assert.Null(tester.Content);
        var lines = fake.OutputLines;
        Assert.Equal(2, lines.Length);
        Assert.Equal("content:", lines[0]);
        Assert.Equal("content", lines[1]);
    }
    
    [Fact]
    public void TestArea_IsLineLimited()
    {
        var fake = new FakeConsole();
        fake.EnqueueChars("one");
        fake.EnqueueEnter();
        fake.EnqueueChars("two");
        fake.EnqueueEnter();
        fake.EnqueueChars("three");
        fake.EnqueueEnter();
        fake.EnqueueChars("four");
        fake.EnqueueEnter();
        fake.EnqueueChars("five");
        fake.EnqueueEnter();
        fake.EnqueueChars("six");
        fake.EnqueueCtrlEnter();
        var prompt = fake.GetPrompt();
        
        TextAreaTester tester = new TextAreaTester();
        tester.Ask(prompt);
        Assert.NotNull(tester.Content);
        var lines =  tester.Content.GetLines();
        Assert.Equal(5, lines.Length);
        Assert.DoesNotContain("six", lines);
        Assert.Contains("fivesix", lines);
    }

    [Fact]
    public void TestArea_ValidateFails_ThenRetries()
    {
        var fake = new FakeConsole();
        // First attempt: contains "suck" → rejected by validator
        fake.EnqueueChars("this");
        fake.EnqueueEnter();
        fake.EnqueueChars("thing");
        fake.EnqueueEnter();
        fake.EnqueueChars("sucks");
        fake.EnqueueCtrlEnter();
        // Second attempt: valid
        fake.EnqueueChars("this is fine");
        fake.EnqueueCtrlEnter();
        var prompt = fake.GetPrompt();

        TextAreaTester tester = new TextAreaTester();
        tester.Ask(prompt);
        Assert.Equal("this is fine", tester.Content);
        Assert.NotEmpty(fake.ErrorOutput);
    }
    
    [Fact]
    public void TestArea_CallBacksAreCalled()
    {
        var fake = new FakeConsole();
        fake.EnqueueChars("call me please");
        fake.EnqueueCtrlEnter();
        Prompt prompt = new Prompt(console: fake);

        TextAreaTester tester = new TextAreaTester();
        tester.Ask(prompt);
        Assert.NotNull(tester.Content);
        Assert.Equal("call me please", tester.Content);
        Assert.Equal("call me please - callback1", tester.C1);
        Assert.Equal("call me please - callback2", tester.C2);
    }
}