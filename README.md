InteractiveCLI is a source generator that helps building interactive console applications. 

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

Each input can be given an index that defines the order they are prompted.

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

Inputs can be attached to various behavioral attributes:
- `[Condition("myCondition")]` : defines a `Predicate<string>` to be used before asking for the input. `myCondition`must be a method of the `Form`class. Only 1 condition can be define for an input. ;
- `[Callback("myCallback"]` : defines a method to be called after the input has been entered. It's an `Action<T>` where T is the input type. `myCallback`muts be a method of the `Form`class. many callback may be used for an input;
- `[Validator("myValidator")]` : defines a method to be called to validate the value. The method is a `Func<string,(bool ok, string errorMessage)>` that returns a `(bool ok, string errorMessage)` tuple where `ok` states if the value is ok and `errorMessage` is an error message to be displayed. `myValidator`muts be a method of the `Form`class. Only 1 validator can be defined for an input.

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

#### basic inputs

**char validators**

You can limit possible character using a `[Charvalidator]`. This is a `Predicate<(int position, char c)>` that returns true if the char is allowed. the parameter is a tuple where first element is the char position in the currently entered value and the second is the entered character.

**converters**

Inputs accept any type as long as you provide a way to convert the input string to the desired type. A converter is a `Func<string,T>` where T is the inpuut type. 


**pattern**
a A pattern can be define to frorce input to a specific format. a format uses `_` as place for any char. Other chars are constant. For instance we can define the date pattern `____-__-__` to enter the date `2025-01-01`.
Then the strig can be safely converted using a converter (see above)

Here is an example of a `DateTime`  input :

```csharp
 [Input("date :", "____-__-__", 3)]
 [CharValidator(nameof(IsCharValid))]  // limit chars to digits only
 [Validator(nameof(ValidateDate))] // validate the full date
 [Converter(nameof(ConvertDate))] // convert string to DateTime
 [Callback(nameof(DisplayDate))] // display the selected date
 DateTime BirthDay { get; set; }


 public bool IsCharValid((int position, char c) t)
 {
     var isDigit = char.IsDigit(t.c);
     return isDigit;
 }


 (bool ok, string errorMessage) ValidateDate(string s)
 {
     var ok = DateTime.TryParseExact(s, "yyyy-MM-dd", null, DateTimeStyles.None, out var d);
     return (ok, ok ? null : "this is not a valid date");
 }


 DateTime ConvertDate(string s)
 {
     if (DateTime.TryParseExact(s, "yyyy-MM-dd", null, DateTimeStyles.None, out var d))
     {
         return d;
     }
     return DateTime.Now;
 }

 void DisplayDate(DateTime date)     {
     Console.WriteLine($"you selected the date {date:f}");
 }
```

<img width="205" height="49" alt="{1B18A4C8-6470-468B-80E8-C3B1AD9BD7FD}" src="https://github.com/user-attachments/assets/95e09b91-cd15-4393-9c02-1a498554e891" />



#### password

```csharp
// hiddenChar parameter defines the character to be displayed as a mask. default is '*'
[Password("password : ", hiddenChar:'%',index:3)] 
[Callback(nameof(LeakPassword))] // display the password
public string Password { get; set; }

public void LeakPassword(string v)
{
    Console.WriteLine($"your password is {v}");
}
```

<img width="340" height="100" alt="{B41C1FD0-B07C-4944-BD43-EEBA81158E38}" src="https://github.com/user-attachments/assets/ca8a2546-216f-4d9d-a1aa-98a426fb15ec" />


#### select

Will display a list where to select a value. Values can be navigated using up and down arrows. Selection is done with `ENTER` key 

```csharp
 [Input("select a fruit :", index: 10)]
 [DataSource(nameof(SelectMeDataSource))] // defines a datasource method
 public string SelectMe { get; set; }
 
 public string[] SelectMeDataSource() => ["Orange", "Apple", "Banana", "Apricot"];
```
<img width="287" height="116" alt="{7D99464C-657F-455D-9EE7-4BBCA484C2DC}" src="https://github.com/user-attachments/assets/9332ea3f-d9f8-4643-a872-1d1c27363279" />


#### testarea

A textarea is similar to the HTML TextArea. It allows to enter multi line strings.
2 configuration options are available : 
 - maxLines : maximum line number (0 = no limit)
 - finishinKey : the `System.ConsoleKey` to be used together with ctrl key to validate the input. (default is `ENTER`)
  
example : a 5 lines text area validated by `ctrl+D`

```csharp
[TextArea("verbatim : ", index:2, maxLines:5, finishingKey : ConsoleKey.D)]
public string Verbatim {get; set;}
```

#### boolean

```csharp
[Input("do you agree ?", index:3)]
bool Agreement {get; set;} 
```
Similar to a checkbox. Default behavior shows ✅ and ❌ for reprensting (checked and not check values , respectively).

To use other checked/not checked values you can use a converter, together with a validator.

For example a check that use y/yes and n/no as checked / not checked values:

```csharp
[Input("ok  (yes|y /no|n) ? ",index:2)]
    [Validator(nameof(YesOrNoValidation))]
    [Converter(nameof(YesOrNoConverter))]
    public Boolean YesOrNo { get; set; }

    // do not accept anything else than y/yes or n/no
    public (bool ok, string errorMessage) YesOrNoValidation(string v)
    {
        var ok = v == "yes" || v == "no" || v == "y" || v == "n";
        return (ok, ok ? null : "this is not yes or no ! make a choice !!!");
    }

    // true if and only if equals to "y" or "yes"
    public bool YesOrNoConverter(string v) => v == "yes" || v == "y" ;
```
<img width="145" height="27" alt="{6CF84BC5-C2D2-4F4F-9C4B-BB375B0ECF92}" src="https://github.com/user-attachments/assets/09de50e8-8973-4998-a173-55de5eb2ce07" />


<img width="173" height="31" alt="{61DBD174-A31F-49A6-A5B2-4971C1228445}" src="https://github.com/user-attachments/assets/ca2fc4b6-f444-4104-8e3f-e95fec719c75" />


# Use raw API

If you don't want to use the source generator, but rather call prompting methods directly you can use the `Prompt`object.
This can be useful when your `Form`is not static.

```csharp

public void RawAPI() {
    // first create a Prompt
    Prompt prompt = new Prompt();

    // first ask for a string
    Result<string> name = prompt.Ask<string>("name :");
    if (name.Ok) {
        Console.WriteLine($"Hello {name} !");
    }
    else {
        Console.WriteLine($"invalid name : {name.Value}");
    }


    // then ask for an integer
    var age = prompt.Ask<int>("age :", validator: (string s) =>
    {
        if (int.TryParse(s, out int value))
        {
            if (value < 0 || value > 120)
            {
                return (false, "age must be between 0 and 120");
            }
            return (true, null);
        }
        return (false, "age must be an integer");
    });
    if (age.Ok)
    {
        Console.WriteLine($"You are {age} years old !");
    }
    else
    {
        Console.WriteLine($"invalid age : {age.Value}");
    }

    // ask for a selection
    var color = prompt.Ask<string>("favorite color :", possibleValues: new string[] { "red", "green", "blue" });
        if (color.Ok)
    {
        Console.WriteLine($"I agree that {color.Value} is a nice color !");
    }
    else
    {
        Console.WriteLine($"invalid color : {color.Value}");
    }
}

```

Methods for `Prompt`should be self explicative enough. 
