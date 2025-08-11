using Spectre.Console;

public static class Program
{
    public static void Main(string[] args)
    {
        // Ask the user to confirm
        var confirmation = AnsiConsole.Prompt(
            new TextPrompt<bool>("Run prompt example?")
                .AddChoice(true)
                .AddChoice(false)
                .DefaultValue(true)
                .WithConverter(choice => choice ? "y" : "n"));

        // Echo the confirmation back to the terminal
        Console.WriteLine(confirmation ? "Confirmed" : "Declined");

        // Ask the user a couple of simple questions
        var name = AnsiConsole.Prompt(
            new TextPrompt<string>("What's your name?"));
        var age = AnsiConsole.Prompt(
            new TextPrompt<int>("What's your age?"));

        // Echo the name and age back to the terminal
        AnsiConsole.WriteLine($"So you're {name} and you're {age} years old");

        // Ask for the user's favorite fruit
        var fruit = AnsiConsole.Prompt(
            new TextPrompt<string>("What's your favorite fruit?")
                .AddChoices(["Apple", "Banana", "Orange"])
                .DefaultValue("Orange"));

        // Echo the fruit back to the terminal
        Console.WriteLine($"I agree. {fruit} is tasty!");

        // Ask for the user's favorite fruit
        fruit = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("What's your [green]favorite fruit[/]?")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more fruits)[/]")
                .AddChoices(new[] {
                    "Apple", "Apricot", "Avocado",
                    "Banana", "Blackcurrant", "Blueberry",
                    "Cherry", "Cloudberry", "Cocunut",
                }));

        // Echo the fruit back to the terminal
        AnsiConsole.WriteLine($"I agree. {fruit} is tasty!");

        var size = AnsiConsole.Ask<int>("how tall are you ?");
        Console.WriteLine($"{size} is quite tall !");

    }
}
