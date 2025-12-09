using System.Globalization;
using System.Text;
using interactiveCLI.forms;

namespace TestProject;

[Form("\x1b[1;31mInvalid input.\x1b[0m")]
public partial class TestForm
{
    [Input("check bool",index:0)]
    public bool CheckBool { get; set; }
    
    [Input("check Boolean ",index:1)]
    public Boolean CheckBoolean { get; set; }

    [Input("ok  (yes|y /no|n) ? ",index:2)]
    [Validator(nameof(YesOrNoValidation))]
    [Converter(nameof(YesOrNoConverter))]
    public Boolean YesOrNo { get; set; }

    public (bool ok, string errorMessage) YesOrNoValidation(string v)
    {
        var ok = v == "yes" || v == "no" || v == "y" || v == "n";
        return (ok, ok ? null : "this is not yes or no ! make a choice !!!");
    }

    [Password("password : ", hiddenChar:'%',index:3)]
    [Callback(nameof(LeakPassword))]
    public string Password { get; set; }

    public void LeakPassword(string v)
    {
        Console.WriteLine($"your password is {v}");
    }
    
    public bool YesOrNoConverter(string v) => v == "yes" || v == "y" ;


    [Input("Nombre : ")] public double Number { get; set; }

    [Input("select a fruit :", index: 10)]
    [DataSource(nameof(SelectMeDataSource))]
    public string SelectMe { get; set; }
    


    public string[] SelectMeDataSource() => ["Orange", "Apple", "Banana", "Apricot"];

    [Input("nom : ", index: 10)] public string Name { get; set; }

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

    public override string ToString()
    {
        StringBuilder b = new StringBuilder();
        b.Append("number = ").AppendLine(Number.ToString())
            .Append("name : ").AppendLine(Name)
            .Append("check = ").AppendLine(CheckBool.ToString())
            .Append("check Boolean = ").AppendLine(CheckBoolean.ToString())
            .Append("ok ? = ").AppendLine(YesOrNo.ToString())
            .Append("Password : ").AppendLine(Password)
            .Append("fruit = ").AppendLine(SelectMe)
            .Append("birth = ").AppendLine(BirthDay.ToString("f"));
        return b.ToString();
    }
}