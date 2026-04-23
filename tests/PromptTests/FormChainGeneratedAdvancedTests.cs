using interactiveCLI;
using Xunit;

namespace PromptTests;

/// <summary>
/// Tests for validator, converter, condition, callback, charValidator, and
/// dataSource attributes exercised through the source-generated Ask() method.
/// </summary>
public class FormChainGeneratedAdvancedTests
{
    private static Prompt Prompt(FakeConsole fake) => new Prompt(console: fake);

    // ── [Validator] ───────────────────────────────────────────────────────────

    [Fact]
    public void Validator_RejectsShortInput_ThenAccepts()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("ab");       // too short
        fake.EnqueueLine("alice");    // valid
        var form = new GenValidatorForm();

        form.Ask(Prompt(fake));

        Assert.Equal("alice", form.Username);
    }

    [Fact]
    public void Validator_WritesErrorMessage_OnInvalidInput()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("x");
        fake.EnqueueLine("xyz");
        var form = new GenValidatorForm();

        form.Ask(Prompt(fake));

        Assert.Contains("username must be at least 3 characters", fake.ErrorOutput);
    }

    [Fact]
    public void Validator_AcceptsExactlyMinLength()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("abc");
        var form = new GenValidatorForm();

        form.Ask(Prompt(fake));

        Assert.Equal("abc", form.Username);
    }

    // ── [Converter] ───────────────────────────────────────────────────────────

    [Fact]
    public void Converter_TransformsInputBeforeSettingField()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("5");
        var form = new GenConverterForm();

        form.Ask(Prompt(fake));

        Assert.Equal(50, form.Value);   // converter multiplies by 10
    }

    [Fact]
    public void Converter_NotCalledWhenValidationFails()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("not-a-number");   // fails ValidateValue
        fake.EnqueueLine("3");              // passes
        var form = new GenConverterForm();

        form.Ask(Prompt(fake));

        Assert.Equal(30, form.Value);
    }

    // ── [Condition] ───────────────────────────────────────────────────────────

    [Fact]
    public void Condition_False_FieldNotSet_NoInputConsumed()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("main-name");
        // Nickname condition is false — queue only has one line
        var form = new GenConditionForm { ShowNickname = false };

        form.Ask(Prompt(fake));

        Assert.Equal("main-name", form.Name);
        Assert.Equal(string.Empty, form.Nickname);   // not set
    }

    [Fact]
    public void Condition_True_FieldIsSet()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("main-name");
        fake.EnqueueLine("nick");
        var form = new GenConditionForm { ShowNickname = true };

        form.Ask(Prompt(fake));

        Assert.Equal("main-name", form.Name);
        Assert.Equal("nick", form.Nickname);
    }

    [Fact]
    public void Condition_False_LeavesPreviousFieldValueUntouched()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("bob");
        var form = new GenConditionForm { ShowNickname = false, Nickname = "preset" };

        form.Ask(Prompt(fake));

        Assert.Equal("preset", form.Nickname);
    }

    // ── [Callback] ────────────────────────────────────────────────────────────

    [Fact]
    public void Callback_IsInvoked_WithParsedValue()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("99");
        var form = new GenCallbackForm();

        form.Ask(Prompt(fake));

        Assert.Equal(99, form.Score);
        Assert.Contains(99, form.SeenValues);
    }

    [Fact]
    public void Callback_IsNotInvoked_OnInvalidInput()
    {
        var fake = new FakeConsole();
        fake.EnqueueLine("bad");    // rejected
        fake.EnqueueLine("7");      // accepted
        var form = new GenCallbackForm();

        form.Ask(Prompt(fake));

        Assert.Single(form.SeenValues);
        Assert.Equal(7, form.SeenValues[0]);
    }

    // ── [CharValidator] ───────────────────────────────────────────────────────

    [Fact]
    public void CharValidator_OnlyDigitsAccepted_InPattern()
    {
        var fake = new FakeConsole();
        // pattern "__-__": slots at index 0,1,3,4
        fake.EnqueueChar('a');   // rejected (not digit)
        fake.EnqueueChar('1');   // slot 0
        fake.EnqueueChar('2');   // slot 1
        fake.EnqueueChar('3');   // slot 2 (after dash)
        fake.EnqueueChar('4');   // slot 3
        fake.EnqueueEnter();
        var form = new GenCharValidatorForm();

        form.Ask(Prompt(fake));

        Assert.Equal("12-34", form.Code);
    }

    [Fact]
    public void CharValidator_EscapeYieldsEmptyField()
    {
        // ESC in ReadPatternCopilot returns null; Ask<T> converts null→"" for string
        // and returns immediately — the field is left as empty string.
        var fake = new FakeConsole();
        fake.EnqueueEscape();
        var form = new GenCharValidatorForm();

        form.Ask(Prompt(fake));

        Assert.Equal(string.Empty, form.Code);
    }

    // ── [DataSource] ──────────────────────────────────────────────────────────

    [Fact]
    public void DataSource_SelectsFirstItem_WhenEnterPressedImmediately()
    {
        var fake = new FakeConsole();
        fake.EnqueueEnter();
        var form = new GenDataSourceForm();

        form.Ask(Prompt(fake));

        Assert.Equal("Apple", form.Fruit);
    }

    [Fact]
    public void DataSource_SelectsThirdItem_AfterTwoDownArrows()
    {
        var fake = new FakeConsole();
        fake.EnqueueDown();
        fake.EnqueueDown();
        fake.EnqueueEnter();
        var form = new GenDataSourceForm();

        form.Ask(Prompt(fake));

        Assert.Equal("Cherry", form.Fruit);
    }
}
