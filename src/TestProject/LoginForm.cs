using System.Diagnostics;
using interactiveCLI.forms;

namespace TestProject;

[Form]
public partial class LoginForm
{
    [Input("anonymous login ? ")]
    public bool Anonymous {get; set;}
    
    [Input("Login : ")]
    [Condition("IsNotAnonymous")]
    public string Login { get; set; }
    
    [Password("password : ")]
    [Condition("IsNotAnonymous")]
    [Validator(nameof(ValidatePassword))]
    public string Password { get; set; }
    
    public bool IsNotAnonymous() =>  !this.Anonymous;

    public bool ValidatePassword(string password) => password == Login;

}