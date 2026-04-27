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

    [Fact]
    public void AskMultiLineText_WindowTopFollowsCursor()
    {
        var fake = new FakeConsole();
        fake.WindowHeight = 5;
        
        fake.EnqueueChars("L1");
        fake.EnqueueEnter();
        fake.EnqueueEnter();
        fake.EnqueueEnter();
        fake.EnqueueEnter(); 
        fake.EnqueueChars("L5");
        fake.EnqueueEnter(); 
        fake.EnqueueChars("L6");
        fake.EnqueueCtrlEnter();

        var prompt = fake.GetPrompt();
        prompt.AskMultiLineText("in:");

        // StartTop was 1. End was 7. 
        // L1(1), LF(2), LF(3), LF(4), L5(5), LF(6), L6(7).
        // absoluteY for L6 is 7. WindowHeight is 5. WindowTop should be 7-5+1 = 3.
        Assert.Equal(3, fake.WindowTop); 
    }

    [Fact]
    public void AskMultiLineText_ScrollsUpWhenMovingAboveWindow()
    {
        var fake = new FakeConsole();
        fake.WindowHeight = 5;
        
        // Start at a lower position
        for (int i = 0; i < 10; i++) fake.WriteLine();
        
        // startTop will be 11 (after label "in:")
        fake.SetCursorPosition(0, 10);
        fake.WindowTop = 10;
        
        // Enter 10 lines. WindowTop will follow to bottom.
        for(int i=0; i<10; i++) fake.EnqueueEnter(); 
        
        // Now go UP 10 times.
        for(int i=0; i<10; i++) fake.EnqueueUp();
        
        fake.EnqueueCtrlEnter();

        var prompt = fake.GetPrompt();
        prompt.AskMultiLineText("in:");

        // Should be back at startTop - 1 (to show label)
        Assert.Equal(10, fake.WindowTop);
    }

    [Fact]
    public void AskMultiLineText_LongLabel_ScrollsCorrectly()
    {
        var fake = new FakeConsole();
        fake.WindowHeight = 5;
        
        // Label is 10 lines
        string longLabel = "L1\nL2\nL3\nL4\nL5\nL6\nL7\nL8\nL9\nL10";
        
        // Textarea will start at row 10.
        
        fake.EnqueueEnter(); // absY = 11.
        fake.EnqueueUp(); // absY = 10.
        
        fake.EnqueueCtrlEnter();

        var prompt = fake.GetPrompt();
        prompt.AskMultiLineText(longLabel);

        // Should be at row 9 (to show L10 + Line 0)
        Assert.Equal(9, fake.WindowTop);
    }
}
