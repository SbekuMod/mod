using System;
using System.IO;
using System.Reflection;

namespace SbekuMod.utils
{
    internal class UtilityHelper
    {

        public static string GetProjectBasePath()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

    }
}
