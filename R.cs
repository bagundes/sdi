using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace SDI
{
    public static class R
    {
        public static class Project
        {
            public static System.Reflection.Assembly assembly => System.Reflection.Assembly.GetExecutingAssembly();
            public static Version Version => assembly.GetName().Version;
            public const string Namespace = "SDI";
            public static int ID => Version.Major;
            public const string SP_PARAM = "KK_PARAM";
            public static string[] Resources => assembly.GetManifestResourceNames();
            public static ResourceManager LocationResx => new ResourceManager($"{typeof(R).Namespace}.content.Location{klib.R.Project.Language}", assembly);
        }
    }
}
