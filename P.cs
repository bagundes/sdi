using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SDI
{
    public static class P
    {
        // v0001
        private static string LOG => "PARAM";
        #region Create Field - This fields will be used by method;
        //static System.IO.DirectoryInfo Create_Field1 => klib.dbase.DbClientEx.Parameter(R.Project.ID, "FileOut", "WASP", null, null, @"C:\Teamsoft\temp\").ToDirectory();
        #endregion
        #region Properties - Access directly the values
        //public static string FTPServer => klib.dbase.DbClientEx.Parameter(R.Project.ID, "FTP", "Server", null, null, "integration.intellibrand.net").ToString();
        //public static klib.model.Credentials1 FTPCredentials => new klib.model.Credentials1(
        //    klib.dbase.DbClientEx.Parameter(R.Project.ID, "FTP", "User", null, null, "brandshapers").ToString(),
        //    klib.dbase.DbClientEx.Parameter(R.Project.ID, "FTP", "Password(key)", null, null, "bXScjhvxN2RK8fasq6AeAZyg7k6IYrSK").ToString());
        #endregion
        #region Cache - should be loading by method
        private static List<models.UDT_Tables> CacheTables = new List<models.UDT_Tables>();

        #endregion
        #region Methods
        /// <summary>
        /// Return Informations of table.
        /// </summary>
        /// <param name="table">table name or null</param>
        /// <param name="objtype">object type or null</param>
        /// <param name="formID">Form ID or null</param>
        /// <param name="cache">Load in cache</param>
        /// <returns></returns>
        public static models.UDT_Tables Tables(string table, string objtype, string formID, bool cache = false)
        {
            throw new NotImplementedException();
            //if(cache)
            //{
            //    if(CacheTables.Count > 0)
            //    {
            //        if(!String.IsNullOrEmpty(table))
            //            return CacheTables.Where(t => t.Code == table).FirstOrDefault();
            //        if (!String.IsNullOrEmpty(objtype))
            //            return CacheTables.Where(t => t.U_ObjType == objtype).FirstOrDefault();
            //        if (!String.IsNullOrEmpty(formID))
            //            return CacheTables.Where(t => t.U_FormDef == formID).FirstOrDefault();
            //    }
            //    else
            //    {
                    //CacheTables = SDI.RSEx.
                //}
            //}
        }
        //public static bool RuleFolder(string rule, bool cache = false)
        //{
        //    //0001
        //    var param = "[Param name]";
        //    var diff1 = rule;
        //    var diff2 = String.Empty;
        //    var diff3 = String.Empty;
        //    if (cache)
        //    {
        //        if (CacheRuleFolder.Count > 0)
        //        {
        //            return CacheRuleFolder.Where(t => t.Name == rule).Select(t => t).FirstOrDefault();
        //        }
        //        else
        //        {
        //            CacheOverwriteFile = klib.dbase.DbClientEx.ParameterList(R.Project.ID, param, diff1, diff2, diff3);
        //            if (CacheOverwriteFile.Count > 0)
        //                return OverwriteFile(rule, cache);
        //            else
        //                throw new LException(1, $"{param}.{rule}");
        //        }
        //    }
        //    else
        //    {
        //        return SDI.RSEx.Parameter(R.Project.ID, param, diff1, diff2, diff3).ToBool();
        //    }
        //}

        #endregion
        public static void CreateParameters()
        {
            var p = typeof(P);
            foreach (var proper in p.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                //klib.Shell.WriteLine(R.Project.ID, LOG, $"Preparing the {proper.Name} param");
                var name = proper.Name;
                try
                {
                    var foo = proper.GetValue(null, null);
                }
                catch (Exception lex)
                {
                    klib.Shell.WriteLine(R.Project.ID, LOG, lex);
                }
            }
        }
    }
}
