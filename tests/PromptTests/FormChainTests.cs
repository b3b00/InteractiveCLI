using interactiveCLI;
using interactiveCLI.forms;
using Xunit;

namespace PromptTests;

// ── Sample form models ──────────────────────────────────────────────────────

public class SimpleTextForm
{
    [Input("Name")]
    public string Name { get; set; } = string.Empty;

    [Input("City")]
    public string City { get; set; } = string.Empty;
}

public class MixedTypesForm
{
    [Input("Username")]
    public string Username { get; set; } = string.Empty;

    [Input("Age")]
    public int Age { get; set; }

    [Input("Score")]
    public double Score { get; set; }
}

public class BoolForm
{
    [BoolInput("Agree", new[] { "yes", "y" }, new[] { "no", "n" })]
    public bool Agree { get; set; }
}

public class SelectForm
{
    [Select("Colour", new[] { "Red", "Green", "Blue" })]
    public string Colour { get; set; } = string.Empty;
}

public class PasswordForm
{
    [Password("Secret")]
    public string Secret { get; set; } = string.Empty;
}

// ── Tests ───────────────────────────────────────────────────────────────────

public class FormChainTests
{
    // Helpers
    private static Prompt Prompt(FakeConsole fake) => new Prompt(console: fake);

    private static T BuildAndAsk<T>(FakeConsole fake) where T : new()
    {
        var prompt = Prompt(fake);
        var builder = new FormBuilder<T>();
        var instance = new T();
        var form = builder.Build(instance, prompt);
        return form.Ask();
    }

    // ── Simple text form ────────────────────────────────────────────────

    [Fact]
    public void FillsAllStringFields_InOrder()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("Alice");
        fake.EnqueueLine("Paris");

        var result = BuildAndAsk<SimpleTextForm>(fake);

        Assert.Equal("Alice", result.Name);
        Assert.Equal("Paris", result.City);
    }

    [Fact]
    public void FillsSecondFieldCorrectly_WhenFirstFieldHasDefaultValidator()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("Bob");
        fake.EnqueueLine("London");

        var result = BuildAndAsk<SimpleTextForm>(fake);

        Assert.Equal("Bob", result.Name);
        Assert.Equal("London", result.City);
    }

    // ── Mixed-type form ─────────────────────────────────────────────────

    [Fact]
    public void FillsStringIntDouble_Fields()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("charlie");
        fake.EnqueueLine("30");
        fake.EnqueueLine("9.5");

        var result = BuildAndAsk<MixedTypesForm>(fake);

        Assert.Equal("charlie", result.Username);
        Assert.Equal(30, result.Age);
        Assert.Equal(9.5, result.Score, precision: 10);
    }

    [Fact]
    public void RetriesIntField_WhenNonIntegerEntered()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("dave");
        fake.EnqueueLine("not-a-number");   // rejected
        fake.EnqueueLine("25");             // accepted
        fake.EnqueueLine("7.0");

        var result = BuildAndAsk<MixedTypesForm>(fake);

        Assert.Equal(25, result.Age);
    }

    // ── Bool form ────────────────────────────────────────────────────────

    [Fact]
    public void FillsBoolField_WhenTrueValueEntered()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("yes");

        var result = BuildAndAsk<BoolForm>(fake);

        Assert.True(result.Agree);
    }

    [Fact]
    public void FillsBoolField_WhenFalseValueEntered()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("no");

        var result = BuildAndAsk<BoolForm>(fake);

        Assert.False(result.Agree);
    }

    // ── Select form ──────────────────────────────────────────────────────

    [Fact]
    public void FillsSelectField_WhenFirstItemChosen()
    {
        var fake = new FakeConsole();
        fake.EnqueueEnter();   // select highlighted (first) item

        var result = BuildAndAsk<SelectForm>(fake);

        Assert.Equal("Red", result.Colour);
    }

    [Fact]
    public void FillsSelectField_WhenSecondItemChosen()
    {
        var fake = new FakeConsole();
        fake.EnqueueSpecialKey(ConsoleKey.DownArrow);
        fake.EnqueueEnter();

        var result = BuildAndAsk<SelectForm>(fake);

        Assert.Equal("Green", result.Colour);
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

        var result = BuildAndAsk<PasswordForm>(fake);

        Assert.Equal("sec", result.Secret);
    }

    // ── Chained / multi-step form ────────────────────────────────────────

    [Fact]
    public void AskForm_FillesMixedForm_ViaPromptHelper()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("eve");
        fake.EnqueueLine("20");
        fake.EnqueueLine("8.8");

        var prompt = Prompt(fake);
        var result = prompt.AskForm<MixedTypesForm>();

        Assert.Equal("eve", result.Username);
        Assert.Equal(20, result.Age);
        Assert.Equal(8.8, result.Score, precision: 10);
    }
}
