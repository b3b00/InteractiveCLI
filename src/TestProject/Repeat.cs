using interactiveCLI.forms;

namespace TestProject;

public class Item
{
    public Item(string kpi, int value)
    {
        Name = kpi;
        Value = value;
    }
    
    public string Name { get; set; }
    public int Value { get; set; }
    
    public override string ToString() => $"{Name} = {Value}";
}

[Form]
public partial class Repeat
{
    
    
    [Input("KPI :", index: 0)]
    [DataSource(nameof(KpiSource))]
    public string Kpi { get; set; }
    
    public string[] KpiSource() => ["DT_EVP","NB_BUL","NB_BUL_CALCULES","NB_BUL_CALCULER","NB_EDIT","NB_REMIS"];
    
    [Input("value : ", index:1)]
    public int Value { get; set; }
    
    public Item ToItem() => new Item(Kpi, Value);
    
    [Input("add (a) or send (s) ? ")]
    [Validator(nameof(MoreValidation))]
    [Converter(nameof(YesOrNoConverter))]
    public Boolean More { get; set; }
    
    public (bool ok, string errorMessage) MoreValidation(string v)
    {
        var ok = v == "add" || v == "send" || v == "a" || v == "s";
        return (ok, ok ? null : "this is not add or send ! make a choice !!!");
    }

    public bool YesOrNoConverter(string v) => v == "add" || v == "a"  ;

    
}