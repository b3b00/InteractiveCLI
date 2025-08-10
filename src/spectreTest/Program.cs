using Spectre.Console;

public static class Program
{
        public static void Main(string[] args)
                {
                            AnsiConsole.Markup("[underline red]Hello[/] World!");
                            var confirmation = AnsiConsole.Prompt(
                                        new TextPrompt<bool>("Run prompt example?")
                                                .AddChoice(true)
                                                        .AddChoice(false)
                                                                .DefaultValue(true)
                                                                        .WithConverter(choice => choice ? "y" : "n"));

                            // Echo the confirmation back to the terminal
                            //Console.WriteLine(confirmation ? "Confirmed" :
//                             "Declined");
                            //a
                            // Ask the user to confirm
                            // var confirmation = AnsiConsole.Prompt(
                            //     new ConfirmationPrompt("Run prompt
                            //     example?"));
                            //
                            //     // Echo the confirmation back to the terminal
                            //     Console.WriteLine(confirmation ? "Confirmed"
                            //     : "Declined");
                            //
                            // Ask for the user's favorite fruit
                            // var fruit = AnsiConsole.Prompt(
                            //     new SelectionPrompt<string>()
                            //             .Title("What's your [green]favorite
                            //             fruit[/]?")
                            //                     .PageSize(10)
                            //                             .MoreChoicesText("[grey](Move
                            //                             up and down to reveal
                            //                             more fruits)[/]")
                            //                                     .AddChoices(new[]
                            //                                     {
                            //                                                 "Apple",
                            //                                                 "Apricot",
                            //                                                 "Avocado", 
                            //                                                             "Banana",
                            //                                                             "Blackcurrant",
                            //                                                             "Blueberry",
                            //                                                                         "Cherry",
                            //                                                                         "Cloudberry",
                            //                                                                         "Cocunut",
                            //                                                                                 }));
                            //
                            //                                                                                 //
                            //                                                                                 Echo
                            //                                                                                 the
                            //                                                                                 fruit
                            //                                                                                 back
                            //                                                                                 to
                            //                                                                                 the
                            //                                                                                 terminal
                            //                                                                                 AnsiConsole.WriteLine($"I
                            //                                                                                 agree.
                            //                                                                                 {fruit}
                            //                                                                                 is
                            //                                                                                 tasty!");

                                }
}
