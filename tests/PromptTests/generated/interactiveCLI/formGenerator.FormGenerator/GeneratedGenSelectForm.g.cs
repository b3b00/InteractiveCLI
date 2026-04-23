
using interactiveCLI;
using interactiveCLI.forms;

namespace PromptTests;
 


public partial class GenSelectForm {
    
   public void Ask(Prompt prompt = null) {
   
        prompt ??= new Prompt(null);
        
//
// field Colour
//

    var ColourResult = prompt.Ask<string>("Colour",
        pattern:null,
        possibleValues:null,
        validator:null,
        converter:null,
        dataSource:() => ColourSource(),
        charValidator:null,
        condition:null,
        isIndexed:false,
        callbacks:null
        );
    if (ColourResult.Ok && ColourResult.IsApplicable) {
        Colour = ColourResult.Value;
    }

   }
}
