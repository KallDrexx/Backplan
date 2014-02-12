using System;

namespace Backplan.Client.Utilities
{
    public interface IHashUtils
    {
        string GenerateHash(System.IO.Stream stream);
    }
}
