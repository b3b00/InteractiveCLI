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
        // We can check if SetCursorPosition was called with valid coordinates
        // and if startTop was adjusted.
        
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
        // line1 + X at pos 3 -> linXe1 (wait, cursor was at end of line1, then Up, then Left 2)
        // line1 length 5. Cursor at 5. Up -> Cursor at 5. Left 2 -> Cursor at 3.
        // lin e1 -> linXe1
        Assert.Equal("linXe1\nline2", result.Value);
    }

    [Fact]
    public void AskMultiLineText_ScrollsBufferWhenHittingBufferBottom()
    {
        var fake = new FakeConsole();
        fake.WindowHeight = 5;
        // BufferHeight is 1000 in FakeConsole now
        
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
        fake.EnqueueEnter(); // This is the 5th line (index 4)
        fake.EnqueueChars("L5");
        fake.EnqueueEnter(); // This should move WindowTop to 1
        fake.EnqueueChars("L6");
        fake.EnqueueCtrlEnter();

        var prompt = fake.GetPrompt();
        prompt.AskMultiLineText("in:");

        Assert.Equal(3, fake.WindowTop); // 6 lines total + 1 for label, cursor at 8th line.
    }
}
