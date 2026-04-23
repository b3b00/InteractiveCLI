
using interactiveCLI;
using interactiveCLI.forms;

namespace PromptTests;
 


public partial class GenMixedTypesForm {
    
   public void Ask(Prompt prompt = null) {
   
        prompt ??= new Prompt(null);
        
//
// field Username
//

    var UsernameResult = prompt.Ask<string>("Username",
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
    if (UsernameResult.Ok && UsernameResult.IsApplicable) {
        Username = UsernameResult.Value;
    }

//
// field Age
//

    var AgeResult = prompt.Ask<int>("Age",
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
    if (AgeResult.Ok && AgeResult.IsApplicable) {
        Age = AgeResult.Value;
    }

//
// field Score
//

    var ScoreResult = prompt.Ask<double>("Score",
        pattern:null,
        possibleValues:null,
        validator:(string s) => ValidateScore(s),
        converter:(string s) => ParseScore(s),
        dataSource:null,
        charValidator:null,
        condition:null,
        isIndexed:false,
        callbacks:null
        );
    if (ScoreResult.Ok && ScoreResult.IsApplicable) {
        Score = ScoreResult.Value;
    }

   }
}
