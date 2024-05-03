using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViennaDotNet.Common.Utils
{
    public static class ProcessExtensions
    {
        public static bool TryStopGracefully(this Process process)
        {
            try
            {
                nint mainWindowHandle = process.MainWindowHandle;
                if (mainWindowHandle != IntPtr.Zero)
                {
                    process.CloseMainWindow();
                    return true;
                }
            }
            catch { }

            process.Kill();
            return false;
        }
    }
}
