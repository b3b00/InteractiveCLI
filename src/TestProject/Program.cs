namespace TestProject;

public class Program
{
    public static void Main(string[] args)
    {
        TestForm form = new TestForm();
        
        //bar.Ask();
        //Console.WriteLine(bar);

        form.Ask();
        
        Console.WriteLine(form.ToString());
        
    }
}