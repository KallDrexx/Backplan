using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backplan.Client.Models
{
    public class TrackedFile
    {
        public int Id { get; set; }
        public IEnumerable<TrackedFileAction> Actions { get; set; }
    }
}