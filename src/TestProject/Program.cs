using interactiveCLI;

namespace TestProject;

public class Program
{

    public static void Test()
    {
        TestForm form = new TestForm();

        form.Ask();
        
        Console.WriteLine(form.ToString());
    }

    public static void Login()
    {
        LoginForm form = new LoginForm();

        form.Ask();
        
        Console.WriteLine($"anon ? {form.Anonymous} || login={form.Login}, password={form.Password}");
    }
    public static void Main(string[] args)
    {
        Prompt prompt = new Prompt();
        //var BirthDayResult = prompt.Ask<DateTime>("date :", pattern: "____-__-__", possibleValues: null, validator: (string s) => (true,null), converter: (string s) => DateTime.Now, dataSource: null, charValidator: ((int position, char c) s) => Char.IsDigit(s.c), condition: null, callbacks: (string s) => DisplayDate(s));
       

        Selector selector = new Selector();
        selector.Ask();
        switch (selector.Option)
        {
            case "test":
                {
                    Test();
                    break;
                }
            case "login":
                {
                    Login();
                    break;
                }
            case "multi":
                {
                    Multi form = new Multi();
                    form.Ask();
                    Console.WriteLine(form.ToString());
                    break;
                }
            case "quit":
                {
                    Console.WriteLine("Goodbye !");
                    break;
                }
            default:
                {
                    Console.WriteLine($"I don't know what to do with {selector.Option}....");
                    break;
                }
        }
    }
}