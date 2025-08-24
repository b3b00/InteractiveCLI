using System.Globalization;
using System.Text;
using interactiveCLI.forms;

namespace TestProject;

[Form("\x1b[1;31mInvalid input.\x1b[0m")]
public partial class TestForm
{
    [Input("bool (yes|y /no|n)")]
    [Validator(nameof(YesOrNoValidation))]
    [Converter(nameof(YesOrNoConverter))]
    public bool YesOrNo { get; set; }


    public bool YesOrNoValidation(string v) => v == "yes" || v == "no" || v == "y" || v == "n";


    public bool YesOrNoConverter(string v) => v == "yes";


    [Input("Nombre : ")] public double Number { get; set; }

    [Input("select me :", index: 1)]
    [DataSource(nameof(SelectMeDataSource))]
    public string SelectMe { get; set; }


    public string[] SelectMeDataSource() => ["Orange", "Apple", "Banana", "Apricot"];

    [Input("nom : ", index: 0)] public string Name { get; set; }

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


    bool ValidateDate(string s) => DateTime.TryParseExact(s, "dd/MM/yyyy", null, DateTimeStyles.None, out var d);


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
            .Append("yes/no = ").AppendLine(YesOrNo.ToString())
            .Append("fruit = ").AppendLine(SelectMe)
            .Append("birth = ").AppendLine(BirthDay.ToString("f"));
        return b.ToString();
    }
}