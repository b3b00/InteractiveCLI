using interactiveCLI;
using Xunit;

namespace PromptTests;

public class ReadPatternTests
{
    private static Prompt Build(FakeConsole console) => new Prompt(console: console);

    [Fact]
    public void FillsAllSlots_AndReturnsFilledPattern()
    {
        // Pattern __/__/____  → dd/MM/yyyy
        var fake = new FakeConsole();
        fake.EnqueueChar('2'); fake.EnqueueChar('3'); // day
        fake.EnqueueChar('0'); fake.EnqueueChar('4'); // month
        fake.EnqueueChar('2'); fake.EnqueueChar('0'); fake.EnqueueChar('2'); fake.EnqueueChar('6'); // year
        fake.EnqueueEnter();
        var prompt = Build(fake);

        var result = prompt.ReadPatternCopilot("__/__/____");

        Assert.Equal("23/04/2026", result);
    }

    [Fact]
    public void ReturnsNull_WhenEscapePressed()
    {
        var fake = new FakeConsole();
        fake.EnqueueChar('1');
        fake.EnqueueEscape();
        var prompt = Build(fake);

        var result = prompt.ReadPatternCopilot("__/__/____");

        Assert.Null(result);
    }

    [Fact]
    public void HandlesBackspace_ErasesLastCharacter()
    {
        // Pattern: __-__  → 2 chars, dash, 2 chars
        var fake = new FakeConsole();
        fake.EnqueueChar('A');
        fake.EnqueueChar('X');       // typo
        fake.EnqueueBackspace();     // erase
        fake.EnqueueChar('B');
        fake.EnqueueChar('C');
        fake.EnqueueChar('D');
        fake.EnqueueEnter();
        var prompt = Build(fake);

        var result = prompt.ReadPatternCopilot("__-__");

        Assert.Equal("AB-CD", result);
    }

    [Fact]
    public void RejectsCharacter_WhenCharValidatorReturnsFalse()
    {
        // Allow only digits
        var fake = new FakeConsole();
        fake.EnqueueChar('a');   // rejected — not a digit
        fake.EnqueueChar('1');   // accepted
        fake.EnqueueChar('2');   // accepted
        fake.EnqueueEnter();
        var prompt = Build(fake);

        var result = prompt.ReadPatternCopilot(
            "__",
            isAllowed: t => char.IsDigit(t.c)
        );

        Assert.Equal("12", result);
    }
}
