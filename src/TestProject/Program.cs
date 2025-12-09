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
            default:
                {
                    Console.WriteLine($"I don't know what to do with {selector.Option}....");
                    break;
                }
        }
    }
}