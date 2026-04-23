using interactiveCLI.forms;
using System.Globalization;

namespace PromptTests;

// ── Generated form models ─────────────────────────────────────────────────
// These are partial classes decorated with [Form] so the source generator
// produces an Ask(Prompt prompt = null) method for each of them.
// Only attribute names that the generator recognises are used:
// [Input], [Password], [Validator], [Converter], [DataSource], [CharValidator]

[Form]
public partial class GenSimpleTextForm
{
    [Input("Name")]
    public string Name { get; set; } = string.Empty;

    [Input("City")]
    public string City { get; set; } = string.Empty;
}

[Form]
public partial class GenMixedTypesForm
{
    [Input("Username")]
    public string Username { get; set; } = string.Empty;

    [Input("Age")]
    public int Age { get; set; }

    // Validator + Converter use InvariantCulture so tests are locale-independent.
    [Input("Score")]
    [Validator(nameof(ValidateScore))]
    [Converter(nameof(ParseScore))]
    public double Score { get; set; }

    public (bool ok, string errorMessage) ValidateScore(string s) =>
        double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out _)
            ? (true, null)
            : (false, "not a valid number");

    public double ParseScore(string s) =>
        double.Parse(s, CultureInfo.InvariantCulture);
}

[Form]
public partial class GenBoolForm
{
    // Use [Validator]+[Converter] because the generator does not handle [BoolInput].
    [Input("Agree")]
    [Validator(nameof(ValidateAgree))]
    [Converter(nameof(ConvertAgree))]
    public bool Agree { get; set; }

    public (bool ok, string errorMessage) ValidateAgree(string s) =>
        (s == "yes" || s == "y" || s == "no" || s == "n",
         "answer yes/y or no/n");

    public bool ConvertAgree(string s) => s == "yes" || s == "y";
}

[Form]
public partial class GenSelectForm
{
    // Use [DataSource] because the generator does not handle [Select].
    [Input("Colour")]
    [DataSource(nameof(ColourSource))]
    public string Colour { get; set; } = string.Empty;

    public string[] ColourSource() => ["Red", "Green", "Blue"];
}

[Form]
public partial class GenPasswordForm
{
    [Password("Secret")]
    public string Secret { get; set; } = string.Empty;
}

// ── Models for advanced-attribute tests ──────────────────────────────────────

[Form]
public partial class GenValidatorForm
{
    [Input("Username")]
    [Validator(nameof(ValidateUsername))]
    public string Username { get; set; } = string.Empty;

    public (bool ok, string errorMessage) ValidateUsername(string s) =>
        (s.Length >= 3, "username must be at least 3 characters");
}

[Form]
public partial class GenConverterForm
{
    [Input("Value")]
    [Validator(nameof(ValidateValue))]
    [Converter(nameof(ConvertValue))]
    public int Value { get; set; }

    public (bool ok, string errorMessage) ValidateValue(string s) =>
        (int.TryParse(s, out _), "not an integer");

    public int ConvertValue(string s) => int.Parse(s) * 10;
}

[Form]
public partial class GenConditionForm
{
    [Input("Name")]
    public string Name { get; set; } = string.Empty;

    [Input("Nickname")]
    [Condition(nameof(NicknameCondition))]
    public string Nickname { get; set; } = string.Empty;

    // Toggled by tests
    public bool ShowNickname { get; set; } = false;
    public bool NicknameCondition() => ShowNickname;
}

[Form]
public partial class GenCallbackForm
{
    [Input("Score")]
    [Callback(nameof(OnScore))]
    public int Score { get; set; }

    public List<int> SeenValues { get; } = new();
    public void OnScore(int v) => SeenValues.Add(v);
}

[Form]
public partial class GenCharValidatorForm
{
    [Input("Code", "__-__")]
    [CharValidator(nameof(IsDigit))]
    public string Code { get; set; } = string.Empty;

    public bool IsDigit((int position, char c) t) => char.IsDigit(t.c);
}

[Form]
public partial class GenDataSourceForm
{
    [Input("Fruit")]
    [DataSource(nameof(Fruits))]
    public string Fruit { get; set; } = string.Empty;

    public string[] Fruits() => ["Apple", "Banana", "Cherry"];
}
