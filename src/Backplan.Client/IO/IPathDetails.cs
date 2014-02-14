using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemWrapper.IO;

namespace Backplan.Client.IO
{
    public interface IPathDetails
    {
        IEnumerable<string> GetFilesInPath(string path);
        bool IsDirectory(string path);
        IFileInfoWrap GetFileInfo(string path);
    }
}
