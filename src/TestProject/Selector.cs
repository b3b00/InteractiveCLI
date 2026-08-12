using interactiveCLI.forms;

namespace TestProject;

[Form]
public partial class Selector
{
    [Input("select a test : ",index : 0 )]
    [DataSource("options")]
    [Indexed]
    public string Option { get; set; }


    public string[] options() => new string[] {
        "test", "login", "multi","repeat", "file_picker", "autocomp", "quit"
    };
}


[Form]
public partial class Autocomp
{
    [Input("select a test : ",index : 0 )]
    [DataSource(nameof(Countries))]
    [Indexed]
    public string Country { get; set; }


    public string[] Countries() => new string[] {
        "France","Feroe","Belgium","Belarus","Germany","Spain","Italy","Swiss","Nederland","Luxembourg", "Austria","Poland","Greece"
    };
}