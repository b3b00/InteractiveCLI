using System.Diagnostics;
using interactiveCLI.forms;

namespace TestProject;

[Form]
public partial class LoginForm
{
    [Input("anonymous login ? ")]
    public bool Anonymous {get; set;}
    
    [Input("Login : ")]
    [Condition(nameof(IsNotAnonymous))]
    [Callback(nameof(SayHello))]
    public string Login { get; set; }
    
    [Password("password : ")]
    [Condition(nameof(IsNotAnonymous))]
    [Callback(nameof(CompromisePassword))]
    [Validator(nameof(ValidatePassword))]
    public string Password { get; set; }
    
    public bool IsNotAnonymous() =>  !this.Anonymous;

    public bool ValidatePassword(string password) => password == Login;

    public void CompromisePassword(string password)
    {
        Console.WriteLine($"your {password} password is now compromised");
    }

    public void SayHello(string login)
    {
        Console.WriteLine($"Hello {login}!");
    }

}