using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace ViennaDotNet.Common.Utils
{
    public static class IOExtenions
    {
        public static bool CanRead(this DirectoryInfo dirInfo)
        {
            // TODO: implement
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                return true;

            return true;
        }
    }
}
