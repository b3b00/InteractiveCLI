InteractiveCLI is a component that helps building interactive console applications. 
It can be use 2 ways :
 - as a source generator
 - using class and method calls.

## source generator

how to use :

```xml
<packagereference Include="interactiveCLI" version="1.1.0"/>
```

The source generator looks for POCO classes tagged with the `[Form]` attribute. 
Members of the Form class are tagged with attributes that declare them as inputs :
- `[Input]`
- `[Select]`
- `[Password]`
- `[TextArea]`

Then the form can be called using the generated `Ask()` method.
For example a real simple login form could be
```csharp
using interactiveCLI.forms;

namespace TestProject;

[Form]
public partial class LoginForm
{    
    
    [Input("Login : ")]    
    public string Login { get; set; }
    
    [Password("password : ")]    
    public string Password { get; set; }
}
```

### validations and callbacks

Inputs can be added various behavioral attributes:
- `[Condition("myCondition")]` : defines a Predicate to be used before asking for the input. `myCondition`must be a method of the `Form`class. Only 1 condition can be define for an input. ;
- `[Callback("myCallback"]` : defines a method to be called after the input has been entered. `myCallback`muts be a method of the `Form`class. many callback may be used for an input;
- `[Validator("myValidator")]` : defines a method to be called to validate the value. the method mus return a `(bool ok, string errorMessage)` tuple here òk`states if the value is ok and `errorMessage` is an error message to be displayed. `myValidator`muts be a method of the `Form`class. Only 1 validator can be defined for an input.

Keeping the LoginForm we can define the following behavioral changes : 

```csharp
using interactiveCLI.forms;

namespace TestProject;

[Form]
public partial class LoginForm
{    
    
    [Input("anonymous login ? ")]
public bool Anonymous {get; set;}

[Input("Login : ")]
[Condition(nameof(IsNotAnonymous))] // do not ask if login anonymously
[Callback(nameof(SayHello))] // say hello right after login is entered
public string Login { get; set; }

[Password("password : ")]
[Condition(nameof(IsNotAnonymous))]  // do not ask if login anonymously
[Callback(nameof(CompromisePassword))] // display a message stating that the password is compromised
[Validator(nameof(ValidatePassword))] // validate the password : password=login
public string Password { get; set; }

public bool IsNotAnonymous() =>  !this.Anonymous;

public (bool ok, string errorMessage) ValidatePassword(string password)
{
    if (password == Login)
    {
        return (true, string.Empty);
    }
    else
    {
        return (false, $"ERROR ! Invalid password: {password}");
    }
}


public void CompromisePassword(string password)
{
    Console.WriteLine($"your {password} password is now compromised");
}

public void SayHello(string login)
{
    Console.WriteLine($"Hello {login}!");
}
}
```

### input types

#### basic input

#### password

#### select

#### testarea

