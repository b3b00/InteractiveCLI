using interactiveCLI;
using Xunit;

namespace PromptTests;

public class SelectPromptTests
{
    private static SelectPrompt Build(FakeConsole console, string[] choices, bool isIndexed = false)
        => new SelectPrompt("Pick one", choices, isIndexed: isIndexed, console: console);

    [Fact]
    public void ReturnsFirstChoice_WhenEnterPressedImmediately()
    {
        var fake = new FakeConsole();
        fake.EnqueueEnter();
        var select = Build(fake, ["Apple", "Banana", "Cherry"]);

        var result = select.Select();

        Assert.Equal("Apple", result);
    }

    [Fact]
    public void ReturnsSecondChoice_AfterOneDownArrow()
    {
        var fake = new FakeConsole();
        fake.EnqueueSpecialKey(ConsoleKey.DownArrow);
        fake.EnqueueEnter();
        var select = Build(fake, ["Apple", "Banana", "Cherry"]);

        var result = select.Select();

        Assert.Equal("Banana", result);
    }

    [Fact]
    public void ReturnsThirdChoice_AfterTwoDownArrows()
    {
        var fake = new FakeConsole();
        fake.EnqueueSpecialKey(ConsoleKey.DownArrow);
        fake.EnqueueSpecialKey(ConsoleKey.DownArrow);
        fake.EnqueueEnter();
        var select = Build(fake, ["Apple", "Banana", "Cherry"]);

        var result = select.Select();

        Assert.Equal("Cherry", result);
    }

    [Fact]
    public void DoesNotGoAboveFirst_WhenUpPressedAtTop()
    {
        var fake = new FakeConsole();
        fake.EnqueueSpecialKey(ConsoleKey.UpArrow);   // already at first — no move
        fake.EnqueueEnter();
        var select = Build(fake, ["Apple", "Banana"]);

        var result = select.Select();

        Assert.Equal("Apple", result);
    }

    [Fact]
    public void DoesNotGoBelowLast_WhenDownPressedAtBottom()
    {
        var fake = new FakeConsole();
        fake.EnqueueSpecialKey(ConsoleKey.DownArrow);
        fake.EnqueueSpecialKey(ConsoleKey.DownArrow); // already at last — no move
        fake.EnqueueSpecialKey(ConsoleKey.DownArrow);
        fake.EnqueueEnter();
        var select = Build(fake, ["Apple", "Banana"]);

        var result = select.Select();

        Assert.Equal("Banana", result);
    }

    [Fact]
    public void ReturnsNull_WhenEscapePressed()
    {
        var fake = new FakeConsole();
        fake.EnqueueEscape();
        var select = Build(fake, ["Apple", "Banana"]);

        var result = select.Select();

        Assert.Null(result);
    }

    [Fact]
    public void NavigatesDownThenUp_ReturnsFirstChoice()
    {
        var fake = new FakeConsole();
        fake.EnqueueSpecialKey(ConsoleKey.DownArrow);
        fake.EnqueueSpecialKey(ConsoleKey.UpArrow);
        fake.EnqueueEnter();
        var select = Build(fake, ["Apple", "Banana", "Cherry"]);

        var result = select.Select();

        Assert.Equal("Apple", result);
    }

    [Fact]
    public void IndexedSelection_SelectsByNumberKey()
    {
        var fake = new FakeConsole();
        fake.EnqueueChar('2');   // select second item by pressing '2'
        var select = Build(fake, ["Apple", "Banana", "Cherry"], isIndexed: true);

        var result = select.Select();

        Assert.Equal("Banana", result);
    }

    [Fact]
    public void IndexedSelection_SelectsThirdByNumberKey()
    {
        var fake = new FakeConsole();
        fake.EnqueueChar('3');
        var select = Build(fake, ["Apple", "Banana", "Cherry"], isIndexed: true);

        var result = select.Select();

        Assert.Equal("Cherry", result);
    }

    [Fact]
    public void FiltersList_WhenTyping()
    {
        var fake = new FakeConsole();
        fake.EnqueueChar('e');
        fake.EnqueueChar('r');
        fake.EnqueueEnter();
        var select = new SelectPrompt("Pick one", ["Apple", "Banana", "Cherry"], console: fake, autocomplete: true);

        var result = select.Select();

        Assert.Equal("Cherry", result);
    }

    [Fact]
    public void FiltersList_IgnoreCase()
    {
        var fake = new FakeConsole();
        fake.EnqueueChar('C');
        fake.EnqueueChar('H');
        fake.EnqueueEnter();
        var select = new SelectPrompt("Pick one", ["Apple", "Banana", "Cherry"], console: fake, autocomplete: true);

        var result = select.Select();

        Assert.Equal("Cherry", result);
    }

    [Fact]
    public void FiltersList_AndNavigates()
    {
        var fake = new FakeConsole();
        fake.EnqueueChar('a'); // Matches "Apple", "Banana"
        fake.EnqueueSpecialKey(ConsoleKey.DownArrow);
        fake.EnqueueEnter();
        var select = new SelectPrompt("Pick one", ["Apple", "Banana", "Cherry"], console: fake, autocomplete: true);

        var result = select.Select();

        Assert.Equal("Banana", result);
    }

    [Fact]
    public void FiltersList_BackspaceRestoresList()
    {
        var fake = new FakeConsole();
        fake.EnqueueChar('z'); // Matches nothing
        fake.EnqueueSpecialKey(ConsoleKey.Backspace); // Matches everything
        fake.EnqueueSpecialKey(ConsoleKey.DownArrow);
        fake.EnqueueEnter();
        var select = new SelectPrompt("Pick one", ["Apple", "Banana", "Cherry"], console: fake, autocomplete: true);

        var result = select.Select();

        Assert.Equal("Banana", result);
    }

    [Fact]
    public void FiltersList_MaintainsPositionWithinBounds()
    {
        var fake = new FakeConsole();
        fake.EnqueueSpecialKey(ConsoleKey.DownArrow);
        fake.EnqueueSpecialKey(ConsoleKey.DownArrow); // At Cherry (index 2)
        fake.EnqueueChar('a'); // Matches Apple, Banana. Length is 2. Position should be bounded to 1 (Banana)
        fake.EnqueueEnter();
        var select = new SelectPrompt("Pick one", ["Apple", "Banana", "Cherry"], console: fake, autocomplete: true);

        var result = select.Select();

        Assert.Equal("Banana", result);
    }
}
