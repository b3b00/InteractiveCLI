
using interactiveCLI;
using interactiveCLI.forms;

namespace PromptTests;
 


public partial class GenPasswordForm {
    
   public void Ask(Prompt prompt = null) {
   
        prompt ??= new Prompt(null);
        
//
// field Secret
//


    var SecretResult = prompt.AskPassword("Secret",hiddenChar:'*', validator:null, condition:null, callbacks:null);
    if (SecretResult.Ok && SecretResult.IsApplicable) {
        Secret = SecretResult.Value;
    }

   }
}
