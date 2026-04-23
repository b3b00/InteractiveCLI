
using interactiveCLI;
using interactiveCLI.forms;

namespace PromptTests;
 


public partial class GenBoolForm {
    
   public void Ask(Prompt prompt = null) {
   
        prompt ??= new Prompt(null);
        
//
// field Agree
//

    var AgreeResult = prompt.Ask<bool>("Agree",
        pattern:null,
        possibleValues:null,
        validator:(string s) => ValidateAgree(s),
        converter:(string s) => ConvertAgree(s),
        dataSource:null,
        charValidator:null,
        condition:null,
        isIndexed:false,
        callbacks:null
        );
    if (AgreeResult.Ok && AgreeResult.IsApplicable) {
        Agree = AgreeResult.Value;
    }

   }
}
