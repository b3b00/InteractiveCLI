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
