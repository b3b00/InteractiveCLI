using interactiveCLI.forms;

namespace TestProject;

[Form]
public partial class Autocomp
{
    [Input("select a test : ",index : 0  )]
    [DataSource(nameof(Countries))]
    [Indexed]6
    [Autocomplete]
    public string Country { get; set; }


    public string[] Countries() => new string[] {
        "France","Feroe","Belgium","Belarus","Germany","Spain","Italy","Swiss","Nederland","Luxembourg", "Austria","Poland","Greece"
    };
}