using interactiveCLI;
using Xunit;

namespace PromptTests;

/// <summary>
/// Same scenarios as FormChainTests but exercised through the source-generated
/// Ask(Prompt prompt = null) method instead of FormBuilder reflection.
/// </summary>
public class FormChainGeneratedTests
{
    private static Prompt Prompt(FakeConsole fake) => new Prompt(console: fake);

    // ── Simple text form ────────────────────────────────────────────────

    [Fact]
    public void FillsAllStringFields_InOrder()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("Alice");
        fake.EnqueueLine("Paris");
        var form = new GenSimpleTextForm();

        form.Ask(Prompt(fake));

        Assert.Equal("Alice", form.Name);
        Assert.Equal("Paris", form.City);
    }

    [Fact]
    public void FillsSecondFieldCorrectly_WhenFirstFieldHasDefaultValidator()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("Bob");
        fake.EnqueueLine("London");
        var form = new GenSimpleTextForm();

        form.Ask(Prompt(fake));

        Assert.Equal("Bob", form.Name);
        Assert.Equal("London", form.City);
    }

    // ── Mixed-type form ─────────────────────────────────────────────────

    [Fact]
    public void FillsStringIntDouble_Fields()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("charlie");
        fake.EnqueueLine("30");
        fake.EnqueueLine("9.5");
        var form = new GenMixedTypesForm();

        form.Ask(Prompt(fake));

        Assert.Equal("charlie", form.Username);
        Assert.Equal(30, form.Age);
        Assert.Equal(9.5, form.Score, precision: 10);
    }

    [Fact]
    public void RetriesIntField_WhenNonIntegerEntered()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("dave");
        fake.EnqueueLine("not-a-number");   // rejected
        fake.EnqueueLine("25");             // accepted
        fake.EnqueueLine("7.0");
        var form = new GenMixedTypesForm();

        form.Ask(Prompt(fake));

        Assert.Equal(25, form.Age);
    }

    // ── Bool form ────────────────────────────────────────────────────────

    [Fact]
    public void FillsBoolField_WhenTrueValueEntered()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("yes");
        var form = new GenBoolForm();

        form.Ask(Prompt(fake));

        Assert.True(form.Agree);
    }

    [Fact]
    public void FillsBoolField_WhenFalseValueEntered()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("no");
        var form = new GenBoolForm();

        form.Ask(Prompt(fake));

        Assert.False(form.Agree);
    }

    // ── Select form ──────────────────────────────────────────────────────

    [Fact]
    public void FillsSelectField_WhenFirstItemChosen()
    {
        var fake = new FakeConsole();
        fake.EnqueueEnter();
        var form = new GenSelectForm();

        form.Ask(Prompt(fake));

        Assert.Equal("Red", form.Colour);
    }

    [Fact]
    public void FillsSelectField_WhenSecondItemChosen()
    {
        var fake = new FakeConsole();
        fake.EnqueueSpecialKey(ConsoleKey.DownArrow);
        fake.EnqueueEnter();
        var form = new GenSelectForm();

        form.Ask(Prompt(fake));

        Assert.Equal("Green", form.Colour);
    }

    // ── Password form ────────────────────────────────────────────────────

    [Fact]
    public void FillsPasswordField_WithTypedCharacters()
    {
        var fake = new FakeConsole();
        fake.EnqueueChar('s');
        fake.EnqueueChar('e');
        fake.EnqueueChar('c');
        fake.EnqueueEnter();
        var form = new GenPasswordForm();

        form.Ask(Prompt(fake));

        Assert.Equal("sec", form.Secret);
    }
}
