# InteractiveCLI — Input Types Reference

This document covers every input type available in InteractiveCLI, both through the **Form source-generator** (attribute-based) and through the **raw `Prompt` API**.

---

## Table of Contents

1. [Getting Started](#getting-started)
2. [Form Attribute](#form-attribute)
3. [Input Types](#input-types)
   - [Text Input](#text-input)
   - [Integer Input](#integer-input)
   - [Double Input](#double-input)
   - [Boolean / Checkbox Input](#boolean--checkbox-input)
   - [Password Input](#password-input)
   - [Select Input](#select-input)
   - [TextArea Input](#textarea-input)
   - [Pattern Input](#pattern-input)
   - [Generic Typed Input](#generic-typed-input)
4. [Behavioral Attributes](#behavioral-attributes)
   - [Validator](#validator)
   - [Converter](#converter)
   - [Condition](#condition)
   - [Callback](#callback)
   - [CharValidator](#charvalidator)
   - [DataSource](#datasource)
   - [Indexed](#indexed)
5. [Input Ordering](#input-ordering)
6. [Raw Prompt API](#raw-prompt-api)
7. [Complete Example](#complete-example)

---

## Getting Started

Add the NuGet package to your project:

```xml
<PackageReference Include="interactiveCLI" Version="1.1.0" />
```

The library provides two usage modes:

- **Form mode** — Decorate a `partial class` with `[Form]` and its properties with input attributes. The source generator produces an `Ask(Prompt prompt = null)` method.
- **Raw API mode** — Instantiate `Prompt` directly and call its methods (`Ask<T>`, `AskText`, `AskInt`, etc.).

---

## Form Attribute

Every form class must be decorated with `[Form]`. The class must be `partial` so the source generator can add the `Ask` method.

```csharp
[Form]
public partial class MyForm
{
    // input properties go here
}
```

**Optional parameter:**

| Parameter | Type | Description |
|---|---|---|
| `invalidInputMessage` | `string` | Custom error message shown when any input fails validation. Supports ANSI escape codes. Default is `"Invalid answer."` |

```csharp
[Form("\x1b[1;31mInvalid input.\x1b[0m")]  // bold red error message
public partial class MyForm { ... }
```

**Usage:**

```csharp
var form = new MyForm();
form.Ask();                  // uses default Prompt
form.Ask(new Prompt());      // uses a custom Prompt instance
```

---

## Input Types

### Text Input

**Attribute:** `[Input(label)]`  
**Property types:** `string`, `int`, `double`, `bool` (type is inferred automatically)

The most general input type. Prompts the user for a single line of text. The behavior adapts based on the property type:

- `string` — reads the raw text as-is
- `int` — keeps retrying until a valid integer is entered
- `double` — keeps retrying until a valid decimal number is entered (uses `InvariantCulture`)
- `bool` — displays a ✅/❌ toggle (see [Boolean Input](#boolean--checkbox-input))

```csharp
[Input("Your name:")]
public string Name { get; set; }

[Input("Your age:")]
public int Age { get; set; }

[Input("Your score:")]
public double Score { get; set; }
```

**Constructor overloads:**

```csharp
[Input("label")]
[Input("label", pattern: "__/__/____")]
[Input("label", pattern: null, index: 2)]
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `label` | `string` | *(required)* | Text displayed before the input cursor |
| `pattern` | `string` | `null` | Fixed-format pattern using `_` as editable slots (see [Pattern Input](#pattern-input)) |
| `index` | `int` | `-1` | Explicit prompt order (see [Input Ordering](#input-ordering)) |

---

### Integer Input

**Raw API method:** `AskInt(label, validator?)`

Reads a line and parses it as an `int`. Retries indefinitely until a valid integer is entered.

```csharp
// Form mode — use [Input] on an int property
[Input("Enter age:")]
public int Age { get; set; }

// Raw API
Prompt prompt = new Prompt();
int age = prompt.AskInt("Enter age:");
```

**With a custom validator:**

```csharp
int age = prompt.AskInt(
    "Enter age:",
    validator: s => int.TryParse(s, out var v) && v is >= 0 and <= 120
        ? (true, null)
        : (false, "Age must be between 0 and 120")
);
```

---

### Double Input

**Raw API method:** `AskDouble(label, validator?)`

Reads a line and parses it as a `double` using `InvariantCulture` (decimal separator is always `.`). Retries indefinitely until a valid number is entered.

```csharp
// Form mode — use [Input] on a double property
[Input("Score:")]
public double Score { get; set; }

// Raw API
double score = prompt.AskDouble("Score:");
```

**With a validator:**

```csharp
double price = prompt.AskDouble(
    "Price:",
    validator: s => double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) && v > 0
        ? (true, null)
        : (false, "Price must be positive")
);
```

---

### Boolean / Checkbox Input

**Attribute:** `[Input(label)]` on a `bool`/`Boolean` property  
**Raw API method:** `AskBool(label, trueValues, falseValues, validator?)`

#### Default Behavior (Checkbox)

When `[Input]` is placed on a `bool` property without a converter, the prompt renders as an interactive toggle:

- Displays `❌` (unchecked) initially
- Press **Space** to toggle between ✅ (checked) and ❌ (unchecked)
- Press **Enter** to confirm

```csharp
[Input("Do you agree?")]
public bool Agree { get; set; }
```

#### Custom True/False Values

To accept text values like `yes`/`no` instead of a checkbox, combine `[Validator]` and `[Converter]`:

```csharp
[Input("Accept terms? (yes/no):")]
[Validator(nameof(ValidateYesNo))]
[Converter(nameof(ConvertYesNo))]
public bool AcceptsTerms { get; set; }

public (bool ok, string errorMessage) ValidateYesNo(string v)
{
    bool ok = v is "yes" or "y" or "no" or "n";
    return (ok, ok ? null : "Please enter yes/y or no/n");
}

public bool ConvertYesNo(string v) => v is "yes" or "y";
```

#### Raw API

```csharp
bool answer = prompt.AskBool(
    "Continue?",
    trueValues: ["yes", "y"],
    falseValues: ["no", "n"]
);
```

The prompt label is automatically appended with the accepted values, e.g.:  
`Continue? [yes, y] or [no, n]`

---

### Password Input

**Attribute:** `[Password(label, hiddenChar?, index?)]`  
**Raw API method:** `AskPassword(label, hiddenChar?, validator?, condition?, callbacks)`

Reads characters one at a time without echoing them. Each character is replaced in the display by `hiddenChar` (default `*`). Backspace is supported to erase the last character.

```csharp
[Password("Password:")]
public string Password { get; set; }

// Custom mask character
[Password("Password:", hiddenChar: '•')]
public string Password { get; set; }
```

**Returns:** `Result<string>` — wraps the value with `Ok` and `IsApplicable` flags (raw API only).

| Parameter | Type | Default | Description |
|---|---|---|---|
| `label` | `string` | *(required)* | Prompt text |
| `hiddenChar` | `char` | `'*'` | Character displayed in place of each typed character |
| `index` | `int` | `-1` | Prompt order |

**With a validator and callback (form mode):**

```csharp
[Password("Password:", hiddenChar: '*')]
[Validator(nameof(ValidatePassword))]
[Callback(nameof(OnPasswordEntered))]
public string Password { get; set; }

public (bool ok, string errorMessage) ValidatePassword(string pw)
    => (pw.Length >= 8, "Password must be at least 8 characters");

public void OnPasswordEntered(string pw)
    => Console.WriteLine("Password accepted.");
```

**Raw API:**

```csharp
var result = prompt.AskPassword(
    "Password:",
    hiddenChar: '*',
    validator: pw => (pw.Length >= 8, "Too short"),
    callbacks: [pw => Console.WriteLine("Password set!")]
);

if (result.Ok)
    Console.WriteLine("Login successful");
```

---

### Select Input

**Attribute:** `[Input(label)]` + `[DataSource(methodName)]`  
**Alt attribute:** `[Select(label, values[])]` (static values)  
**Raw API method:** `Select(label, formatter?, choices?, isIndexed?)`

Renders an interactive list. The user navigates with **↑**/**↓** arrow keys and confirms with **Enter**. Pressing **Escape** returns `null`.

#### Using DataSource (Recommended for Forms)

The data source method must be a `public` or `private` method on the form class that returns `string[]`.

```csharp
[Input("Choose a fruit:")]
[DataSource(nameof(GetFruits))]
public string SelectedFruit { get; set; }

public string[] GetFruits() => ["Apple", "Banana", "Cherry", "Orange"];
```

#### Using Static Values

```csharp
[Select("Choose a size:", new[] { "Small", "Medium", "Large" })]
public string Size { get; set; }
```

#### Indexed Selection

Adding `[Indexed]` allows selecting items by pressing the corresponding number key (works for lists with fewer than 10 items):

```csharp
[Input("Choose option:")]
[DataSource(nameof(Options))]
[Indexed]
public string Option { get; set; }

public string[] Options() => ["Start", "Settings", "Quit"];
```

With `[Indexed]`, items are displayed as:
```
  1. Start
  2. Settings
  3. Quit
```

The user can press `1`, `2`, or `3` to select directly.

#### Raw API

```csharp
string? color = prompt.Select(
    "Favorite color:",
    choices: ["Red", "Green", "Blue"]
);

// Indexed mode
string? option = prompt.Select(
    "Choose:",
    choices: ["Start", "Quit"],
    isIndexed: true
);

// Custom formatter
string? item = prompt.Select(
    "Pick:",
    choices: ["A", "B", "C"],
    formatter: (value, selected, index) =>
        selected ? $">>> {value} <<<"
                 : $"    {value}"
);
```

**Keyboard controls:**

| Key | Action |
|---|---|
| ↑ / ↓ | Move selection |
| Enter | Confirm selection |
| Escape | Cancel (returns `null`) |
| 1–9 | Jump to item by number (only when `isIndexed: true` and < 10 items) |

---

### TextArea Input

**Attribute:** `[TextArea(label, maxLines?, index?, finishKey?)]`  
**Raw API method:** `AskMultiLineText(label, maxLines?, finishKey?, validator?, condition?, callbacks)`

Allows entering multi-line text, similar to an HTML `<textarea>`. Lines are joined with `\n` in the resulting string.

```csharp
[TextArea("Description:")]
public string Description { get; set; }
```

**Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `label` | `string` | *(required)* | Prompt text |
| `maxLines` | `int` | `0` | Maximum number of lines (0 = unlimited) |
| `finishKey` | `ConsoleKey` | `ConsoleKey.Enter` | Key used together with **Ctrl** to submit |
| `index` | `int` | `-1` | Prompt order |

**Keyboard controls:**

| Key | Action |
|---|---|
| Enter | Insert a new line |
| Ctrl + *finishKey* | Submit the input |
| Escape | Cancel (returns `null` / `IsApplicable = false`) |
| Backspace | Erase last character; if line is empty, move to previous line |

```csharp
// 5-line limit, submit with Ctrl+D
[TextArea("Code snippet:", maxLines: 5, finishKey: ConsoleKey.D, index: 2)]
public string Code { get; set; }
```

**With validator and callbacks:**

```csharp
[TextArea("Comment:", maxLines: 10)]
[Validator(nameof(ValidateComment))]
[Callback(nameof(OnCommentSaved))]
public string Comment { get; set; }

public (bool ok, string errorMessage) ValidateComment(string text)
    => (text.Length >= 10, "Comment must be at least 10 characters");

public void OnCommentSaved(string text)
    => Console.WriteLine($"Saved {text.Length} characters.");
```

**Raw API:**

```csharp
var result = prompt.AskMultiLineText(
    "Notes:",
    maxLines: 0,
    finishKey: ConsoleKey.Enter,
    validator: s => (s.Length > 0, "Cannot be empty")
);

if (result.Ok)
    Console.WriteLine(result.Value);
```

---

### Pattern Input

**Attribute:** `[Input(label, pattern)]`  
**Raw API method:** `AskText(label, validator?, pattern?, charValidator?)`  
**Direct method:** `ReadPatternCopilot(pattern, isAllowed?)`

A pattern-constrained input where certain characters are fixed and `_` marks an editable slot. The cursor moves only between editable positions.

```csharp
// Date entry: dd/MM/yyyy
[Input("Birth date:", "__/__/____")]
public string BirthDate { get; set; }
```

The fixed characters (`/` in the example above) are shown to the user and cannot be overwritten.

**Keyboard controls:**

| Key | Action |
|---|---|
| Any printable character | Fill the current slot (if `[CharValidator]` allows it) |
| ← / → | Move between editable slots |
| Backspace | Clear the current or previous slot |
| Escape | Cancel (returns `null`) |
| Enter | Submit |

**Full example with char validator, validator, and converter:**

```csharp
[Input("Birth date:", "__/__/____", index: 3)]
[CharValidator(nameof(IsDigit))]
[Validator(nameof(ValidateDate))]
[Converter(nameof(ConvertDate))]
[Callback(nameof(PrintDate))]
public DateTime BirthDay { get; set; }

public bool IsDigit((int position, char c) t) => char.IsDigit(t.c);

public (bool ok, string errorMessage) ValidateDate(string s)
{
    bool ok = DateTime.TryParseExact(s, "dd/MM/yyyy", null, DateTimeStyles.None, out _);
    return (ok, ok ? null : "Not a valid date (dd/MM/yyyy expected)");
}

public DateTime ConvertDate(string s)
    => DateTime.TryParseExact(s, "dd/MM/yyyy", null, DateTimeStyles.None, out var d)
        ? d
        : DateTime.Now;

public void PrintDate(DateTime d)
    => Console.WriteLine($"Date: {d:D}");
```

---

### Generic Typed Input

**Raw API method:** `Ask<T>(label, pattern?, possibleValues?, validator?, converter?, dataSource?, charValidator?, condition?, isIndexed?, callbacks)`

The most flexible raw API method. It can handle any type `T` that has a `TypeConverter` from `string`, or for which you provide a `converter` function.

```csharp
// String
Result<string> name = prompt.Ask<string>("Name:");

// Integer with range validator
Result<int> age = prompt.Ask<int>(
    "Age:",
    validator: s => int.TryParse(s, out var v) && v is >= 0 and <= 120
        ? (true, null)
        : (false, "Age must be 0–120")
);

// Custom type with converter
Result<DateTime> date = prompt.Ask<DateTime>(
    "Date:",
    pattern: "____-__-__",
    charValidator: t => char.IsDigit(t.Item2),
    validator: s => (DateTime.TryParseExact(s, "yyyy-MM-dd", null, DateTimeStyles.None, out _), "Invalid date"),
    converter: s => DateTime.ParseExact(s, "yyyy-MM-dd", null)
);

// Select from inline values
Result<string> color = prompt.Ask<string>(
    "Color:",
    possibleValues: ["Red", "Green", "Blue"]
);

// Select from dynamic data source
Result<string> fruit = prompt.Ask<string>(
    "Fruit:",
    dataSource: () => FetchFruitsFromDatabase()
);

// With condition and callbacks
Result<string> nickname = prompt.Ask<string>(
    "Nickname:",
    condition: () => isAdmin,
    callbacks: [v => Console.WriteLine($"Nickname set to: {v}")]
);
```

**Parameter reference:**

| Parameter | Type | Description |
|---|---|---|
| `label` | `string` | Prompt text |
| `pattern` | `string` | Fixed-format pattern with `_` slots |
| `possibleValues` | `string[]` | Static list of choices (renders a select prompt) |
| `validator` | `Func<string,(bool ok, string errorMessage)>` | Validation function |
| `converter` | `Func<string, T>` | Converts the raw string to `T` |
| `dataSource` | `Func<string[]>` | Dynamic list of choices (renders a select prompt) |
| `charValidator` | `Predicate<(int position, char c)>` | Per-character filter for pattern inputs |
| `condition` | `Func<bool>` | If `false`, the prompt is skipped and `IsApplicable = false` |
| `isIndexed` | `bool` | Enables number-key selection in select prompts |
| `callbacks` | `Action<T>[]` | Zero or more callbacks called after a valid value is entered |

**Return value:** `Result<T>`

| Property | Description |
|---|---|
| `Value` | The confirmed, converted value |
| `Ok` | `true` if a valid value was entered |
| `IsApplicable` | `false` if `condition` returned `false` (prompt was skipped) |

---

## Behavioral Attributes

Behavioral attributes are placed on form properties alongside input attributes. They add validation, transformation, conditional display, and post-input actions.

### Validator

**Attribute:** `[Validator(methodName)]`

Defines a validation method called after the user submits a value. Only one validator is allowed per property.

**Method signature:** `(bool ok, string errorMessage) MethodName(string value)`

```csharp
[Input("Username:")]
[Validator(nameof(ValidateUsername))]
public string Username { get; set; }

public (bool ok, string errorMessage) ValidateUsername(string v)
    => (v.Length >= 3, "Username must be at least 3 characters");
```

- If `ok` is `false`, the error message is displayed and the user is re-prompted.
- If `errorMessage` is `null` or empty when `ok` is `false`, the form's default `invalidInputMessage` is used.

---

### Converter

**Attribute:** `[Converter(methodName)]`

Defines a conversion method that transforms the raw input string into the property's type. This is necessary for types that have no built-in `TypeConverter`, such as custom structs or complex objects.

**Method signature:** `T MethodName(string value)` where `T` is the property type.

```csharp
[Input("Date (yyyy-MM-dd):", "____-__-__")]
[Validator(nameof(ValidateDate))]
[Converter(nameof(ParseDate))]
public DateTime BirthDay { get; set; }

public (bool ok, string errorMessage) ValidateDate(string s)
    => (DateTime.TryParseExact(s, "yyyy-MM-dd", null, DateTimeStyles.None, out _), "Invalid date");

public DateTime ParseDate(string s)
    => DateTime.ParseExact(s, "yyyy-MM-dd", null);
```

> **Note:** The converter is called only after validation succeeds. An exception from a converter is treated as invalid input.

---

### Condition

**Attribute:** `[Condition(methodName)]`

A gate that determines whether the prompt is shown at all. If the condition returns `false`, the property is not prompted and retains its default value.

**Method signature:** `bool MethodName()`

```csharp
[Input("Anonymous login?")]
public bool IsAnonymous { get; set; }

[Input("Login:")]
[Condition(nameof(RequiresLogin))]
public string Login { get; set; }

[Password("Password:")]
[Condition(nameof(RequiresLogin))]
public string Password { get; set; }

public bool RequiresLogin() => !IsAnonymous;
```

Because form fields are filled in order, the condition method can safely read properties that were already prompted (e.g., `IsAnonymous` is filled before `Login` is considered).

---

### Callback

**Attribute:** `[Callback(methodName)]`

Defines a method called immediately after a valid value is entered and converted. Multiple callbacks may be attached to the same property.

**Method signature:** `void MethodName(T value)` where `T` is the property type.

```csharp
[Input("Score:")]
[Callback(nameof(OnScoreEntered))]
[Callback(nameof(LogScore))]
public int Score { get; set; }

public void OnScoreEntered(int score)
    => Console.WriteLine($"New high score: {score}");

public void LogScore(int score)
    => _logger.Log(score);
```

Callbacks run in the order they are declared. They are called with the already-converted value, not the raw string.

---

### CharValidator

**Attribute:** `[CharValidator(methodName)]`

Filters which characters the user is allowed to type. Only meaningful for `[Input]` with a `pattern`. If a character is rejected, it is silently ignored (the cursor does not advance).

**Method signature:** `bool MethodName((int position, char c) t)`

- `position` — the zero-based index of the editable slot being filled
- `c` — the character the user just pressed

```csharp
[Input("Postal code:", "__-____")]
[CharValidator(nameof(OnlyDigits))]
public string PostalCode { get; set; }

public bool OnlyDigits((int position, char c) t) => char.IsDigit(t.c);
```

You can also vary the rule by position:

```csharp
public bool AlphanumericByZone((int position, char c) t) =>
    t.position < 2
        ? char.IsLetter(t.c)   // first two slots: letters only
        : char.IsDigit(t.c);   // remaining slots: digits only
```

---

### DataSource

**Attribute:** `[DataSource(methodName)]`

Provides dynamic choices for a select prompt at runtime. The method is called each time the prompt is rendered, so it can return different values based on current state.

**Method signature:** `string[] MethodName()`

```csharp
[Input("Category:")]
[DataSource(nameof(GetCategories))]
public string Category { get; set; }

public string[] GetCategories() => _categoryRepository.GetAll();
```

If both `possibleValues` (in `[Select]`) and `[DataSource]` are applicable, `[DataSource]` takes precedence at runtime when the static list is empty or has fewer than 2 entries.

---

### Indexed

**Attribute:** `[Indexed]`

Enables numeric key selection in select prompts. When applied, each choice is prefixed with its one-based index, and the user can press the corresponding digit key to select it instantly (no arrow-key navigation required). Only works when there are fewer than 10 choices.

```csharp
[Input("Action:")]
[DataSource(nameof(GetActions))]
[Indexed]
public string Action { get; set; }

public string[] GetActions() => ["Start", "Resume", "Options", "Quit"];
```

Displayed as:
```
  1. Start
  2. Resume
  3. Options
  4. Quit
```

---

## Input Ordering

By default, inputs are prompted in the order their properties appear in the class. You can override this with the `index` parameter available on `[Input]`, `[Password]`, and `[TextArea]`.

```csharp
[Form]
public partial class OrderedForm
{
    [Input("Last name:", index: 2)]
    public string LastName { get; set; }

    [Input("First name:", index: 1)]
    public string FirstName { get; set; }

    [Input("Email:", index: 3)]
    public string Email { get; set; }
}
// Prompt order: FirstName → LastName → Email
```

- Properties with the same index are prompted in declaration order relative to each other.
- Properties with `index: -1` (the default) are placed at the end, after all explicitly indexed ones.

---

## Raw Prompt API

The `Prompt` class can be used directly without the source generator. This is useful when your form structure is dynamic or when you need fine-grained control.

```csharp
// Create a Prompt (optionally with a custom error message)
Prompt prompt = new Prompt(invalidInputMessage: "Bad input, try again.");

// Text
string name = prompt.AskText("Name:");

// Integer
int age = prompt.AskInt("Age:");

// Double (invariant culture — use '.' as decimal separator)
double price = prompt.AskDouble("Price:");

// Boolean (checkbox toggle)
// bool agree = ???  — use Ask<bool> or AskBool instead

// Boolean with text values
bool agree = prompt.AskBool("Agree?", trueValues: ["yes", "y"], falseValues: ["no", "n"]);

// Password
Result<string> pwd = prompt.AskPassword("Password:", hiddenChar: '*');
if (pwd.Ok) Console.WriteLine("Password accepted");

// Select
string? color = prompt.Select("Color:", choices: ["Red", "Green", "Blue"]);
if (color != null) Console.WriteLine($"You chose {color}");

// TextArea
Result<string> notes = prompt.AskMultiLineText("Notes:", maxLines: 5);
if (notes.Ok) Console.WriteLine(notes.Value);

// Generic typed input
Result<int> score = prompt.Ask<int>(
    "Score:",
    validator: s => int.TryParse(s, out var v) && v >= 0 ? (true, null) : (false, "Must be ≥ 0")
);
if (score.Ok) Console.WriteLine($"Score: {score.Value}");

// Ask all inputs on a Form class (reflection-based, without source generator)
MyForm data = prompt.AskForm<MyForm>();
```

### Result&lt;T&gt;

Methods that support `condition` and `callbacks` return `Result<T>`:

| Property | Type | Description |
|---|---|---|
| `Value` | `T` | The entered and converted value |
| `Ok` | `bool` | `true` when a valid value was provided |
| `IsApplicable` | `bool` | `false` when the prompt was skipped due to a condition |

---

## Complete Example

The following example combines most input types into a single form:

```csharp
using System.Globalization;
using interactiveCLI.forms;

[Form("\x1b[1;31mInvalid input.\x1b[0m")]
public partial class RegistrationForm
{
    // --- Boolean checkbox ---
    [Input("Anonymous registration?", index: 0)]
    public bool IsAnonymous { get; set; }

    // --- Conditional text inputs ---
    [Input("First name:", index: 1)]
    [Condition(nameof(RequiresIdentity))]
    public string FirstName { get; set; }

    [Input("Last name:", index: 2)]
    [Condition(nameof(RequiresIdentity))]
    public string LastName { get; set; }

    // --- Password input ---
    [Password("Password:", hiddenChar: '*', index: 3)]
    [Validator(nameof(ValidatePassword))]
    [Callback(nameof(OnPasswordSet))]
    public string Password { get; set; }

    // --- Integer input ---
    [Input("Age:", index: 4)]
    public int Age { get; set; }

    // --- Double input ---
    [Input("Height (m):", index: 5)]
    [Validator(nameof(ValidateHeight))]
    public double Height { get; set; }

    // --- Pattern input for date ---
    [Input("Birth date (dd/MM/yyyy):", "__/__/____", index: 6)]
    [CharValidator(nameof(DigitsOnly))]
    [Validator(nameof(ValidateBirthDate))]
    [Converter(nameof(ParseBirthDate))]
    [Callback(nameof(PrintBirthDate))]
    public DateTime BirthDate { get; set; }

    // --- Select from dynamic list ---
    [Input("Country:", index: 7)]
    [DataSource(nameof(GetCountries))]
    public string Country { get; set; }

    // --- Select with number keys ---
    [Input("Membership tier:", index: 8)]
    [DataSource(nameof(GetTiers))]
    [Indexed]
    public string MembershipTier { get; set; }

    // --- Multi-line textarea ---
    [TextArea("Bio (Ctrl+Enter to submit):", maxLines: 5, index: 9)]
    [Validator(nameof(ValidateBio))]
    public string Bio { get; set; }

    // --- Conditions ---
    public bool RequiresIdentity() => !IsAnonymous;

    // --- Validators ---
    public (bool ok, string errorMessage) ValidatePassword(string pw)
        => (pw.Length >= 8, "Password must be at least 8 characters");

    public (bool ok, string errorMessage) ValidateHeight(string s)
        => double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) && v is > 0 and < 3
            ? (true, null)
            : (false, "Height must be between 0 and 3 metres");

    public (bool ok, string errorMessage) ValidateBirthDate(string s)
        => (DateTime.TryParseExact(s, "dd/MM/yyyy", null, DateTimeStyles.None, out _),
            "Enter a valid date in dd/MM/yyyy format");

    public (bool ok, string errorMessage) ValidateBio(string s)
        => (s.Length <= 500, "Bio must be 500 characters or fewer");

    // --- Converters ---
    public DateTime ParseBirthDate(string s)
        => DateTime.ParseExact(s, "dd/MM/yyyy", null);

    // --- CharValidators ---
    public bool DigitsOnly((int position, char c) t) => char.IsDigit(t.c);

    // --- Callbacks ---
    public void OnPasswordSet(string pw)
        => Console.WriteLine("Password set successfully.");

    public void PrintBirthDate(DateTime d)
        => Console.WriteLine($"Birth date: {d:D}");

    // --- DataSources ---
    public string[] GetCountries() => ["France", "Germany", "Japan", "United States"];
    public string[] GetTiers()     => ["Free", "Standard", "Premium"];
}
```

**Usage:**

```csharp
var form = new RegistrationForm();
form.Ask();
Console.WriteLine($"Welcome, {form.FirstName} {form.LastName}!");
```
