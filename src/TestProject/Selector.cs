using interactiveCLI.forms;

namespace TestProject;

[Form]
public partial class Selector
{
    [Input("select a test : ",index : 0 )]
    [DataSource("options")] 
    public string Option { get; set; }


    public string[] options() => new string[] {
        "test", "login", "multi","quit"
    };
}