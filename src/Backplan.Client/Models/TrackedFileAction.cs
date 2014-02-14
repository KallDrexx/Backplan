using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backplan.Client.Models
{
    public class TrackedFileAction
    {
        public int Id { get; set; } 
        public string FileName { get; set; }
        public string Path { get; set; }
        public DateTime FileLastModifiedDateUtc { get; set; }
        public long FileLength { get; set; }
        public FileActions Action { get; set; }
        public DateTime EffectiveDateUtc { get; set; }
    }
}
