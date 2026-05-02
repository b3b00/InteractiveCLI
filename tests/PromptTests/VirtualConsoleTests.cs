using interactiveCLI;
using Xunit;

namespace PromptTests;

public class VirtualConsoleTests
{
    [Fact]
    public void MoveUp_AtTopOfBuffer_DoesNothing()
    {
        var fake = new FakeConsole();
        fake.WindowHeight = 10;
        var vc = new VirtualConsole(fake);
        
        vc.MoveUp(); // at 0,0
        
        Assert.Equal(0, vc.CursorY);
        Assert.Equal(0, vc.ViewTopY);
    }
    
    [Fact]
    public void MoveUp_AtTopOfScreenButNotBuffer_RedrawsFromLineMinusOne()
    {
        var fake = new FakeConsole();
        fake.WindowHeight = 5;
        var vc = new VirtualConsole(fake);
        
        // Add lines to scroll down
        for (int i = 0; i < 6; i++)
        {
            vc.InsertChar((char)('0' + i));
            vc.Enter();
        }
        
        Assert.True(vc.ViewTopY > 0);
        int currentViewTop = vc.ViewTopY;
        
        // Move up to the top of the screen
        while (vc.CursorY > currentViewTop)
        {
            vc.MoveUp();
        }
        
        // Now at top of screen, but not top of buffer. Move up again.
        vc.MoveUp();
        
        Assert.Equal(currentViewTop - 1, vc.CursorY);
        Assert.Equal(currentViewTop - 1, vc.ViewTopY);
    }

    [Fact]
    public void EnterKey_AtTopScreen_InsertsLineAtCurrentLineAndRedraws()
    {
        var fake = new FakeConsole();
        fake.WindowHeight = 10;
        var vc = new VirtualConsole(fake);
        
        vc.InsertChar('A');
        vc.Enter();
        vc.InsertChar('B');
        
        // Move back to (0,0)
        vc.MoveUp();
        vc.MoveHome(false);
        
        Assert.Equal(0, vc.CursorY);
        Assert.Equal(0, vc.CursorX);
        
        vc.Enter();
        
        // A new empty line should be inserted, moving 'A' down.
        Assert.Equal(3, vc.Lines.Count);
        Assert.Equal("", vc.Lines[0].ToString());
        Assert.Equal("A", vc.Lines[1].ToString());
        Assert.Equal("B", vc.Lines[2].ToString());
        Assert.Equal(1, vc.CursorY);
        Assert.Equal(0, vc.CursorX);
    }

    [Fact]
    public void MoveDown_AtBottomScreen_ScrollsDown()
    {
        var fake = new FakeConsole();
        fake.WindowHeight = 5;
        var vc = new VirtualConsole(fake);
        
        // Fill up to bottom of screen
        for (int i = 0; i < 10; i++)
        {
            vc.Enter();
        }
        
        Assert.True(vc.ViewTopY > 0);
    }

    [Fact]
    public void EnterKey_AtBottomScreen_InsertsLineAndMovesDisplayUp()
    {
        var fake = new FakeConsole();
        fake.WindowHeight = 5;
        // Make window height available exactly 5, StartTop = 0
        var vc = new VirtualConsole(fake);
        
        // Fill up lines
        for (int i = 0; i < 5; i++)
        {
            vc.Enter();
        }
        
        int viewTopBefore = vc.ViewTopY;
        
        vc.Enter(); // This should trigger scrolling
        
        Assert.True(vc.ViewTopY > viewTopBefore);
    }

    [Fact]
    public void BugFix_MovingUpAtZeroZero_KeepsCursorVisible()
    {
        var fake = new FakeConsole();
        fake.WindowHeight = 10;
        var vc = new VirtualConsole(fake);
        
        vc.InsertChar('A');
        vc.MoveHome(false);
        Assert.Equal(0, vc.CursorY);
        Assert.Equal(0, vc.CursorX);
        
        // Action that previously made cursor disappear
        vc.MoveUp(); 
        
        // Still at 0,0
        Assert.Equal(0, vc.CursorY);
        Assert.Equal(0, vc.CursorX);
        
        // We can still insert chars correctly
        vc.InsertChar('B');
        Assert.Equal("BA", vc.Lines[0].ToString());
        
        // Ensure FakeConsole recorded the cursor appropriately at 0,0 or within bounds.
        Assert.True(fake.CursorTop >= 0);
    }
    [Fact]
    public void MoveDown_AtBottomOfBuffer_DoesNothing()
    {
        var fake = new FakeConsole();
        fake.WindowHeight = 10;
        var vc = new VirtualConsole(fake);
        
        vc.MoveDown(); // At 0,0
        
        Assert.Equal(0, vc.CursorY);
        Assert.Equal(0, vc.ViewTopY);
    }
    
    [Fact]
    public void MoveDown_NotAtBottom_MovesCursorDown()
    {
        var fake = new FakeConsole();
        fake.WindowHeight = 10;
        var vc = new VirtualConsole(fake);
        
        vc.InsertChar('A');
        vc.Enter();
        vc.InsertChar('B');
        vc.MoveUp();
        
        Assert.Equal(0, vc.CursorY);
        vc.MoveDown();
        Assert.Equal(1, vc.CursorY);
        Assert.Equal(1, vc.CursorX); // CursorX is limited by the line length which is 1
    }
    
    [Fact]
    public void MoveHome_WithCtrl_MovesToTopLeft()
    {
        var fake = new FakeConsole();
        fake.WindowHeight = 10;
        var vc = new VirtualConsole(fake);
        
        vc.InsertChar('A');
        vc.Enter();
        vc.InsertChar('B');
        
        Assert.Equal(1, vc.CursorY);
        Assert.Equal(1, vc.CursorX);
        
        vc.MoveHome(ctrl: true);
        
        Assert.Equal(0, vc.CursorY);
        Assert.Equal(0, vc.CursorX);
        Assert.Equal(0, vc.ViewTopY);
    }

    [Fact]
    public void MoveHome_WithoutCtrl_MovesToStartOfCurrentLine()
    {
        var fake = new FakeConsole();
        fake.WindowHeight = 10;
        var vc = new VirtualConsole(fake);
        
        vc.InsertChar('A');
        vc.Enter();
        vc.InsertChar('B');
        vc.InsertChar('C');
        
        Assert.Equal(1, vc.CursorY);
        Assert.Equal(2, vc.CursorX);
        
        vc.MoveHome(ctrl: false);
        
        Assert.Equal(1, vc.CursorY);
        Assert.Equal(0, vc.CursorX);
    }

    [Fact]
    public void MoveEnd_WithCtrl_MovesToBottomRight()
    {
        var fake = new FakeConsole();
        fake.WindowHeight = 10;
        var vc = new VirtualConsole(fake);
        
        vc.InsertChar('A');
        vc.Enter();
        vc.InsertChar('B');
        vc.InsertChar('C');
        vc.MoveHome(ctrl: true);
        
        vc.MoveEnd(ctrl: true);
        
        Assert.Equal(1, vc.CursorY);
        Assert.Equal(2, vc.CursorX); // "BC" length is 2
    }

    [Fact]
    public void MoveEnd_WithoutCtrl_MovesToEndOfCurrentLine()
    {
        var fake = new FakeConsole();
        fake.WindowHeight = 10;
        var vc = new VirtualConsole(fake);
        
        vc.InsertChar('A');
        vc.InsertChar('B');
        vc.MoveHome(ctrl: false);
        
        Assert.Equal(0, vc.CursorX);
        
        vc.MoveEnd(ctrl: false);
        
        Assert.Equal(2, vc.CursorX); // "AB" length is 2
    }

    [Fact]
    public void Delete_WithinLine_RemovesCharacterAtCursor()
    {
        var fake = new FakeConsole();
        fake.WindowHeight = 10;
        var vc = new VirtualConsole(fake);
        
        vc.InsertChar('A');
        vc.InsertChar('B');
        vc.InsertChar('C');
        vc.MoveHome(ctrl: false);
        vc.MoveRight(); // cursor is at index 1 ('B')
        
        vc.Delete();
        
        Assert.Equal("AC", vc.Lines[0].ToString());
        Assert.Equal(1, vc.CursorX);
    }

    [Fact]
    public void Delete_AtEndOfLine_MergesWithNextLine()
    {
        var fake = new FakeConsole();
        fake.WindowHeight = 10;
        var vc = new VirtualConsole(fake);
        
        vc.InsertChar('A');
        vc.Enter();
        vc.InsertChar('B');
        vc.MoveHome(ctrl: true);
        vc.MoveEnd(ctrl: false); // End of line 0
        
        vc.Delete();
        
        Assert.Equal(1, vc.Lines.Count);
        Assert.Equal("AB", vc.Lines[0].ToString());
        Assert.Equal(1, vc.CursorX);
    }
}
