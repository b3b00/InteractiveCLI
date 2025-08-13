// See https://aka.ms/new-console-template for more information

using System.Globalization;
using interactiveCLI;
using interactiveCLI.forms;


[Form]
public class MyForm
{
    [Password("password :")]
    public string Password { get; set; }
        
    [Input("name :")]
    public string Name { get; set; }
        
    [Input("age :")]
    public int Age { get; set; }
        
    [Input("salary :")]
    public double Salary { get; set; }

    [BoolInput("Ok ?", ["y"], ["n"])]
    public bool Ok {get; set;}
}

public class Program
{
 

    public static void TestForm()
    {
        var prompt = new Prompt();
        var myForm = prompt.AskForm<MyForm>();
        // FormBuilder<MyForm> formBuilder = new FormBuilder<MyForm>();
        // var form = formBuilder.Build(new MyForm(),prompt);
        // MyForm myForm = form.Ask();
        Console.WriteLine($"password : {myForm.Password}");
        Console.WriteLine($"name : {myForm.Name}");
        Console.WriteLine($"age : {myForm.Age}");
        Console.WriteLine($"salary : {myForm.Salary}");
        Console.WriteLine($"Ok ? : {myForm.Ok}");
        

    }

    public static void reading()
    {
        var key = Console.ReadKey();
        while (key.Key != ConsoleKey.Escape)
        {
            Console.WriteLine(key.Key);
            key = Console.ReadKey();
        }
        Console.WriteLine("done reading. ");
    }
    
    public static void Main(string[] args)
    {
        //reading();
        Console.WriteLine("\x1b[1mTEST\x1b[0m");
        Prompt prompter = new Prompt();
        Predicate<string> validateDate = s =>
        {
            var test = DateTime.TryParseExact(s, "dd/MM/yyyy", null, DateTimeStyles.None, out var d);
            return test;
        };
        var d = prompter.AskText("with pattern", validator:validateDate, pattern:"__/__/____", ((int position, char c) t) => char.IsDigit(t.c));
        Console.WriteLine("with pattern :: "+d);
        
        TestForm();
        /*
        Prompt prompter = new Prompt();
        var name = prompter.AskText("What's your name ?",(s) => !s.Equals("bill", StringComparison.InvariantCultureIgnoreCase));
        var age = prompter.AskInt("How old are you ?");
        var happy = prompter.AskBool("Are you happy ?",new []{"y","Y","o","O"}, new []{"n","N"}, s => s != "n" && s != "N");
        var tall = prompter.AskDouble("How tall are you ?");

        Console.WriteLine($"So your name is {name}. You're {age} years old. And you are {(happy ? "": "not ")}happy");
        Console.WriteLine($"And you pretend being {tall} cm tall. Is this real ?");

        Func<string, bool, string> formatter = (value, selected) => {
            if (selected) {
                return $"\x1b[1;32m > {value}\x1b[0m";
            }
            return value+"   ";
        };

        var choice = prompter.Select("choose :", formatter:formatter,
        "choice 1", "choice 2", "choice 3", "choice 4", "choice 5");
        if (choice != null)
            Console.WriteLine($" your choice : {choice}");
        else
            Console.WriteLine("no choice");*/
    }
}