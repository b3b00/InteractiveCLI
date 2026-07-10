using interactiveCLI.forms;

namespace TestProject;

[Form]
public partial class FilePickerForm
{
    [File("Select a file path: ")]
    public string FilePath { get; set; }
}
