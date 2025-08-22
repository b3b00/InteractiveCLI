using System.Globalization;
using System.Text;
using interactiveCLI.forms;

namespace TestProject;


[Form]
public partial class TestForm
{

   [Input("bool (yes|y /no|n)")]
   public bool YesOrNo {get; set;}
   
   [Validator(nameof(YesOrNo))]
   public bool YesOrNoValidation(string v) => v == "yes" || v == "no" || v == "y" || v == "n";
   
   [Converter(nameof(YesOrNo))]
   public bool YesOrNoConverter(string v) => v == "yes" ;
   

   [Input("Nombre : ")]
   public double Number {get; set;}

   [Input("select me :")]
   public string SelectMe {get; set;} 

   [DataSource(nameof(SelectMe))]
   public string[] SelectMeDataSource() => ["Orange","Apple","Banana","Apricot"]; 
   
   [Input("nom : ")]
   public string Name {get; set;}
   
[Input("date :","__/__/____")]
DateTime BirthDay {get; set;}

[Validator(nameof(BirthDay))]
bool ValidateDate(string s) =>DateTime.TryParseExact(s, "dd/MM/yyyy", null, DateTimeStyles.None, out var d);

[Converter(nameof(BirthDay))]
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
        .Append("birth = ").AppendLine(BirthDay.ToString("dd/MM/yyyy"));
    return b.ToString();
}

}