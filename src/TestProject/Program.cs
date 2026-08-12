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
        // Enable UTF-8 encoding for emoji support
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Selector selector = new Selector();
        Multi formM = new Multi();
        //formM.Ask();
        //return;
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
            case "file_picker":
                {
                    var filePicker = new FilePickerForm();
                    filePicker.Ask();
                    Console.WriteLine($"Selected file: {filePicker.FilePath}");
                    break;
                }
            case "quit":
                {
                    Console.WriteLine("Goodbye !");
                    break;
                }
            case "autocomp":
            {
                var auto = new Autocomp();
                auto.Ask();
                Console.WriteLine(auto.Country);
                break;
            }
            case "repeat":
            {
                Repeat repeat = new Repeat();
                
                bool more = true;
                List<Item> items = new List<Item>();
                while (more)
                {
                    repeat.Ask();
                    items.Add(repeat.ToItem());
                    more = repeat.More;
                }

                foreach (var item in items)
                {
                    Console.WriteLine(item);
                }
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