namespace interactiveCLI.forms;

[AttributeUsage(AttributeTargets.Class,AllowMultiple = false)]
public class FormAttribute : Attribute
{

 public FormAttribute(string invalidInputMessage = null)
 {
  InvalidInputMessage = invalidInputMessage;
 }
 public string InvalidInputMessage { get; set; }   
}