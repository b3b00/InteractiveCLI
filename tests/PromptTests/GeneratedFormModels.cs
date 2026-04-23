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
