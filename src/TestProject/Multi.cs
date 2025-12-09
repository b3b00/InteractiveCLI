using interactiveCLI.forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    [Form("\x1b[1;31mInvalid input.\x1b[0m")]
    public partial class Multi
    {
        //[Input("titre : ", index:1)]
        //public string Title { get; set; }

        [TextArea("code : ", index: 0, maxLines: 0, finishKey: ConsoleKey.D)]
        public string Text { get; set; }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            
                b.AppendLine($"\x1b[23m{Text}\x1b[0m");
            return b.ToString();
        }
    }
}
