// See https://aka.ms/new-console-template for more information

using interactiveCLI;

public class Program
{
    public static void Main(string[] args)
    {
        Prompt prompter = new Prompt();
        var name = prompter.AskText("What's your name ?",(s) => !s.Equals("bill", StringComparison.InvariantCultureIgnoreCase));
        var age = prompter.AskInt("How old are you ?");
        var happy = prompter.AskBool("Are you happy ?",new []{"y","Y","o","O"}, new []{"n","N"}, s => s != "n" && s != "N");
        var tall = prompter.AskDouble("How tall are you ?");
        
        Console.WriteLine($"So your name is {name}. You're {age} years old. And you are {(happy ? "": "not ")}happy");
        Console.WriteLine($"And you pretend being {tall} cm tall. Is this real ?");

        var choice = Prompt.Select("choose :" , "choice 1", "choice 2", "choice 3", "choice 4", "choice 5" );
        if (choice != null)
            Console.WriteLine($" your choice : {choice}");
        else 
            Console.WriteLine("no choice");
    }
}