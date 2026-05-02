using interactiveCLI;
using Xunit;

namespace PromptTests;

public class ScrollingTests
{
    [Fact]
    public void AskMultiLineText_ScrollsWhenHittingWindowBottom()
    {
        var fake = new FakeConsole();
        fake.WindowHeight = 5;
        fake.BufferHeight = 10;
        
        // Move cursor to the last line of the window
        fake.WriteLine("line 1");
        fake.WriteLine("line 2");
        fake.WriteLine("line 3");
        fake.WriteLine("line 4");
        // CursorTop is now 4 (5th line)
        
        Assert.Equal(4, fake.CursorTop);

        // Enter some text and press Enter
        fake.EnqueueChars("hello");
        fake.EnqueueEnter(); // This should trigger scroll
        fake.EnqueueChars("world");
        fake.EnqueueCtrlEnter();

        var prompt = fake.GetPrompt();
        var result = prompt.AskMultiLineText("input:");

        // If it scrolled, the display should have moved up.
        
        Assert.True(result.Ok);
        Assert.Equal("hello\nworld", result.Value);
    }

    [Fact]
    public void AskMultiLineText_NavigationMovesCursor()
    {
        var fake = new FakeConsole();
        fake.EnqueueChars("line1");
        fake.EnqueueEnter();
        fake.EnqueueChars("line2");
        fake.EnqueueUp();
        fake.EnqueueLeft(2);
        fake.EnqueueChars("X");
        fake.EnqueueCtrlEnter();

        var prompt = fake.GetPrompt();
        var result = prompt.AskMultiLineText("in:");

        Assert.True(result.Ok);
        Assert.Equal("linXe1\nline2", result.Value);
    }

    [Fact]
    public void AskMultiLineText_ScrollsBufferWhenHittingBufferBottom()
    {
        var fake = new FakeConsole();
        fake.WindowHeight = 5;
        
        // Fill up to BufferHeight - 1
        for (int i = 0; i < 999; i++) fake.WriteLine();
        
        Assert.Equal(999, fake.CursorTop);

        fake.EnqueueChars("A");
        fake.EnqueueEnter(); // Must scroll buffer here
        fake.EnqueueChars("B");
        fake.EnqueueCtrlEnter();

        var prompt = fake.GetPrompt();
        var result = prompt.AskMultiLineText("in:");

        Assert.True(result.Ok);
        Assert.Equal("A\nB", result.Value);
    }


}
