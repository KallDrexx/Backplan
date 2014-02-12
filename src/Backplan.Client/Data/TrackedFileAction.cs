using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backplan.Client.Data
{
    public class TrackedFileAction
    {
        public string FileName { get; set; }
        public string Path { get; set; }
        public string Hash { get; set; }
        public FileActions Action { get; set; }
        public DateTime Date { get; set; }
    }
}
