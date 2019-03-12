using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SAPbobsCOM;

namespace SDI
{
    /// <summary>
    /// Service to execute one time.
    /// </summary>
    public static class Services
    {
        /// <summary>
        /// Attachement the file in SAP
        /// </summary>
        /// <param name="file">Attachement number</param>
        /// <returns></returns>
        public static int AttachFile(FileInfo file)
        {
            var oAtt = Conn.DI.GetBusinessObject(BoObjectTypes.oAttachments2) as Attachments2;
            oAtt.Lines.SourcePath = file.DirectoryName;
            oAtt.Lines.FileName = Path.GetFileNameWithoutExtension(file.Name);
            oAtt.Lines.FileExtension = (file.Extension.Substring(1));// to remove the dot

            if (oAtt.Add() != 0)
                throw new SDIException(7, file.Name, Conn.DI.GetLastErrorDescription());
            else
                return oAtt.AbsoluteEntry;
        }

        #region qmanager
        /// <summary>
        /// List of Querys saved in Query Manager
        /// </summary>
        /// <param name="name">It's possible use %</param>
        /// <returns></returns>
        public static List<string> QueriesManager(string name)
        {
            var sql = $"SELECT QName FROM OUQR WHERE Qname LIKE '{name}'";
            return klib.DB.ExtensionDb.Column<string>(sql);
        }

        /// <summary>
        /// Execute the query saved in query manager
        /// </summary>
        /// <param name="name">Query name</param>
        /// <param name="values">Parameter for query (if necessary)</param>
        /// <returns></returns>
        public static ResultSet QueryManagerRun(string name, params object[] values)
        {
            var sql = @"SELECT QString FROM OUQR WHERE Qname = {0}";
            var qstring = String.Empty;

            using (var rs = new ResultSet())
            {
                rs.DoQuery(sql, name);

                if (rs.Next())
                    qstring = rs.Field(0).ToString();
                else
                    throw new SDIException(6, name);
            }

            var rgx = new Regex(@"\[\%[0-9]\]");
            var foo = rgx.Matches(qstring);

            if (foo.Count != values.Length)
                throw new SDIException(2, $"Quantity of paramters in {name} isn't valid ({foo.Count}/{values.Length})");

            for (int i = 0; i < foo.Count; i++)
                qstring = qstring.Replace($"[%{i}]", "{" + i + "}");


            var rs1 = new ResultSet();
            rs1.DoQuery(sql, values);

            return rs1;
        }
        #endregion

        #region Autorizations
        public static bool IsAutorizedUser(string permissionId)
        {
            var username = Conn.DI.UserName;

            // In SAP 9.2 has a bug that allways return true. It's necessary to consult by query.
            // Old function
            //var oSBObob = (SAPbobsCOM.SBObob)SF.Conn.DI.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoBridge);
            //var retSAP = oSBObob.GetSystemPermission(SF.Conn.DI.UserName, permissionId);
            //return retSAP.Fields.Item(0).Value.ToString() == "1";

            var sql = $@"
SELECT   ""SUPERUSER"" 
FROM     ""OUSR"" 
WHERE    ""OUSR"".""USER_CODE"" = '{username}'
    AND  ""SUPERUSER"" = 'Y'";

            if (klib.DB.ExtensionDb.HasLines(sql))
                return true;

            sql = $@"
SELECT   
     CASE   WHEN USR3.""Permission"" = 'F' 
            THEN 1
		    ELSE 0 END  AS ""Permission""
FROM     ""USR3"" 
INNER JOIN ""OUSR"" 
    ON   ""USR3"".""UserLink"" = ""OUSR"".""USERID""
WHERE	 ""USR3"".""PermId"" = '{permissionId}'
	AND	 ""OUSR"".""USER_CODE"" = '{username}'
    AND  ""USR3"".""Permission"" = 'F'";


            return klib.DB.ExtensionDb.HasLines(sql);
        }
        #endregion
    }
}
