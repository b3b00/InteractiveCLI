
using interactiveCLI;
using interactiveCLI.forms;

namespace PromptTests;
 


public partial class GenSimpleTextForm {
    
   public void Ask(Prompt prompt = null) {
   
        prompt ??= new Prompt(null);
        
//
// field Name
//

    var NameResult = prompt.Ask<string>("Name",
        pattern:null,
        possibleValues:null,
        validator:null,
        converter:null,
        dataSource:null,
        charValidator:null,
        condition:null,
        isIndexed:false,
        callbacks:null
        );
    if (NameResult.Ok && NameResult.IsApplicable) {
        Name = NameResult.Value;
    }

//
// field City
//

    var CityResult = prompt.Ask<string>("City",
        pattern:null,
        possibleValues:null,
        validator:null,
        converter:null,
        dataSource:null,
        charValidator:null,
        condition:null,
        isIndexed:false,
        callbacks:null
        );
    if (CityResult.Ok && CityResult.IsApplicable) {
        City = CityResult.Value;
    }

   }
}
