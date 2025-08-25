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
    
    public (bool ok,string errorMessage) YesOrNoValidation(string v)
    {
        var ok = v == "yes" || v == "no" || v == "y" || v == "n";
        return (ok, ok ? null : "this is not yes or no ! make a choice !!!");
    } 


    public bool YesOrNoConverter(string v) => v == "yes" || v == "y" ;


    [Input("Nombre : ")] public double Number { get; set; }

    [Input("select me :", index: 10)]
    [DataSource(nameof(SelectMeDataSource))]
    public string SelectMe { get; set; }
    


    public string[] SelectMeDataSource() => ["Orange", "Apple", "Banana", "Apricot"];

    [Input("nom : ", index: 10)] public string Name { get; set; }

    [Input("date :", "__/__/____", 3)]
    [CharValidator(nameof(IsCharValid))]
    [Validator(nameof(ValidateDate))]
    [Converter(nameof(ConvertDate))]
    DateTime BirthDay { get; set; }


    public bool IsCharValid((int position, char c) t)
    {
        var isDigit = char.IsDigit(t.c);
        return isDigit;
    }


    (bool ok, string errorMessage) ValidateDate(string s)
    {
        var ok = DateTime.TryParseExact(s, "dd/MM/yyyy", null, DateTimeStyles.None, out var d);
        return (ok, ok ? null : "this is not a valid date");
    }


    DateTime ConvertDate(string s)
    {
        if (DateTime.TryParseExact(s, "dd/MM/yyyy", null, DateTimeStyles.None, out var d))
        {
            return d;
        }

        return DateTime.Now;
    }

    public override string ToString()
    {
        StringBuilder b = new StringBuilder();
        b.Append("number = ").AppendLine(Number.ToString())
            .Append("name : ").AppendLine(Name)
            .Append("check = ").AppendLine(CheckBool.ToString())
            .Append("check Boolean = ").AppendLine(CheckBoolean.ToString())
            .Append("ok ? = ").AppendLine(YesOrNo.ToString())
            .Append("fruit = ").AppendLine(SelectMe)
            .Append("birth = ").AppendLine(BirthDay.ToString("f"));
        return b.ToString();
    }
}