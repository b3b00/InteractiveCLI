using System.Globalization;
using System.Text;
using interactiveCLI;
using interactiveCLI.forms;

namespace TestProject;


[Form]
public partial class Bar
{
    public void Main()
    {
        Console.WriteLine("Hello generator!");
    }

    [Input("hello")]
    public bool Hello {get; set;}

    [Input("good bye !")]
    public string GoodBye {get; set;}

    [Validator("Hello")]
    public bool IsValid(string value) => true;

    [Converter(nameof(Hello))]
    public bool Convert(string value) => value == "yes";

    [Input("select me :")]
    public string SelectMe {get; set;} 

    [DataSource(nameof(SelectMe))]
    public string[] SelectMeDataSource() => new string[]{"Orange","Apple","Banana","Apricot"}; 

    [Input("BirthDay", "__/__/____")]
    DateTime BirthDay {get; set;}

    [Validator(nameof(BirthDay))]
    bool ValidateDate(string s) =>DateTime.TryParseExact(s, "dd/MM/yyyy", null, DateTimeStyles.None, out var d);

    [Converter(nameof(BirthDay))]
    DateTime ConvertDate(string s) {{
        if (DateTime.TryParseExact(s, "dd/MM/yyyy", null, DateTimeStyles.None, out var d)) {{
            return d;
        }} ;
        return DateTime.Now;
    }}

    public override string ToString()
    {
        StringBuilder b = new StringBuilder();
        b.Append("hello = ").AppendLine(Hello.ToString())
            .Append("goodbye = ").AppendLine(GoodBye)
            .Append("fruit = ").AppendLine(SelectMe)
            .Append("birth = ").AppendLine(BirthDay.ToString("dd/MM/yyyy"));
        return b.ToString();
    }
 
}
public partial class Bar {
    
    public void Ask() {
   
        Prompt prompt = new Prompt();
        
//
// field Hello
//

        var helloResult = prompt.Ask<bool>("hello",pattern:null,possibleValues:null, validator:(string s) => IsValid(s),converter:(string s) => Convert(s),dataSource:null);
        if (helloResult.Ok) {
            Hello = helloResult.Value;
        }

//
// field GoodBye
//

        var goodByeResult = prompt.Ask<string>("good bye !",pattern:null,possibleValues:null, validator:null,converter:null,dataSource:null);
        if (goodByeResult.Ok) {
            GoodBye = goodByeResult.Value;
        }

//
// field SelectMe
//

        var selectMeResult = prompt.Ask<string>("select me :",pattern:null,possibleValues:null, validator:null,converter:null,dataSource:() => SelectMeDataSource());
        if (selectMeResult.Ok) {
            SelectMe = selectMeResult.Value;
        }

//
// field BirthDay
//

        var birthDayResult = prompt.Ask<DateTime>("BirthDay",pattern:"__/__/____",possibleValues:null, validator:(string s) => ValidateDate(s),converter:(string s) => ConvertDate(s),dataSource:null);
        if (birthDayResult.Ok) {
            BirthDay = birthDayResult.Value;
        }

    }
}

